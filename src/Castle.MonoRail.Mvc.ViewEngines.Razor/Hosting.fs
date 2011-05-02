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

    open System.Web.WebPages.Razor
    open System.Web.Razor
    open System.Web.Razor.Generator

    type MonoRailRazorHostFactory() = 
        inherit WebRazorHostFactory() 

        override x.CreateHost (vpath, ppath) = 
            let host = base.CreateHost(vpath, ppath)

            if not host.IsSpecialPage then
                upcast MonoRailRazorHost(vpath, ppath)
            else
                host


    and MonoRailRazorHost (vpath, ppath) as self =
        inherit WebPageRazorHost(vpath, ppath)

        let remove_ns names =
            for n in names do
                self.NamespaceImports.Remove n |> ignore

        do
            base.DefaultPageBaseClass <- typeof<Castle.MonoRail.Razor.WebViewPage>.FullName
            // RemoveNamespace "WebMatrix.Data", "System.Web.WebPages.Html", "WebMatrix.WebData" 

    (*
        member x.DecorateCodeGenerator(codeGen) = 
            CustomRazorCodeGen(codeGen)

    and CustomRazorCodeGen(className:string, ns:string, source:string, host:RazorEngineHost) = 
        inherit RazorCodeGenerator(className, ns, source, host)
    *)


