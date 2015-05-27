# Filters

Filters are executed before and/or after your actions. It is useful for security, dynamic content and to keep away repetitive code.

## Head branch version information

* it is not currently possible to define a filter in an opt-in way by tagging a method directly, it has to be done on the controller
* it is not documented whether or not the filter could work on a DynamicAction (via reflection/attributes, or a filter provider specific to dynamic actions?
* the opt-in / opt-out supported way should be summarized

## Creating a Filter

To create a filter, create a class that implements the `IFilter` interface, then associate the filter with your controller.

**Filtered controllers:** You can always create an abstract controller class and associate a filter with it and make your controllers extend it.

```csharp
using Castle.MonoRail.Framework;

public class AuthenticationFilter : IFilter
{
    public bool Perform(ExecuteWhen exec, IRailsEngineContext context, Controller controller)
    {
        if (context.Session.Contains("user"))
        {
            return true;
        }
        else
        {
            context.Response.Redirect("account", "login");
        }

        return false;
    }
}
```

The `Perform` return value indicates to the framework if the process should be ended. If you return `false` no further process will happen for the current request. It is important that you take some action before, like in the example above, issuing a redirect.

The `ExecuteWhen` parameter informs the filter about the context of the invocation. It is also used on the `FilterAttribute` to define when you want to have the filter executed. The possible values are listed in the table below:

`ExecuteWhen` fields | Description
---------------------|------------
BeforeAction | The filter is invoked before the action.
AfterAction | The filter is invoked after the action.
AfterRendering | The filter is invoked after the rendering.
Always | The filter is invoked around all steps.

To associate the filter with the controller, use the `FilterAttribute`:

```csharp
using Castle.MonoRail.Framework;

[FilterAttribute(ExecuteWhen.BeforeAction, typeof(AuthenticationFilter))]
public class AdminController : Controller
{
    public void Index()
    {
    }
}
```

## Ordering Filters

You can always associate more than one filter with a controller. However the order of execution cannot be guaranteed. If the order of execution is important, use the `ExecutionOrder` property. The lower the value, the higher is the priority. For example:

```csharp
using Castle.MonoRail.Framework;

[FilterAttribute(ExecuteWhen.BeforeAction, typeof(AuthenticationFilter), ExecutionOrder=0)]
[FilterAttribute(ExecuteWhen.BeforeAction, typeof(LocalizationFilter), ExecutionOrder=1)]
public class AdminController : Controller
{
    public void Index()
    {
    }
}
```

## Skipping Filters

For some situations you may not want to execute a filter, or all filters, for one or more actions. Use the `SkipFilterAttribute` for those cases. For example:

```csharp
using Castle.MonoRail.Framework;

[FilterAttribute(ExecuteWhen.BeforeAction, typeof(AuthenticationFilter), ExecutionOrder=0)]
[FilterAttribute(ExecuteWhen.BeforeAction, typeof(LocalizationFilter), ExecutionOrder=1)]
public class AdminController : Controller
{
    [SkipFilter]
    public void Index()
    {
    }

    [SkipFilter(typeof(LocalizationFilter))]
    public void Create()
    {
    }

    public void Update()
    {
    }
}
```

For the example above we have defined that:

* No filters will be executed on the `Index` action
* The `LocalizationFilter` will not be executed on the `Create` action
* All filters will run on the `Update` action

## Passing Parameters to Filters

More advanced scenarios might arise where you parameterize a filter. For example, you can create a filter that is able to load text files and add each line of text to the `PropertyBag`. The file name is not fixed.

The first thing to do is to create a new attribute that extends `FilterAttribute`:

```csharp
using Castle.MonoRail.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true), Serializable]
public class MyCoolFilterAttribute : FilterAttribute
{
    private readonly string fileName;

    public MyCoolFilterAttribute(String fileName) : base(ExecuteWhen.BeforeAction, typeof(CoolFilterImpl))
    {
        this.fileName = fileName;
    }

    public string FileName
    {
        get { return fileName; }
    }
}
```

As you can see, the custom attribute inherits from `FilterAttribute` and configures the filter on the user's behalf.

Now we just need to implement the filter itself. We also need to signalize to the framework that we are interested in gaining access to the attribute instance as we will extract information from it. This is done using the `IFilterAttributeAware` interface.

```csharp
using Castle.MonoRail.Framework;

public class CoolFilterImpl : IFilter, IFilterAttributeAware
{
    private MyCoolFilterAttribute attribute;

    // Implementation of IFilterAttributeAware
    public FilterAttribute Filter
    {
        set { attribute = (MyCoolFilterAttribute)value; }
    }

    // Implementation of IFilter
    public bool Perform(ExecuteWhen exec, IRailsEngineContext context, Controller controller)
    {
        // Now you can access the parameters:
        string fileName = attribute.FileName;

        // Work

        // Allow the process to go on
        return true;
    }
}
```

Now using the filter is very simple:

```csharp
using Castle.MonoRail.Framework;

[MyCoolFilterAttribute("customer_messages.txt")]
public class CustomerController : Controller
{
    public void Index()
    {
    }
}
```