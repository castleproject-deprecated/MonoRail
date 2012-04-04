namespace Castle.MonoRail.Extension.OData

open System
open System.Linq
open System.Linq.Expressions
open System.Xml
open System.Collections
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open Castle.MonoRail

// http://msdn.microsoft.com/en-us/library/dd233205.aspx

type SegmentOp = 
    | View
    | Create
    | Update
    | Delete
    // | Merge

module SegmentBinder = 
    begin
        let (|HttpGet|HttpPost|HttpPut|HttpDelete|HttpMerge|HttpHead|) (arg:string) = 
            match arg.ToUpperInvariant() with 
            | "POST"  -> HttpPost
            | "PUT"   -> HttpPut
            | "MERGE" -> HttpMerge
            | "HEAD"  -> HttpHead
            | "DELETE"-> HttpDelete
            | "GET"   -> HttpGet
            | _ -> failwithf "Could not understand method %s" arg
            

        type This = 
            static member Assembly = typeof<This>.Assembly

        let typed_select_methodinfo = 
            This.Assembly.GetType("Castle.MonoRail.Extension.OData.SegmentBinder").GetMethod("typed_select")


        let typed_select<'a> (source:IQueryable) (key:obj) (keyProp:ResourceProperty) = 
            
            let typedSource = source :?> IQueryable<'a>

            let parameter = Expression.Parameter(source.ElementType, "element")
            let e = Expression.Property(parameter, keyProp.Name)
            let bExp = Expression.Equal(e, Expression.Constant(key))
            let exp = Expression.Lambda(bExp, [parameter]) :?> Expression<Func<'a, bool>>

            typedSource.FirstOrDefault(exp)

        let private select_by_key (rt:ResourceType) (source:IQueryable) (key:string) =
            
            // for now support for a single key
            let keyProp = Seq.head rt.KeyProperties

            let keyVal = 
                // weak!!
                System.Convert.ChangeType(key, keyProp.ResourceType.InstanceType)

            let rtType = rt.InstanceType
            let ``method`` = typed_select_methodinfo.MakeGenericMethod([|rtType|])

            ``method``.Invoke(null, [|source; keyVal; keyProp|])
            

        let public bind (op:SegmentOp) (segments:UriSegment[]) (model:ODataModel) (singleEntityAccessInterceptor) (manyEntityAccessInterceptor) = 
            
            let intercept_single (contextop:SegmentOp) (value:obj) (rt:ResourceType) (canContinue:Ref<bool>) = 
                true
            let intercept_many (contextop:SegmentOp) (value:IEnumerable) (rt:ResourceType) (canContinue:Ref<bool>) = 
                true

            let get_property_value (container:obj) (property:ResourceProperty) = 
                // super weak
                container.GetType().GetProperty(property.Name).GetValue(container, null)

            let rec rec_bind (index:int) =
                let shouldContinue = ref true

                if index < segments.Length then
                    let previous = 
                        if index > 0 then segments.[index - 1]
                        else UriSegment.Nothing

                    let container, prevRt = 
                        match previous with 
                        | UriSegment.EntityType d -> d.SingleResult, d.ResourceType
                        | UriSegment.ComplexType d 
                        | UriSegment.PropertyAccessSingle d -> d.SingleResult, d.ResourceType
                        | _ -> null, null

                    let segment = segments.[index]
                    
                    match segment with 
                    | UriSegment.Meta m -> ()
                    | UriSegment.EntitySet d -> 
                        System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")
                        
                        let value = model.GetQueryable (d.Name)
                        if intercept_many op value d.ResourceType shouldContinue then
                            d.ManyResult <- value

                    | UriSegment.EntityType d ->  
                        System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> true | _ -> false), "must be root")

                        let wholeSet = model.GetQueryable (d.Name)
                        let singleResult = select_by_key d.ResourceType wholeSet d.Key
                        if intercept_single op singleResult d.ResourceType shouldContinue then
                            d.SingleResult <- singleResult

                    | UriSegment.ServiceDirectory -> ()
                    | UriSegment.ServiceOperation -> ()

                    | UriSegment.PropertyAccessCollection p -> 
                        System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

                        let value = (get_property_value container p.Property ) :?> IEnumerable
                        if intercept_many op value p.ResourceType shouldContinue then
                            p.ManyResult <- value

                    | UriSegment.ComplexType p 
                    | UriSegment.PropertyAccessSingle p -> 
                        System.Diagnostics.Debug.Assert ((match previous with | UriSegment.Nothing -> false | _ -> true), "cannot be root")

                        let propValue = get_property_value container p.Property
                        if p.Key <> null then
                            let collAsQueryable = (propValue :?> IEnumerable).AsQueryable()
                            let value = select_by_key p.ResourceType collAsQueryable p.Key 
                            if intercept_single op value p.ResourceType shouldContinue then
                                p.SingleResult <- value
                        else
                            if intercept_single op propValue p.ResourceType shouldContinue then
                                p.SingleResult <- propValue

                    | _ -> ()

                    if !shouldContinue then rec_bind (index+1)

                else ()

            rec_bind 0
            

    end
