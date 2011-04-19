//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Hosting.Mvc

    open System
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Extensibility

    module TypeBasedPM = 

        [<AbstractClass>] 
        type BaseDescriptor() = 
            let _meta = lazy Dictionary<string,obj>() // 
            member this.Metadata
                with get() = _meta.Force()

        and 
            ControllerDescriptor(controller:Type) =
                inherit BaseDescriptor()
                let _actions = List<ControllerActionDescriptor>() // no need to be T-safe
                
                member this.Actions 
                    with get() = _actions :> IList<ControllerActionDescriptor>

        and 
            [<AbstractClass>] 
            ControllerActionDescriptor() = 
                inherit BaseDescriptor()
                abstract member Execute : instance:obj * args:obj[] -> obj

        and 
            MethodInfoActionDescriptor(methodInfo:MethodInfo) = 
                inherit ControllerActionDescriptor()
                let _methodInfo = methodInfo

                override this.Execute(instance:obj, args:obj[]) = 
                    new obj()
        and 
            ParamInfoActionDescriptor(name:string) = 
                let _name = name
                member this.Name
                    with get() = _name

        [<Interface>]
        type ITypeDescriptorBuilderContributor = 
            abstract member Process : target:Type * desc:ControllerDescriptor -> unit

        [<Interface>]
        type IMemberDescriptorBuilderContributor = 
            abstract member Process : target:MemberInfo * desc:MethodInfoActionDescriptor * parent:ControllerDescriptor -> unit

        [<Interface>]
        type IParameterDescriptorBuilderContributor = 
            abstract member Process : target:ParameterInfo * desc:ParamInfoActionDescriptor * desc:MethodInfoActionDescriptor * parent:ControllerDescriptor -> unit


        [<Export>]
        type ControllerDescriptorBuilder() = 
            
            let mutable _typeContributors = Enumerable.Empty<Lazy<ITypeDescriptorBuilderContributor, IComponentOrder>>()
            let mutable _memberContributors = Enumerable.Empty<Lazy<IMemberDescriptorBuilderContributor, IComponentOrder>>()
            let mutable _paramContributors = Enumerable.Empty<Lazy<IParameterDescriptorBuilderContributor, IComponentOrder>>()

            [<ImportMany(AllowRecomposition=true)>]
            member this.TypeContributors
                with get() = _typeContributors and set(v) = _typeContributors <- Helpers.order_lazy_set v

            [<ImportMany(AllowRecomposition=true)>]
            member this.MemberContributors
                with get() = _memberContributors and set(v) = _memberContributors <- Helpers.order_lazy_set v

            [<ImportMany(AllowRecomposition=true)>]
            member this.ParamContributors
                with get() = _paramContributors and set(v) = _paramContributors <- Helpers.order_lazy_set v

            member this.Build(controller:Type) = 
                Assertions.ArgNotNull (controller, "controller")

                let desc = ControllerDescriptor(controller)
                let potentialActions = controller.GetMethods(BindingFlags.Public ||| BindingFlags.Instance)

                for c in this.TypeContributors do
                    c.Force().Process (controller, desc)
                
                for a in potentialActions do
                    for c in this.MemberContributors do
                        if (not a.IsSpecialName) then 
                            let method_desc = MethodInfoActionDescriptor(a)
                            c.Force().Process (a, method_desc, desc)
                        
                            for p in a.GetParameters() do
                                for pc in this.ParamContributors do
                                    let p_desc = ParamInfoActionDescriptor(p.Name)
                                    pc.Force().Process (p, p_desc, method_desc, desc)

                desc

        [<AbstractClass>]
        type BaseTypeBasedControllerProvider() = 
            inherit ControllerProvider()
            let mutable _desc_builder = Unchecked.defaultof<ControllerDescriptorBuilder>

            [<Import>]
            member this.ControllerDescriptorBuilder
                with get() = _desc_builder and set(v) = _desc_builder <- v

            abstract ResolveControllerType : data:RouteMatch * context:HttpContextBase -> System.Type
            abstract ActivateController : cType:System.Type * desc:ControllerDescriptor -> obj
            abstract BuildPrototype : inst:obj * desc:ControllerDescriptor -> ControllerPrototype

            default this.BuildPrototype(inst:obj, desc:ControllerDescriptor) = 
                TypedControllerPrototype(desc, inst) :> ControllerPrototype

            override this.Create(data:RouteMatch, context:HttpContextBase) = 
                let cType = this.ResolveControllerType(data, context)
                if (cType <> null) then
                    let desc = _desc_builder.Build(cType)
                    let instance = this.ActivateController(cType, desc)
                    this.BuildPrototype(instance, desc)
                else
                    Unchecked.defaultof<_>


        and 
          [<ControllerProviderExport(9000000)>] 
          ReflectionBasedControllerProvider [<ImportingConstructor>] (hosting:IAspNetHostingBridge) =
            inherit BaseTypeBasedControllerProvider()
            let _hosting = hosting
            let _entries = Dictionary<string,Type>(StringComparer.OrdinalIgnoreCase)
        
            do
                let size_of_controller = "Controller".Length
            
                seq { 
                        for asm in _hosting.ReferencedAssemblies do 
                            let all_types = 
                                Helpers.typesInAssembly asm (fun t -> not t.IsAbstract && t.Name.EndsWith("Controller"))
                            yield all_types
                    }
                |> Seq.concat
                |> Seq.iter (fun t -> 
                    let name = t.Name.Substring(0, t.Name.Length - size_of_controller)
                    _entries.[name] <- t )

            override this.ResolveControllerType(data:RouteMatch, context:HttpContextBase) = 
                let name = data.RouteParams.["controller"]
                if (name <> null) then
                    let r, typ = _entries.TryGetValue name
                    if (r) then
                        typ
                    else
                        null
                else
                    null

            override this.ActivateController(cType:Type, desc:ControllerDescriptor) = 
                Activator.CreateInstance(cType) 

        and 
            TypedControllerPrototype(desc, instance) = 
                inherit ControllerPrototype(instance)
                let _desc = desc


        [<ControllerExecutorProviderExport(9000000)>]
        type PocoControllerExecutorProvider() = 
            inherit ControllerExecutorProvider()
            let mutable _execFactory = Unchecked.defaultof<ExportFactory<PocoControllerExecutor>>

            [<Import>]
            member this.ExecutorFactory
                with get() = _execFactory and set(v) = _execFactory <- v

            override this.Create(prototype:ControllerPrototype, data:RouteMatch, context:HttpContextBase) = 
                match prototype with
                | :? TypedControllerPrototype as inst_prototype ->
                    let executor = _execFactory.CreateExport().Value
                    executor.Prototype <- inst_prototype
                    executor :> ControllerExecutor
                | _ -> 
                    Unchecked.defaultof<ControllerExecutor>
        
        and 
            [<Export>] PocoControllerExecutor() = 
                inherit ControllerExecutor()
                let mutable _prototype = Unchecked.defaultof<TypedControllerPrototype>
                
                member this.Prototype 
                    with get() = _prototype and set(v) = _prototype <- v

                override this.Execute(controller:ControllerPrototype, route_data:RouteMatch, context:HttpContextBase) = 
                    ignore()


