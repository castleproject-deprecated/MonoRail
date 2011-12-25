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
        

    // Creates scopes based on the Scope Metadata. 
    // Root/App is made of parts with Scope = App or absence or the marker
    // Everything else goes to the Request scope
    type MetadataBasedScopingPolicy private (catalog, children, pubsurface) =
        inherit CompositionScopeDefinition(catalog, children, pubsurface) 

        new (ubercatalog:ComposablePartCatalog) = 
            let app = ubercatalog.Filter(fun cpd -> 
                    (not (cpd.ContainsPartMetadataWithKey("Scope")) || 
                        cpd.ContainsPartMetadata("Scope", ComponentScope.Application)))
            let psurface = app.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions )

            let childcat = app.Complement
            let childexports = 
                childcat.Parts.SelectMany( fun (cpd:ComposablePartDefinition) -> cpd.ExportDefinitions )
            let childdef = new CompositionScopeDefinition(childcat, [], childexports)

            new MetadataBasedScopingPolicy(app, [childdef], psurface)

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

    type AggregatePartDefinition(folder:string) = 
        class
            inherit ComposablePartDefinition()
            let _dirCatalog = new DirectoryCatalogGuarded(folder)
            
            override x.ExportDefinitions = _dirCatalog.Parts |> Seq.collect (fun p -> p.ExportDefinitions)
            override x.ImportDefinitions : ImportDefinition seq = Seq.empty
            override x.CreatePart() = 
                upcast new AggregatePart(_dirCatalog)
        end

    and AggregatePart(catalog:ComposablePartCatalog) = 
        class 
            inherit ComposablePart()
            let _flags = CompositionOptions.DisableSilentRejection ||| CompositionOptions.IsThreadSafe ||| CompositionOptions.ExportCompositionService
            let _container = lazy( new CompositionContainer(new MetadataBasedScopingPolicy(catalog), _flags) )
            
            override x.ExportDefinitions = catalog.Parts |> Seq.collect (fun p -> p.ExportDefinitions)
            override x.ImportDefinitions : ImportDefinition seq = Seq.empty
            override x.Activate() = 
                _container.Force() |> ignore
                () 

            override x.GetExportedValue(expDef) = 
                // very naive implementation, but should do for now
                _container.Force().GetExportedValue<obj>(expDef.ContractName)
                
            override x.SetImport( importDef, exports) = 
                // we dont import anything
                ()

            interface IDisposable with 
                member x.Dispose() = 
                    _container.Force().Dispose()
        end

    type BasicComposablePartCatalog(partDefs:ComposablePartDefinition seq) = 
        inherit ComposablePartCatalog() 
        let _parts = lazy ( partDefs.AsQueryable() )

        override x.Parts = _parts.Force()
            
        override x.Dispose(disposing) = 
            ()


    let private binFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "bin")
    let private extFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "modules")

    let private uber_catalog = 
        let catalogs = List<ComposablePartCatalog>()
        // catalogs.Add (new DirectoryCatalogGuarded(binFolder))
        catalogs.Add (new BasicComposablePartCatalog([|AggregatePartDefinition(binFolder)|]))
        // if (File.Exists(extFolder)) then
            // catalogs.Add (new DirectoryCatalog(extFolder))
            // catalogs.Add (new ModuleManagerCatalog(extFolder))
        new AggregateCatalog(catalogs)

    let private app_catalog = 
        // new MetadataBasedScopingPolicy(uber_catalog)
        uber_catalog

    let private __locker = new obj()
    let mutable private _sharedContainerInstance = Unchecked.defaultof<CompositionContainer>

    let private getOrCreateContainer =
        if (_sharedContainerInstance = null) then 
            Monitor.Enter(__locker)
            try
                if (_sharedContainerInstance = null) then 
                    let opts = CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection
                    let tempContainer = new CompositionContainer(app_catalog, opts)
                    System.Threading.Thread.MemoryBarrier()
                    _sharedContainerInstance <- tempContainer
            finally
                Monitor.Exit(__locker)
        _sharedContainerInstance

    let private _cache = System.Collections.Concurrent.ConcurrentDictionary()

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

    (*
    [<Interface>]
    type IModuleManager = 
        abstract member Modules : IEnumerable<ModuleEntry>
        abstract member Toggle : entry:ModuleEntry * newState:bool -> unit

    // work in progress
    // the idea is that each folder with the path becomes an individual catalog
    // representing an unique "feature" or "module"
    // and can be turned off independently
    
    and ModuleManagerCatalog(path:string) =
        inherit ComposablePartCatalog() 

        let _path = path
        let _mod2Catalog = Dictionary<string, ModuleEntry>()
        let _aggregate = new AggregateCatalog()

        do
            let subdirs = System.IO.Directory.GetDirectories(_path)
            
            for subdir in subdirs do
                let module_name = Path.GetDirectoryName subdir
                let dir_catalog = new DirectoryCatalog(subdir)
                let entry = ModuleEntry(dir_catalog, true)
                _mod2Catalog.Add(module_name, entry)
                _aggregate.Catalogs.Add dir_catalog
        
        override this.Parts 
            with get() = _aggregate.Parts
        
        override this.GetExports(import:ImportDefinition) =
            _aggregate.GetExports(import)

        interface IModuleManager with
            member x.Modules 
                with get() = _mod2Catalog.Values :> IEnumerable<ModuleEntry>
            member x.Toggle (entry:ModuleEntry, newState:bool) = 
                if (newState && not entry.State) then
                    _aggregate.Catalogs.Add(entry.Catalog)
                elif (not newState && entry.State) then 
                    ignore(_aggregate.Catalogs.Remove(entry.Catalog))
                entry.State <- newState

        interface INotifyComposablePartCatalogChanged with
            member x.add_Changed(h) =
                _aggregate.Changed.AddHandler h
            member x.remove_Changed(h) =
                _aggregate.Changed.RemoveHandler h
            member x.add_Changing(h) =
                _aggregate.Changing.AddHandler h
            member x.remove_Changing(h) =
                _aggregate.Changing.RemoveHandler h


    and ModuleEntry(catalog, state) = 
        let _catalog = catalog
        let mutable _state = state
        member x.Catalog 
            with get() = _catalog
        member x.State
            with get() = _state and set(v) = _state <- v

    *)
