# Controllers

Controllers are extremely important as they are the intelligent piece of the application that orchestrate the application flow. The Getting started section depicts the basic usage of controllers.

Controllers must, at the very least, implement `IController` and but are normally derived from the `Controller` class, which provides all the basic features and helper methods a controller must provide, or the `SmartDispatchController` class, which adds some very useful data binding features.

## Naming Convention

The class name is used by default as the controller identification. If your controller's name ends with `Controller`, it will be stripped from the name. You can also use the `ControllerDetails` attribute to associate a different name to your controller class.

It is advisable that controller classes follow the `Name + Controller` suffix convention. When the controller is registered, MonoRail strips the suffix and uses the name as the key. The controller name is used on the URL to access it.

The attribute `ControllerDetailsAttribute` can be used to force the definition of a name to the controller. For example:

```csharp
using Castle.MonoRail.Framework;

[ControllerDetails("cust")]
public class Customer : Controller
{
}
```

The controller defined above will be accessible using `cust` on the url.

## Areas

MonoRail supports the concept of areas, which are logic groups of controllers. All controllers belongs to an area. The default area is an empty (unnamed) one.

You can think of a tree of controllers. Each node is an area, each leaf is a controller.

To define an area, use the ControllerDetailsAttribute attribute:

```csharp
using Castle.MonoRail.Framework;

[ControllerDetails(Area="admin")]
public class UsersController : Controller
{
    public void Index()
    {
    }
}
```

This controller now is accessible using the following url path:

`/admin/users/index.rails`

You can also define more than one level:

```csharp
using Castle.MonoRail.Framework;

[ControllerDetails(Area="admin/users")]
public class PasswordMngController : Controller
{
    public void Index()
    {
    }
}
```

This controller now is accessible using the following url path:

`/admin/users/passwordmng/index.rails`

## Actions

We refer to actions as the procedures that can be invoked on your controller. Basically it translates to any public instance method your controller exposes.

**Only ''public'' instances:** If you do not want that a specific method be "invocable", it cannot be public.

### Default Action

The `DefaultActionAttribute` attribute provides a way to associate a default action method that will be called if a matching action method can not be found. One possible use is so that a web designer can add views without the need for a developer to add new action methods. To associate a default action with your controller, use the `DefaultActionAttribute` attribute. This attribute can only be applied at the class level.

Unless you specify which action should be invoked if none is matched, `DefaultAction` will be used.

```csharp
[DefaultAction]
public class HomeController : Controller
{
    public void Index()
    {
    }

    public void DefaultAction()
    {
        string template = "notfound";

        if (HasTemplate("home/" + Action))
        {
            template = Action;
        }

        RenderView(template);
    }
}
```

In the following example, the code specifies the action to be invoked:

```csharp
[DefaultAction("Foo")]
public class HomeController : Controller
{
    public void Index()
    {
    }

    public void Foo()
    {
        RenderText(Action + " was not found");
    }
}
```

### Asynchronous Actions

Most actions in a typical application that take time to execute are not doing so because they are using the CPU exhaustively; rather, they are often performing I/O functions.  Since the thread executing the action is coming from the ASP.NET thread pool any large number of long running actions that are performing I/O or waiting for any other reason than using the CPU is unnecessarily starving the pool of threads and may be harming the performance and scalability of the application.

To address this problem MonoRail provides support for asynchronous actions which will release the executing thread back to the ASP.NET thread pool so that it can handle another request while the application is waiting for whatever long running, non-CPU-intensive process to complete execution.

Let's assume that we have a controller with a Find action which takes a long time to run:

```csharp
public class MovieController : SmartDispatcherController
{
    IMovieFinder movieFinder;

    public void Find(string movieName)
    {
        PropertyBag["movies"] = movieFinder.Find(movieName);
    }
}
```

For the sake of argument, the Find method on the IMovieFinder instance does a web service call that might take a few seconds to respond or it calls to a database on a remote machine that likewise takes a long time to respond.

To make this into an asynchronous action we need to split the Find action into a Begin/End method pair.

The "begin" action's method name must be prefixed with the word "Begin" and must return an `IAsyncResult`.  This method would perform whatever setup was required for the long running action and would then execute it.

```csharp
public IAsyncResult BeginFind(string movieName)
{
    return movieFinder.BeginFind(movieName,
        ControllerContext.Async.Callback,
        ControllerContext.Async.State);
}
```

**Asynchronous Method Invocation Pattern:** Note that we've also changed the Find method on our IMovieFinder to use the standard async invocation pattern for the CLR calls - you can read more about them [here](http://msdn.microsoft.com/en-us/library/22t547yb(v=VS.85).aspx).

The corresponding end action must have the same name but prefixed instead with the word "End" and return void.  This method can access the result of the begin method and would perform whatever additional tasks the method requires before sending the response as a normal action would:

```csharp
public void EndFind()
{
    PropertyBag["movies"] = movieFinder.EndFind(ControllerContext.Async.Result);
}
```

MonoRail's asynchronous actions are using the standard .NET async semantics, and MonoRail will infer them automatically based on the Begin/End method names and signatures and use will the underlying ASP.NET async request infrastructure.  You can read more about asynchronous requests under ASP.NET [here](http://msdn2.microsoft.com/en-us/magazine/cc163463.aspx).

## Redirecting

The `Controller` base class offers a handful of `Redirect` overloads. Some of them allow you to pass along query string parameters.

The following table list some of the overloads:

Name | Description
-----|------------
RedirectToAction(string action) | Redirects to another action in the same controller.
RedirectToAction(String action, params String[] queryStringParameters) | Redirects to another action in the same controller specifying query string entries.
RedirectToAction(String action, IDictionary parameters) | Redirects to another action in the same controller specifying query string entries.
Redirect(String url) | Redirects to the specified URL
Redirect(String url, IDictionary parameters) | Redirects to the specified URL specifying query string entries.
Redirect(String controller, String action) | Redirects to another controller and action.
Redirect(String area, String controller, String action) | Redirects to another controller and action (within an area).

## Other Useful Properties

### Request/Response

Property | Type | Description
---------|------|------------
Context | `Castle.MonoRail.Framework.IRailsEngineContext` | Gets the context of this request execution.
Session | `IDictionary` | Gets the Session dictionary.
Flash | `Castle.MonoRail.Framework.Flash` | Gets a dictionary of volative items. Ideal for showing success and failures messages.
HttpContext | `System.Web.HttpContext` | Gets the web context of ASP.NET API.
Request | `Castle.MonoRail.Framework.IRequest` | Gets the request object.
Response | `Castle.MonoRail.Framework.IResponse` | Gets the response object.
Params | `NameValueCollection` | Shortcut to IRequest.Params
Form | `NameValueCollection` | Shortcut to IRequest.Form
Query | `NameValueCollection` | Shortcut to IRequest.QueryString
IsClientConnected | `bool` | Shortcut to IResponse.IsClientConnected

### Controller Information

Property | Type | Description
---------|------|------------
Name | `string` | Gets the controller's name (as MonoRail knows it)
AreaName | `string` | Gets the controller's area name.
LayoutName | `string` | Gets or set the layout being used.
Action | `string` | Gets the name of the action being processed.
SelectedViewName | `string` || Gets or sets the view which will be rendered by this action. We encourage you to use RenderView or RenderSharedView instead of setting this property.

### Others

Property | Type | Description
---------|------|------------
Logger | `Castle.Core.Logging.ILogger` | Logger for the controller (you must enable logging first)
IsPostBack | `bool` | Determines if the current Action resulted from an ASP.NET PostBack. As a result, this property is only relavent when using WebForms views. It is placed on the base Controller for convenience only to avoid the need to extend the Controller or provide additional helper classes.

## Data Binding

MonoRail is able to bind simple values and complex objects to parameters on action methods. Data binding is discussed in detail in the [Data Binding](controllers-data-binding.md) section.

## Wizards

You can use wizards to present smaller chunks of information to the user, with more immediate feedback. For example, during a registration process or cart check-out you could save their objects into session at each step, and then persist to the database, at the end, when it is all valid and confirmed, instead of having to either save intermediary objects that are not in an acceptable state, or make one massive form.

MonoRail controllers have built-in support to create wizard-like chained pages; further information can be found [here](wizards.md).