module OldModel
(*
            let entityTypes = _entities |> Seq.map (fun e -> e.TargetType)

            let subControllerMap, extraTypes = build_subcontrollers_map entityTypes services
            _type2SubControllerInfo := subControllerMap

            let rts = ResourceMetadataBuilder.build(_schemaNs, _entities, extraTypes)
            
            // set everything as readonly
            rts |> Seq.iter (fun rt -> rt.SetReadOnly() )
                
            _resourcetypes := rts
            _resourcesets  := 
                rts 
                |> Seq.filter (fun rt -> rt.ResourceTypeKind = ResourceTypeKind.EntityType && (_entities |> Seq.exists (fun e -> e.EntityName === rt.Name) ) )
                |> Seq.map (fun rt -> (let name = (_entSeq |> Seq.find(fun e -> e.TargetType = rt.InstanceType)).EntitySetName 
                                       let rs = ResourceSet(name, rt)
                                       rs.SetReadOnly()
                                       rs )
                           )
                |> box :?> ResourceSet seq

            build_actions()

            populate_service_operations_model()

        member internal x.GetControllerCreator (rt:Type) = 
            let succ, info = getsubcontrollerInfo(rt)
            if succ 
            then info.creator
            else null


        member x.Entities = _entSeq
        member internal x.ResourceSets  = !_resourcesets
        member internal x.ResourceTypes = !_resourcetypes

        member internal x.GetResourceType(name) = 
            x.ResourceTypes 
            |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )
        member internal x.GetResourceSet(name) = 
            x.ResourceSets 
            |> Seq.tryFind (fun rs -> StringComparer.OrdinalIgnoreCase.Equals( rs.Name, name ) )
        member internal x.GetQueryable(rs:ResourceSet) = 
            match _entities |> Seq.tryFind (fun e -> StringComparer.OrdinalIgnoreCase.Equals(e.EntitySetName, rs.Name)) with
            | Some e -> e.Source
            | _ -> null
        member internal x.GetRelatedResourceSet (rt:ResourceType) =
            x.ResourceSets 
            |> Seq.tryFind (fun rs -> rs.ResourceType = rt)

        // TODO: refactor to get rid of duplication on the following 3 methods
        member internal x.SupportsAction (rt:ResourceType, name:string) = 
            let succ, info = getsubcontrollerInfo(rt)
            if succ 
            then info.desc.HasAction name
            else false

        member internal x.GetControllerCreator (rt:ResourceType) = 
            let succ, info = getsubcontrollerInfo(rt)
            if succ 
            then info.creator
            else null

        member internal x.GetNestedOperation (rt:ResourceType, name:string) : ControllerActionOperation = 
            let succ, info = getsubcontrollerInfo(rt)
            if succ && info.desc.HasAction name // TODO: should use HTTP VERB to narrow options (??)
            then 
                match info.odataActions |> Seq.tryFind (fun a -> a.Name === name) with
                | Some action -> action
                | _ -> info.ordinaryActions |> Seq.find (fun a -> a.Name === name) 
            else null


        interface IDataServiceMetadataProvider with 
            member x.ContainerNamespace = x.SchemaNamespace
            member x.ContainerName = x.ContainerName
            member x.ResourceSets = x.ResourceSets
            member x.Types = x.ResourceTypes
            member x.ServiceOperations =  !_serviceOps
            member x.GetDerivedTypes(resType) = 
                // we dont support hierarchies yet
                Seq.empty
            member x.HasDerivedTypes(resType) = 
                // we dont support hierarchies yet
                false
            member x.TryResolveResourceSet(name, rtToReturn) = 
                match x.GetResourceSet(name) with 
                | Some rt -> rtToReturn <- rt; true
                | None -> false
            member x.TryResolveResourceType(name, rtToReturn) = 
                match x.GetResourceType(name) with 
                | Some rt -> rtToReturn <- rt; true
                | None -> false 
            member x.TryResolveServiceOperation(name, opToReturn) = 
                match !_serviceOps |> Seq.tryFind (fun op -> op.Name = name) with
                | Some svc -> opToReturn <- svc; true
                | _ -> false
            member x.GetResourceAssociationSet(resSet, resType, property) = 
                let targetResType = property.ResourceType
                match x.ResourceSets |> Seq.tryFind (fun rs -> targetResType.InstanceType.IsAssignableFrom(rs.ResourceType.InstanceType)) with
                | Some containerResSet -> 
                    ResourceAssociationSet(resType.Name + "_" + property.Name, 
                                           ResourceAssociationSetEnd(resSet, resType, property), 
                                           ResourceAssociationSetEnd(containerResSet, targetResType, null))
                | _ -> null

        
        let _executors = List<ControllerExecutor>()
        let _invoker_cache   = Dictionary<Type, string ->  bool  ->  IList<Type * obj> -> RouteMatch -> HttpContextBase -> obj>()

        let get_action_invoker rt routematch context = 
            let create_controller_prototype (rt:Type) = 
                let creator = (!_modelToUse).GetControllerCreator (rt)
                if creator <> null 
                then
                    let creationCtx = ControllerCreationContext(routematch, context)
                    creator.Invoke(creationCtx)
                else null

            // we will have issues with object models with self referencies
            // a better implementation would "consume" the items used, taking them off the list
            let tryResolveParamValue (paramType:Type) isCollection (parameters:IList<Type * obj>) = 
                let entryType =
                    
                    if isCollection then
                        match InternalUtils.getEnumerableElementType paramType with
                        | Some t -> t
                        | _ -> paramType
                    elif paramType.IsGenericType then
                        paramType.GetGenericArguments().[0]
                    else paramType

                match parameters |> Seq.tryFind (fun (ptype, _) -> ptype = entryType || entryType.IsAssignableFrom(ptype)) with 
                | Some (_, value) ->
                    // param is Model<T>
                    if paramType.IsGenericType && paramType.GetGenericTypeDefinition() = typedefof<Model<_>> 
                    then Activator.CreateInstance ((typedefof<Model<_>>).MakeGenericType(paramType.GetGenericArguments()), [|value|])
                    else // entryType <> paramType && paramType.IsAssignableFrom(entryType) then
                        value
                | _ -> null

            // returns a function able to invoke actions
            let create_executor_fn (rt:Type) prototype = 
                let executor = (!_services).ControllerExecutorProvider.CreateExecutor(prototype)
                Diagnostics.Debug.Assert ( executor <> null && executor :? ODataEntitySubControllerExecutor )
                _executors.Add executor
                let odataExecutor = executor :?> ODataEntitySubControllerExecutor
                (fun action isCollection parameters routeMatch context -> 
                    let callback = Func<Type,obj>(fun ptype -> tryResolveParamValue ptype isCollection parameters)
                    odataExecutor.GetParameterCallback <- callback
                    executor.Execute(action, prototype, routeMatch, context))
            let succ, existing = _invoker_cache.TryGetValue rt
            if succ then existing
            else
                let prototype = create_controller_prototype rt
                let executor = create_executor_fn rt prototype 
                _invoker_cache.[rt] <- executor
                executor

        let invoke_action rt action isCollection parameters (route:RouteMatch) context = 
            let invoker = get_action_invoker rt route context
            invoker action isCollection parameters route context

        let invoke_controller (action:string) isCollection (rt:Type) parameters optional route context = 
            if (!_modelToUse).SupportsAction(rt, action) then
                let result = invoke_action rt action isCollection parameters route context
                if result = null || ( result <> null && result :? EmptyResult )
                // if the action didn't return anything meaningful, we consider it a success
                then true, null
                // else, the action took over, and we should therefore end our execution
                else false, result
            else
                // if we couldnt run the action, then the results 
                // depends on whether the call was optional or not 
                if optional
                then true, null
                else false, null

        let clean_up =
            _executors |> Seq.iter (fun exec -> (exec :> IDisposable).Dispose() )
        *)

//let _type2SubControllerInfo : Ref<Dictionary<Type, SubControllerInfo>> = ref null
        (*
        let _resourcetypes : Ref<ResourceType seq> = ref null
        let _resourcesets  : Ref<ResourceSet seq> = ref null
        let _serviceOps : Ref<ServiceOperation seq> = ref null
        let _entSeq : EntitySetConfig seq = upcast _entities

        let get_rt_from_type (t:Type) =
            (!_resourcetypes) |> Seq.find (fun rt -> rt.InstanceType = t)
        *)

        (*

        let getsubcontrollerInfo(rt:Type) = 
            if !_type2SubControllerInfo <> null 
            then (!_type2SubControllerInfo).TryGetValue rt.InstanceType
            else false, SubControllerInfo.Empty


        // for each subcontroller found, creates a "creator function" and the controller descriptor
        let resolve_subcontrollerinfo (entityType:Type) (services:IServiceRegistry) = 
            let template = typedefof<IODataEntitySubController<_>>
            let concrete = template.MakeGenericType([|entityType|])
            let spec = PredicateControllerCreationSpec(fun t -> concrete.IsAssignableFrom(t))
            let creator = services.ControllerProvider.CreateController(spec)
            if creator <> null then 
                let prototype = creator.Invoke() :?> TypedControllerPrototype
                let desc = prototype.Descriptor :?> TypedControllerDescriptor
                
                let serviceOps, ordinaryActions = 
                    desc.Actions 
                    |> List.ofSeq
                    |> List.partition (fun action -> action.HasAnnotation<ODataOperationAttribute>())

                // TODO: prune ordinaryActions list to remove callbacks

                {
                    containerType = entityType
                    creator = creator
                    desc = desc
                    serviceOps = serviceOps
                    ordinaryOps = ordinaryActions
                    odataActions = null
                    ordinaryActions = null
                    containerRt = null
                }
            else SubControllerInfo.Empty

        // builds a list of ControllerActionOperation to match the 
        // service operations found previously
        let build_actions () = 
            let build_actionop useodataStack containerRt (desc:ControllerActionDescriptor) = 
                if useodataStack then
                    let isColl, elType = 
                        match getEnumerableElementType(desc.ReturnType) with
                        | Some elType -> true , elType
                        | _ -> false, desc.ReturnType
                    let retRt = get_rt_from_type elType
                    ControllerActionOperation(useodataStack, containerRt, desc.NormalizedName, isColl, retRt)
                else
                    ControllerActionOperation(false, containerRt, desc.NormalizedName, false, null)


            let build_odata_actions_list containerRt (actions:ControllerActionDescriptor seq) = 
                actions |> Seq.map (fun action -> build_actionop true containerRt action)

            let build_actions_list containerRt (actions:ControllerActionDescriptor seq) = 
                actions |> Seq.map (fun action -> build_actionop false containerRt action)

            let subControllers = _type2SubControllerInfo.Value.Values

            subControllers
            |> Seq.iter (fun p -> p.containerRt  <- get_rt_from_type p.containerType
                                  p.odataActions <- build_odata_actions_list p.containerRt p.serviceOps
                                  p.ordinaryActions <- build_actions_list    p.containerRt p.ordinaryOps)


        let build_subcontrollers_map (entityTypes:Type seq) (services:IServiceRegistry) = 
            let dict = Dictionary<Type, SubControllerInfo>()
            entityTypes
            |> Seq.map (fun t -> t, resolve_subcontrollerinfo t services)
            |> Seq.filter (fun t -> snd t <> SubControllerInfo.Empty)
            |> Seq.iter (fun t -> dict.Add (fst t, snd t))
            
            let extraTypesOnServiceOpsSignatures = 
                let extract_complextypes_from_action (action:ControllerActionDescriptor) = 
                    // TODO: need to support the parameters as well
                    // action.Parameters |> Seq.map (fun p -> p.ParamType)
                    [| action.ReturnType |]
                dict.Values
                |> Seq.collect (fun subInfo -> subInfo.serviceOps ) 
                |> Seq.collect extract_complextypes_from_action
                |> Seq.filter (fun t -> not ( t.IsPrimitive || t = typeof<string> || t = typeof<unit> ) )

            dict, extraTypesOnServiceOpsSignatures

        let populate_service_operations_model () = 
            
            // string name, ServiceOperationResultKind resultKind, ResourceType resultType,
            // ResourceSet resultSet, string method, IEnumerable<ServiceOperationParameter> parameters 
            let op = ServiceOperation("name", ServiceOperationResultKind.Void, null, null, "GET", Seq.empty)
            op.SetReadOnly()

            let ops = List<ServiceOperation>([|op|])

            _serviceOps := upcast ops

            ()
        *)
