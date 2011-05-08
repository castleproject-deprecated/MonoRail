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

namespace Castle.MonoRail.Razor

open System
open System.Web
open System.Web.WebPages
open Castle.MonoRail.Helpers
open Castle.MonoRail.Mvc.ViewEngines

[<Interface>]
type IViewPage = 
    abstract member Layout : string  with get,set
    abstract member ViewContext : ViewContext  with get,set
    abstract member RawModel : obj with get,set
    (*
	    void SetData(object model);
	    object GetData();
	    string VirtualPath { set; }
	    HttpContextBase Context { set; }
	    DataContainer DataContainer { get; set; }
	    ViewComponentRenderer ViewComponentRenderer { get; set; }
    *)

[<AbstractClass>]
type WebViewPage<'TModel>() = 
    inherit WebPageBase()

    let mutable _model : 'TModel = Unchecked.defaultof<_>
    let mutable _viewctx = Unchecked.defaultof<ViewContext>
    let _form = lazy FormHelper(_viewctx)
    let _html = lazy HtmlHelper(_viewctx)

    member x.ViewCtx with get() = _viewctx and set v = _viewctx <- v
    member x.Model  with get() = _model and set v = _model <- v
    member x.Form = _form.Force()
    member x.Html = _html.Force()

    override x.ExecutePageHierarchy() = 
        x.ViewCtx.Writer <- x.Output

        base.ExecutePageHierarchy()

    override x.ConfigurePage (parent) = 
        let parent_as_vp = parent |> box :?> IViewPage
        let this_as_vp = x |> box :?> IViewPage

        x.Context <- parent.Context
        x.ViewCtx <- parent_as_vp.ViewContext
        x.Model <- parent_as_vp.RawModel |> box :?> 'TModel

    interface IViewPage with 
        member x.Layout with get() = base.Layout and set v = base.Layout <- v
        member x.ViewContext with get() = _viewctx and set v = _viewctx <- v
        member x.RawModel with get() = _model |> box  and set v = _model <- v :?> 'TModel

    
[<AbstractClass>]
type WebViewPage() = 
    inherit WebViewPage<obj>()



