namespace Exts.Controllers

open Castle.MonoRail

    type Global() =
        inherit System.Web.HttpApplication()

[<Controller("account")>]
module AccountController = 

    let index() = 
        ViewResult()

    // [<HttpMethod(HttpVerb.Post)>]
    let postcreate() = 
        ()
