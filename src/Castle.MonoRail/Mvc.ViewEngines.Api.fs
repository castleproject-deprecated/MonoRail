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

namespace Castle.MonoRail.Mvc.ViewEngines

    open System
    open System.IO

    // we need to make sure this interface allows for recursive view engines
    // ie. view engines that would allow for application of layouts recursively with no change needed in its api

    type ViewRequest(viewName:string, layout:string, area:string) = 
        let _viewName = viewName
        let _layout = layout
        let _area = area

        member this.ViewName = _viewName
        member this.LayoutName = _layout
        member this.AreaName = _area


    type ViewEngineResult(view:IView, engine:IViewEngine) = 
        let _view = view
        let _engine = engine

        new () = 
            ViewEngineResult(Unchecked.defaultof<_>, Unchecked.defaultof<_>)

        member this.View = _view
        member this.Engine = _engine
    

    and [<Interface>] 
        public IViewEngine =
            abstract member ResolveView : req:ViewRequest -> ViewEngineResult


    and [<Interface>] 
        public IView =
            abstract member Process : (* some param here *) writer:TextWriter -> unit
