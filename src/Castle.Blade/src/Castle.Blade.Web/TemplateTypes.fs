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

namespace Castle.Blade.Web

    open System
    open System.IO
    open System.Collections.Generic
    open System.Web
    open System.Web.Compilation
    open System.Globalization
    open Castle.Blade




    type PageContext (ctx:HttpContextBase, vpath:string) = 
        let mutable _bodyContent : string = null
        let _sections = Dictionary<string, Action<TextWriter>>(StringComparer.InvariantCultureIgnoreCase)
        let _pageData = lazy ( System.Collections.Specialized.HybridDictionary(true) ) 

        member x.PageData = _pageData.Force()
        member x.VirtualPath = vpath
        member x.HttpContext = ctx
        member x.BodyContent
            with get() = _bodyContent and set v = _bodyContent <- v
        member x.RegisterSection(name:string, section:Action<TextWriter>) = 
            _sections.[name] <- section
        member x.TryGetSection name = 
            _sections.TryGetValue name
    

    [<AbstractClass>]
    type WebBladePage() = 
        inherit BaseBladePage()

        let mutable _layoutName : string = null
        let mutable _isInitialized = false
        let mutable _writer : TextWriter = null
        let mutable _pageCtx : PageContext = Unchecked.defaultof<_>
        let _outputStack = Stack<TextWriter>()

        member x.Cache = _pageCtx.HttpContext.Cache
        member x.Context : HttpContextBase = _pageCtx.HttpContext
        member x.Output = _outputStack.Peek()
        member x.VirtualPath = _pageCtx.VirtualPath
        member x.Layout 
            with get() = _layoutName and set v = _layoutName <- v

        member x.IsSectionDefined name = 
            let res = _pageCtx.TryGetSection name
            res

        member x.DefineSection(name:string, action:Action<TextWriter>) =
            _pageCtx.RegisterSection(name, action)

        member x.RenderSection(name:string, required:bool) = 
            let res, action = _pageCtx.TryGetSection name
            if not res then
                if required then
                    // todo: define a decent exception hierarchy
                    failwithf "Section named %s not found for rendering" name
                else
                    HtmlString("")
            else 
                use writer = new StringWriter()
                action.Invoke( writer )
                HtmlString( writer.ToString() )

        member x.RenderSection(name:string) = 
            x.RenderSection (name, true)

        member x.RenderPage(ctx:PageContext, writer:TextWriter) = 
            if not _isInitialized then
                x.Initialize()
                _isInitialized <- true

            x.PushContext(ctx, writer);
            x.PreRenderPage()
            x.RenderPage()
            x.PostRenderPage()
            x.PopContext();            
            
        member private x.RenderOuter( bodyContent:string ) = 
            _pageCtx.BodyContent <- bodyContent

            let compiled = BuildManager.GetCompiledType(x.Layout)
            let webPage = System.Activator.CreateInstance(compiled) :?> WebBladePage
            webPage.ConfigurePage x
            webPage.RenderPage(_pageCtx, _writer )

        member x.PushContext (ctx:PageContext, writer:TextWriter) = 
            _pageCtx <- ctx
            _writer <- writer
            _outputStack.Push (new StringWriter())
        
        member x.PopContext () = 
            let content = _outputStack.Pop().ToString()
            
            if (x.Layout != null) then
                _outputStack.Push _writer |> ignore
                x.RenderOuter(content)
                _outputStack.Pop() |> ignore
            else
                _writer.Write content
            
        abstract member ConfigurePage : parent:BaseBladePage -> unit 
        
        default x.ConfigurePage parent = ()

        override x.Initialize() = ()

        override x.RenderPage() = ()
        
        override x.Write(content) = 
            x.Write(x.Output, content)

        override x.Write(writer, content) = 
            writer.Write (HttpUtility.HtmlEncode content)
        
        override x.WriteLiteral(content) = 
            x.WriteLiteral(x.Output, content)

        override x.WriteLiteral(writer, content) = 
            writer.Write content

        member x.RenderBody() = 
            System.Web.HtmlString( _pageCtx.BodyContent )

        member x.Href(path:string) = 
            VirtualPathUtility.Combine(x.VirtualPath, path)

        member x.PageContext = _pageCtx
        member x.Request = x.Context.Request
        member x.Response = x.Context.Response
        member x.Server = x.Context.Server
        member x.Session = x.Context.Session
        member x.User = x.Context.User
        member x.IsPost = x.Request.HttpMethod = "POST"
        member x.IsAjax = 
            let req = x.Request.Headers.["X-Requested-With"]
            req = "XMLHttpRequest"
        member x.Culture 
            with get() : string = System.Threading.Thread.CurrentThread.CurrentCulture.Name
            and set (v:string) = 
                let ci = CultureInfo.GetCultureInfo(v)
                System.Threading.Thread.CurrentThread.CurrentCulture <- ci
        member x.UICulture 
            with get() : string = System.Threading.Thread.CurrentThread.CurrentUICulture.Name
            and set (v:string) = 
                let ci = CultureInfo.GetCultureInfo(v)
                System.Threading.Thread.CurrentThread.CurrentUICulture <- ci
        member x.PageData = _pageCtx.PageData
        (* 
        public abstract IDictionary<object, dynamic> PageData { get;}
        public abstract dynamic Page { get; }
        *)