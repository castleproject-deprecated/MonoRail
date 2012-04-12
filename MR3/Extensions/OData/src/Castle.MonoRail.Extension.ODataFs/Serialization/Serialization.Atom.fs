namespace Castle.MonoRail.Extension.OData

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open System.Xml
open System.IO
open System.Text
open System.ServiceModel.Syndication
open System.Data.OData
open System.Data.Services.Providers
open System.Data.Services.Common

module AtomSerialization = 
    begin
        let private emptyPerson = SyndicationPerson(null, String.Empty, null)
        let private qualifiedNullAttribute = XmlQualifiedName("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
        let private qualifiedDataWebPrefix = XmlQualifiedName("d", "http://www.w3.org/2000/xmlns/")
        let private qualifiedDataWebMetadataPrefix = XmlQualifiedName("m", "http://www.w3.org/2000/xmlns/")
        let private linkRelResource = Uri("http://schemas.microsoft.com/ado/2007/08/dataservices/related/")
        let private categoryScheme = "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"

        type System.Data.Services.Providers.ResourceProperty
            with
                member x.GetValue(instance:obj) = 
                    let prop = instance.GetType().GetProperty(x.Name)
                    prop.GetValue(instance, null)
                member x.GetValueAsStr(instance:obj) = 
                    let prop = instance.GetType().GetProperty(x.Name)
                    let value = prop.GetValue(instance, null)
                    // XmlConvert.ToString(value)
                    if value = null 
                    then null 
                    else value.ToString()

        type System.Data.Services.Providers.ResourceType 
            with 
                member x.PathWithKey(instance:obj) = 
                    let keyValue = 
                        if x.KeyProperties.Count = 1 
                        then x.KeyProperties.[0].GetValueAsStr(instance)
                        else failwith "Composite keys are not supported"
                    sprintf "%s(%s)" x.Name keyValue


        type ContentDict (items:(string*string*obj) seq) = 
            inherit SyndicationContent()
            let _items = List(items)

            let rec write_primitive_prop (writer:XmlWriter) name typename (value:obj) = 
                writer.WriteStartElement (name, "http://schemas.microsoft.com/ado/2007/08/dataservices")
                
                if typename <> "Edm.String" then 
                    writer.WriteAttributeString ("type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", typename)

                if value = null then 
                    writer.WriteAttributeString("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true")
                elif value :? ContentDict then
                    let inner = value :?> ContentDict
                    inner.InternalWrite (writer, name)
                else 
                    writer.WriteString(value.ToString())
                
                writer.WriteEndElement()

            new () = ContentDict(Seq.empty)

            member x.Add(name, typename, value) = 
                _items.Add( (name, typename, value) )

            member internal x.InternalWrite (writer, name) = 
                _items |> Seq.iter (fun (name,typename,value) -> write_primitive_prop writer name typename value) 

            override x.Type = "application/xml"
            override x.Clone() = upcast ContentDict(_items)
            override x.WriteContentsTo (writer) = 
                if _items.Count > 0 then 
                    writer.WriteStartElement("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
                    _items |> Seq.iter (fun (name,typename,value) -> write_primitive_prop writer name typename value) 
                    writer.WriteEndElement()


        let rec private build_content_from_properties (relResUri:Uri) instance (rt:ResourceType) (item:SyndicationItem) = 
            let content = ContentDict()
            let skipContent = ref false

            for prop in rt.Properties do
                let otherRt = prop.ResourceType
                skipContent := false
                
                let atts = rt.OwnEpmAttributes |> Seq.filter (fun epm -> epm.SourcePath = prop.Name)

                for att in atts do 
                    skipContent := not att.KeepInContent
                    match att.TargetSyndicationItem with
                    // todo, lots of extra cases
                    | SyndicationItemProperty.Title -> 
                        item.Title <- TextSyndicationContent (prop.GetValueAsStr(instance))
                    | _ -> ()

                if prop.IsOfKind ResourcePropertyKind.ResourceReference then
                    // <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Supplier" type="application/atom+xml;type=entry" title="Supplier" href="Products(0)/Supplier" />
                    item.Links.Add (SyndicationLink(Uri(relResUri.OriginalString + "/" + prop.Name, UriKind.Relative), 
                                                    linkRelResource.AbsoluteUri + otherRt.Name, 
                                                    otherRt.Name, 
                                                    "application/atom+xml;type=entry", 0L))
                    
                
                elif prop.IsOfKind ResourcePropertyKind.ResourceSetReference then
                    // <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Products" type="application/atom+xml;type=feed" title="Products" href="Categories(2)/Products" />
                    item.Links.Add (SyndicationLink(Uri(relResUri.OriginalString + "/" + prop.Name, UriKind.Relative), 
                                                    linkRelResource.AbsoluteUri + otherRt.Name, 
                                                    otherRt.Name, 
                                                    "application/atom+xml;type=feed", 0L))


                elif prop.IsOfKind ResourcePropertyKind.ComplexType then
                    // <d:Address m:type="[namespace].Address"> ... </d:Address>

                    let innerinstance = prop.GetValue(instance)
                    let inner = build_content_from_properties relResUri innerinstance otherRt item
                    content.Add (prop.Name, prop.ResourceType.FullName, inner)

                elif prop.IsOfKind ResourcePropertyKind.Primitive && not !skipContent then
                    content.Add (prop.Name, prop.ResourceType.FullName, (prop.GetValue(instance)))
            
            content

        let internal build_item (wrapper:DataServiceMetadataProviderWrapper) (instance) (svcBaseUri:Uri) (containerUri:Uri) (rt:ResourceType) addNs = 
            let item = SyndicationItem()
            let relResUri = Uri(rt.PathWithKey(instance), UriKind.Relative)
            let fullResUri = Uri(svcBaseUri, relResUri)
            let resourceSet = wrapper.ResourceSets |> Seq.tryFind (fun rs -> rs.ResourceType = rt)

            if addNs then
                item.AttributeExtensions.Add (qualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices")
                item.AttributeExtensions.Add (qualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            item.BaseUri <- svcBaseUri
            // item.AttributeExtensions.Add (qualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices")
            // item.AttributeExtensions.Add (qualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            item.Title <- TextSyndicationContent(String.Empty)

            (* 
                if rt is associated with ResourceSet
                    ID = baseUri + resourceId
                    eg  /Products(1)/categories
                        id = /categories(1)
                else
                    ID = AggregateRoot + path + resourceId
                    eg  /Products(1)/categories
                        id = /Products(1)/categories(121)
            *)

            item.Id <- 
                (match resourceSet with 
                 | Some rs -> Uri(svcBaseUri, relResUri)
                 | _ -> Uri(containerUri, relResUri)).AbsoluteUri

            item.Links.Add(SyndicationLink(relResUri, "edit", rt.InstanceType.Name, null, 0L))
            
            item.Authors.Add emptyPerson

            item.Categories.Add(SyndicationCategory(rt.Namespace + "." + rt.InstanceType.Name, categoryScheme, null))

            item.Content <- build_content_from_properties relResUri instance rt item

            item



        let internal write_items (wrapper:DataServiceMetadataProviderWrapper) (svcBaseUri:Uri) (containerUri:Uri) (rt:ResourceType) 
                                 (items:IEnumerable) (writer:TextWriter) (enc:Encoding) = 

            let rootUri = if containerUri <> null then containerUri else svcBaseUri

            System.Diagnostics.Debug.Assert (rootUri <> null)

            let syndicationItems = 
                let lst = List<SyndicationItem>()
                for item in items do
                    lst.Add (build_item wrapper item svcBaseUri containerUri rt false)
                lst

            let feed = SyndicationFeed(syndicationItems)

            feed.BaseUri <- svcBaseUri
            feed.AttributeExtensions.Add (qualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices")
            feed.AttributeExtensions.Add (qualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            feed.Title <- TextSyndicationContent(rt.Name)
            feed.Id    <- rootUri.AbsoluteUri
            
            // temporary
            feed.LastUpdatedTime <- DateTimeOffset.MinValue

            feed.Links.Add(SyndicationLink(Uri(rt.Name, UriKind.Relative), "self", rt.Name, null, 0L))
            
            let xmlWriter = SerializerCommons.create_xmlwriter writer enc
            feed.GetAtom10Formatter().WriteTo( xmlWriter )
            xmlWriter.Flush()

        let internal write_item (wrapper:DataServiceMetadataProviderWrapper) (svcBaseUri:Uri) (containerUri:Uri) (rt:ResourceType) 
                                (item:obj) (writer:TextWriter) (enc:Encoding) = 

            let syndicationItem = build_item wrapper item svcBaseUri containerUri rt true
            let xmlWriter = SerializerCommons.create_xmlwriter writer enc
            syndicationItem.GetAtom10Formatter().WriteTo( xmlWriter )
            xmlWriter.Flush()

        let internal read_item (rt:ResourceType) (reader:TextReader) (enc:Encoding) = 
            let reader = SerializerCommons.create_xmlreader reader enc
            let fmt = Atom10ItemFormatter()
            fmt.ReadFrom(reader)
            let item = fmt.Item

            for prop in rt.PropertiesDeclaredOnThisType do
                // rt.OwnEpmAttributes
                ()

            Activator.CreateInstance rt.InstanceType


        let internal read_feed (rt:ResourceType) (reader:TextReader) (enc:Encoding) = 
            raise(NotImplementedException())

        let CreateDeserializer () = 
            { new Deserializer() with 
                override x.DeserializeMany (rt, reader, enc) = 
                    read_feed rt reader enc
                override x.DeserializeSingle (rt, reader, enc) = 
                    read_item rt reader enc
            }

        let CreateSerializer () = 
            { new Serializer() with 
                override x.SerializeMany(wrapper, svcBaseUri, containerUri , rt, items, writer, enc) = 
                    write_items wrapper svcBaseUri containerUri rt items writer enc
                override x.SerializeSingle(wrapper, svcBaseUri, containerUri, rt, item, writer, enc) = 
                    write_item wrapper svcBaseUri containerUri rt item writer enc 
            }

    end
