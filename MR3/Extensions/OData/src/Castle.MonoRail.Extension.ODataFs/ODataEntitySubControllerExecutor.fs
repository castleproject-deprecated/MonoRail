namespace Castle.MonoRail.Extension.OData

open System
open System.Linq
open System.Linq.Expressions
open System.Xml
open System.Collections
open System.Collections.Generic
open System.ComponentModel.Composition
open System.Data.OData
open System.Data.Services.Providers
open Castle.MonoRail
open Castle.MonoRail.Hosting.Mvc
open Castle.MonoRail.Hosting.Mvc.Extensibility
open Castle.MonoRail.Hosting.Mvc.Typed

type ODataEntitySubControllerExecutor() = 
    inherit ControllerExecutor()
    let mutable _lifetime : ExportLifetimeContext<ODataEntitySubControllerExecutor> = null

    member this.Lifetime with get() = _lifetime and set(v) = _lifetime <- v

    override x.Execute(action, controller, route, http) = 
        ()

[<ControllerExecutorProviderExport(8000000)>]
type ODataEntitySubControllerExecutorProvider() = 
    inherit ControllerExecutorProvider()
    let mutable _execFactory : ExportFactory<ODataEntitySubControllerExecutor> = null

    [<Import(RequiredCreationPolicy=CreationPolicy.NewScope)>]
    member this.ExecutorFactory
        with get() = _execFactory and set(v) = _execFactory <- v

    override this.Create(prototype, data, context) = 
        match prototype with
        | :? TypedControllerPrototype as inst_prototype ->
            let exp = _execFactory.CreateExport()
            let executor = exp.Value
            executor.Lifetime <- exp
            upcast executor 
        | _ -> null
