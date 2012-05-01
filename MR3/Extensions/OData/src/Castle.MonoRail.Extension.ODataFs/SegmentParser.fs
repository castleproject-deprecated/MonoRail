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

namespace Castle.MonoRail.Extension.OData

open System
open System.Collections
open System.Collections.Specialized
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open System.Linq
open System.Linq.Expressions
open System.Web
open Castle.MonoRail

type EntityAccessInfo = {
    RawPathSegment : string;
    Uri : Uri;
    mutable ManyResult : IQueryable;
    mutable SingleResult : obj;
    ResSet : ResourceSet;
    ResourceType : ResourceType;
    Name : string; 
    Key : string;
    mutable Filter : string;
}

type PropertyAccessInfo = {
    RawPathSegment : string;
    Uri : Uri;
    mutable ManyResult : IEnumerable;
    mutable SingleResult : obj;
    ResourceType : ResourceType; 
    Property : ResourceProperty;
    Key : string
}

type MetaSegment = 
    | Metadata 
    | Count
    | Value
    | Links of UriSegment[]
    | Batch

and MetaQuerySegment = 
    | Format of string
    | Skip of int
    | Top of int
    | OrderBy of string[]
    | Expand of string[]
    | Select of string[]
    | InlineCount of string
    | Filter of string
 
and UriSegment = 
    | Meta of MetaSegment 
    | ServiceDirectory 
    | EntitySet of EntityAccessInfo
    | EntityType of EntityAccessInfo
    | ComplexType of PropertyAccessInfo
    | PropertyAccessSingle of PropertyAccessInfo
    | PropertyAccessCollection of PropertyAccessInfo
    | RootServiceOperation 
    | ActionOperation of ControllerActionOperation
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

        let (|RootOperationAccess|_|) (model:ODataModel) (arg:string) =  
            None

        let (|OperationAccess|_|) (model:ODataModel) (rt:ResourceType option) (arg:string) =  
            if rt.IsNone then None
            else
                let op = model.GetNestedOperation(rt.Value, arg)
                if op = null 
                then None
                else Some(op)

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
        let parse(path:string, qs:NameValueCollection, model:ODataModel, svcUri:Uri) : UriSegment[] = 

            let odataParams, ordinaryParams = 
                let odata, ordinary =
                    qs.AllKeys 
                    |> List.ofSeq |> List.partition (fun k -> k.StartsWith("$", StringComparison.Ordinal))
                let ordinaryParams = NameValueCollection(StringComparer.OrdinalIgnoreCase)
                ordinary |> List.iter (fun i -> ordinaryParams.[i] <- qs.[i])
                let odataparms = NameValueCollection(StringComparer.OrdinalIgnoreCase)
                odata    |> List.iter (fun i -> odataparms.[i] <- qs.[i])
                odataparms, ordinary

            let filter = MetaQuerySegment.Filter "Name eq 'test'" // odataParams.["$filter"]

            let lastCollAccessSegment : Ref<UriSegment> = ref UriSegment.Nothing
            
            let rec parse_segment (all:UriSegment list) (previous:UriSegment) (contextRT:ResourceType option) (rawSegments:string[]) (index:int) : UriSegment[] = 
                
                if index < rawSegments.Length && (rawSegments.[index] <> String.Empty && index <= rawSegments.Length - 1) then

                    let rawSegment = rawSegments.[index]
                    let resourceType : Ref<ResourceType> = ref null
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
                            UriSegment.Meta(m)
                        
                        | OperationAccess model contextRT o -> 
                            UriSegment.ActionOperation(o)

                        | PropertyAccess contextRT (prop, key) -> 
                            resourceType := prop.ResourceType

                            match prop.Kind with 
                            | ResourcePropKind kind -> 
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
                            | _ -> raise(HttpException(500, "Unsupported property kind for segment "))
                        | _ -> raise(HttpException(400, "Segment does not match a property or operation"))

                    match newSegment with 
                    | UriSegment.EntitySet _ 
                    | UriSegment.PropertyAccessCollection _ -> lastCollAccessSegment := newSegment
                    | _ -> ()

                    let rt = (if !resourceType <> null then Some(!resourceType) else None)
                    parse_segment (all @ [newSegment]) newSegment rt rawSegments (index + 1)
                
                else all |> Array.ofList

            let normalizedPath = 
                if path.StartsWith("/", StringComparison.Ordinal) 
                then path.Substring(1)
                else path

            let rawSegments = normalizedPath.Split('/')
            let firstSeg = rawSegments.[0]
            let resourceType : Ref<ResourceType> = ref null

            let segment = 
                match firstSeg with 
                | "" -> UriSegment.ServiceDirectory
                | Meta m -> // todo: semantic validation
                    UriSegment.Meta(m)
                
                | RootOperationAccess model o -> 
                    UriSegment.RootServiceOperation

                // todo: support for:
                // | OperationAccess within ResourceType
                
                | EntitySetAccess model (rs, name) -> 
                    resourceType := rs.ResourceType 
                    UriSegment.EntitySet({ Uri=Uri(svcUri, name); RawPathSegment=firstSeg; ResSet = rs; 
                                           ResourceType = !resourceType; Name = name; Key = null; 
                                           SingleResult = null; ManyResult = null; Filter = null })
                
                | EntityTypeAccess model (rs, name, key) -> 
                    resourceType := rs.ResourceType
                    UriSegment.EntityType({ Uri=Uri(svcUri, firstSeg); RawPathSegment=firstSeg; ResSet = rs; 
                                            ResourceType = !resourceType; Name = name; Key = key; 
                                            SingleResult = null; ManyResult = null; Filter = null })
                
                | _ -> raise(HttpException(400, "First segment of uri could not be parsed"))

            let segments = parse_segment [segment] segment (if !resourceType <> null then Some(!resourceType) else None) rawSegments 1 

            if !lastCollAccessSegment <> UriSegment.Nothing then
                (* 
                 | Format of string
                 | Skip of int
                 | Top of int
                 | OrderBy of string[]
                 | Expand of string[]
                 | Select of string[]
                 | InlineCount of string
                 | Filter of string    *)
                
                match !lastCollAccessSegment with
                | UriSegment.EntitySet d -> 
                    d.Filter <- null 
                | UriSegment.PropertyAccessCollection d -> () // lastCollAccessSegment := newSegment
                | _ -> ()
                ()

            segments

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

    end

