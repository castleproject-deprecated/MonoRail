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
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.Edm
    open Microsoft.Data.Edm.Library
    open Microsoft.Data.Edm.Expressions 
    open Microsoft.Data.OData


    type EntityAccessInfo = {
        RawPathSegment : string;
        Uri : Uri;
        mutable ManyResult : IQueryable;
        mutable SingleResult : obj;
        EdmSet : IEdmEntitySet
        EdmEntityType : IEdmEntityTypeReference
        ReturnType : IEdmTypeReference
        container : IEdmEntityContainer
        Name : string; 
        Key : string;
    }

    type PropertyAccessInfo = {
        RawPathSegment : string;
        Uri : Uri;
        mutable ManyResult : IEnumerable;
        mutable SingleResult : obj;
        EdmSet : IEdmEntitySet option
        ReturnType : IEdmTypeReference
        Property : IEdmProperty
        Key : string
    }

    type MetaSegment = 
        | Nothing
        | Metadata 
        | Count
        | Value
        | Batch

    and MetaQuerySegment = 
        | Nothing
        | Format of ODataFormat
        | Skip of int
        | Top of int
        | OrderBy of string
        | Expand of string
        | Select of string
        | InlineCount of string
        | Filter of string
 
    and UriSegment = 
        | Nothing
        | ServiceDirectory 
        | EntitySet of EntityAccessInfo
        | ComplexType of PropertyAccessInfo
        | PropertyAccess of PropertyAccessInfo
        | FunctionOperation of EntityAccessInfo


    module SegmentParser =
        begin
            // TODO, change MonoRail to also recognize 
            // "X-HTTP-Method", and gives it the value MERGE, PUT or DELETE.

            let private get_entityset_from_model (container:IEdmModel) (name) = 
                container.EntityContainers() 
                |> Seq.map (fun c -> c, c.EntitySets())
                |> Seq.collect (fun (c, set) -> set |> Seq.map (fun s -> c, s))
                |> Seq.tryFind (fun (c, e) -> e.Name === name)

            let private tryget_entityset (container:IEdmEntityContainer) (edmType:IEdmEntityType) = 
                container.EntitySets()
                |> Seq.tryFind (fun e -> e.ElementType = edmType)

            let private get_entityset_from_container (container:IEdmEntityContainer) (name) = 
                container.EntitySets() 
                |> Seq.tryFind (fun set -> set.Name === name)

            let (|Meta|_|) (arg:string) = 
                if arg.StartsWith("$", StringComparison.Ordinal) 
                then match arg.Substring(1).ToLowerInvariant() with 
                     | "metadata" -> Some(MetaSegment.Metadata)
                     | "batch"    -> Some(MetaSegment.Batch)
                     | "count"    -> Some(MetaSegment.Count)
                     | "value"    -> Some(MetaSegment.Value)
                     // | "links"    -> Some(MetaSegment.Links)
                     | _ -> None
                else None

            let (|SegmentWithKey|_|) (arg:string) = 
                let ``match`` = Constants.SegmentKeyRegex.Match(arg)
                if ``match``.Success && ``match``.Groups.Count = 3 then
                    let name = ``match``.Groups.[1].Captures.[0].Value
                    let id = ``match``.Groups.[2].Captures.[0].Value.Trim([|'(';')'|])
                    Some(name,id)
                else None

            let (|SegmentWithoutKey|_|) (arg:string) = 
                if arg.IndexOfAny([|'$';'('|]) = -1
                then Some(arg)
                else None

            let (|PropertyAccess|_|) (rt:IEdmTypeReference option) (arg:string) = 
                let name, key = 
                    match arg with 
                    | SegmentWithKey (name, key) -> (name,key)
                    | _ -> arg, null
                match rt with
                | Some r -> 
                    match r.TypeKind() with
                    | EdmTypeKind.Entity 
                    | EdmTypeKind.Complex -> 
                        let structured = r.Definition :?> IEdmStructuredType
                        match structured.Properties() |> Seq.tryFind (fun p -> p.Name = name) with
                        | Some prop -> Some(prop, key)
                        | _ -> None
                    | _ -> None
                | _ -> None

            let (|BindableFunctionAccess|_|) (container:IEdmEntityContainer) (typeRef:IEdmTypeReference option) (arg:string) =
                let functionImports = container.FindFunctionImports(arg)
                
                let candidates = 
                    functionImports
                    |> Seq.filter (fun fi -> fi.IsBindable && 
                                             typeRef.IsSome && 
                                             fi.Parameters.Count() > 0 && 
                                             EdmTypeSystem.edmTypeRefComparer.Equals(fi.Parameters.ElementAt(0).Type, typeRef.Value) )
                    
                if candidates.Any() then 
                    match typeRef.Value.TypeKind() with
                    | EdmTypeKind.Entity 
                    | EdmTypeKind.Collection 
                    | EdmTypeKind.Complex -> 
                        Some(candidates.Single())
                    // | EdmTypeKind.Enum 
                    // | EdmTypeKind.EntityReference 
                    | _ -> None
                else None


            let (|EntitySetKeyedAccess|_|) (container:IEdmModel) (arg:string)  =  
                match arg with 
                | SegmentWithKey (name, key) -> 
                    match get_entityset_from_model container name with
                    | Some (c,entSet) -> Some(c, entSet, name, key)
                    | _ -> None
                | _ -> None
            
            let (|EntitySetAccess|_|) (container:IEdmModel) (arg:string)  =  
                match arg with 
                | SegmentWithoutKey name -> 
                    match get_entityset_from_model container name with
                    | Some (c,entSet) -> Some(c, entSet, name)
                    | _ -> None
                | _ -> None


            let internal process_first (firstSeg:string) (model:IEdmModel) (svcUri:Uri) 
                                       (meta:Ref<MetaSegment>) = 
                match firstSeg with 
                | "" -> 
                    UriSegment.ServiceDirectory

                | Meta m -> // todo: semantic validation
                    Diagnostics.Debug.Assert (!meta = MetaSegment.Nothing)
                    meta := m
                    UriSegment.Nothing
                
                | EntitySetAccess model (container, entset, name) -> 
                    UriSegment.EntitySet({ Uri=Uri(svcUri, name); 
                                           RawPathSegment=firstSeg; 
                                           EdmSet = entset; 
                                           EdmEntityType = EdmEntityTypeReference( entset.ElementType, false )
                                           ReturnType = EdmCollectionTypeReference( EdmCollectionType( (EdmEntityTypeReference(entset.ElementType, false)) ), false)
                                           container = container
                                           Name = name; Key = null; 
                                           SingleResult = null; ManyResult = null; })
                
                | EntitySetKeyedAccess model (container, entset, name, key) -> 
                    UriSegment.EntitySet({ Uri=Uri(svcUri, firstSeg); 
                                           RawPathSegment=firstSeg; 
                                           EdmSet = entset; 
                                           EdmEntityType = EdmEntityTypeReference( entset.ElementType, false ) 
                                           ReturnType = EdmEntityTypeReference( entset.ElementType, false )
                                           container = container
                                           Name = name; Key = key; 
                                           SingleResult = null; ManyResult = null; })
                
                | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))


            let internal build_segment_for_property entContainer (kind:EdmTypeKind) (baseUri:Uri) (rawSegment:string) (prop:IEdmProperty) key = 
                match kind with 
                | EdmTypeKind.Primitive -> 
                    let info = {    EdmSet = None
                                    Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; 
                                    ReturnType=prop.Type 
                                    Property=prop; Key = null; SingleResult = null; ManyResult = null 
                               }
                    UriSegment.PropertyAccess(info)

                | EdmTypeKind.Complex -> 
                    let info = {    EdmSet = None
                                    Uri=Uri(baseUri, rawSegment)
                                    RawPathSegment=rawSegment 
                                    ReturnType=prop.Type
                                    Property=prop; Key = null; SingleResult = null; ManyResult = null 
                               }
                    UriSegment.ComplexType(info)

                | EdmTypeKind.Entity -> 
                    let entSet = 
                        if prop.Type.IsEntity()
                        then tryget_entityset entContainer (prop.Type.Definition :?> IEdmEntityType)
                        else None

                    let info = {    EdmSet = entSet
                                    Uri=Uri(baseUri, rawSegment)
                                    RawPathSegment=rawSegment 
                                    ReturnType=prop.Type
                                    Property=prop; Key = key; SingleResult = null; ManyResult = null  
                               }
                    UriSegment.PropertyAccess(info)

                | EdmTypeKind.Collection -> 
                    let entSet = 
                        let collElType = (prop.Type.Definition :?> IEdmCollectionType).ElementType
                        if collElType.IsEntity()
                        then tryget_entityset entContainer (collElType.Definition :?> IEdmEntityType)
                        else None
                    
                    if key = null then
                        let coll = prop.Type :?> IEdmCollectionTypeReference
                        let info = {    EdmSet = entSet 
                                        Uri=Uri(baseUri, rawSegment)
                                        RawPathSegment=rawSegment 
                                        ReturnType=coll
                                        Property=prop; Key = null; SingleResult = null; ManyResult = null 
                                   }
                        UriSegment.PropertyAccess(info)
                    else
                        let coll = prop.Type.Definition :?> IEdmCollectionType
                        let info = {    EdmSet = entSet
                                        Uri=Uri(baseUri, rawSegment); 
                                        RawPathSegment=rawSegment 
                                        ReturnType=coll.ElementType
                                        Property=prop; Key = key; SingleResult = null; ManyResult = null 
                                   }
                        UriSegment.PropertyAccess(info)

                | _ -> raise(HttpException(500, "Unsupported property kind for segment "))


            let partition_qs_parameters (qs:NameValueCollection) = 
                let odataParams, ordinaryParams = 
                    let odata, ordinary =
                        qs.AllKeys 
                        |> List.ofSeq |> List.partition (fun k -> k.StartsWith("$", StringComparison.Ordinal))
                    let ordinaryParams = NameValueCollection(StringComparer.OrdinalIgnoreCase)
                    ordinary |> List.iter (fun i -> ordinaryParams.[i] <- qs.[i])
                    let odataparms = NameValueCollection(StringComparer.OrdinalIgnoreCase)
                    odata    |> List.iter (fun i -> odataparms.[i] <- qs.[i])
                    odataparms, ordinary
                odataParams, ordinaryParams

            // this rec function parses all segments but the first
            let rec parse_segment (all:UriSegment list) (previous:UriSegment) (svcUri:Uri) (meta:Ref<MetaSegment>)
                                  (contextRT:IEdmTypeReference option) (entContainer:IEdmEntityContainer) 
                                  (lastCollAccessSegment:Ref<UriSegment>)
                                  (rawSegments:string[]) (index:int) : UriSegment[] = 
                
                let contextTypeRef : IEdmTypeReference option = 
                    match contextRT with
                    | Some t -> 
                        match t.TypeKind() with
                        | EdmTypeKind.Entity ->
                            Some t

                        | EdmTypeKind.Collection ->
                            Some t

                        | _ -> None
                    | _ -> None

                if index < rawSegments.Length && (rawSegments.[index] <> String.Empty && index <= rawSegments.Length - 1) then
                    let rawSegment = rawSegments.[index]
                    let resourceType : Ref<IEdmTypeReference> = ref null
                    let baseUri = 
                        match previous with 
                        | UriSegment.EntitySet d -> Uri(d.Uri.AbsoluteUri + "/")
                        | UriSegment.PropertyAccess d -> Uri(d.Uri.AbsoluteUri + "/")
                        | _ -> svcUri

                    let newSegment = 
                        match rawSegment with
                        | Meta m -> 
                            // todo: semantic validation 
                            // (e.g. at this point, $metadata is not accepted)
                            Diagnostics.Debug.Assert (!meta = MetaSegment.Nothing)
                            meta := m
                            UriSegment.Nothing
                        
                        // | OperationAccess model contextRT o -> 
                        //    UriSegment.ActionOperation(o)

                        | PropertyAccess contextRT (prop, key) -> 
                            resourceType := prop.Type
                            match prop.PropertyKind with 
                            | EdmPropertyKind.Structural 
                            | EdmPropertyKind.Navigation ->
                                build_segment_for_property entContainer (prop.Type.Definition.TypeKind) baseUri rawSegment prop key
                            | _ -> raise(HttpException(500, "Unsupported property kind for segment "))

                        | BindableFunctionAccess entContainer contextTypeRef func ->
                            let entSet = (func.EntitySet :?> IEdmEntitySetReferenceExpression).ReferencedEntitySet

                            resourceType := func.ReturnType

                            let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; 
                                         EdmSet = entSet
                                         EdmEntityType = EdmEntityTypeReference( entSet.ElementType, false )
                                         ReturnType = func.ReturnType
                                         container = entContainer
                                         Name = rawSegment; Key = null; 
                                         SingleResult = null; ManyResult = null; }

                            UriSegment.FunctionOperation(info)

                        | _ -> raise(HttpException(400, "Segment does not match a property or operation"))
                    
                    match newSegment with 
                    | UriSegment.EntitySet _ 
                    | UriSegment.PropertyAccess _ -> 
                        lastCollAccessSegment := newSegment
                    | _ -> ()                    

                    let rt = if !resourceType <> null then Some(!resourceType) else None

                    let newList = 
                        all @ (if newSegment = UriSegment.Nothing then [] else [newSegment])

                    parse_segment newList newSegment svcUri meta rt entContainer lastCollAccessSegment rawSegments (index + 1)
                
                else all |> Array.ofList

            // we also need to parse QS 
            // ex url/Suppliers?$filter=Address/City eq 'Redmond' 
            let parse(path:string, qs:NameValueCollection, model:IEdmModel, svcUri:Uri) : UriSegment[] * MetaSegment * MetaQuerySegment[] = 

                let odataParams, ordinaryParams = partition_qs_parameters qs

                // tracks the meta we will discover later
                let meta = ref MetaSegment.Nothing

                let normalizedPath = 
                    if path.StartsWith("/", StringComparison.Ordinal) 
                    then path.Substring(1)
                    else path

                let rawSegments = normalizedPath.Split('/')
                let firstSeg = rawSegments.[0]

                // first segment is a special situation
                let segment = process_first firstSeg model svcUri meta
                
                // gets the selected entitycontainer
                let entContainer : Ref<IEdmEntityContainer> = ref null
                let mutable contextualType : IEdmTypeReference option = None
                
                match segment with 
                | UriSegment.EntitySet d ->
                    entContainer := d.container
                    contextualType <- Some(d.ReturnType)
                | _ -> ()

                // tracks the last segment where (is a collection type = true)
                let lastCollAccessSegment : Ref<UriSegment> = ref UriSegment.Nothing

                // calls the parser
                let uriSegments = parse_segment [segment] segment svcUri meta 
                                                contextualType
                                                !entContainer lastCollAccessSegment 
                                                rawSegments 1

                // process odata query parameters, if any
                let metaQuerySegments = 
                    let list = List<MetaQuerySegment>()
                    for key in odataParams.Keys do
                        let value = odataParams.[key]
                        match key with 
                        | "$filter"     -> MetaQuerySegment.Filter (value)
                        | "$orderby"    -> MetaQuerySegment.OrderBy (value)
                        | "$top"        -> MetaQuerySegment.Top (Int32.Parse(value))
                        | "$skip"       -> MetaQuerySegment.Skip (Int32.Parse(value))
                        | "$expand"     -> MetaQuerySegment.Expand (value)
                        | "$format"     -> 
                            let format = 
                                if value === "atom" then ODataFormat.Atom
                                elif value === "json" then ODataFormat.JsonLight
                                elif value === "jsonverbose" then ODataFormat.VerboseJson
                                else failwithf "Unknown format specified %s" value
                            MetaQuerySegment.Format (format)
                        | "$inlinecount"-> MetaQuerySegment.InlineCount (value)
                        | "$select"     -> MetaQuerySegment.Select (value)
                        | _ -> failwithf "special query parameter is not supported: %s (note that these parameters are case sensitive)" key
                        |> list.Add 
                    list |> Array.ofSeq

                uriSegments, !meta, metaQuerySegments

        end

