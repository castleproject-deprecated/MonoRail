namespace Castle.MonoRail.Extension.OData

open System
open System.Linq
open System.Linq.Expressions
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open System.Data.OData
open System.Data.Services.Providers


module AstLinqTranslator = 
    
    type This = static member Assembly = typeof<This>.Assembly

    let typed_select_methodinfo = 
        let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.AstLinqTranslator").GetMethod("typed_select")
        System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_select methodinfo")
        m

    let typed_queryable_filter_methodinfo = 
        let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.AstLinqTranslator").GetMethod("typed_queryable_filter")
        System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_queryable_filter methodinfo")
        m

    let typed_enumerable_filter_methodinfo = 
        let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.AstLinqTranslator").GetMethod("typed_enumerable_filter")
        System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_enumerable_filter methodinfo")
        m

    let typed_queryable_orderby_methodinfo = 
        let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.AstLinqTranslator").GetMethod("typed_queryable_orderby")
        System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_queryable_orderby methodinfo")
        m

    let typed_enumerable_orderby_methodinfo = 
        let m = This.Assembly.GetType("Castle.MonoRail.Extension.OData.AstLinqTranslator").GetMethod("typed_enumerable_orderby")
        System.Diagnostics.Debug.Assert(m <> null, "Could not get typed_enumerable_orderby methodinfo")
        m

    let select_by_key (rt:ResourceType) (source:IQueryable) (key:string) =
        // for now support for a single key
        let keyProp = Seq.head rt.KeyProperties

        let keyVal = 
            // weak!!
            System.Convert.ChangeType(key, keyProp.ResourceType.InstanceType)

        let rtType = rt.InstanceType
        let ``method`` = typed_select_methodinfo.MakeGenericMethod([|rtType|])
        let result = ``method``.Invoke(null, [|source; keyVal; keyProp|])
        if result = null then failwithf "Lookup of entity %s for key %s failed." rt.Name key
        result

    let apply_queryable_filter (rt:ResourceType) (items:IQueryable) (ast:QueryAst) = 
        let rtType = rt.InstanceType
        let ``method`` = typed_queryable_filter_methodinfo.MakeGenericMethod([|rtType|])
        ``method``.Invoke(null, [|items; ast|])

    

    let apply_queryable_orderby (rt:ResourceType) (items:IQueryable) (ast:OrderByAst seq) = 
        let rtType = rt.InstanceType
        let ``method`` = typed_queryable_orderby_methodinfo.MakeGenericMethod([|rtType|])
        ``method``.Invoke(null, [|items; ast|]) 


    let typed_select<'a> (source:IQueryable) (key:obj) (keyProp:ResourceProperty) = 
        let typedSource = source :?> IQueryable<'a>
        let parameter = Expression.Parameter(source.ElementType, "element")
        let e = Expression.Property(parameter, keyProp.Name)
            
        let bExp = Expression.Equal(e, Expression.Constant(key))
        let exp = Expression.Lambda(bExp, [parameter]) :?> Expression<Func<'a, bool>>
        typedSource.FirstOrDefault(exp)


    let internal build_linq_exp_tree (paramType:Type) (ast:QueryAst) = 
        
        let parameter = Expression.Parameter(paramType, "element")

        let rec build_tree (node) : Expression = 
            match node with
            | Element           -> upcast parameter
            | Null              -> upcast Expression.Constant(null)
            | Literal (t, v)    -> upcast Expression.Constant(v, t)

            | PropertyAccess (s, prop, rt) ->
                let target = build_tree s
                upcast Expression.Property(target, prop)

            | UnaryExp (e, op, rt) ->
                let exp = build_tree e
                match op with
                | UnaryOp.Negate    -> upcast Expression.Negate (exp)
                | UnaryOp.Not       -> upcast Expression.Not (exp)
                | UnaryOp.Cast      -> upcast Expression.Convert(exp, rt.InstanceType)
                // | UnaryOp.IsOf      -> upcast Expression.TypeIs 
                | _ -> failwithf "Unsupported unary op %O" op
                    
            | BinaryExp (l, r, op, rt) ->
                let leftExp = build_tree l
                let rightExp = build_tree r
                match op with
                | BinaryOp.Eq       -> upcast Expression.Equal(leftExp, rightExp)
                | BinaryOp.Neq      -> upcast Expression.NotEqual(leftExp, rightExp)
                | BinaryOp.Add      -> upcast Expression.Add(leftExp, rightExp)
                | BinaryOp.And      -> upcast Expression.And(leftExp, rightExp)
                | BinaryOp.Or       -> upcast Expression.Or(leftExp, rightExp)
                | BinaryOp.Mul      -> upcast Expression.Multiply(leftExp, rightExp) 
                | BinaryOp.Div      -> upcast Expression.Divide(leftExp, rightExp) 
                | BinaryOp.Mod      -> upcast Expression.Modulo(leftExp, rightExp) 
                | BinaryOp.Sub      -> upcast Expression.Subtract(leftExp, rightExp)
                | BinaryOp.LessT    -> upcast Expression.LessThan(leftExp, rightExp)
                | BinaryOp.GreatT   -> upcast Expression.GreaterThan(leftExp, rightExp)
                | BinaryOp.LessET   -> upcast Expression.LessThanOrEqual(leftExp, rightExp)
                | BinaryOp.GreatET  -> upcast Expression.GreaterThanOrEqual(leftExp, rightExp)

                | _ -> failwithf "Unsupported binary op %O" op
                
            | _ -> failwithf "Unsupported node %O" node

        (build_tree ast, parameter)

    // a predicate is a Func<T,bool>
    let build_linq_exp_predicate<'a> (paramType:Type) (ast:QueryAst) = 
        let rootExp, parameter = build_linq_exp_tree paramType ast
        Expression.Lambda(rootExp, [parameter]) :?> Expression<Func<'a, bool>>

    // a member access is a Func<T,R>
    let build_linq_exp_memberaccess<'a> (paramType:Type) (ast:QueryAst) = 
        let rootExp, parameter = build_linq_exp_tree paramType ast
        Expression.Lambda(rootExp, [parameter]) :?> Expression<Func<'a, 'b>>


    let typed_queryable_filter<'a> (source:IQueryable) (ast:QueryAst) : IQueryable = 
        let typedSource = source :?> IQueryable<'a>
        let orExp = build_linq_exp_predicate<'a> source.ElementType ast
        let exp : Expression = upcast Expression.Quote( orExp )
        let where = Expression.Call(typeof<Queryable>, "Where", [|source.ElementType|], [|source.Expression; exp|])
        typedSource.Provider.CreateQuery(where) 


    let typed_queryable_orderby<'a> (source:IQueryable) (nodes:OrderByAst seq) : IQueryable = 
        let typedSource = source :?> IQueryable<'a>
        let elemType = typeof<'a>

        let nodes = nodes |> Array.ofSeq

        let first = 
            if not <| Array.isEmpty nodes 
            then Array.get nodes 0
            else OrderByAst.Nothing
        let rest = 
            if nodes.Length > 1 then nodes.[1..nodes.Length]
            else [||]

        let createOrdered node = 
            match node with 
            | OrderByAst.Asc c  -> typedSource.OrderBy ( build_linq_exp_memberaccess elemType c )
            | OrderByAst.Desc c -> typedSource.OrderByDescending ( build_linq_exp_memberaccess elemType c )
            | _ -> failwith "Unsupported node"

        let apply_then_by node elements = 
            elements

        let ordered = createOrdered first 
        // let last = rest |> Array.fold (fun last c -> apply_then_by c ) ordered 


        // let nodes |> List.ofSeq |> List.tail
        // List.Cons
        // nodes |> Seq.fold () 

        // let exp = build_linq_exp_tree source.ElementType ast
        // typedSource.OrderBy(exp).ThenBy    :> IQueryable
        ordered :> IQueryable

