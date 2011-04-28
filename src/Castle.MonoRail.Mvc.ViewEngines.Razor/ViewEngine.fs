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

namespace Castle.MonoRail.Mvc.ViewEngines.Razor

    open System.Linq
    open System.ComponentModel.Composition
    open Castle.MonoRail.Resource
    open Castle.MonoRail.Mvc.ViewEngines
    open Castle.MonoRail.Hosting.Mvc.Typed
    open System.Web.Compilation
    open System.Web.WebPages

    [<Export(typeof<IViewEngine>)>]
    [<ExportMetadata("Order", 100000)>]
    type RazorViewEngine() = 
        let mutable _hosting = Unchecked.defaultof<IAspNetHostingBridge>
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()

        static member Initialize() = 
            BuildProvider.RegisterBuildProvider(".cshtml", typeof<System.Web.WebPages.Razor.RazorBuildProvider>);

        [<Import>]
        member x.HostingBridge 
            with get() = _hosting and set v = _hosting <- v

        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders 
            with get() = _resProviders and set v = _resProviders <- v

        interface IViewEngine with 

            member this.ResolveView viewLocations = 
                let views = seq {
                                    for l in viewLocations do
                                        yield l + ".cshtml"
                                } 

                use enumerator = _resProviders.GetEnumerator()
                
                let rec provider_sel() : string * ResourceProvider = 
                    if (enumerator.MoveNext()) then
                        let provider = enumerator.Current
                        let existing_view = 
                            views 
                            |> Seq.find (fun (v) -> provider.Exists(v)) 
                        if existing_view = null then 
                            provider_sel()
                        else 
                            existing_view, provider
                    else
                        null, Unchecked.defaultof<_>

                let view, provider = provider_sel()

                if (provider <> Unchecked.defaultof<_>) then
                    // let res = provider.GetResource view 
                    let razorview = RazorView(view, _hosting)
                    ViewEngineResult(razorview, this)
                else
                    ViewEngineResult()

    and
        RazorView(viewPath, hosting) = 
            let _viewInstance = lazy (
                    let compiled = hosting.GetCompiledType(viewPath)
                    System.Activator.CreateInstance(compiled) :?> WebPageBase
                )

            interface IView with
                member x.Process (writer, httpctx) = 
                    let pageBase = _viewInstance.Force()
                    let pageCtx = WebPageContext(httpctx, pageBase, obj())
                    pageBase.ExecutePageHierarchy(pageCtx, writer, pageBase)

                



