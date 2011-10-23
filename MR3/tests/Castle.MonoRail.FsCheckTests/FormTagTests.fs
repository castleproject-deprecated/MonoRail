
open System
open System.Collections.Generic
open System.Web
open System.Text.RegularExpressions
open Castle.MonoRail
open Castle.MonoRail.Helpers
open Castle.MonoRail.ViewEngines

let httpCtxBase = 
    let req = {
            new HttpRequestBase() with 
                override x.Params = null
        }
    let svr = {
            new HttpServerUtilityBase() with 
                override x.HtmlEncode(s) =
                    HttpUtility.HtmlEncode(s)
        }
    {   
        new HttpContextBase() with 
            override x.Request = req
            override x.Server = svr
    }

let bag = Dictionary<string,obj>()
let ctx = new ViewContext(httpCtxBase, bag, null, ViewRequest())
let tagHelper = FormTagHelper(ctx)

let validTag = 
    fun res -> Regex.IsMatch(res, "^<(.)+/>$")
let exists = 
    fun (res:string) (v:string) -> (res.IndexOf v) <> -1

let opts = [ 
    ((fun name id value required html -> tagHelper.TextFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"text\"")
    // ((fun name id value required html -> tagHelper.NumberFieldTag(name, id, value, required, html).ToHtmlString()), "number")
    ((fun name id value required html -> tagHelper.PasswordFieldTag(name, id, value, required, html).ToHtmlString()), "password")
    ((fun name id value required html -> tagHelper.EmailFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"email\"")
    ((fun name id value required html -> tagHelper.CheckboxTag(name, id, value, required, html).ToHtmlString()), "type=\"checkbox\"")
    ((fun name id value required html -> tagHelper.FileFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"file\"")
    // ((fun name id value required html -> tagHelper.HiddenFieldTag(name, id, value, html).ToHtmlString()), "type=\"hidden\"")
    ((fun name id value required html -> tagHelper.RadioFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"radio\"")
    ((fun name id value required html -> tagHelper.UrlFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"url\"")
    ((fun name id value required html -> tagHelper.PhoneFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"tel\"")
    ((fun name id value required html -> tagHelper.SearchFieldTag(name, id, value, required, html).ToHtmlString()), "type=\"search\"")
] 

let perms = ref 0
for tup in opts do
    let hmethod = fst tup
    let out = snd tup
    for requiredGen in [|false;true|] do 
        for possibleVal in [|null;"";"value"|] do
            let result = hmethod "name" "id" possibleVal requiredGen null
            incr perms

            if not (validTag result) then
                failwithf "%O fail" hmethod
            if requiredGen && not (exists result "required aria-required=\"true\"") then
                failwithf "%O fail" hmethod
            if not (exists result out) then
                failwithf "%O fail. %s not found in %s" hmethod out result
            if not (exists result "name=\"name\"") then
                failwithf "%O fail" hmethod
            if not (exists result "id=\"id\"") then
                failwithf "%O fail" hmethod
            if not (exists result ("value=\"" + possibleVal + "\"")) then
                failwithf "%O fail" hmethod
            if not (exists result "<input ") then
                failwithf "%O fail" hmethod

printfn "Permutations run %d" !perms

