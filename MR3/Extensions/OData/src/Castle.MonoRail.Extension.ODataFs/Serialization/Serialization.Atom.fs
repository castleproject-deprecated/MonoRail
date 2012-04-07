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
        let private linkRelResource = "http://schemas.microsoft.com/ado/2007/08/dataservices/related/"

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


        let rec private build_content_from_properties instance (rt:ResourceType) (item:SyndicationItem) = 
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
                    // item.Links.Add (SyndicationLink(Uri(fullResUri, otherRt.Name), linkRelResource, otherRt.Name, "application/atom+xml;type=entry", 0L))
                    ()
                
                elif prop.IsOfKind ResourcePropertyKind.ResourceSetReference then
                    // <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Products" type="application/atom+xml;type=feed" title="Products" href="Categories(2)/Products" />
                    // item.Links.Add (SyndicationLink(Uri(fullResUri, otherRt.Name), linkRelResource, otherRt.Name, "application/atom+xml;type=entry", 0L))
                    ()

                elif prop.IsOfKind ResourcePropertyKind.ComplexType then
                    // <d:Address m:type="[namespace].Address"> ... </d:Address>

                    let innerinstance = prop.GetValue(instance)
                    let inner = build_content_from_properties innerinstance otherRt item
                    content.Add (prop.Name, prop.ResourceType.FullName, inner)
                    ()

                elif prop.IsOfKind ResourcePropertyKind.Primitive && not !skipContent then
                    content.Add (prop.Name, prop.ResourceType.FullName, (prop.GetValue(instance)))
            
            content


        let internal build_item (instance) (baseUri:Uri) (rt:ResourceType) = 
            (*<entry>
                <id>http://services.odata.org/OData/OData.svc/Products(0)</id>
                <title type="text">Bread</title>
                <summary type="text">Whole grain bread</summary>
                <updated>2012-04-07T09:59:19Z</updated>
                <author>
                  <name />
                </author>
                <link rel="edit" title="Product" href="Products(0)" />
                <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Category" type="application/atom+xml;type=entry" title="Category" href="Products(0)/Category" />
                <link rel="http://schemas.microsoft.com/ado/2007/08/dataservices/related/Supplier" type="application/atom+xml;type=entry" title="Supplier" href="Products(0)/Supplier" />
                <category term="[namespace].Product" scheme="http://schemas.microsoft.com/ado/2007/08/dataservices/scheme" />
                <content type="application/xml">
                  <m:properties>
                    <d:ID m:type="Edm.Int32">0</d:ID>
                    <d:ReleaseDate m:type="Edm.DateTime">1992-01-01T00:00:00</d:ReleaseDate>
                    <d:DiscontinuedDate m:type="Edm.DateTime" m:null="true" />
                    <d:Rating m:type="Edm.Int32">4</d:Rating>
                    <d:Price m:type="Edm.Decimal">2.5</d:Price>
                  </m:properties>
                </content>
              </entry> *)
            let item = SyndicationItem()
            let relResUri = Uri(rt.Name, UriKind.Relative)
            let fullResUri = Uri(baseUri, relResUri)

            item.BaseUri <- baseUri
            // item.AttributeExtensions.Add (qualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices")
            // item.AttributeExtensions.Add (qualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            item.Title <- TextSyndicationContent(rt.Name)
            item.Id    <- fullResUri.AbsoluteUri
            // item.Links.Add(SyndicationLink(Uri(rt.Name, UriKind.Relative), "self", rt.Name, null, 0L))
            item.Authors.Add emptyPerson

            item.Content <- build_content_from_properties instance rt item

            item


        let internal write_items (baseUri:Uri) (rt:ResourceType) (items:IEnumerable) (writer:TextWriter) (enc:Encoding) = 

            let resUri = Uri(rt.Name, UriKind.Relative)

            let syndicationItems = 
                let lst = List<SyndicationItem>()
                for item in items do
                    lst.Add (build_item item baseUri rt)
                lst

            let feed = SyndicationFeed(syndicationItems)

            feed.BaseUri <- baseUri
            feed.AttributeExtensions.Add (qualifiedDataWebPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices")
            feed.AttributeExtensions.Add (qualifiedDataWebMetadataPrefix, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            feed.Title <- TextSyndicationContent(rt.Name)
            feed.Id    <- Uri(baseUri, resUri).AbsoluteUri
            feed.Links.Add(SyndicationLink(Uri(rt.Name, UriKind.Relative), "self", rt.Name, null, 0L))
            
            let xmlWriter = SerializerCommons.create_xmlwriter writer enc
            feed.GetAtom10Formatter().WriteTo( xmlWriter )
            xmlWriter.Flush()


        let CreateDeserializer () = null

        let CreateSerializer () = 
            { new Serializer() with 
                  override x.SerializeMany(baseUri, rt, items, writer, enc) = 
                      write_items baseUri rt items writer enc
                      
            }

    end
