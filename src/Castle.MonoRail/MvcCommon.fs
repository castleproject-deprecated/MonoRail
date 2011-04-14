
namespace Castle.MonoRail

    open System.Web


    type public ActionResult =
        abstract member Execute : request:HttpContextBase * servRegistry:IServiceRegistry -> unit
