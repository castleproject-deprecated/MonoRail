
namespace Castle.MonoRail.Extension.OData

open System
open System.Data.OData
open System.Data.Services.Providers
open System.Web
open Castle.MonoRail


module File1 =
    begin
        // TODO, change MonoRail to also recognize 
        // "X-HTTP-Method", and gives it the value MERGE, PUT or DELETE.

        type EntityDetails = {
            ResourceSet : ResourceSet; 
            Name : string; 
            Key : string
        }

        type MetaSegment = 
            | Metadata 
            | Batch
            | Count
            | Value
            | Links
            
        type UriSegment = 
            | Meta of MetaSegment // $metadata, $filter, $value, $batch, $links
            | EntitySet of EntityDetails
            | EntityType of EntityDetails
            | ComplexType
            | PropertyAccessSingle
            | PropertyAccessCollection
            | ServiceOperation
    
            
        let (|Meta|_|) (arg:string) = 
            if arg.StartsWith("$", StringComparison.Ordinal) 
            then match arg.Substring(1).ToLowerInvariant() with 
                 | "metadata" -> Some(MetaSegment.Metadata)
                 | "batch"    -> Some(MetaSegment.Batch)
                 | "count"    -> Some(MetaSegment.Count)
                 | "value"    -> Some(MetaSegment.Value)
                 | "links"    -> Some(MetaSegment.Links)
                 | _ -> None
            else None
                 
        let (|SegmentWithKey|_|) (arg:string) = 
            let ``match`` = Constants.SegmentKeyRegex.Match(arg)
            if ``match``.Success then
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

        let (|OperationAccess|_|) (rt:ResourceType opt) (arg:string)  =  
            None

        let (|PropertyAccess|_|) (rt:ResourceType opt) (arg:string) = 
            match rt with
            | Some rt -> rt.Properties |> Seq.tryFind (fun p -> p.Name = arg) 
            | _ -> None

        let (|EntityTypeAccess|_|) (model:ODataModel) (arg:string)  =  
            match arg with 
            | SegmentWithKey (name, key) -> 
                match model.GetResourceType(name) with 
                | Some rt -> Some(rt, name, key)
                | _ -> None
            | _ -> None
            
        let (|EntitySetAccess|_|) (model:ODataModel) (arg:string)  =  
            match arg with 
            | SegmentWithoutKey name -> 
                match model.GetResourceType(name) with 
                | Some rt -> Some(rt, name)
                | _ -> None
            | _ -> None
            
        let public parse(path:string, qs:string, model:ODataModel) : UriSegment[] = 
            
            let rec parse_segment (all:UriSegment list) (previous:UriSegment) (contextRT:ResourceType opt) (rawSegments:string[]) (index:int) : UriSegment[] = 
                if index < rawSegments.Length then
                    let rawSegment = rawSegments.[index]

                    let newSegment = 
                        match rawSegment with
                        | Meta m -> UriSegment.Meta(m)
                        | OperationAccess contextRT o -> UriSegment.ServiceOperation
                        | PropertyAccess contextRT prop -> 
                            match prop.Kind with
                            | ResourcePropertyKind.ComplexType -> 
                            | ResourcePropertyKind.Primitive -> 
                            | ResourcePropertyKind.ResourceReference -> 
                            | ResourcePropertyKind.ResourceSetReference -> 
                            | _ -> raise(HttpException(500, "Unsupported property kind for segment "))
                        | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))

                    parse_segment (all @ [newSegment]) newSegment rawSegments (index + 1)
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
                | Meta m -> UriSegment.Meta(m)
                | RootOperationAccess model o -> UriSegment.ServiceOperation
                | EntitySetAccess model (rt, name) -> 
                    resourceType := rt 
                    UriSegment.EntitySet({ ResourceSet = rt; Name = name; Key = null })
                | EntityTypeAccess model (rt, name, key) -> 
                    resourceType := rt 
                    UriSegment.EntityType({ ResourceSet = rt; Name = name; Key = key })
                | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))

            parse_segment [segment] segment (if !resourceType <> null then Some(!resourceType) else None) rawSegments 1 

            Array.empty


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

        (*
        member x.Parse(path:string, qs:string, model:ODataModel) = 
            // we also need to parse QS 
            // ex url/Suppliers?$filter=Address/City eq 'Redmond' 

            ""
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