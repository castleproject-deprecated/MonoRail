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

namespace Castle.MonoRail.Hosting

    open System
    open System.IO
    open System.Linq
    open System.Reflection
    open System.Threading
    open System.Collections.Generic
    open System.ComponentModel.Composition
    open System.ComponentModel.Composition.Hosting
    open System.ComponentModel.Composition.Primitives
    open System.Web
    open Castle.MonoRail
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.Extensibility.Hosting

    // Creates scopes based on the Scope Metadata. 
    // Root/App is made of parts with Scope = App or absence of the marker
    // Everything else goes to the Request scope
    type MetadataBasedScopingPolicy private (catalog, children, pubsurface) =
        inherit CompositionScopeDefinition(catalog, children, pubsurface) 

        new (ubercatalog:ComposablePartCatalog) = 
            // App
            // App-Override
            // Request
            // Request-Override

            let appDefault = 
                ubercatalog.Filter(fun cpd -> 
                    (not (cpd.ContainsPartMetadataWithKey("Scope")) || cpd.ContainsPartMetadata("Scope", ComponentScope.Application)))
            (*
            let appOverride = 
                appDefault.Complement.Filter(fun cpd -> cpd.ContainsPartMetadata("Scope", ComponentScope.ApplicationOverride))

            let requestDefault = 
                appOverride.Complement.Filter(fun cpd -> cpd.ContainsPartMetadata("Scope", ComponentScope.Request))

            let requestOverride = 
                requestDefault.Complement.Filter(fun cpd -> cpd.ContainsPartMetadata("Scope", ComponentScope.Request))
            *)
            
            let psurface = appDefault.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions)

            let childcat = appDefault.Complement
            let childexports = 
                childcat.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions )
            let childdef = new CompositionScopeDefinition(childcat, [], childexports)
            
            new MetadataBasedScopingPolicy(appDefault, [childdef], psurface)


    // Since MEF's DirectoryCatalog does not guard against Assembly.GetTypes failing
    // I had to write my own
    type DirectoryCatalogGuarded(folder) = 
        inherit ComposablePartCatalog()

        let _catalogs = List<TypeCatalog>()
        let mutable _parts : ComposablePartDefinition seq = null

        let load_assembly_guarded (file:string) : Assembly = 
            try
                let name = AssemblyName.GetAssemblyName(file);
                Assembly.Load name
            with | ex -> null

        do 
            let files = Directory.GetFiles(folder, "*.dll")
            for file in files do
                let asm = load_assembly_guarded file
                if asm != null then
                    let types = RefHelpers.guard_load_types(asm) |> Seq.filter (fun t -> t != null) 
                    if not (Seq.isEmpty types) then
                        _catalogs.Add (new TypeCatalog(types))
            _parts <- _catalogs |> Seq.collect (fun c -> c.Parts) |> Linq.Queryable.AsQueryable

        override x.Parts = _parts.AsQueryable()

        override x.GetExports(definition) = 
            _catalogs |> Seq.collect (fun c -> c.GetExports(definition))


    [<AllowNullLiteral>]
    type IContainer = 
        interface 
            abstract member Get<'T> : unit -> 'T
            abstract member GetAll<'T> : unit -> 'T seq
            // abstract member SatisfyImports : target:obj -> unit
        end

    [<AllowNullLiteral>]
    type Container (path:string) = 
        class

            let uber_catalog : ComposablePartCatalog = upcast new DirectoryCatalogGuarded(path)

            let catalog : ComposablePartCatalog = 
                upcast new MetadataBasedScopingPolicy(uber_catalog)
                              
            let mef_container =
                let opts = CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection
                new CompositionContainer(catalog, opts)

            let _cache = System.Collections.Concurrent.ConcurrentDictionary()

            new () = 
                let binFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "bin")
                Container(binFolder)

            member x.DefaultMrCatalog = catalog

            interface IContainer with 

                member x.Get() = 
                    mef_container.GetExportedValueOrDefault()

                member x.GetAll() = 
                   mef_container.GetExportedValues()
                
                (*    
                member x.SatisfyImports (target) = 
                    let app = mef_container 
                    let targetType = target.GetType()
                    let found, definition = _cache.TryGetValue(targetType)
                    if not found then
                        let partdef = System.ComponentModel.Composition.AttributedModelServices.CreatePartDefinition(target.GetType(), null)
                        _cache.TryAdd (targetType, partdef) |> ignore
                        let part = System.ComponentModel.Composition.AttributedModelServices.CreatePart(partdef, target)
                        app.SatisfyImportsOnce(part)
                    else
                        let part = System.ComponentModel.Composition.AttributedModelServices.CreatePart(definition, target)
                        app.SatisfyImportsOnce(part)
                *)
        end
             
    type ContainerAdapter(container:HostingContainer) = 
        interface IContainer with
            member x.Get<'T>() = 
                container.GetExportedValue<'T>()
            
            member x.GetAll<'T>() = 
                container.GetExportedValues<'T>()             
 
    module MRComposition = 
        
        let mutable _composer : IContainer = upcast Container()

        let public SetCustomContainer (container) = 
            _composer <- container

        let Get<'T> () = _composer.Get<'T>()

        let GetAll<'T> () = _composer.GetAll<'T>()

        // let SatisfyImports (target) = _composer.SatisfyImports(target)

