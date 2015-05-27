# Layouts

Layouts allow you to template your site by specifying common html and controls, such as structural html and navigation controls, in one place that are available for any view to use.

Layouts are just standard views, but they need to be created in a folder named `layouts`, notice that it is plural. The `layouts` folder needs to be directly under your `views` root directory.

**Same extension:** Note that the extension for the layout files need to match whatever view engine you are using, such as `.aspx`. ASP.NET users that are tempted to use the master page model and use `.master` will be sadly disappointed with a "resource cannot be found" error.

## Using Layouts

You can associate a layout with a controller or with an action using the `LayoutAttribute`. For example:

```csharp
using Castle.MonoRail.Framework;

[Layout("application")]
public class CustomerController : Controller
{
    public void Index()
    {
    }
}
```

In some scenarios you might want to turn off the layout processing. To do so use the `CancelLayout` method. There are other cases where you want to render a specific view and turn off layout at the same time. The `RenderView` and `RenderSharedView` have overloads to allow you to do that.

* `RenderView(string name, bool skipLayout)`
* `RenderView(string controller, string name, bool skipLayout)`
* `RenderSharedView(string name, bool skipLayout)`

For example:

```csharp
using Castle.MonoRail.Framework;

[Layout("application")]
public class CustomerController : Controller
{
    public void Index()
    {
        RenderView("welcome", true);
    }
}
```

## Nested Layouts

MonoRail also supports nested layouts; when providing the layout name in the controller you can provide comma separated list of layouts to be applied, from the most general (outer) inward.  Each deeper nested layout will be rendered in the content area of the parent layout.

```csharp
[Layout("Site", "Admin")]
```

In this example the "Admin" layout will be rendered in the content section of the "Site" layout.

## Layouts and Views

Each view engine uses its own specific approach to using layouts. Information on how to use layouts in your view can be found in the documentation for the [view engine](view-engines.md) you have elected to use.