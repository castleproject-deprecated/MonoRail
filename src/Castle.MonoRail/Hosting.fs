
namespace Castle.MonoRail.Hosting

module Hosting =

    open System.Web

    [<Interface>]
    type IComposableHandler =
        abstract member ProcessRequest : request:HttpContextBase -> unit

    
    [<AbstractClass>]
    type ComposableHandler() as self =

        abstract member ProcessRequest : request:HttpContextBase -> unit
        
        interface IHttpHandler with
            member this.ProcessRequest(context:HttpContext) : unit =
                let ctxWrapped = HttpContextWrapper(context)
                self.ProcessRequest(ctxWrapped);

//                var ctx = new HttpContextWrapper(context);
//
//			    var container = ContainerManager.CreateRequestContainer(ctx);
//			
//			    container.HookOn(context);
//
//			    container.Compose(this);
//
//			    ProcessRequest(ctx);
                ignore()

            member this.IsReusable = 
                true

        interface IComposableHandler with
            // funny way to define abstract members associated with interfaces
            member x.ProcessRequest (request:HttpContextBase) : unit = self.ProcessRequest(request)
        
