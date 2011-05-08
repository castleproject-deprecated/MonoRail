namespace Exts.Controllers

open Castle.MonoRail

[<Controller("account")>]
module AccountController = 

    let index() = 
        ViewResult()

    [<HttpMethod(HttpVerb.Post)>]
    let create() = 
        ()

