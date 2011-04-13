namespace Castle.MonoRail.Routing

open System
open System.Collections.Generic
open System.Threading
open System.Web
open System.Web.SessionState


type RoutingHttpHandler(router:Router) = 

    let mutable _router = router


    // interface IRequiresSessionState 
        // ignore
    interface IRequiresSessionState 

    interface IHttpHandler with
        member this.IsReusable 
            with get() = true

        member this.ProcessRequest(ctx:HttpContext) : unit =
            // IRouteHandler
            ignore()
    

type RoutingHttpModule(router:Router) = 
    
    let mutable _router = router

    let OnPostResolveRequestCache(sender:obj, args) : unit = 
        
        let app = sender :?> HttpApplication
        let context = app.Context
        let httpRequest = context.Request
        let request = RequestInfoAdapter(httpRequest);
        
        let data = _router.TryMatch(request)

        if (data <> Unchecked.defaultof<_>) then
            let handlerMediator = data.Route.HandlerMediator
            let httpHandler = handlerMediator.GetHandler(httpRequest, data)
            Assertions.IsNotNull (httpHandler, "httpHandler")

            context.RemapHandler httpHandler



    let OnPostResolveRequestCache_Handler = 
        new EventHandler( fun obj args -> OnPostResolveRequestCache(obj, args) )

    new () = 
        RoutingHttpModule(Router.Instance)

    interface IHttpModule with
        member this.Dispose() = 
            ignore()

        member this.Init(app:HttpApplication) =
            app.PostResolveRequestCache.AddHandler OnPostResolveRequestCache_Handler
            ignore()



