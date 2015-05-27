# Routing

The new Routing engine provides a method of mapping arbitrary URLs to specified area/controller/action combinations. Parameters can be matched based on regular expressions, text or numerical values and a fluent API is provided for creating routing rules.

## Configuration

Simply add the following to your HttpModules section in web.config:

```xml
<add name="routing" type="Castle.MonoRail.Framework.Routing.RoutingModuleEx, Castle.MonoRail.Framework" />
```

## Registering a Route

RoutingModuleEx is the static class used for registering a route. Generally you'll want to set up your route in the Global.asax Application_Start, and here's an example of that to get us started:

```csharp
public class Global : HttpApplication
{
    public void Application_OnStart()
    {
        RoutingModuleEx.Engine.Add(
            new PatternRoute("/<controller>/<id>/view.aspx")
                .DefaultForAction().Is("view")
        );
    }
}
```

In this case we are handling URLs which take the form /category/5/view.aspx or /product/2/view.aspx. The part of the PatternRoute constructor parameter indicates that we wish to capture that part of the URL and use it as the controller name. Similarly, the part indicates that we want to capture a parameter called "id". We then register the action for this particular URL, in order to tell the routing engine which URL should be used as the default. Without this last step, it will not know which action to use.

### Restrictions

In the above example, we're using integers for our ids. We can express this in the route:

```csharp
RoutingModuleEx.Engine.Add(
    new PatternRoute("/<controller>/<id>/view.aspx")
        .DefaultForAction().Is("view")
        .Restrict("id").ValidInteger
);
```

We've used the Restrict() method with the "id" parameter name and specified that only valid integers can be used here.

## Named Routes

In previous examples we have accessed routes by supplying controller, action and parameter data to trigger a match with a registered route. There is a shortcut to this: named routes. By registering a route with an identifier, we can refer to it more easily:

```csharp
RoutingModuleEx.Engine.Add("adminhome",
    new PatternRoute("/admin/home/index.aspx")
        .DefaultForAction().Is("view")
);
```

And here's a before and after look at how we refer to this route:

```
$Url.Link('Admin Home', "%{area='admin', controller='home', action='index'}")
    ...
$Url.Link('Admin Home', "%{named='adminhome'}")
```

This is a little more concise and it also enables a URL shorthand for template developers.

## Generating URLs

The default URL building services within Monorail are fully routing aware. Given the rule:

```csharp
RoutingModuleEx.Engine.Add(
    new PatternRoute("/<controller>/<id>/view.aspx")
        .DefaultForAction().Is("view")
);
```

We can use the following NVelocity template code to generate a URL:

```
$Url.Link('Product Name', "%{controller='product', action='list', params={id=15}}")
```

This will produce:

```
/product/15/list.aspx
```

We can add further parameters into the URL:

```
RoutingModuleEx.Engine.Add(
    new PatternRoute("/<controller>/<name>/<id>/view.aspx")
        .DefaultForAction().Is("view")
        .Restrict("id").ValidInteger
);
```

Now template code like this:

```
$Url.Link('Product Name', "%{controller='product', action='view', params={id=15, name='ProductName'}}")
```

This will produce:

```
/product/ProductName/15/list.aspx
```

## Other Integration

Any part of MonoRail which uses the DefaultUrlBulder service will automatically be aware of your routes - so helpers and redirections will work as expected with no adjustments:

```
$Form.FormTag("%{named='login'}") ## generate an opening form tag with the action attribute populated with the URL for the "login" named route
...
RedirectUsingNamedRoute("adminhome") // Redirect to the URL for the named route "adminhome"
```

## Gotchas

There are a couple of scenarios in which solutions aren't immediately clear:

### Reserved Extensions

* On Windows, if the extension is equal to a DOS 1.0 legacy reserved filename (aux, con, com, lpt, etc) it will not match
* If the extension is equal to an ASP.NET forbidden extension (.cs, .java, .asax, etc ) it will not match

## Advanced Features

`ForDomain` and `ForSubDomain` are methods on RoutingEngine which are yet to be implemented but should provide support for per-domain and per-subdomain routes. This may be useful in multi-tenanted applications. AddFirst is available on RoutingEngine and allows a routing rule to be inserted as the first rule to be considered. Due to the way in which ordering of rules is important within the routing system, this can be used to "prioritize" particular rules.

## Unit Testing Routes

The routing engine can be isolated and tested stand-alone by using the RouteMatch class:

```csharp
[Test]
public void CatchAllRouteTest() {
    PatternRoute route = new PatternRoute("/<controller>/<id>/<action>.aspx");

    RouteMatch match = new RouteMatch();

    Assert.IsTrue(route.Matches("/product/123/index.aspx", new RouteContext(new StubRequest("GET"), null, "/", new Hashtable(), match) > 0);
    Assert.AreEqual("product", match.Parameters["controller"]);
    Assert.AreEqual("123", match.Parameters["id"]);
    Assert.AreEqual("index", match.Parameters["action"]);
}
```

## Known Limitations

We are aware of a few situations in which the behavior of the routing engine is non-optimal.

### Dots Could be present in url parts

But a work-around has to be implemented, see [Routing tips](http://using.castleproject.org/display/MR/Routing+tips) for a working solution

## Legacy XML Based Routing

MonoRail's [old routing system was based on XML](legacy-routing.md), but has been deprecated in favor of the new routing engine described above.