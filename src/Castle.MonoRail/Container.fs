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
    open Castle.MonoRail.Extensibility

    let private binFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "\bin")
    let private extFolder = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "\modules")

    let private uber_catalog = 
        let catalogs = List<ComposablePartCatalog>()
        catalogs.Add (new DirectoryCatalog(binFolder))
        if (File.Exists(extFolder)) then
            catalogs.Add (new DirectoryCatalog(extFolder))
        new AggregateCatalog(catalogs)

    let private app_catalog = 
        uber_catalog.Filter(fun cpd -> 
            not (cpd.ContainsPartMetadataWithKey("Scope")) || 
            cpd.ContainsPartMetadata("Scope", ComponentScope.Application))

    let private req_catalog = 
        app_catalog.Complement

    let private __locker = new obj()
    let mutable private _sharedContainerInstance = Unchecked.defaultof<CompositionContainer>

    let private getOrCreateContainer =
        if (_sharedContainerInstance = null) then 
            Monitor.Enter(__locker)
            try
                if (_sharedContainerInstance = null) then 
                    let tempContainer = new CompositionContainer(app_catalog, CompositionOptions.IsThreadSafe ||| CompositionOptions.DisableSilentRejection)
                    
                    System.Threading.Thread.MemoryBarrier()
                    
                    _sharedContainerInstance <- tempContainer
            finally
                Monitor.Exit(__locker)
        _sharedContainerInstance

    let private to_contract = 
        AttributedModelServices.GetContractName

    let internal CreateRequestContainer(context:HttpContextBase) = 
        let container = new CompositionContainer(req_catalog, CompositionOptions.DisableSilentRejection, getOrCreateContainer)
        context.Items.["__mr_req_container"] <- container
        let batch = new CompositionBatch();
        ignore(batch.AddExportedValue(to_contract typeof<HttpRequestBase>, context.Request))
        ignore(batch.AddExportedValue(to_contract typeof<HttpResponseBase>, context.Response))
        ignore(batch.AddExportedValue(to_contract typeof<HttpContextBase>, context))
        ignore(batch.AddExportedValue(to_contract typeof<HttpServerUtilityBase>, context.Server))
        container.Compose(batch);
        container

    let private OnRequestEnded (sender:obj, args:EventArgs) = 
        let app = sender :?> HttpApplication
        let context = app.Context
        let req_container = context.Items.["__mr_req_container"] :?> CompositionContainer
        if (req_container <> null) then
            req_container.Dispose()


    let private end_request_handler = 
        new EventHandler( fun obj args -> OnRequestEnded(obj, args) )

    let mutable private __alreadyHooked = 0

    let internal SubscribeToRequestEndToDisposeContainer(app:HttpApplication) = 

        if (Interlocked.CompareExchange(&__alreadyHooked, 1, 0) = 0) then // not hooked yet
            app.EndRequest.AddHandler end_request_handler

        ignore()


    // work in progress
    // the idea is that each folder with the path becomes an individual catalog
    // representing an unique "feature" or "module"
    // and can be turned off independently
    type private DeploymentAggregateCatalog(path:string) =
        inherit ComposablePartCatalog() 

        let _path = path
        let mutable _aggregate = new AggregateCatalog()

        override this.Parts 
            with get() : IQueryable<ComposablePartDefinition> = null
        
        // public virtual IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition);
        override this.GetExports(import:ImportDefinition) =
            _aggregate.GetExports(import)


