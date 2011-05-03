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

namespace Castle.MonoRail.Helpers

open System
open System.Web
open Castle.MonoRail
open Castle.MonoRail.Mvc.ViewEngines


[<AbstractClass>]
type public BaseHelper(context:ViewContext) = 
    let _ctx = context

    member x.Context = _ctx
    member x.Writer = _ctx.Writer


type public FormHelper(ctx) = 
    inherit BaseHelper(ctx)

    member x.BeginForm (url:TargetUrl) = 
        base.Writer.WriteLine "<form>"
        new FormState(x.Writer)


and FormState(writer) = 
    let _writer = writer
        
    interface IDisposable with
        member x.Dispose() = 
            _writer.WriteLine "</form>"
            ()


type public HtmlHelper(ctx) = 
    inherit BaseHelper(ctx)

    member x.Label(id:string, text:string) =
        HtmlString("<label for='' >" + text + "</label>")

    member x.TextInput(name:string) = 
        HtmlString("<input />")

