# Resources and Localization

You can associate one or more assembly resources with a controller. The resource entries will be available to your view as a dictionary. This is a good way to externalize static content.

In addition you can also use the `LocalizationFilter` to set up the `CurrentCulture` and `CurrentUICulture` on the thread. This makes .NET select the correct resources to load according to the language set. It also defines how it should format numbers and dates.

## Using Resources

Resources can be associate with a controller using the `ResourceAttribute`. For example:

```csharp
using Castle.MonoRail.Framework;

[Resource("text", "LocalizationSample.Resources.Home")]
public class HomeController : SmartDispatcherController
{
    ...
}
```

The first parameter defines the key that you can use from your view. The second is the resource full name.

**Visual Studio Bug:** There is a bug in Visual Studio that causes changes on the resource files to no be detected. If this happens, force a rebuild.

The entries can be accessed from the view as a regular dictionary:

```html
<h2> $text.welcome </h2>

<p>
$text.intro
</p>
```

You can optionally set up the `CultureName` and the `AssemblyName` on the `ResourceAttribute`.

## Setting Up the Current Culture

The `LocalizationFilter` allows you to define a strategy to extract the language code and sets up the `CurrentCulture` and `CurrentUICulture` for the request thread.

For example, it can extract the locale from the headers the browser sends, and allows overriding the locale using a cookie entry, or an entry in the session. For example:

```csharp
using Castle.MonoRail.Framework;

[LocalizationFilter(RequestStore.Cookie, "locale")]
public class HomeController : SmartDispatcherController
{
    ...
}
```

The usage above defines that it should look for a cookie named `locale` and in case it cannot be found, fallback to the browser locale.

## Localization

Localization support is provided as a combination of both approaches:

```csharp
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Filters;

[Resource("text", "LocalizationSample.Resources.Home")]
[LocalizationFilter(RequestStore.Cookie, "locale")]
public class HomeController : SmartDispatcherController
{
	public void Index()
	{
	}

	public void SetLanguage(String langCode)
	{
		Response.CreateCookie("locale", langCode);

		RedirectToAction("index");
	}
}
```

It is up to .NET to load the correct resource or fallback to the default language if the resource for the specified locale cannot be found.

The `SetLanguage` action is an example of how to override the locale.