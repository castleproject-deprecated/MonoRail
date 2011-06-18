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
    open Castle.Blade

    type HtmlResult (ac:Action<TextWriter>) = 
        override x.ToString() = 
            use writer = new StringWriter() 
            ac.Invoke(writer)
            writer.ToString()

        interface IHtmlString with 
            member x.ToHtmlString() = x.ToString()


    type PageContext (ctx:HttpContextBase) = 
        let mutable _bodyContent : string = null

        member x.HttpContext = ctx
        member x.BodyContent
            with get() = _bodyContent and set v = _bodyContent <- v

    
    [<AbstractClass>]
    type WebBladePage() = 
        inherit BaseBladePage()

        let mutable _layoutName : string = null
        let mutable _isInitialized = false
        let mutable _writer : TextWriter = null
        let mutable _pageCtx : PageContext = Unchecked.defaultof<_>
        let _outputStack = Stack<TextWriter>()

        member x.RenderPage(ctx:PageContext, writer:TextWriter) = 
            if not _isInitialized then
                x.Initialize()
                _isInitialized <- true

            x.PushContext(ctx, writer);
            x.RenderPage()
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
            
        member x.Output = _outputStack.Peek()

        member x.Layout 
            with get() = _layoutName and set v = _layoutName <- v

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

        (* 
        public bool IsSectionDefined(string name) {
            EnsurePageCanBeRequestedDirectly("IsSectionDefined");
            return PreviousSectionWriters.ContainsKey(name);
        }

        public virtual HttpContextBase Context { get; set; }

        public virtual string VirtualPath { get; set; }

        public virtual Cache Cache {
            get {
                if (Context != null) {
                    return Context.Cache;
                }
                return null;
            }
        }

        public abstract string Layout { get; set; }

        public abstract IDictionary<object, dynamic> PageData { get;}

        public abstract dynamic Page { get; }

        public WebPageContext PageContext { get; internal set; }

        public virtual HttpRequestBase Request {
            get {
                if (Context != null) {
                    return Context.Request;
                }
                return null;
            }
        }

        public virtual HttpResponseBase Response {
            get {
                if (Context != null) {
                    return Context.Response;
                }
                return null;
            }
        }

        public virtual HttpServerUtilityBase Server {
            get {
                if (Context != null) {
                    return Context.Server;
                }
                return null;
            }
        }

        public virtual HttpSessionStateBase Session {
            get {
                if (Context != null) {
                    return Context.Session;
                }
                return null;
            }
        }

        public virtual IList<string> UrlData {
            get {
                if (_urlData == null) {
                    WebPageMatch match = WebPageRoute.GetWebPageMatch(Context);
                    if (match != null) {
                        _urlData = new UrlDataList(match.PathInfo);
                    }
                    else {
                        // REVIEW: Can there ever be no route match?
                        _urlData = new UrlDataList(null);
                    }
                }
                return _urlData;
            }
        }

        public virtual IPrincipal User {
            get {
                if (_user == null) {
                    return Context.User;
                }
                return _user;
            }
            internal set {
                _user = value;
            }
        }

        public virtual TemplateFileInfo TemplateInfo {
            get {
                if (_templateFileInfo == null) {
                    _templateFileInfo = new TemplateFileInfo(VirtualPath);
                }
                return _templateFileInfo;
            }
        }

        public virtual bool IsPost {
            get { return Request.HttpMethod == "POST"; }
        }

        public virtual bool IsAjax {
            get {
                var request = Request;
                if (request == null) {
                    return false;
                }
                return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"));
            }
        }

        public virtual string Href(string path, params object[] pathParts) {
            return Util.Url(VirtualPath, path, pathParts);
        }

        public string Culture {
            get {
                return Thread.CurrentThread.CurrentCulture.Name;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    // GetCultureInfo accepts empty strings but throws for null strings. To maintain consistency in our string handling behavior, throw
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value");
                }
                CultureUtil.SetCulture(Thread.CurrentThread, Context, value);
            }
        }

        public string UICulture {
            get {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    // GetCultureInfo accepts empty strings but throws for null strings. To maintain consistency in our string handling behavior, throw
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value");
                }
                CultureUtil.SetUICulture(Thread.CurrentThread, Context, value);
            }
        }
        
        public static void WriteTo(TextWriter writer, HelperResult content) {
            if (content != null) {
                content.WriteTo(writer);
            }
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteTo(TextWriter writer, object content) {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        // This method is called by generated code and needs to stay in sync with the parser
        public static void WriteLiteralTo(TextWriter writer, object content) {
            writer.Write(content);
        }

        *)