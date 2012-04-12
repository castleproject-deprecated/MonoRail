
namespace Castle.MonoRail.Extension.OData

open System
open System.Collections
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open System.Linq
open System.Web
open Castle.MonoRail

type EntityDetails = {
    mutable ManyResult : IQueryable;
    mutable SingleResult : obj;
    ResSet : ResourceSet;
    ResourceType : ResourceType;
    Name : string; 
    Key : string
}

type PropertyAccessDetails = {
    mutable ManyResult : IEnumerable;
    mutable SingleResult : obj;
    ResourceType : ResourceType; 
    Property : ResourceProperty;
    Key : string
}

type MetaSegment = 
    | Metadata 
    | Batch
    | Count
    | Value
    | Links of UriSegment[]
    | Format of string
    | Skip of int
    | Top of int
    | OrderBy of string[]
    | Expand of string[]
    | Select of string[]
    | InlineCount 
    | Filter of string
 
and UriSegment = 
    | Meta of MetaSegment 
    | ServiceDirectory 
    | EntitySet of EntityDetails
    | EntityType of EntityDetails
    | ComplexType of PropertyAccessDetails
    | PropertyAccessSingle of PropertyAccessDetails
    | PropertyAccessCollection of PropertyAccessDetails
    | ServiceOperation
    | Nothing


module SegmentParser =
    begin
        // TODO, change MonoRail to also recognize 
        // "X-HTTP-Method", and gives it the value MERGE, PUT or DELETE.

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

        let (|RootOperationAccess|_|) (model:ODataModel) (arg:string)  =  
            None

        let (|OperationAccess|_|) (rt:ResourceType option) (arg:string)  =  
            None

        let (|PropertyAccess|_|) (rt:ResourceType option) (arg:string) = 
            let name, key = 
                match arg with 
                | SegmentWithKey (name, key) -> (name,key)
                | _ -> arg, null
            match rt with
            | Some r -> 
                match r.Properties |> Seq.tryFind (fun p -> p.Name = name) with
                | Some prop -> Some(prop, key)
                | _ -> None
            | _ -> None

        let (|EntityTypeAccess|_|) (model:ODataModel) (arg:string)  =  
            match arg with 
            | SegmentWithKey (name, key) -> 
                match model.GetResourceSet(name) with 
                | Some rt -> Some(rt, name, key)
                | _ -> None
            | _ -> None
            
        let (|EntitySetAccess|_|) (model:ODataModel) (arg:string)  =  
            match arg with 
            | SegmentWithoutKey name -> 
                match model.GetResourceSet(name) with 
                | Some rs -> Some(rs, name)
                | _ -> None
            | _ -> None
            
        // we also need to parse QS 
        // ex url/Suppliers?$filter=Address/City eq 'Redmond' 
        let public parse(path:string, qs:string, model:ODataModel) : UriSegment[] = 
            
            let rec parse_segment (all:UriSegment list) (previous:UriSegment) (contextRT:ResourceType option) (rawSegments:string[]) (index:int) : UriSegment[] = 
                
                // check for empty is temporary, should find better solution
                if index < rawSegments.Length && (rawSegments.[index] <> String.Empty && index <= rawSegments.Length - 1) then

                    let rawSegment = rawSegments.[index]
                    let resourceType : Ref<ResourceType> = ref null

                    let newSegment = 
                        match rawSegment with
                        | Meta m -> 
                            // todo: semantic validation
                            UriSegment.Meta(m)
                        | OperationAccess contextRT o -> UriSegment.ServiceOperation

                        | PropertyAccess contextRT (prop, key) -> 
                            resourceType := prop.ResourceType

                            match prop.Kind with 
                            | ResourcePropKind kind -> 
                                match kind with 
                                | ResourcePropertyKind.Primitive -> 
                                    // todo: assert key is null
                                    UriSegment.PropertyAccessSingle({ ResourceType=prop.ResourceType; Property=prop; Key = null; SingleResult = null; ManyResult = null })

                                | ResourcePropertyKind.ComplexType -> 
                                    // todo: assert key is null
                                    UriSegment.ComplexType({ ResourceType=prop.ResourceType; Property=prop; Key = null; SingleResult = null; ManyResult = null })

                                | ResourcePropertyKind.ResourceReference -> 
                                    UriSegment.PropertyAccessSingle({ ResourceType=prop.ResourceType; Property=prop; Key = key; SingleResult = null; ManyResult = null })

                                | ResourcePropertyKind.ResourceSetReference -> 
                                    if key = null then
                                        UriSegment.PropertyAccessCollection({ ResourceType=prop.ResourceType; Property=prop; Key = null; SingleResult = null; ManyResult = null })
                                    else
                                        UriSegment.PropertyAccessSingle({ ResourceType=prop.ResourceType; Property=prop; Key = key; SingleResult = null; ManyResult = null })

                                | _ -> raise(HttpException(500, "Unsupported property kind for segment "))
                            | _ -> raise(HttpException(500, "Unsupported property kind for segment "))
                        | _ -> raise(HttpException(400, "Segment does not match a property or operation"))

                    parse_segment (all @ [newSegment]) newSegment (if !resourceType <> null then Some(!resourceType) else None) rawSegments (index + 1)
                else 
                    all |> Array.ofList

            let normalizedPath = 
                if path.StartsWith("/", StringComparison.Ordinal) 
                then path.Substring(1)
                else path

            let rawSegments = normalizedPath.Split('/')
            let firstSeg = rawSegments.[0]
            let resourceType : Ref<ResourceType> = ref null

            let segment = 
                match firstSeg with 
                | "" -> 
                    UriSegment.ServiceDirectory
                | Meta m -> 
                    // todo: semantic validation
                    UriSegment.Meta(m)
                | RootOperationAccess model o -> 
                    UriSegment.ServiceOperation
                | EntitySetAccess model (rs, name) -> 
                    resourceType := rs.ResourceType 
                    UriSegment.EntitySet({ ResSet = rs; ResourceType = !resourceType; Name = name; Key = null; SingleResult = null; ManyResult = null })
                | EntityTypeAccess model (rs, name, key) -> 
                    resourceType := rs.ResourceType
                    UriSegment.EntityType({ ResSet = rs; ResourceType = !resourceType; Name = name; Key = key; SingleResult = null; ManyResult = null })
                | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))

            parse_segment [segment] segment (if !resourceType <> null then Some(!resourceType) else None) rawSegments 1 



        (* 
        http://services.odata.org/OData/OData.svc/Categories
            Identifies all Categories Collection.
            Is described by the Entity Set named "Categories" in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)
            Identifies a single Category Entry with key value 1.
            Is described by the Entity Type named "Categories" in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)/Name
            Identifies the Name property of the Categories Entry with key value 1.
            Is described by the Property named "Name" on the "Categories" Entity Type in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)/Products
            Identifies the collection of Products associated with Category Entry with key value 1.
            Is described by the Navigation Property named "Products" on the "Category" Entity Type in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)/Products/$count
            Identifies the number of Product Entries associated with Category 1.
            Is described by the Navigation Property named "Products" on the "Category" Entity Type in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)/Products(1)/Supplier/Address/City
            Identifies the City of the Supplier for Product 1 which is associated with Category 1.
            Is described by the Property named "City" on the "Address" Complex Type in the service metadata document.
        http://services.odata.org/OData/OData.svc/Categories(1)/Products(1)/Supplier/Address/City/$value
            Same as the URI above, but identifies the "raw value" of the City property.
        http://services.odata.org/OData/OData.svc/Categories(1)/$links/Products
            Identifies the set of Products related to Category 1.
            Is described by the Navigation Property named "Products" on the "Category" Entity Type in the associated service metadata document.
        http://services.odata.org/OData/OData.svc/Products(1)/$links/Category
            Identifies the Category related to Product 1.
            Is described by the Navigation Property named "Category" on the "Product" Entity Type in the associated service metadata document.
        *)


// need to process segments from an endpoint
// Example localhost/vpath/odata.svc/Products(1)/Categories

// http://odata.research.microsoft.com/FAQ.aspx
// http://services.odata.org/%28S%28zjtwckq5iumy0qno2wbf413y%29%29/OData/OData.svc/

// http://odata.netflix.com/v2/Catalog/
// http://odata.netflix.com/v2/Catalog/$metadata
// http://odata.netflix.com/v2/Catalog/Movies

// http://vancouverdataservice.cloudapp.net/v1/

    end