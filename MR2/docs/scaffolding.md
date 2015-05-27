# Scaffolding

Scaffolding is a idea borrowed from [Ruby on Rails](http://wiki.rubyonrails.org/rails/pages/Scaffold). It refers to the ability to create pages with a simple interface to data in a database with very little effort.

As every project under the Castle Project umbrella does not obligate you to embrace it all, scaffolding is implemented with the `IScaffoldingSupport` interface.

MonoRail will instantiate the implementation specified if it discovers a controller with one or more `ScaffoldingAttribute`:

```csharp
[Scaffolding(typeof(Blog))]
public class BlogsController : Controller
{
    public BlogsController()
    {
    }
}
```

The scaffolding implementor should register actions as a dynamic action provider.

The default implementation of scaffolding support relies on Castle ActiveRecord and is discussed in the [ActiveRecord Integration](activerecord-integration.md) section.