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

module ViewEngine = 

    open System.Linq
    open System.ComponentModel.Composition
    open Castle.MonoRail.Resource
    open Castle.MonoRail.Mvc.ViewEngines
    open System.Web.Compilation

    [<Export(typeof<IViewEngine>)>]
    [<ExportMetadata("Order", 100000)>]
    type RazorViewEngine() = 
        let mutable _resProviders : ResourceProvider seq = Enumerable.Empty<ResourceProvider>()

        (*
        // not sure we really need this
        static do
            BuildProvider.RegisterBuildProvider(".cshtml", typeof<System.Web.WebPages.Razor.RazorBuildProvider>);
        *)

        [<ImportMany(AllowRecomposition=true)>]
        member x.ResourceProviders 
            with get() = _resProviders and set v = _resProviders <- v

        interface IViewEngine with 

            member this.ResolveView req = 
                let view = req.ViewName

                let provider =  
                    _resProviders.FirstOrDefault( fun (p:ResourceProvider) -> p.Exists(view) )

                if (provider <> Unchecked.defaultof<_>) then
                    let res = provider.GetResource view 
                    ViewEngineResult(RazorView(res), this)
                else
                    ViewEngineResult()

    and
        RazorView(resource) = 
            
            interface IView with
                // abstract member Process : (* some param here *) writer:TextWriter -> unit
                member x.Process writer = 
                    writer.WriteLine("from razor - kinda")





