# Rescues

A rescue is an association of a special view that will only be rendered if an exception happens. The view file must be present in a `rescues` folder directly under your `views` folder.

## Using Rescues

A rescue can be associated with a controller or per action. You can also bound a rescue with an exception. If the action throws an exception (the action cannot swallow the exception), MonoRail will match the rescue definition that is closely related to the exception type and use the specified view.

To create an association you must use the `RescueAttribute`. For example:

```csharp
using Castle.MonoRail.Framework;

[Rescue("dberror", typeof(System.Data.SqlException))]
public class ProductController : Controller
{
    [Rescue("commonerror")]
    public void Index()
    {
        throw new System.Data.SqlException("fake error");
    }

    [Rescue("dumbprogrammer", typeof(DivideByZeroException))]
    public void List()
    {
        int val = 0;
        int x = 10 / val;
    }

    public void Search()
    {
    }
}
```

The usage of the `RescueAttribute` in the example above defines the following rules:

* If any action throws a `SqlException`, the view `view/rescues/dberror` will be selected
* If the action `Index` throws any kind of exception (including `SqlException`), the view `view/rescues/commonerror` will be selected. This overrides the definition in the controller level.
* If the action `List` throws a `DivideByZeroException`, the view `view/rescues/dumbprogrammer` will be selected.

Whenever an exception happens, the MonoRail context (which is per request) will populate the property `LastException` so your view can show the exception details.