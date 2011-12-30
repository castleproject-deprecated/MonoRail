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

module Container

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
    // open Castle.Extensibility
        

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
                    (not (cpd.ContainsPartMetadataWithKey("Scope")) || 
                          cpd.ContainsPartMetadata("Scope", ComponentScope.Application)))
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
                let asm = Assembly.Load name
                asm
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

    let private binFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "bin")
    let private extFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "modules")

    let mutable private _customCatalog : ComposablePartCatalog = null

    let private uber_catalog : ComposablePartCatalog = 
        // let catalogs = List<ComposablePartCatalog>()
        // catalogs.Add (new DirectoryCatalogGuarded(binFolder))
        // new AggregateCatalog(catalogs)
        upcast new DirectoryCatalogGuarded(binFolder)

    let set_custom_catalog (catalog) = 
        _customCatalog <- catalog 

    let private __locker = new obj()
    let mutable private _sharedContainerInstance = Unchecked.defaultof<CompositionContainer>

    let private getOrCreateContainer =
        if (_sharedContainerInstance = null) then 
            lock(__locker) 
                (fun _ ->   
                    (
                        if (_sharedContainerInstance = null) then 
                            let opts = CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection
                            let catalog : ComposablePartCatalog = upcast new MetadataBasedScopingPolicy(_customCatalog <?> uber_catalog)
                            
                            let tempContainer = new CompositionContainer(catalog, opts)
                            System.Threading.Thread.MemoryBarrier()
                            _sharedContainerInstance <- tempContainer
                    ))
        _sharedContainerInstance
   
    let internal Get<'a>() = 
        let app = getOrCreateContainer
        app.GetExportedValueOrDefault<'a>()

    let internal GetAll<'a>() = 
        let app = getOrCreateContainer
        app.GetExportedValues<'a>()

    // let private _cache = System.Collections.Concurrent.ConcurrentDictionary()

    (* 
    let internal SatisfyImports (target:obj) =
        let app = getOrCreateContainer
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
