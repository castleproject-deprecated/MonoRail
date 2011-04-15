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

        [<Interface>]
        type ITypeDescriptorBuilderContributor = 
            abstract member Process : target:Type -> unit

        [<Interface>]
        type IMemberDescriptorBuilderContributor = 
            abstract member Process : target:MemberInfo -> unit

        [<Interface>]
        type IParameterDescriptorBuilderContributor = 
            abstract member Process : target:ParameterInfo -> unit


        [<Export>]
        type ControllerDescriptorBuilder() = 
            
            let mutable _typeContributors = Enumerable.Empty<ITypeDescriptorBuilderContributor>()
            let mutable _memberContributors = Enumerable.Empty<IMemberDescriptorBuilderContributor>()
            let mutable _paramContributors = Enumerable.Empty<IParameterDescriptorBuilderContributor>()

            [<ImportMany(AllowRecomposition=true)>]            
            member this.TypeContributors
                with get() = _typeContributors and set(v) = _typeContributors <- v

            [<ImportMany(AllowRecomposition=true)>]            
            member this.MemberContributors
                with get() = _memberContributors and set(v) = _memberContributors <- v

            [<ImportMany(AllowRecomposition=true)>]            
            member this.ParamContributors
                with get() = _paramContributors and set(v) = _paramContributors <- v

            member this.Build(controller:Type) = 
                Assertions.ArgNotNull (controller, "controller")

                for c in this.TypeContributors do
                    c.Process controller


                ignore()

        and ControllerDescriptor(instance) =
            inherit ControllerPrototype(instance)

            

        
        [<ControllerProviderExport(9000000)>]
        type ReflectionBasedControllerProvider [<ImportingConstructor>] (hosting:IAspNetHostingBridge) =
            inherit ControllerProvider()
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

            override this.Create(data:RouteData, context:HttpContextBase) : ControllerPrototype = 
                let name = data.RouteParams.["controller"]
            
                if (name <> null) then
                    let r, typ = _entries.TryGetValue name
                    
                    if (r) then
                        let instance = Activator.CreateInstance(typ) 
                        let desc = ControllerDescriptor(instance)
                        


                        desc :> ControllerPrototype

                    else
                        Unchecked.defaultof<ControllerPrototype>
                else 
                    Unchecked.defaultof<ControllerPrototype>




