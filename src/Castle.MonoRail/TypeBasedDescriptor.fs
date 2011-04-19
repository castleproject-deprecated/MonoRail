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
    open System.Reflection
    open System.Collections.Generic
    open System.Linq
    open System.ComponentModel.Composition
    open Castle.MonoRail.Extensibility

    [<AbstractClass>] 
    type BaseDescriptor() = 
        let _meta = lazy Dictionary<string,obj>()
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
