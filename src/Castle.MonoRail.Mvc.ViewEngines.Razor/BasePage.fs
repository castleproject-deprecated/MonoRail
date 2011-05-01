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

[<Interface>]
type IViewPage = 
    abstract member Layout : string  with get,set
    (*
	    void SetData(object model);
	    object GetData();
	    string VirtualPath { set; }
	    HttpContextBase Context { set; }
	    DataContainer DataContainer { get; set; }
	    ViewContext ViewContext { get; set; }
	    ViewComponentRenderer ViewComponentRenderer { get; set; }
    *)

[<AbstractClass>]
type WebViewPage<'TModel>() = 
    inherit WebPageBase()

    let mutable _model : 'TModel = Unchecked.defaultof<_>

    member x.Model  with get() = _model and set v = _model <- v

    override x.ConfigurePage (parent) = 
        //x.Model <- obj()
        x.Context <- parent.Context
            (*
			var parent = parentPage as WebViewPage<TModel>;

			if (parent == null)
				throw new Exception("View base type is invalid");

			Context = parent.Context;
			Model = parent.Model;
            *)        

    interface IViewPage with 
        member x.Layout with get() = base.Layout and set v = base.Layout <- v


[<AbstractClass>]
type WebViewPage() = 
    inherit WebViewPage<obj>()



