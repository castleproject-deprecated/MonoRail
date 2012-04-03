
// From FSharp Power pack, with some changes

namespace Castle.MonoRail.Extension.OData

    open System
    open System.Collections.Generic
    open System.Runtime.CompilerServices
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns
    open Microsoft.FSharp.Quotations.DerivedPatterns
    open Microsoft.FSharp.Reflection
    open System.Linq.Expressions
    open System.Reflection
    open System.Reflection.Emit


    [<Extension; AbstractClass; AutoOpen>]
    module public LinqExtensions = 
        
        type ConvEnv = 
            {   eraseEquality : bool;
                varEnv : Map<Var,Expression>
            }
        let asExpr x = (x :> Expression)

        let bindingFlags = BindingFlags.Public ||| BindingFlags.NonPublic
        let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly

        let GetActionType (args:Type[])  = 
            if args.Length <= 4 
            then Expression.GetActionType args
            else failwith "no support for delegates with more than 4 args"

        let IsVoidType (ty:System.Type) = (ty = typeof<System.Void>)

        let WrapVoid b objOpt args (env: ConvEnv) (e:Expression) = 
            if b then 
                let frees (e:Expr) = e.GetFreeVars()
                let addFrees e acc = List.foldBack Set.add (frees e |> Seq.toList) acc
                let fvs = Option.foldBack addFrees objOpt (List.foldBack addFrees args Set.empty) |> Set.toArray
                let fvsP = fvs |> Array.map (fun v -> (Map.find v env.varEnv :?> ParameterExpression))
                let fvtys = fvs |> Array.map (fun v -> v.Type) 
    
                let dty = GetActionType fvtys 
                let e = Expression.Lambda(dty,e,fvsP)
                let d = e.Compile()
    
                let argtys = Array.append fvtys [| dty |]
                let delP = Expression.Parameter(dty, "del")
    
                let m = new System.Reflection.Emit.DynamicMethod("wrapper",typeof<unit>,argtys)
                let ilg = m.GetILGenerator()
                
                ilg.Emit(OpCodes.Ldarg ,fvs.Length)
                fvs |> Array.iteri (fun i _ -> ilg.Emit(OpCodes.Ldarg ,int16 i))
                ilg.EmitCall(OpCodes.Callvirt,dty.GetMethod("Invoke",instanceBindingFlags),null)
                ilg.Emit(OpCodes.Ldnull)
                ilg.Emit(OpCodes.Ret)
                let args = Array.append (fvsP |> Array.map asExpr) [| (Expression.Constant(d) |> asExpr) |]
                Expression.Call((null:Expression),(m:>MethodInfo),args) |> asExpr
            else
                e

        let (|GenericEqualityQ|_|) = (|SpecificCall|_|) <@ LanguagePrimitives.GenericEquality @>
        let (|EqualsQ|_|)    = (|SpecificCall|_|) <@ ( = ) @>
        let (|PlusQ|_|)      = (|SpecificCall|_|) <@ ( + ) @>
        
        let (|GreaterQ|_|)   = (|SpecificCall|_|) <@ ( > ) @>
        let (|GreaterEqQ|_|) = (|SpecificCall|_|) <@ ( >=) @>
        let (|LessQ|_|)      = (|SpecificCall|_|) <@ ( <)  @>
        let (|LessEqQ|_|) = (|SpecificCall|_|) <@ ( <=) @>
        let (|NotEqQ|_|) = (|SpecificCall|_|) <@ ( <>) @>
        let (|NotQ|_|) =  (|SpecificCall|_|) <@ not   @>
        let (|NegQ|_|) = (|SpecificCall|_|) <@ ( ~-) : int -> int @>
        let (|DivideQ|_|) = (|SpecificCall|_|) <@ ( / ) @> 
        let (|MinusQ|_|) = (|SpecificCall|_|) <@ ( - ) @>
        let (|MultiplyQ|_|) = (|SpecificCall|_|) <@ ( * ) @>
        let (|ModuloQ|_|) = (|SpecificCall|_|) <@ ( % ) @>
        let (|ShiftLeftQ|_|) = (|SpecificCall|_|) <@ ( <<< ) @>
        let (|ShiftRightQ|_|) = (|SpecificCall|_|) <@ ( >>> ) @>
        let (|BitwiseAndQ|_|) = (|SpecificCall|_|) <@ ( &&& ) @>
        let (|BitwiseOrQ|_|) = (|SpecificCall|_|) <@ ( ||| ) @>
        let (|BitwiseXorQ|_|) = (|SpecificCall|_|) <@ ( ^^^ ) @>
        let (|BitwiseNotQ|_|) = (|SpecificCall|_|) <@ ( ~~~ ) @>
        let (|CheckedNeg|_|) = (|SpecificCall|_|) <@ Checked.( ~-) : int -> int @>
        let (|CheckedPlusQ|_|)      = (|SpecificCall|_|) <@ Checked.( + ) @>
        let (|CheckedMinusQ|_|) = (|SpecificCall|_|) <@ Checked.( - ) @>
        let (|CheckedMultiplyQ|_|) = (|SpecificCall|_|) <@ Checked.( * ) @>
        let (|ConvCharQ|_|) = (|SpecificCall|_|) <@ char @>
        let (|ConvDecimalQ|_|) = (|SpecificCall|_|) <@ decimal @>
        let (|ConvFloatQ|_|) = (|SpecificCall|_|) <@ float @>
        let (|ConvFloat32Q|_|) = (|SpecificCall|_|) <@ float32 @>
        let (|ConvSByteQ|_|) = (|SpecificCall|_|) <@ sbyte @>
        let (|ConvInt16Q|_|) = (|SpecificCall|_|) <@ int16 @>
        let (|ConvInt32Q|_|) = (|SpecificCall|_|) <@ int32 @>
        let (|ConvIntQ|_|) = (|SpecificCall|_|) <@ int @>
        let (|ConvInt64Q|_|) = (|SpecificCall|_|) <@ int64 @>
        let (|ConvByteQ|_|) = (|SpecificCall|_|) <@ byte @>
        let (|ConvUInt16Q|_|) = (|SpecificCall|_|) <@ uint16 @>
        let (|ConvUInt32Q|_|) = (|SpecificCall|_|) <@ uint32 @>
        let (|ConvUInt64Q|_|) = (|SpecificCall|_|) <@ uint64 @>
    
        let (|CheckedConvCharQ|_|) = (|SpecificCall|_|) <@ Checked.char @>
        let (|CheckedConvSByteQ|_|) = (|SpecificCall|_|) <@ Checked.sbyte @>
        let (|CheckedConvInt16Q|_|) = (|SpecificCall|_|) <@ Checked.int16 @>
        let (|CheckedConvInt32Q|_|) = (|SpecificCall|_|) <@ Checked.int32 @>
        let (|CheckedConvInt64Q|_|) = (|SpecificCall|_|) <@ Checked.int64 @>
        let (|CheckedConvByteQ|_|) = (|SpecificCall|_|) <@ Checked.byte @>
        let (|CheckedConvUInt16Q|_|) = (|SpecificCall|_|) <@ Checked.uint16 @>
        let (|CheckedConvUInt32Q|_|) = (|SpecificCall|_|) <@ Checked.uint32 @>
        let (|CheckedConvUInt64Q|_|) = (|SpecificCall|_|) <@ Checked.uint64 @>
        // let (|LinqExpressionHelperQ|_|) = (|SpecificCall|_|) <@ LinqExpressionHelper @>
        let (|ArrayLookupQ|_|) = (|SpecificCall|_|) <@ LanguagePrimitives.IntrinsicFunctions.GetArray : int[] -> int -> int @>
        // let (|ArrayAssignQ|_|) = (|SpecificCall|_|) <@ LanguagePrimitives.IntrinsicFunctions.SetArray : int[] -> int -> int -> unit @>
        let (|ArrayTypeQ|_|) (ty:System.Type) = if ty.IsArray && ty.GetArrayRank() = 1 then Some(ty.GetElementType()) else None
        

        /// Convert F# quotations to LINQ expression trees.
        /// A more polished LINQ-Quotation translator will be published
        /// concert with later versions of LINQ.
        let rec ConvExpr (env:ConvEnv) (inp:Expr) = 
           //printf "ConvExpr : %A\n" e;
            match inp with 
    
            // Generic cases 
            | Patterns.Var(v) -> 
                    try
                        Map.find v env.varEnv
                    with
                    |   :? KeyNotFoundException when v.Name = "this" ->
                            let message = 
                                "Encountered unxpected variable named 'this'. This might happen because " +
                                "quotations used in queries can’t contain references to let-bound values in classes unless the quotation literal occurs in an instance member. " +
                                "If this is the case, workaround by replacing references to implicit fields with references to " +
                                "local variables, e.g. rewrite\r\n" +
                                "   type Foo() =\r\n" +
                                "       let x = 1\r\n" +
                                "       let bar() = <@ x @>\r\n" +
                                "as: \r\n" +
                                "   type Foo() =\r\n" +
                                "       let x = 1\r\n" +
                                "       let bar() = let x = x in <@ x @>\r\n";
    
                            NotSupportedException(message) |> raise    
            | DerivedPatterns.AndAlso(x1,x2)             -> Expression.AndAlso(ConvExpr env x1, ConvExpr env x2) |> asExpr
            | DerivedPatterns.OrElse(x1,x2)              -> Expression.OrElse(ConvExpr env x1, ConvExpr env x2)  |> asExpr
            | Patterns.Value(x,ty)                -> Expression.Constant(x,ty)              |> asExpr
    
            // REVIEW: exact F# semantics for TypeAs and TypeIs
            | Patterns.Coerce(x,toTy)             -> Expression.TypeAs(ConvExpr env x,toTy)     |> asExpr
            | Patterns.TypeTest(x,toTy)           -> Expression.TypeIs(ConvExpr env x,toTy)     |> asExpr
            
            // Expr.*Get
            | Patterns.FieldGet(objOpt,fieldInfo) -> 
                Expression.Field(ConvObjArg env objOpt None, fieldInfo) |> asExpr
    
            | Patterns.TupleGet(arg,n) -> 
                 let argP = ConvExpr env arg 
                 let rec build ty argP n = 
                     match Reflection.FSharpValue.PreComputeTuplePropertyInfo(ty,n) with 
                     | propInfo,None -> 
                         Expression.Property(argP, propInfo)  |> asExpr
                     | propInfo,Some(nestedTy,n2) -> 
                         build nestedTy (Expression.Property(argP,propInfo) |> asExpr) n2
                 build arg.Type argP n
                  
            | Patterns.PropertyGet(objOpt,propInfo,args) -> 
                let coerceTo = 
                    if objOpt.IsSome && FSharpType.IsUnion propInfo.DeclaringType && FSharpType.IsUnion propInfo.DeclaringType.BaseType  then  
                        Some propInfo.DeclaringType
                    else 
                        None
                match args with 
                | [] -> 
                    Expression.Property(ConvObjArg env objOpt coerceTo, propInfo) |> asExpr
                | _ -> 
                    let argsP = ConvExprs env args
                    Expression.Call(ConvObjArg env objOpt coerceTo, propInfo.GetGetMethod(true),argsP) |> asExpr
    
            // Expr.*Set
            (*
            | Patterns.PropertySet(objOpt,propInfo,args,v) -> 
                let args = (args @ [v])
                let argsP = ConvExprs env args 
                let minfo = propInfo.GetSetMethod(true)
                Expression.Call(ConvObjArg env objOpt None, minfo,argsP) |> asExpr |> WrapVoid (IsVoidType minfo.ReturnType) objOpt args env 
            *)
    
            // Expr.(Call,Application)
            | Patterns.Call(objOpt,minfo,args) -> 
                let transComparison x1 x2 exprConstructor exprErasedConstructor (intrinsic : MethodInfo) =
                    let e1 = ConvExpr env x1
                    let e2 = ConvExpr env x2
    
                    if env.eraseEquality then
                        exprErasedConstructor(e1,e2) |> asExpr
                    else 
                        exprConstructor(e1, e2, false, intrinsic.MakeGenericMethod([|x1.Type|])) |> asExpr
    
                match inp with 
                // Special cases for this translation
                |  PlusQ (_, [ty1;ty2;ty3],[x1;x2]) when (ty1 = typeof<string>) && (ty2 = typeof<string>) ->
                     ConvExpr env (<@@  System.String.Concat( [| %%x1 ; %%x2 |] : string array ) @@>)
    
                //| SpecificCall <@ LanguagePrimitives.GenericEquality @>([ty1],[x1;x2]) 
                //| SpecificCall <@ ( = ) @>([ty1],[x1;x2]) when (ty1 = typeof<string>) ->
                //     ConvExpr env (<@@  System.String.op_Equality(%%x1,%%x2) @@>)
    
                | GenericEqualityQ (_, _,[x1;x2]) 
                | EqualsQ (_, _,[x1;x2]) ->
                    transComparison x1 x2 Expression.Equal Expression.Equal null // (typeof<string>.GetMethod("op_Equals", BindingFlags.Static ||| BindingFlags.Public))
                
                | GreaterQ (_, _,[x1;x2]) ->    transComparison x1 x2 Expression.GreaterThan Expression.GreaterThan null // genericGreaterThanIntrinsic
    
                | GreaterEqQ (_, _,[x1;x2]) ->  transComparison x1 x2 Expression.GreaterThanOrEqual Expression.GreaterThanOrEqual null  // genericGreaterOrEqualIntrinsic
    
                | LessQ (_, _,[x1;x2]) ->       transComparison x1 x2 Expression.LessThan Expression.LessThan  null //genericLessThanIntrinsic
    
                | LessEqQ (_, _,[x1;x2]) ->     transComparison x1 x2 Expression.LessThanOrEqual Expression.LessThanOrEqual null //genericLessOrEqualIntrinsic
    
                | NotEqQ (_, _,[x1;x2]) ->      transComparison x1 x2 Expression.NotEqual Expression.NotEqual null // genericNotEqualIntrinsic
    
                | NotQ (_, _,[x1])    -> Expression.Not(ConvExpr env x1)                                   |> asExpr
    
    
                | NegQ (_, _,[x1])    -> Expression.Negate(ConvExpr env x1)                                |> asExpr
                | PlusQ (_, _,[x1;x2]) -> Expression.Add(ConvExpr env x1, ConvExpr env x2)      |> asExpr
                | DivideQ (_, _,[x1;x2]) -> Expression.Divide (ConvExpr env x1, ConvExpr env x2)  |> asExpr
                | MinusQ (_, _,[x1;x2]) -> Expression.Subtract(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | MultiplyQ (_, _,[x1;x2]) -> Expression.Multiply(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | ModuloQ (_, _,[x1;x2]) -> Expression.Modulo (ConvExpr env x1, ConvExpr env x2) |> asExpr
                     /// REVIEW: basic arithmetic with method witnesses
                     /// REVIEW: negate,add, divide, multiply, subtract with method witness
    
                | ShiftLeftQ (_, _,[x1;x2]) -> Expression.LeftShift(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | ShiftRightQ (_, _,[x1;x2]) -> Expression.RightShift(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | BitwiseAndQ (_, _,[x1;x2]) -> Expression.And(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | BitwiseOrQ (_, _,[x1;x2]) -> Expression.Or(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | BitwiseXorQ (_, _,[x1;x2]) -> Expression.ExclusiveOr(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | BitwiseNotQ (_, _,[x1]) -> Expression.Not(ConvExpr env x1) |> asExpr
                     /// REVIEW: bitwise operations with method witnesses
    
                | CheckedNeg (_, _,[x1]) -> Expression.NegateChecked(ConvExpr env x1)                                |> asExpr
                | CheckedPlusQ (_, _,[x1;x2]) -> Expression.AddChecked(ConvExpr env x1, ConvExpr env x2)      |> asExpr
                | CheckedMinusQ (_, _,[x1;x2]) -> Expression.SubtractChecked(ConvExpr env x1, ConvExpr env x2) |> asExpr
                | CheckedMultiplyQ (_, _,[x1;x2]) -> Expression.MultiplyChecked(ConvExpr env x1, ConvExpr env x2) |> asExpr
    
                | ConvCharQ (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<char>) |> asExpr
                | ConvDecimalQ (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<decimal>) |> asExpr
                | ConvFloatQ (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<float>) |> asExpr
                | ConvFloat32Q (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<float32>) |> asExpr
                | ConvSByteQ (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<sbyte>) |> asExpr
                | ConvInt16Q (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<int16>) |> asExpr
                | ConvInt32Q (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<int32>) |> asExpr
                | ConvIntQ (_, [ty],[x1])    -> Expression.Convert(ConvExpr env x1, typeof<int32>) |> asExpr
                | ConvInt64Q (_, [ty],[x1])  -> Expression.Convert(ConvExpr env x1, typeof<int64>) |> asExpr
                | ConvByteQ (_, [ty],[x1])   -> Expression.Convert(ConvExpr env x1, typeof<byte>) |> asExpr
                | ConvUInt16Q (_, [ty],[x1]) -> Expression.Convert(ConvExpr env x1, typeof<uint16>) |> asExpr
                | ConvUInt32Q (_, [ty],[x1]) -> Expression.Convert(ConvExpr env x1, typeof<uint32>) |> asExpr
                | ConvUInt64Q (_, [ty],[x1]) -> Expression.Convert(ConvExpr env x1, typeof<uint64>) |> asExpr
                 /// REVIEW: convert with method witness
    
                | CheckedConvCharQ (_, [ty],[x1])  -> Expression.ConvertChecked(ConvExpr env x1, typeof<char>) |> asExpr
                | CheckedConvSByteQ (_, [ty],[x1])  -> Expression.ConvertChecked(ConvExpr env x1, typeof<sbyte>) |> asExpr
                | CheckedConvInt16Q (_, [ty],[x1])  -> Expression.ConvertChecked(ConvExpr env x1, typeof<int16>) |> asExpr
                | CheckedConvInt32Q (_, [ty],[x1])  -> Expression.ConvertChecked(ConvExpr env x1, typeof<int32>) |> asExpr
                | CheckedConvInt64Q (_, [ty],[x1])  -> Expression.ConvertChecked(ConvExpr env x1, typeof<int64>) |> asExpr
                | CheckedConvByteQ (_, [ty],[x1])   -> Expression.ConvertChecked(ConvExpr env x1, typeof<byte>) |> asExpr
                | CheckedConvUInt16Q (_, [ty],[x1]) -> Expression.ConvertChecked(ConvExpr env x1, typeof<uint16>) |> asExpr
                | CheckedConvUInt32Q (_, [ty],[x1]) -> Expression.ConvertChecked(ConvExpr env x1, typeof<uint32>) |> asExpr
                | CheckedConvUInt64Q (_, [ty],[x1]) -> Expression.ConvertChecked(ConvExpr env x1, typeof<uint64>) |> asExpr
                | ArrayLookupQ (_, [ArrayTypeQ(elemTy);_;_],[x1;x2]) -> 
                    Expression.ArrayIndex(ConvExpr env x1, ConvExpr env x2) |> asExpr
                
                (*
                | ArrayAssignQ (_, [ArrayTypeQ(elemTy);_;_],[arr;idx;elem]) -> 
                    let minfo = ArrayAssignMethod.GetGenericMethodDefinition().MakeGenericMethod [| elemTy;typeof<unit> |]
                    Expression.Call(minfo,[| ConvExpr env arr; ConvExpr env idx; ConvExpr env elem |]) |> asExpr
                *)
                
                // Throw away markers inserted to satisfy C#'s design where they pass an argument
                // or type T to an argument expecting Expr<T>.
                // | LinqExpressionHelperQ (_, [_],[x1]) -> ConvExpr env x1
                 
                  /// ArrayLength
                  /// ListBind
                  /// ListInit
                  /// ElementInit
                | _ -> 
                    let argsP = ConvExprs env args 
                    Expression.Call(ConvObjArg env objOpt None, minfo, argsP) |> asExpr |> WrapVoid (IsVoidType minfo.ReturnType) objOpt args env 
    
            | Patterns.IfThenElse(g,t,e) -> 
                Expression.Condition(ConvExpr env g, ConvExpr env t,ConvExpr env e) |> asExpr

            | Patterns.NewDelegate(dty,vs,b) -> 
                let vsP = List.map ConvVar vs 
                let env = {env with varEnv = List.foldBack2 (fun (v:Var) vP -> Map.add v (vP |> asExpr)) vs vsP env.varEnv }
                let bodyP = ConvExpr env b 
                Expression.Lambda(dty, bodyP, vsP) |> asExpr 

            // f x1 x2 x3 x4 --> InvokeFast4
            (*
            | Patterns.Application(Patterns.Application(Patterns.Application(Patterns.Application(f,arg1),arg2),arg3),arg4) -> 
                let domainTy1, rangeTy = getFunctionType f.Type
                let domainTy2, rangeTy = getFunctionType rangeTy
                let domainTy3, rangeTy = getFunctionType rangeTy
                let domainTy4, rangeTy = getFunctionType rangeTy
                let (-->) ty1 ty2 = Reflection.FSharpType.MakeFunctionType(ty1,ty2)
                let ty = domainTy1 --> domainTy2 
                let meth = (ty.GetMethods() |> Array.find (fun minfo -> minfo.Name = "InvokeFast" && minfo.GetParameters().Length = 5)).MakeGenericMethod [| domainTy3; domainTy4; rangeTy |]
                let argsP = ConvExprs env [f; arg1;arg2;arg3; arg4]
                Expression.Call((null:Expression), meth, argsP) |> asExpr
    
            // f x1 x2 x3 --> InvokeFast3
            | Patterns.Application(Patterns.Application(Patterns.Application(f,arg1),arg2),arg3) -> 
                let domainTy1, rangeTy = getFunctionType f.Type
                let domainTy2, rangeTy = getFunctionType rangeTy
                let domainTy3, rangeTy = getFunctionType rangeTy
                let (-->) ty1 ty2 = Reflection.FSharpType.MakeFunctionType(ty1,ty2)
                let ty = domainTy1 --> domainTy2 
                let meth = (ty.GetMethods() |> Array.find (fun minfo -> minfo.Name = "InvokeFast" && minfo.GetParameters().Length = 4)).MakeGenericMethod [| domainTy3; rangeTy |]
                let argsP = ConvExprs env [f; arg1;arg2;arg3]
                Expression.Call((null:Expression), meth, argsP) |> asExpr
    
            // f x1 x2 --> InvokeFast2
            | Patterns.Application(Patterns.Application(f,arg1),arg2) -> 
                let domainTy1, rangeTy = getFunctionType f.Type
                let domainTy2, rangeTy = getFunctionType rangeTy
                let (-->) ty1 ty2 = Reflection.FSharpType.MakeFunctionType(ty1,ty2)
                let ty = domainTy1 --> domainTy2 
                let meth = (ty.GetMethods() |> Array.find (fun minfo -> minfo.Name = "InvokeFast" && minfo.GetParameters().Length = 3)).MakeGenericMethod [| rangeTy |]
                let argsP = ConvExprs env [f; arg1;arg2]
                Expression.Call((null:Expression), meth, argsP) |> asExpr
    
            // f x1 --> Invoke
            | Patterns.Application(f,arg) -> 
                let fP = ConvExpr env f
                let argP = ConvExpr env arg
                let meth = f.Type.GetMethod("Invoke")
                Expression.Call(fP, meth, [| argP |]) |> asExpr
    
            // Expr.New*
            | Patterns.NewRecord(recdTy,args) -> 
                let ctorInfo = Reflection.FSharpValue.PreComputeRecordConstructorInfo(recdTy,showAll) 
                Expression.New(ctorInfo,ConvExprs env args) |> asExpr
    
            | Patterns.NewArray(ty,args) -> 
                Expression.NewArrayInit(ty,ConvExprs env args) |> asExpr
    
            | Patterns.DefaultValue(ty) -> 
                Expression.New(ty) |> asExpr
    
            | Patterns.NewUnionCase(unionCaseInfo,args) -> 
                let methInfo = Reflection.FSharpValue.PreComputeUnionConstructorInfo(unionCaseInfo,showAll)
                let argsR = ConvExprs env args 
                Expression.Call((null:Expression),methInfo,argsR) |> asExpr
    
            | Patterns.UnionCaseTest(e,unionCaseInfo) -> 
                let methInfo = Reflection.FSharpValue.PreComputeUnionTagMemberInfo(unionCaseInfo.DeclaringType,showAll)
                let obj = ConvExpr env e 
                let tagE = 
                    match methInfo with 
                    | :? PropertyInfo as p -> 
                        Expression.Property(obj,p) |> asExpr
                    | :? MethodInfo as m -> 
                        Expression.Call((null:Expression),m,[| obj |]) |> asExpr
                    | _ -> failwith "unreachable case"
                Expression.Equal(tagE, Expression.Constant(unionCaseInfo.Tag)) |> asExpr
    
            | Patterns.NewObject(ctorInfo,args) -> 
                Expression.New(ctorInfo,ConvExprs env args) |> asExpr
    

    
            | Patterns.NewTuple(args) -> 
                 let tupTy = args |> List.map (fun arg -> arg.Type) |> Array.ofList |> Reflection.FSharpType.MakeTupleType
                 let argsP = ConvExprs env args 
                 let rec build ty (argsP: Expression[]) = 
                     match Reflection.FSharpValue.PreComputeTupleConstructorInfo(ty) with 
                     | ctorInfo,None -> Expression.New(ctorInfo,argsP) |> asExpr 
                     | ctorInfo,Some(nestedTy) -> 
                         let n = ctorInfo.GetParameters().Length - 1
                         Expression.New(ctorInfo, Array.append argsP.[0..n-1] [| build nestedTy argsP.[n..] |]) |> asExpr
                 build tupTy argsP
    
            
    
            | Patterns.Sequential (e1,e2) -> 
                let e1P = ConvExpr env e1
                let e2P = ConvExpr env e2
                let minfo = match <@@ SequentialHelper @@> with Lambdas(_,Call(_,minfo,_)) -> minfo | _ -> failwith "couldn't find minfo"
                let minfo = minfo.GetGenericMethodDefinition().MakeGenericMethod [| e1.Type; e2.Type |]
                Expression.Call(minfo,[| e1P; e2P |]) |> asExpr
    
            | Patterns.Let (v,e,b) -> 
                let vP = ConvVar v
                let envinner = { env with varEnv = Map.add v (vP |> asExpr) env.varEnv } 
                let bodyP = ConvExpr envinner b 
                let eP = ConvExpr env e
                let ty = GetFuncType [| v.Type; b.Type |] 
                let lam = Expression.Lambda(ty,bodyP,[| vP |]) |> asExpr
                Expression.Call(lam,ty.GetMethod("Invoke",instanceBindingFlags),[| eP |]) |> asExpr
            *)
    
            | Patterns.Lambda(v,body) -> 
                let vP = ConvVar v
                let env = { env with varEnv = Map.add v (vP |> asExpr) env.varEnv }
                let tyargs = [| v.Type; body.Type |]
                let bodyP = ConvExpr env body
                let convType = typedefof<System.Converter<obj,obj>>.MakeGenericType tyargs
                let convDelegate = Expression.Lambda(convType, bodyP, [| vP |]) |> asExpr
                Expression.Call(typeof<FuncConvert>,"ToFSharpFunc",tyargs,[| convDelegate |]) |> asExpr
        
            (*
            | Patterns.WhileLoop(gd,b) -> 
                let gdP = ConvExpr env <@@ (fun () -> (%%gd:bool)) @@>
                let bP = ConvExpr env <@@ (fun () -> (%%b:unit)) @@>
                let minfo = WhileMethod.GetGenericMethodDefinition().MakeGenericMethod [| typeof<unit> |]
                Expression.Call(minfo,[| gdP; bP |]) |> asExpr
            
            | Patterns.TryFinally(e,h) -> 
                let eP = ConvExpr env (Expr.Lambda(new Var("unitVar",typeof<unit>), e))
                let hP = ConvExpr env <@@ (fun () -> (%%h:unit)) @@>
                let minfo = TryFinallyMethod.GetGenericMethodDefinition().MakeGenericMethod [| e.Type |]
                Expression.Call(minfo,[| eP; hP |]) |> asExpr
            
            | Patterns.TryWith(e,vf,filter,vh,handler) -> 
                let eP = ConvExpr env (Expr.Lambda(new Var("unitVar",typeof<unit>), e))
                let filterP = ConvExpr env (Expr.Lambda(vf,filter))
                let handlerP = ConvExpr env (Expr.Lambda(vh,handler))
                let minfo = TryWithMethod.GetGenericMethodDefinition().MakeGenericMethod [| e.Type |]
                Expression.Call(minfo,[| eP; filterP; handlerP |]) |> asExpr
    
            | Patterns.LetRecursive(binds,body) -> 
    
                let vfs = List.map fst binds
                
                let pass1 = 
                    binds |> List.map (fun (vf,expr) -> 
                        match expr with 
                        | Lambda(vx,expr) -> 
                            let domainTy,rangeTy = getFunctionType vf.Type
                            let vfdTy = GetFuncType [| domainTy; rangeTy |]
                            let vfd = new Var("d",vfdTy)
                            (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd)
                        | _ -> failwith "cannot convert recursive bindings that do not define functions")
    
                let trans = pass1 |> List.map (fun (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd) -> (vf,vfd)) |> Map.ofList
    
                // Rewrite uses of the recursively defined functions to be invocations of the delegates
                // We do this because the delegate are allocated "once" and we can normally just invoke them efficiently
                let rec rw t = 
                    match t with 
                    | Application(Var(vf),t) when trans.ContainsKey(vf) -> 
                         let vfd = trans.[vf]
                         Expr.Call(Expr.Var(vfd),vfd.Type.GetMethod("Invoke",instanceBindingFlags),[t])
                    | ExprShape.ShapeVar(vf) when trans.ContainsKey(vf)-> 
                         let vfd = trans.[vf]
                         let nv = new Var("nv",fst(getFunctionType vf.Type)) 
                         Expr.Lambda(nv,Expr.Call(Expr.Var(vfd),vfd.Type.GetMethod("Invoke",instanceBindingFlags),[Expr.Var(nv)]))
                    | ExprShape.ShapeVar(_) -> t
                    | ExprShape.ShapeCombination(obj,args) -> ExprShape.RebuildShapeCombination(obj,List.map rw args)
                    | ExprShape.ShapeLambda(v,arg) -> Expr.Lambda(v,rw arg)
    
                let vfdTys    = pass1 |> List.map (fun (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd) -> vfdTy) |> Array.ofList
                let vfds      = pass1 |> List.map (fun (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd) -> vfd)
    
                let FPs = 
                    [| for (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd) in pass1 do
                          let expr = rw expr
                          let tyF = GetFuncType (Array.append vfdTys [| vx.Type; expr.Type |])
                          let F = Expr.NewDelegate(tyF,vfds@[vx],expr)
                          let FP = ConvExpr env F
                          yield FP |]
    
                let body = rw body
    
                let methTys   = 
                    [| for (vf,vx,expr,domainTy,rangeTy,vfdTy,vfd) in pass1 do
                          yield domainTy
                          yield rangeTy
                       yield body.Type |]
    
                let B = Expr.NewDelegate(GetFuncType (Array.append vfdTys [| body.Type |]),vfds,body)
                let BP = ConvExpr env B
    
                let minfo = 
                    let q = 
                        match vfds.Length with 
                        | 1 -> <@@ LetRec1Helper @@>
                        | 2 -> <@@ LetRec2Helper @@>
                        | 3 -> <@@ LetRec3Helper @@>
                        | 4 -> <@@ LetRec4Helper @@>
                        | 5 -> <@@ LetRec5Helper @@>
                        | 6 -> <@@ LetRec6Helper @@>
                        | 7 -> <@@ LetRec7Helper @@>
                        | 8 -> <@@ LetRec8Helper @@>
                        | _ -> raise <| new NotSupportedException("In this release of the F# Power Pack, mutually recursive function groups involving 9 or more functions may not be converted to LINQ expressions")
                    match q with Lambdas(_,Call(_,minfo,_)) -> minfo | _ -> failwith "couldn't find minfo"
    
                let minfo = minfo.GetGenericMethodDefinition().MakeGenericMethod methTys
                Expression.Call(minfo,Array.append FPs [| BP |]) |> asExpr
    
            | Patterns.AddressOf _ -> raise <| new NotSupportedException("Address-of expressions may not be converted to LINQ expressions")
            | Patterns.AddressSet _ -> raise <| new NotSupportedException("Address-set expressions may not be converted to LINQ expressions")
            | Patterns.FieldSet _ -> raise <| new NotSupportedException("Field-set expressions may not be converted to LINQ expressions")
            *)
    
            | _ -> 
                raise <| new NotSupportedException(sprintf "Could not convert the following F# Quotation to a LINQ Expression Tree\n--------\n%A\n-------------\n" inp)
    
        and ConvObjArg env objOpt coerceTo : Expression = 
            match objOpt with
            | Some(obj) -> 
                let expr = ConvExpr env obj
                match coerceTo with 
                | None -> expr
                | Some ty -> Expression.TypeAs(expr, ty) :> Expression
            | None -> 
                null
    
        and ConvExprs env es : Expression[] = 
            es |> List.map (ConvExpr env) |> Array.ofList 
    
        and ConvVar (v: Var) = 
            //printf "** Expression .Parameter(%a, %a)\n" output_any ty output_any nm;
            Expression.Parameter(v.Type, v.Name)
    
        let Conv (e: #Expr,eraseEquality) = ConvExpr { eraseEquality = eraseEquality; varEnv = Map.empty } (e :> Expr)

        let toLinq (expr : Expr<'a -> 'b>) =
            // let linqExp = QuotationEvaluator.ToLinqExpression( expr )
            let linqExp = Conv (expr, true)
            let call = linqExp :?> MethodCallExpression
            let lambda = call.Arguments.[0] :?> LambdaExpression
            Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters) 
