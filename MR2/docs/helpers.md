# Helpers

Helpers are associated with a controller and made available to be used on the view. They are usually used to reuse some generation code.

## Built-In Helpers

MonoRail comes packaged with several built in helpers to simplify development; please follow the links below for details on these helpers.

Helper | Description
-------|------------
[FormHelper](formhelper.md) | Generate data bound form elements.
[UrlHelper](urlhelper.md) | Easily build URL's and HTML anchors in your views.
[AjaxHelper](ajaxhelper.md) | Provices AJAX support using the prototype jslib.
[PaginationHelper](paginationhelper.md) | Simplifies the creation of paginated navigation on items.
WizardHelper | Used in combination with MonoRail's Wizard support to create dynamic wizard navigation.
TextHelper | Provides methods for working with strings and grammar.
Effects2Helper | Exposes `script.aculo.us` script features.
DateFormatHelper | Formats `DateTime` instances.

## Creating Custom Helpers

A helper is just an ordinary class. It might optionally extend `AbstractHelper` in order to have access to the controller instance and some utility methods. For example:

```csharp
public class MyHelper
{
    public string BuildUserLink(User user)
    {
        return string.Format("<a href='/users/showuser.rails?id={0}'>{1}</a>",
            user.Id, user.Name);
    }
}
```

The helper must be associated with the controller whose views might use it. This is done using the `HelperAttribute`:

```csharp
using Castle.MonoRail.Framework;

[Helper(typeof(MyHelper))]
public class MemberController : Controller
{
    public void List()
    {
        PropertyBag.Add("users", ObtainUsers());
    }
}
```

Now it is just a matter of using the helper by its name:

```
#foreach($user in $users)
  $MyHelper.BuildUserLink(${user})
#end
```