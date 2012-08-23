namespace Castle.MonoRail.Extension.OData

    open System
    open System.Collections
    open System.Collections.Specialized
    open System.Collections.Generic
    open System.Linq
    open System.Linq.Expressions
    open System.Web
    open Castle.MonoRail
    open Microsoft.Data.Edm

    type EntityAccessInfo = {
        RawPathSegment : string;
        Uri : Uri;
        mutable ManyResult : IQueryable;
        mutable SingleResult : obj;
        EdmSet : IEdmEntitySet
        EdmEntityType : IEdmEntityType
        Name : string; 
        Key : string;
    }

    type PropertyAccessInfo = {
        RawPathSegment : string;
        Uri : Uri;
        mutable ManyResult : IEnumerable;
        mutable SingleResult : obj;
        EdmType : IEdmStructuredType
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
        | Format of string
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
        | EntityType of EntityAccessInfo
        | ComplexType of PropertyAccessInfo
        | PropertyAccessSingle of PropertyAccessInfo
        | PropertyAccessCollection of PropertyAccessInfo
        // | RootServiceOperation
        | FunctionOperationOnSet of EntityAccessInfo
        | FunctionOperationOnProp of PropertyAccessInfo
        // | ActionOperation
        
        // | ActionOperation of ControllerActionOperation
        // | Links


    module SegmentParser =
        begin
            // TODO, change MonoRail to also recognize 
            // "X-HTTP-Method", and gives it the value MERGE, PUT or DELETE.

            let private get_entityset (model:IEdmModel) (name) = 
                let containers = model.EntityContainers() 
                let entitySet = containers |> Seq.collect (fun container -> container.EntitySets())
                entitySet |> Seq.tryFind (fun set -> set.Name === name)


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

            (*
            let (|ResourcePropKind|_|) (arg:ResourcePropertyKind) = 
                if int(arg &&& ResourcePropertyKind.Primitive) <> 0 then 
                    Some(ResourcePropertyKind.Primitive)
                elif int(arg &&& ResourcePropertyKind.ComplexType) <> 0 then 
                    Some(ResourcePropertyKind.ComplexType)
                elif int(arg &&& ResourcePropertyKind.ResourceReference) <> 0 then 
                    Some(ResourcePropertyKind.ResourceReference)
                elif int(arg &&& ResourcePropertyKind.ResourceSetReference) <> 0 then 
                    Some(ResourcePropertyKind.ResourceSetReference)
                else None
            *)
                 
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

            (*
            let (|RootOperationAccess|_|) (model:ODataModel) (arg:string) =  
                None
            let (|OperationAccess|_|) (model:ODataModel) (rt:ResourceType option) (name:string) =  
                if rt.IsNone then None
                else
                    let op = model.GetNestedOperation(rt.Value, name)
                    if op = null
                    then None
                    else Some(op)
            *)

            let (|PropertyAccess|_|) (rt:IEdmType option) (arg:string) = 
                let name, key = 
                    match arg with 
                    | SegmentWithKey (name, key) -> (name,key)
                    | _ -> arg, null
                match rt with
                | Some r -> 
                    match r.TypeKind with
                    | EdmTypeKind.Entity 
                    | EdmTypeKind.Complex -> 
                        let structured = r :?> IEdmStructuredType
                        match structured.Properties() |> Seq.tryFind (fun p -> p.Name = name) with
                        | Some prop -> Some(prop, key)
                        | _ -> None
                    | _ -> None
                | _ -> None


            let (|EntityTypeAccess|_|) (model:IEdmModel) (arg:string)  =  
                match arg with 
                | SegmentWithKey (name, key) -> 
                    match get_entityset model name with
                    | Some rt -> Some(rt, name, key)
                    | _ -> None
                | _ -> None
            
            let (|EntitySetAccess|_|) (model:IEdmModel) (arg:string)  =  
                match arg with 
                | SegmentWithoutKey name -> 
                    match get_entityset model name with
                    | Some rs -> Some(rs, name)
                    | _ -> None
                | _ -> None


            let internal process_first (firstSeg:string) model (svcUri:Uri) (resourceType:Ref<IEdmType>) (meta:Ref<MetaSegment>) = 
                match firstSeg with 
                | "" -> 
                    UriSegment.ServiceDirectory
                | Meta m -> // todo: semantic validation
                    Diagnostics.Debug.Assert (!meta = MetaSegment.Nothing)
                    meta := m
                    UriSegment.Nothing
                
                // | RootOperationAccess model o -> 
                //     UriSegment.RootServiceOperation

                // todo: support for:
                // | OperationAccess within ResourceType
                
                | EntitySetAccess model (rs, name) -> 
                    resourceType := upcast rs.ElementType
                    UriSegment.EntitySet({ Uri=Uri(svcUri, name); RawPathSegment=firstSeg; EdmSet = rs; 
                                           EdmEntityType = rs.ElementType; 
                                           Name = name; Key = null; 
                                           SingleResult = null; ManyResult = null; })
                
                | EntityTypeAccess model (rs, name, key) -> 
                    resourceType := upcast rs.ElementType
                    UriSegment.EntityType({ Uri=Uri(svcUri, firstSeg); RawPathSegment=firstSeg; EdmSet = rs; 
                                            EdmEntityType = rs.ElementType; 
                                            Name = name; Key = key; 
                                            SingleResult = null; ManyResult = null; })
                
                | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))


            (*

            let internal build_segment_for_property kind (baseUri:Uri) (rawSegment:string) (prop:ResourceProperty) key = 
                match kind with 
                | ResourcePropertyKind.Primitive -> 
                    // todo: assert key is null
                    let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; ResourceType=prop.ResourceType; 
                                    Property=prop; Key = null; SingleResult = null; ManyResult = null }
                    UriSegment.PropertyAccessSingle(info)

                | ResourcePropertyKind.ComplexType -> 
                    // todo: assert key is null
                    let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; ResourceType=prop.ResourceType; 
                                    Property=prop; Key = null; SingleResult = null; ManyResult = null }
                    UriSegment.ComplexType(info)

                | ResourcePropertyKind.ResourceReference -> 
                    let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; ResourceType=prop.ResourceType; 
                                    Property=prop; Key = key; SingleResult = null; ManyResult = null }
                    UriSegment.PropertyAccessSingle(info)

                | ResourcePropertyKind.ResourceSetReference -> 
                    if key = null then
                        let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; ResourceType=prop.ResourceType; 
                                        Property=prop; Key = null; SingleResult = null; ManyResult = null }
                        UriSegment.PropertyAccessCollection(info)
                    else
                        let info = { Uri=Uri(baseUri, rawSegment); RawPathSegment=rawSegment; ResourceType=prop.ResourceType; 
                                        Property=prop; Key = key; SingleResult = null; ManyResult = null }
                        UriSegment.PropertyAccessSingle(info)

                | _ -> raise(HttpException(500, "Unsupported property kind for segment "))

            *)

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

            
            // we also need to parse QS 
            // ex url/Suppliers?$filter=Address/City eq 'Redmond' 
            let parse(path:string, qs:NameValueCollection, model:ODataModel, svcUri:Uri) : UriSegment[] * MetaSegment * MetaQuerySegment[] = 

                let odataParams, ordinaryParams = partition_qs_parameters qs

                // tracks the meta we will discover later
                let meta = ref MetaSegment.Nothing

                // tracks the last segment where (is a collection type = true)
                let lastCollAccessSegment : Ref<UriSegment> = ref UriSegment.Nothing

                // this rec function parses all segments but the first
                let rec parse_segment (all:UriSegment list) (previous:UriSegment) 
                                      (contextRT:IEdmType option) (rawSegments:string[]) (index:int) : UriSegment[] = 
                
                    if index < rawSegments.Length && (rawSegments.[index] <> String.Empty && index <= rawSegments.Length - 1) then

                        let rawSegment = rawSegments.[index]
                        let resourceType : Ref<IEdmType> = ref null
                        let baseUri = 
                            match previous with 
                            | UriSegment.EntitySet d 
                            | UriSegment.EntityType d -> Uri(d.Uri.AbsoluteUri + "/")
                            | UriSegment.PropertyAccessCollection d 
                            | UriSegment.PropertyAccessSingle d -> Uri(d.Uri.AbsoluteUri + "/")
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
                                resourceType := prop.Type.Definition
                                match prop.PropertyKind with 
                                | EdmPropertyKind.Structural 
                                | EdmPropertyKind.Navigation
                                    build_segment_for_property kind baseUri rawSegment prop key
                                | _ -> raise(HttpException(500, "Unsupported property kind for segment "))

                            | _ -> raise(HttpException(400, "Segment does not match a property or operation"))
                    
                        match newSegment with 
                        | UriSegment.EntitySet _ 
                        | UriSegment.PropertyAccessCollection _ -> 
                            lastCollAccessSegment := newSegment
                        | _ -> ()                    

                        let rt = if !resourceType <> null then Some(!resourceType) else None

                        let newList = 
                            all @ (if newSegment = UriSegment.Nothing then [] else [newSegment])

                        parse_segment newList newSegment rt rawSegments (index + 1)
                
                    else all |> Array.ofList

                let normalizedPath = 
                    if path.StartsWith("/", StringComparison.Ordinal) 
                    then path.Substring(1)
                    else path

                let rawSegments = normalizedPath.Split('/')
                let firstSeg = rawSegments.[0]
                // let resourceType : Ref<ResourceType> = ref null
                let edmType : Ref<IEdmType> = ref null

                // first segment is a special situation
                let segment = process_first firstSeg model.EdmModel svcUri edmType meta

                // calls the parser
                let uriSegments = parse_segment [segment] segment (if !edmType <> null then Some(!edmType) else None) rawSegments 1 

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
                        | "$format"     -> MetaQuerySegment.Format (value)
                        | "$inlinecount"-> MetaQuerySegment.InlineCount (value)
                        | "$select"     -> MetaQuerySegment.Select (value)
                        | _ -> failwithf "special query parameter is not supported: %s (note that these parameters are case sensitive)" key
                        |> list.Add 
                    list |> Array.ofSeq

                uriSegments, !meta, metaQuerySegments

        end

