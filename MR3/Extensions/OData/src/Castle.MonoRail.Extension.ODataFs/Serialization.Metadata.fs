namespace Castle.MonoRail.Extension.OData

open System
open System.IO
open System.Data.OData
open System.Data.Services.Providers
open System.Data.Services.Common
open System.Web
open System.Text
open System.Xml
open Castle.MonoRail

module MetadataSerializer =
    begin

        let private create_xmlwriter(writer:TextWriter) (encoding) = 
            let settings = XmlWriterSettings(CheckCharacters = false,
                                             ConformanceLevel = ConformanceLevel.Fragment,
                                             Encoding = encoding,
                                             Indent = true,
                                             NewLineHandling = NewLineHandling.Entitize)
            let xmlWriter = XmlWriter.Create(writer, settings)
            xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"" + encoding.WebName + "\" standalone=\"yes\"")
            xmlWriter

        type Epm = 
            static member EpmKeepInContent = "FC_KeepInContent"
            static member EpmSourcePath    = "FC_SourcePath"
            static member EpmTargetPath    = "FC_TargetPath"
            static member EpmContentKind   = "FC_ContentKind"
            static member EpmNsPrefix      = "FC_NsPrefix"
            static member EpmNsUri         = "FC_NsUri"

        // http://msdn.microsoft.com/en-us/library/dd942559%28v=prot.10%29.aspx
        let write_epm_properties (xmlWriter:XmlWriter) skipSourcePath removePrefix (items:EntityPropertyMappingAttribute seq) = 
            
            let syndication_to_path (property:SyndicationItemProperty) = 
                match property with
                | SyndicationItemProperty.AuthorEmail -> "SyndicationAuthorEmail"
                | SyndicationItemProperty.AuthorName -> "SyndicationAuthorName"
                | SyndicationItemProperty.AuthorUri -> "SyndicationAuthorUri"
                | SyndicationItemProperty.ContributorEmail -> "SyndicationContributorEmail"
                | SyndicationItemProperty.ContributorName -> "SyndicationContributorName"
                | SyndicationItemProperty.ContributorUri -> "SyndicationContributorUri"
                | SyndicationItemProperty.Updated -> "SyndicationUpdated"
                | SyndicationItemProperty.Published -> "SyndicationPublished"
                | SyndicationItemProperty.Rights -> "SyndicationRights"
                | SyndicationItemProperty.Summary -> "SyndicationSummary"
                | _ -> "SyndicationTitle"

            let syndicationtext_to_content (contentKind:SyndicationTextContentKind) = 
                match contentKind with
                | SyndicationTextContentKind.Plaintext -> "text"
                | SyndicationTextContentKind.Html -> "html"
                | _ -> "xhtml"

            // let postfix = ref 0
            
            let write_epm_attributes (att:EntityPropertyMappingAttribute) = 
                // let postAsStr = sprintf "_%d" (!postfix)

                if att.TargetSyndicationItem = SyndicationItemProperty.CustomProperty then
                    xmlWriter.WriteAttributeString(Epm.EpmTargetPath,
                                                   "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                   att.TargetPath)

                    xmlWriter.WriteAttributeString(Epm.EpmNsUri,
                                                   "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                   att.TargetNamespaceUri)
                    
                    if not <| String.IsNullOrEmpty(att.TargetNamespacePrefix) then
                        xmlWriter.WriteAttributeString(Epm.EpmNsPrefix,
                                                       "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                       att.TargetNamespacePrefix)
                else 
                    xmlWriter.WriteAttributeString(Epm.EpmTargetPath,
                                                   "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                   syndication_to_path(att.TargetSyndicationItem))
                    
                    xmlWriter.WriteAttributeString(Epm.EpmContentKind,
                                                   "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                   syndicationtext_to_content(att.TargetTextContentKind))

                
                if not skipSourcePath then
                    xmlWriter.WriteAttributeString(Epm.EpmSourcePath,
                                                   "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                                   if removePrefix then att.SourcePath.Substring(att.SourcePath.IndexOf('/') + 1) else att.SourcePath)

                xmlWriter.WriteAttributeString(Epm.EpmKeepInContent,
                                               "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata",
                                               if att.KeepInContent then "true" else "false")
                
                // incr postfix

            items |> Seq.iter write_epm_attributes


        let private write_property (xmlWriter:XmlWriter) (resRt:ResourceType) (property:ResourceProperty) = 

            let write_facets () = 
                let pType = property.Type
                let value = 
                    if property.IsOfKind(ResourcePropertyKind.Key) || (pType.IsValueType && Nullable.GetUnderlyingType(pType) = null) 
                    then "true" else "false"
                xmlWriter.WriteAttributeString("Nullable", value)

            if (property.ResourceType.ResourceTypeKind = ResourceTypeKind.Primitive) then
                xmlWriter.WriteStartElement("Property")
                xmlWriter.WriteAttributeString("Name", property.Name)
                xmlWriter.WriteAttributeString("Type", property.ResourceType.FullName)
                write_facets()

                // if (!string.IsNullOrEmpty(property.MimeType)) then
                //    MetadataSerializer.WriteDataWebMetadata(xmlWriter, "MimeType", property.MimeType);
                
                if (resRt.ResourceTypeKind == ResourceTypeKind.EntityType && resRt.ETagProperties.Contains(property)) then
                    xmlWriter.WriteAttributeString("ConcurrencyMode", "Fixed")
                
            
                resRt.OwnEpmAttributes 
                |> Seq.filter (fun epm -> (Array.get (epm.SourcePath.Split([|'/'|])) 0) = property.Name)
                |> write_epm_properties xmlWriter true false
                                    
            
            elif (property.Kind = ResourcePropertyKind.ComplexType) then
            
                xmlWriter.WriteStartElement("Property")
                xmlWriter.WriteAttributeString("Name", property.Name)
                xmlWriter.WriteAttributeString("Type", property.ResourceType.FullName)
                xmlWriter.WriteAttributeString("Nullable", "false")
                
                // replace by resRt.OwnEpmAttributes 
                // if (resRt.HasEntityPropertyMappings) then 
                   // ()

                    (*
                    IEnumerable<EntityPropertyMappingAttribute> enumerable =
                        Enumerable.Where<EntityPropertyMappingAttribute>((IEnumerable<EntityPropertyMappingAttribute>) type.OwnEpmInfo,
                                                                         (Func<EntityPropertyMappingAttribute, bool>)
                                                                         (e =>
                                                                          Enumerable.First<string>(
                                                                            (IEnumerable<string>) e.SourcePath.Split(new char[1]
                                                                                                                        {
                                                                                                                            '/'
                                                                                                                        })) ==
                                                                          property.Name));
                    MetadataSerializer.WriteEpmProperties(xmlWriter, enumerable,
                                                          Enumerable.Any<EntityPropertyMappingAttribute>(enumerable,
                                                                                                         (
                                                                                                         Func
                                                                                                            <
                                                                                                            EntityPropertyMappingAttribute
                                                                                                            , bool>)
                                                                                                         (ei =>
                                                                                                          ei.SourcePath ==
                                                                                                          property.Name)),
                                                          true);                
                    *)
            else
                xmlWriter.WriteStartElement("NavigationProperty")
                xmlWriter.WriteAttributeString("Name", property.Name)

                (*
                string fullName = property.ResourceType.FullName
                string associationTypeLookupName = MetadataSerializer.MetadataManager.GetAssociationTypeLookupName(resRt,
                                                                                                                   property)
                ResourceAssociationType resourceAssociationType
                if (!associationsInThisNamespace.TryGetValue(associationTypeLookupName, out resourceAssociationType))
                    throw new InvalidOperationException("MetadataSerializer_NoResourceAssociationSetForNavigationProperty"
                        /*((object)property.Name, (object)type.FullName)*/)
                ResourceAssociationTypeEnd associationTypeEnd = resourceAssociationType.GetResourceAssociationTypeEnd(resRt,
                                                                                                                      property)
                ResourceAssociationTypeEnd associationSetEnd = resourceAssociationType.GetRelatedResourceAssociationSetEnd(resRt,
                                                                                                                           property)
                
                xmlWriter.WriteAttributeString("Relationship", resourceAssociationType.FullName)
                xmlWriter.WriteAttributeString("FromRole", associationTypeEnd.Name)
                xmlWriter.WriteAttributeString("ToRole", associationSetEnd.Name)
                *)
            
            xmlWriter.WriteEndElement()

            ()

        let private write_properties (writer:XmlWriter) (resRt:ResourceType) = 
            resRt.PropertiesDeclaredOnThisType 
            |> Seq.iter (fun p -> write_property writer resRt p)

        let private write_entity (writer:XmlWriter) (resRt:ResourceType) = 
            
            let write_key () = 
                writer.WriteStartElement("Key")
                resRt.KeyProperties
                |> Seq.iter (fun kp -> (writer.WriteStartElement("PropertyRef")
                                        writer.WriteAttributeString("Name", kp.Name)
                                        writer.WriteEndElement() ))
                writer.WriteEndElement()

            writer.WriteStartElement("EntityType")
            writer.WriteAttributeString("Name", XmlConvert.EncodeName(resRt.Name))

            if resRt.IsAbstract || resRt.BaseType <> null then raise(NotImplementedException("Abstract/Inheritance for ResourceTypes is not supported"))
            if resRt.IsOpenType then raise(NotImplementedException("OpenTypes aren't supported"))
            if resRt.IsMediaLinkEntry then raise(NotImplementedException("MediaLinkEntry is not supported"))

            // just to force validation of type
            resRt.PropertiesDeclaredOnThisType |> ignore

            // if resRt.HasEntityPropertyMappings then 
            write_epm_properties writer false false resRt.OwnEpmAttributes // resRt.InheritedEpmInfo

            write_key ()
            write_properties writer resRt

         (* if (entityType.IsAbstract)
                xmlWriter.WriteAttributeString("Abstract", "true");
            if (entityType.IsOpenType && (entityType.BaseType == null || !entityType.BaseType.IsOpenType))
                MetadataSerializer.WriteOpenTypeAttribute(xmlWriter);
            if (entityType.IsMediaLinkEntry && (entityType.BaseType == null || !entityType.BaseType.IsMediaLinkEntry))
                xmlWriter.WriteAttributeString("HasStream", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
            if (entityType.BaseType != null)
            {
                xmlWriter.WriteAttributeString("BaseType", XmlConvert.EncodeName(entityType.BaseType.FullName));
            } *)
            
            writer.WriteEndElement()

        let private write_complex (writer:XmlWriter) (resRt:ResourceType) = 
            writer.WriteStartElement("ComplexType")
            writer.WriteAttributeString("Name", XmlConvert.EncodeName(resRt.Name))
            //MetadataSerializer.WriteProperties(writer, complexType, (Dictionary<string, ResourceAssociationType>) null,
            //                                   metadataManager)
            writer.WriteEndElement()

        let private write_type (writer:XmlWriter) (resRt:ResourceType) = 
            if resRt.ResourceTypeKind = ResourceTypeKind.EntityType then
                write_entity writer resRt
            else
                write_complex writer resRt

        let private write_associations (writer:XmlWriter) = 
            ()

        let assocationSetsCache = System.Collections.Generic.Dictionary<string, ResourceAssociationSet>()

        let private prepare (wrapper:DataServiceMetadataProviderWrapper) = 
            
            let build_association_info (rs:ResourceSetWrapper) (rt:ResourceType) (prop:ResourceProperty) = 

                let key = rs.Name + "_" + rt.FullName + "_" + prop.Name

                let r, association = assocationSetsCache.TryGetValue key
                if r then association
                else
                    let association = wrapper.GetResourceAssociationSet(rs, rt, prop)
                    if association <> null then
                        // todo: cache
                        let associationEnd = association.GetRelatedResourceAssociationSetEnd(rs, rt, prop)
                        if associationEnd.ResourceProperty <> null then
                            let associationSet = wrapper.GetResourceAssociationSet(wrapper.ValidateResourceSet(associationEnd.ResourceSet), associationEnd.ResourceType, associationEnd.ResourceProperty)
                            if associationSet = null || association.Name <> associationSet.Name 
                            then raise(InvalidOperationException("Invalid bi-directional assocation"))
                            
                        let key2 = 
                            if associationEnd.ResourceProperty <> null
                            then sprintf "%s_%s_%s" associationEnd.ResourceSet.Name associationEnd.ResourceProperty.ResourceType.FullName  associationEnd.ResourceProperty.Name
                            else sprintf "%s_Null_%s_%s" associationEnd.ResourceSet.Name rt.FullName prop.Name
                        assocationSetsCache.Add (key2, association)
                        assocationSetsCache.Add (key, association)
                    association

                ()

            let populate_association (rs:ResourceSetWrapper) = 
                let rt = rs.ResourceType
                rt.PropertiesDeclaredOnThisType 
                |> Seq.filter (fun p -> p.ResourceType.ResourceTypeKind = ResourceTypeKind.EntityType)
                |> Seq.iter (fun p -> build_association_info rs rt p)
                

            wrapper.ResourceSets |> Seq.iter populate_association
            
            ()
            
            

            (* 
        foreach (ResourceProperty navigationProperty in Enumerable.Where(resourceType.PropertiesDeclaredOnThisType, p => p.TypeKind == ResourceTypeKind.EntityType))
        {
          ResourceAssociationSet resourceAssociationSet = this.GetAndValidateResourceAssociationSet(resourceSet, resourceType, navigationProperty);
          if (resourceAssociationSet != null)
            resourceAssociationSet.ResourceAssociationType = this.GetResourceAssociationType(resourceAssociationSet, resourceSet, resourceType, navigationProperty);
        }            
            *)
           

        let public serialize(writer:TextWriter, wrapper:DataServiceMetadataProviderWrapper, enc:Encoding) = 
            let xmlWriter = create_xmlwriter writer enc

            let aggregated = prepare(wrapper)

            xmlWriter.WriteStartElement("edmx", "Edmx", "http://schemas.microsoft.com/ado/2007/06/edmx")
            xmlWriter.WriteAttributeString("Version", "1.0")
            xmlWriter.WriteStartElement("edmx", "DataServices", "http://schemas.microsoft.com/ado/2007/06/edmx")
            xmlWriter.WriteAttributeString("xmlns", "m", null, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")
            xmlWriter.WriteAttributeString("m", "DataServiceVersion", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "2.0")
            
            xmlWriter.WriteStartElement("Schema", "http://schemas.microsoft.com/ado/2008/09/edm")
            xmlWriter.WriteAttributeString("Namespace", wrapper.ContainerNamespace)
            xmlWriter.WriteAttributeString("xmlns", "d", null, "http://schemas.microsoft.com/ado/2007/08/dataservices")
            xmlWriter.WriteAttributeString("xmlns", "m", null, "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")

            wrapper.ResourceTypes
            |> Seq.iter (fun rs -> write_type xmlWriter rs )

            write_associations xmlWriter 

            // travese namespaces + types
            //   write schema
            //           entityType
            //           Complextypes
            //           associations
            //           EntityContainer

            // WriteAssociationTypes
            // write_entitycontainer xmlWriter wrapper.ResourceSets

            xmlWriter.WriteEndElement() // Schema
            xmlWriter.WriteEndElement() // edmx:DataServices
            xmlWriter.WriteEndElement() // edmx:Edmx
            xmlWriter.Flush()

    end