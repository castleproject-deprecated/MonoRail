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
    type EntitySerializer(odataWriter:ODataWriter) = 

        let build_odataprop element (edmProp:IEdmProperty) = 
            match edmProp.PropertyKind with
            | EdmPropertyKind.Structural -> 
                let prop = edmProp |> box :?> TypedEdmStructuralProperty
                let name = edmProp.Name
                let value = prop.GetValue(element)
                ODataProperty(Name = name, Value = value)

            | EdmPropertyKind.Navigation -> 
                let prop = edmProp |> box :?> TypedEdmNavigationProperty
                let name = edmProp.Name
                let value = ""
                let value = prop.GetValue(element)
                ODataProperty(Name = name, Value = value)

            | _ -> failwithf "unsupported"


        let get_properties (element:obj) (edmType:IEdmEntityType) = 
            let edmProperties = edmType.Properties()
            edmProperties |> Seq.map (fun p -> build_odataprop element p)


        let write_entry (element:obj) (edmType:IEdmEntityType) = 
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
            // PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType)

            odataWriter.WriteStart(entry)
            // WriteNavigationProperties(expanded, element, resourceInstanceInFeed, actualResourceType, idAndEditLink, relativeUri, enumerable);
            entry.Properties <- get_properties element (edmType) // this.GetEntityProperties(element, actualResourceType, relativeUri, enumerable);
            odataWriter.WriteEnd()
            odataWriter.Flush()

        member x.WriteFeed  (elements, elType) = 
            // write_feed_items elements, elType
            ()

        member x.WriteEntry (element, elType) = 
            write_entry element elType