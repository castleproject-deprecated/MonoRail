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
    open System.Reflection
    open Castle.MonoRail
    open Microsoft.Data.OData
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.OData.Atom
    open Newtonsoft.Json


    [<AutoOpen>]
    module ODataStackPayloadSerializerUtils = 
        begin 
            let rec build_odataprop element (edmProp:IEdmProperty) = 
                let prop = edmProp |> box :?> TypedEdmStructuralProperty
                let name = edmProp.Name
                let value = getPropertyValue element prop
                ODataProperty(Name = name, Value = value)


            and getValue (value:obj) (edmTypeRef:IEdmTypeReference) : obj =

                if edmTypeRef.IsEnum() then 
                    // I dont think the OData lib supports writing enums yet
                    // ODataProperty(Name = name, Value = value)
                    null

                elif edmTypeRef.IsComplex() then
                    if value = null then 
                        null
                    else
                        let complexType = edmTypeRef.Definition :?> IEdmComplexType

                        let props = 
                            complexType.Properties() 
                            |> Seq.map (fun p -> build_odataprop value p)
                        let complexVal = ODataComplexValue(TypeName = edmTypeRef.FullName(), Properties = props)

                        complexVal |> box

                elif edmTypeRef.IsCollection() then
                    let collVal = ODataCollectionValue(TypeName = edmTypeRef.FullName())
                
                    if value <> null then
                        let collType = edmTypeRef.Definition :?> IEdmCollectionType
                        let elRefType = collType.ElementType

                        let items = 
                            (value :?> IEnumerable) 
                            |> Seq.cast<obj> 
                            |> Seq.map (fun e -> if elRefType.IsComplex() then
                                                    let props = 
                                                        (elRefType.Definition :?> IEdmComplexType).Properties() 
                                                        |> Seq.map (fun p -> build_odataprop e p)
                                                    ODataComplexValue(TypeName = elRefType.FullName(), Properties = props)
                                                 else
                                                    failwithf "Element type not support for collection" )
                        collVal.Items <- items

                    collVal |> box

                else 
                    value

            and getPropertyValue element (edmProp:IEdmProperty) : obj = 
                let prop = edmProp |> box :?> TypedEdmStructuralProperty
                let value = prop.GetValue(element)

                getValue value edmProp.Type


        end
    

    // Feed/Entry
    type EntitySerializer(odataMsgWriter:ODataMessageWriter) = 

        let read_item (rt:IEdmEntityType) target (reader:TextReader)  = 
            
            use jsonReader = new JsonTextReader(reader)
            let instance = 
                if target = null 
                then Activator.CreateInstance rt.TargetType
                else target

            // the two formats we support
            // odata verbose json:
            // { "d": { Prop: a, Prop2: 2 } }
            // standard json:
            // { Prop: a, Prop2: 2 }

            let getToPropertyStart () = 
                let doContinue = ref (jsonReader.TokenType <> JsonToken.PropertyName)
                while !doContinue && jsonReader.Read() do
                    if jsonReader.TokenType = JsonToken.PropertyName && jsonReader.Value.ToString() <> "d" then
                        doContinue := false
                

            let rec rec_read_object (instance) (rt:IEdmType) = 
                
                getToPropertyStart()
                
                let doContinue = ref true
                while !doContinue do
                    if jsonReader.TokenType = JsonToken.PropertyName then 
                        
                        match rt.Properties() |> Seq.tryFind (fun p -> p.Name = jsonReader.Value.ToString()) with
                        | Some prop -> 

                            if prop.IsOfKind (ResourcePropertyKind.Primitive) then 
                                jsonReader.Read() |> ignore
                                let value = jsonReader.Value
                                if value <> null then
                                    let sanitizedVal = Convert.ChangeType(value, prop.ResourceType.InstanceType)
                                    prop.SetValue(instance, sanitizedVal)
                                else
                                    prop.SetValue(instance,  null)

                            elif prop.IsOfKind (ResourcePropertyKind.ComplexType) || 
                                 prop.IsOfKind (ResourcePropertyKind.ResourceReference) then 
                                
                                // for complex types, we need to check if it's a collection
                                // use getEnumerableElementType 

                                doContinue := jsonReader.Read()
                                if !doContinue = true then
                                    if jsonReader.TokenType = JsonToken.Null then
                                        prop.SetValue(instance, null)
                                        
                                    elif jsonReader.TokenType = JsonToken.StartObject then
                                        let inner = 
                                            let existinval = prop.GetValue(instance)
                                            if existinval = null then
                                                let newVal = Activator.CreateInstance prop.ResourceType.InstanceType
                                                newVal
                                            else existinval

                                        rec_read_object inner prop.ResourceType
                                        prop.SetValue(instance, inner)

                                    else 
                                        failwithf "Unexpected json node type %O" jsonReader.TokenType

                                ()
                        
                            elif prop.IsOfKind (ResourcePropertyKind.ResourceSetReference) then 
                                let list = prop.GetValue(instance)
                                if list = null then 
                                    failwithf "Null collection property. Please set a default value for property %s on type %s" prop.Name rt.InstanceType.FullName
                                
                                // empty the collection, since this is expected to be a HTTP PUT
                                list?Clear() |> ignore

                                doContinue := jsonReader.Read()
                                if !doContinue = true then
                                    if jsonReader.TokenType = JsonToken.Null then
                                        // nothing to do, since it was cleared already
                                        ()
                                        
                                    elif jsonReader.TokenType = JsonToken.StartArray then

                                        while (jsonReader.Read() && jsonReader.TokenType = JsonToken.StartObject) do
                                            let inner = Activator.CreateInstance prop.ResourceType.InstanceType
                                            rec_read_object inner prop.ResourceType
                                            list?Add(inner) |> ignore
                                    else 
                                        failwithf "Unexpected json node type %O" jsonReader.TokenType

                                prop.SetValue(instance, list)
                        
                            else 
                                failwithf "Unsupported property kind. Expecting Primitive, or ComplexType or ResourceRef/Set"

                            doContinue := jsonReader.Read()

                        | _ -> failwithf "Property not found on model %s: %O" rt.Name jsonReader.Value 

                    else
                        doContinue := jsonReader.TokenType <> JsonToken.EndObject && jsonReader.Read()

            rec_read_object instance rt 
        

        let rec build_odatanavigation (writer:ODataWriter) element (edmProp:IEdmProperty) = 
            let prop = edmProp |> box :?> TypedEdmNavigationProperty
            let name = edmProp.Name
            // let value = prop.GetValue(element)
            // ODataProperty(Name = name, Value = value)
            let navLink = ODataNavigationLink(Name = name, IsCollection = Nullable(edmProp.Type.IsCollection()) )
            navLink.Url <- Uri("testing", UriKind.Relative)
            
            writer.WriteStart(navLink)

            // if expand
            if edmProp.Type.IsCollection() then
                let value = prop.GetValue(element)

                if value = null then
                    writer.WriteStart(ODataFeed())
                    writer.WriteEnd()
                else
                    let elements = Queryable.AsQueryable(value |> box :?> IEnumerable)
                    System.Diagnostics.Debug.Assert( prop.Type.IsCollection() )
                    let collType = prop.Type.Definition :?> IEdmCollectionType
                    System.Diagnostics.Debug.Assert( collType.ElementType.IsEntity() )
                    write_feed_items writer elements (collType.ElementType.Definition :?> IEdmEntityType) name
            else 
                let value = prop.GetValue(element)
                if value = null then
                    writer.WriteStart(null :> ODataEntry)
                    writer.WriteEnd()
                else
                    write_entry writer value (prop.Type.Definition :?> IEdmEntityType)

            writer.WriteEnd()

        and get_properties (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Structural)
            |> Seq.map (fun p -> ODataStackPayloadSerializerUtils.build_odataprop element p) 
            |> Seq.filter (fun p -> p <> null) // hack for enums

        and write_navigations (writer:ODataWriter) (element:obj) (edmType:IEdmEntityType) = 
            edmType.Properties()
            |> Seq.filter (fun p -> p.PropertyKind = EdmPropertyKind.Navigation)
            |> Seq.iter (fun p -> build_odatanavigation writer element p)
            
        and write_feed_items (writer:ODataWriter) (elements:IQueryable) (edmType:IEdmEntityType) title = 
            let feed = ODataFeed()
            feed.Id <- "testingId"
            let annotation = AtomFeedMetadata()
            annotation.Title <- new AtomTextConstruct(Text = title)
            annotation.SelfLink <- new AtomLinkMetadata(Href = Uri("relId1", UriKind.Relative), Title = title)
            feed.SetAnnotation(annotation);

            writer.WriteStart(feed)

            for e in elements do
                write_entry writer e edmType

            writer.WriteEnd()

        and write_entry  (writer:ODataWriter) (element:obj) (edmType:IEdmEntityType) = 
            let entry = ODataEntry()
            let annotation = AtomEntryMetadata()
            entry.SetAnnotation(annotation);
            // Uri id;
            // Uri idAndEditLink = Serializer.GetIdAndEditLink(element, actualResourceType, this.Provider, this.CurrentContainer, this.AbsoluteServiceUri, out id);
            // Uri relativeUri = new Uri(idAndEditLink.AbsoluteUri.Substring(this.AbsoluteServiceUri.AbsoluteUri.Length), UriKind.Relative);

            let name = edmType.FName

            entry.TypeName  <- name // actualResourceType.FullName
            entry.Id        <- "testing"  // id.AbsoluteUri
            annotation.EditLink <- AtomLinkMetadata(Title = name);
            // entry.EditLink  <- relativeUri
            // let etagValue = GetETagValue(element, actualResourceType)
            // entry.ETag = etagValue

            // hypertext support
            // PopulateODataOperations(element, resourceInstanceInFeed, entry, actualResourceType)

            writer.WriteStart (entry)
            write_navigations writer element edmType
            entry.Properties <- get_properties element (edmType)
            writer.WriteEnd()

        member x.WriteFeed  (edmEntSet:IEdmEntitySet, elements:IQueryable, elType:IEdmEntityType) = 
            let writer = odataMsgWriter.CreateODataFeedWriter(edmEntSet, elType)
            let title = "test"
            write_feed_items writer elements elType title

        member x.WriteEntry (edmEntSet:IEdmEntitySet, element:obj, elType:IEdmEntityType) = 
            let writer = odataMsgWriter.CreateODataEntryWriter(edmEntSet, elType)
            write_entry writer element elType

        member x.ReadEntry (elType:IEdmEntityType) reader = 
            let instance = Activator.CreateInstance( elType.TargetType )
            read_item elType instance reader
            instance


