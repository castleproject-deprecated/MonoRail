//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.OData.Internal

    open System
    open System.Collections
    open System.Collections.Specialized
    open System.Collections.Generic
    open System.IO
    open System.Text
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.OData.Atom



    // Feed/Entry
    type EntitySerializer(odataWriter:ODataWriter) as self = 

        let build_odataprop element (edmProp:IEdmProperty) = 
            let prop = edmProp |> box :?> TypedEdmStructuralProperty
            let name = edmProp.Name
            let value = prop.GetValue(element)

            if edmProp.Type.IsEnum() then 
                // I dont think the OData lib supports writing enums yet
                // ODataProperty(Name = name, Value = value)
                null

            elif edmProp.Type.IsCollection() then
                ODataProperty(Name = name, Value = value)

            else 
                ODataProperty(Name = name, Value = value)

        let build_odatanavigation element (edmProp:IEdmProperty) = 
            let prop = edmProp |> box :?> TypedEdmNavigationProperty
            let name = edmProp.Name
            // let value = prop.GetValue(element)
            // ODataProperty(Name = name, Value = value)
            let navLink = ODataNavigationLink(Name = name, IsCollection = Nullable(edmProp.Type.IsCollection()) )
            navLink.Url <- Uri("testing", UriKind.Relative)
            
            odataWriter.WriteStart(navLink)

            // if expand
            if edmProp.Type.IsCollection() then
                let value = prop.GetValue(element)

                if value = null then
                    odataWriter.WriteStart(ODataFeed())
                    odataWriter.WriteEnd()
                else
                    self.WriteFeed (Queryable.AsQueryable(value |> box :?> IEnumerable), prop.Type.Definition :?> IEdmEntityType)
            else 
                let value = prop.GetValue(element)
                if value = null then
                    odataWriter.WriteStart(null :> ODataEntry)
                    odataWriter.WriteEnd()
                else
                    self.WriteEntry(value, prop.Type.Definition :?> IEdmEntityType)

            odataWriter.WriteEnd()

        let get_properties (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Structural)
            |> Seq.map (fun p -> build_odataprop element p) 
            |> Seq.filter (fun p -> p <> null) // hack for enums

        let write_navigations (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Navigation)
            |> Seq.iter (fun p -> build_odatanavigation element p)
            

        let rec write_feed_items (elements:IQueryable) (edmType:IEdmEntityType) title = 
            let feed = ODataFeed()
            feed.Id <- "testingId"
            let annotation = AtomFeedMetadata()
            annotation.Title <- new AtomTextConstruct(Text = title)
            annotation.SelfLink <- new AtomLinkMetadata(Href = Uri("relId1", UriKind.Relative), Title = title)
            feed.SetAnnotation(annotation);

            odataWriter.WriteStart(feed)

            for e in elements do
                write_entry e edmType

            odataWriter.WriteEnd()

        and write_entry (element:obj) (edmType:IEdmEntityType) = 
            let entry = ODataEntry()
            let annotation = AtomEntryMetadata()
            entry.SetAnnotation(annotation);

            // Uri id;
            // Uri idAndEditLink = Serializer.GetIdAndEditLink(element, actualResourceType, this.Provider, this.CurrentContainer, this.AbsoluteServiceUri, out id);
            // Uri relativeUri = new Uri(idAndEditLink.AbsoluteUri.Substring(this.AbsoluteServiceUri.AbsoluteUri.Length), UriKind.Relative);

            let name = edmType.FName

            entry.TypeName  <- name // actualResourceType.FullName
            entry.Id        <- "testing"  // id.AbsoluteUri
            // entry.EditLink  <- relativeUri
            annotation.EditLink <- AtomLinkMetadata(Title = name);

            // let etagValue = GetETagValue(element, actualResourceType)
            // entry.ETag = etagValue

            // hypertext support
            // PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType)

            odataWriter.WriteStart(entry)
            write_navigations element edmType
            entry.Properties <- get_properties element (edmType)
            odataWriter.WriteEnd()

        member x.WriteFeed  (elements:IQueryable, elType:IEdmEntityType) = 
            let title = "test"
            write_feed_items elements elType title
            ()

        member x.WriteEntry (element:obj, elType:IEdmEntityType) = 
            write_entry element elType