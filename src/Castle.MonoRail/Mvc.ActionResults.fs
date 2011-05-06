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

namespace Castle.MonoRail

    open System
    open System.Collections.Generic
    open System.Net
    open System.Web
    open Castle.MonoRail.Mvc.ViewEngines


    type RedirectResult(url:string) = 
        inherit ActionResult()

        new(url:TargetUrl) = RedirectResult((url.Generate null))

        override this.Execute(context:ActionResultContext) = 
            context.HttpContext.Response.Redirect url


    type PermRedirectResult(url:string) = 
        inherit ActionResult()

        new(url:TargetUrl) = PermRedirectResult((url.Generate null))

        override this.Execute(context:ActionResultContext) = 
            context.HttpContext.Response.RedirectPermanent url


    (*
    type HttpResult(status:HttpStatusCode) = 
        inherit ActionResult()
        let _status = status

        override this.Execute(context:ActionResultContext) = 
            ()
    *)


    type ViewResult<'a>(model:'a) = 
        inherit ActionResult()

        let mutable _viewName : string = null
        let mutable _layoutName : string = null
        let mutable _model = model

        member x.ViewName  with get() = _viewName and set v = _viewName <- v
        member x.LayoutName  with get() = _layoutName and set v = _layoutName <- v
        member x.Model  with set v = _model <- v

        override this.Execute(context:ActionResultContext) = 
            let viewreq = new ViewRequest ( 
                                    // AreaName = context.ControllerDescriptor.Area
                                    ViewName = this.ViewName, 
                                    LayoutName = this.LayoutName,
                                    ControllerName = context.ControllerDescriptor.Name, 
                                    ActionName = context.ActionDescriptor.Name
                                )
            let reg = context.ServiceRegistry
            reg.ViewRendererService.Render(viewreq, context.HttpContext, _model)

        interface IModelAccessor<'a> with 
            member x.Model = _model


    type ViewResult() = 
        inherit ViewResult<obj>(obj())


    [<AbstractClass>]
    type SerializerBaseResult<'a>(contentType:string, model:'a) = 
        inherit ActionResult()
        abstract GetMimeType : unit -> MimeType

        override this.Execute(context:ActionResultContext) = 
            let serv = context.ServiceRegistry
            context.HttpContext.Response.ContentType <- contentType
            let mime = this.GetMimeType()
            let serializer = serv.ModelSerializerResolver.CreateSerializer<'a>(mime)
            if (serializer != null) then
                serializer.Serialize (model, contentType, context.HttpContext.Response.Output)
            else 
                failwithf "Could not find serializer for contentType %s and model %s" contentType (typeof<'a>.Name)

        interface IModelAccessor<'a> with 
            member x.Model = model


    type JsonResult<'a>(contentType:string, model:'a) = 
        inherit SerializerBaseResult<'a>(contentType, model)

        new (model:'a) = 
            JsonResult<'a>("application/json", model)

        override x.GetMimeType () = MimeType.JSon


    type JsResult<'a>(contentType:string, model:'a) = 
        inherit SerializerBaseResult<'a>(contentType, model)

        new (model:'a) = 
            JsResult<'a>("text/xml", model)

        override x.GetMimeType () = MimeType.Js


    (*
    type FileResult() = 
        inherit ActionResult()
        override this.Execute(context:ActionResultContext) = 
            ignore()
    *)


    type XmlResult<'a>(contentType:string, model:'a) = 
        inherit SerializerBaseResult<'a>(contentType, model)

        new (model:'a) = 
            XmlResult<'a>("text/xml", model)

        override x.GetMimeType () = MimeType.Xml


    type ContentResult<'a>(model:'a) = 
        inherit ActionResult()
        let mutable _status = HttpStatusCode.OK
        let mutable _redirectTo : TargetUrl = Unchecked.defaultof<_>
        let mutable _location : TargetUrl = Unchecked.defaultof<_>
        let mutable _locationUrl : string = null
        let _actions = lazy Dictionary<MimeType,unit -> ActionResult>()

        let (|Xhtml|Json|Js|Atom|Xml|Rss|Unknown|) (acceptHeader:string []) = 
            if (acceptHeader == null || acceptHeader.Length = 0) then
                Xhtml
            else
                let app, text  = 
                    acceptHeader
                    |> Seq.map (fun (h:string) -> (
                                                    let parts = h.Split([|'/';';'|])
                                                    (parts.[0], parts.[1])
                                                   )  )
                    |> Seq.toList
                    |> List.partition (fun (t1:string,t2:string) -> t1 = "application")

                if not (List.isEmpty app) then
                    let tmp, firstapp = app.Head 
                    match firstapp with 
                    | "json" -> Json
                    | "atom+xml" -> Atom
                    | "rss+xml" -> Rss
                    | "javascript" | "js" -> Js
                    | "soap+xml" -> Js
                    | "xhtml+xml" | "xml" -> Xhtml
                    // | "soap+xml" -> Js
                    | _ -> Unknown
                elif not (List.isEmpty text) then
                    let tmp, firsttxt = text.Head 
                    match firsttxt with 
                    | "xml" -> Xml
                    | "html" -> Xhtml
                    | "javascript" -> Js
                    | _ -> Unknown
                    // csv
                else 
                    Xhtml

        member x.RedirectBrowserTo 
            with get() = _redirectTo and set v = _redirectTo <- v
        member x.StatusCode  
            with get() = _status and set v = _status <- v
        member x.Location 
            with get() = _location and set v = _location <- v
        member x.LocationUrl
            with get() = _locationUrl and set v = _locationUrl <- v
        member x.When(``type``:MimeType, perform:unit -> ActionResult) = 
            _actions.Force().[``type``] <- perform

        override this.Execute(context:ActionResultContext) = 
            let mime = this.ResolveMimeTypeForRequest context
            this.InternalExecute mime context
            ()

        member internal x.ResolveMimeTypeForRequest context = 
            let r, format = context.RouteMatch.RouteParams.TryGetValue "format"
            if r then 
                match format with
                | "html" -> MimeType.Xhtml
                | "json" -> MimeType.JSon
                | "rss" -> MimeType.Rss
                | "js" -> MimeType.Js
                | "atom" -> MimeType.Atom
                | "xml" -> MimeType.Xml
                | _ -> failwithf "Unknown format %s " format
            else 
                let accept_header = context.HttpContext.Request.AcceptTypes
                match accept_header with
                | Xhtml -> MimeType.Xhtml
                | Json -> MimeType.JSon
                | Rss -> MimeType.Rss
                | Js -> MimeType.Js
                | Atom -> MimeType.Atom
                | Xml -> MimeType.Xml
                | Unknown | _ -> failwith "Unknown format in accept header"  

        member internal x.InternalExecute mime context = 
            let response = context.HttpContext.Response
            if _status <> HttpStatusCode.OK then
                response.StatusCode <- int(_status)
            if _locationUrl != null then 
                response.RedirectLocation <- _locationUrl
            elif _location != null then 
                response.RedirectLocation <- _location.Generate null

            let hasCustomAction, func = 
                if _actions.IsValueCreated then 
                    _actions.Value.TryGetValue mime 
                else 
                    false, Unchecked.defaultof<_>

            if hasCustomAction then // customized one found
                let result = func()
                // todo: Assert it was created
                result.Execute(context)

            else // run standard one

                let result : ActionResult = 
                    match mime with 
                    | MimeType.Atom -> upcast XmlResult<'a>("application/atom+xml", model)
                    | MimeType.JSon -> upcast JsonResult<'a>(model)
                    | MimeType.Js -> upcast JsResult<'a>(model)
                    | MimeType.Rss -> upcast XmlResult<'a>("application/rss+xml", model)
                    | MimeType.Xhtml -> 
                        if _redirectTo != null then
                            upcast RedirectResult(_redirectTo)
                        else
                            upcast ViewResult<'a>(model)
                    | MimeType.Xml -> upcast XmlResult<'a>("text/xml", model)
                    | _ -> failwithf "Could not process mime type %s" (mime.ToString())
                result.Execute(context)

        interface IModelAccessor<'a> with 
            member x.Model = model


    type ContentResult() = 
        inherit ContentResult<obj>()


