namespace Castle.MonoRail.Extension.OData

open System
open System.Linq
open System.Linq.Expressions
open System.Xml
open System.Collections.Generic
open System.Data.OData
open System.Data.Services.Providers
open Castle.MonoRail
open Microsoft.FSharp.Reflection

// http://msdn.microsoft.com/en-us/library/dd233205.aspx

module SegmentBinder = 
    begin

        type This = 
            static member Assembly = typeof<This>.Assembly

        let typed_select_methodinfo = 
            This.Assembly.GetType("Castle.MonoRail.Extension.OData.SegmentBinder").GetMethod("typed_select")


        let typed_select <'a> (source:IQueryable) (key:obj) (keyProp:ResourceProperty) = 
            
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
            

        let public bind (segments:UriSegment[]) (model:ODataModel) = 
            
            let rec rec_bind (index:int) =
                if index < segments.Length then
                    let previous = 
                        if index > 0 then segments.[index - 1]
                        else UriSegment.Nothing
                    let segment = segments.[index]
                    
                    match segment with 
                    | UriSegment.Meta m -> ()
                    | UriSegment.EntitySet d -> 
                        // this is implied to be the root
                        System.Diagnostics.Debug.Assert (match previous with | UriSegment.Nothing -> true | _ -> false)
                        d.ManyResult <- model.GetQueryable (d.Name)

                    | UriSegment.EntityType d ->  
                        // this is implied to be the root
                        System.Diagnostics.Debug.Assert (match previous with | UriSegment.Nothing -> true | _ -> false)
                        let wholeSet = model.GetQueryable (d.Name)
                        d.SingleResult <- select_by_key d.ResourceType wholeSet d.Key
                        ()

                    | UriSegment.ServiceDirectory -> 
                        ()

                    | UriSegment.ServiceOperation -> 
                        ()

                    | UriSegment.PropertyAccessCollection p -> 
                        ()

                    | UriSegment.ComplexType p 
                    | UriSegment.PropertyAccessSingle p -> 
                        // this cannot be the root
                        System.Diagnostics.Debug.Assert (match previous with | UriSegment.Nothing -> false | _ -> true)

                        let container, prevRt = 
                            match previous with 
                            | UriSegment.ComplexType d ->
                                d.SingleResult, d.ResourceType
                            | UriSegment.EntityType d -> 
                                d.SingleResult, d.ResourceType
                            | UriSegment.PropertyAccessSingle d -> 
                                d.SingleResult, d.ResourceType
                            | _ ->
                                null, null // todo: exception

                        let get_property_value (container:obj) (property:ResourceProperty) = 
                            // super weak
                            container.GetType().GetProperty(property.Name).GetValue(container, null)

                        p.SingleResult <- get_property_value container p.Property

                        // p. get_property_value container prevRt p.Property

                    | _ -> 
                        ()

                    rec_bind (index+1)

                else
                    ()


            rec_bind 0
            

    end
