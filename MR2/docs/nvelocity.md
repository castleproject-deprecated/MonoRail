# NVelocity

Pros:

* Limited set of functions forces you to code only view logic (good for separation of concerns)
* Easy to learn template language
* Same syntax as Velocity (for Java), allowing view reuse among different platforms
* Reuse skills for people with Java experience

Cons:

* Interpreted
* Community seem very inactive. That forced Castle Project to fork NVelocity and work on improvements and bug fixes.

The `NVelocity View` Engine uses `NVelocity`.

NVelocity is a port of the excellent Apache Jakarta Velocity project. It is a very simple, easy to learn and extensible template engine. Due to the lack of releases, support and bug fixes on the original port, the Castle Team decided to fork the project, bringing it to our code repository, fixing the bugs and improving it with more features.

The first thing you need to read about NVelocity is not even on this web site. You can find on the original Velocity web site:

* [The user guide](http://velocity.apache.org/engine/devel/user-guide.html)

Frequently Asked Questions on NVelocity View Engine can be found on the [standard MonoRail FAQ](http://www.castleproject.org/monorail/faq.html).

To use NVelocity View Engine inform the type on the `customEngine` on the configuration file:

```xml
<viewEngine
	viewPathRoot="views"
	customEngine="Castle.MonoRail.Framework.Views.NVelocity.NVelocityViewEngine, Castle.MonoRail.Framework.Views.NVelocity" />
```

## NVelocity files

NVelocity uses the extension `.vm` so just create your views with that extension. Remember that from your controller you should not reference file extensions when defining views to render.

## Layouts

Use the `$childContent` context variable to render the content of the view on the layout template. The following is a simple layout using NVelocity:

```html
<html>

Welcome

$childContent

Footer

</html>
```

**Order of execution:** The view template selected by the controller is executed before the layout template. In fact the layout template is merged with the result of the view template execution.

## Configuration

The NVelocity View Engine looks for a file `nvelocity.properties` in the root of the view folder. You can use this file to configure how NVelocity should behave.

For example, to configure NVelocity to support Chinese encoding create a text file named `nvelocity.properties`, save it to your views folder and add the following content:

```
input.encoding=GB2312
output.encoding=GB2312
```

More information on the entries can be found on the original Velocity documentation.

## Macros

NVelocity supports macros, but keep in mind that they have problems. If you want to use macros you can create a folder `macros` under your views root folder.

All `.vm` files in this folder will be loaded as a NVelocity Macro library so the macros will be available to all templates.

## Fancy foreach Loops

Inspired on [FrogCreek's fancy loops](http://www.fogcreek.com/CityDesk/2.0/help/Scripting_With_CityScript/FancyLoops.html). The following code should be self-explanatory:

```
#foreach($i in $items)
#each (this is optional since it's the default section)
       text which appears for each item
#before
       text which appears before each item
#after
       text which appears after each item
#between
       text which appears between each two items
#odd
       text which appears for every other item, including the first
#even
       text which appears for every other item, starting with the second
#nodata
       Content rendered if $items evaluated to null or empty
#beforeall
       text which appears before the loop, only if there are items
       matching condition
#afterall
       text which appears after the loop, only of there are items
       matching condition
#end
```

All sections are optional, and they can appear in any order multiple times (sections with same name will have their content appended). So for example you can use it to create table contents with alternating styles:

```
#foreach($person in $people)
#beforeall
       <table>
               <tr>
               <th>Name</th>
               <th>Age</th>
               </tr>
#before
       <tr
#odd
       Style='color:gray'>
#even
       Style='color:white'>

#each
       <td>$person.Name</td>
       <td>$person.Age</td>

#after
       </tr>

#between
       <tr><td colspan='2'>$person.bio</td></tr>

#afterall
       </table>

#nodata
       Sorry No Person Found
#end
```

Which will output something like:

```html
<table>
       <tr>
       <th>Name</th>
       <th>Age</th>
       </tr>
       <tr style='color:white'>
               <td>John</td>
               <td>32</td>
       </tr>
       <tr><td colspan='2'>Monorail programmer</td></tr>
       <tr style='color:gray'>
               <td>Jin</td>
               <td>12</td>
       </tr>
       <tr><td colspan='2'>Castle guru</td></tr>
</table>
```

If the `$people` variable was `null` the output will be:

```
Sorry No Person Found
```

## NVelocityViewEngine Variables

The NVelocityViewEngine is responsible for making "useful" variables available to your view. Here's the list of variables added to the context by the NVelocityViewEngine:

Context Variable | Description
-----------------|------------
$controller | The controller being executed.
$context | The IRailsEngineContext.
$request | context.Request
$response | context.Response
$session | context.Session
$childContent | Used inside Layouts. It defines the content rendered by a View.
$page | Available in `*.njs` views and is added in the `GenerateJS` method.
$siteroot | `context.ApplicationPath`

Additionally - the contents of the following collections are merged into the context:

* `controller.Resources`
* `context.Params`
* `controller.Helpers`
* `context.Flash`
* `controller.PropertyBag`

Each key in each of the collections becomes a `$variable`. For example:

```
class MyController
{
	public void Index()
	{
	   PropertyBag["myvariable"] = "some value";
	   Context.Params["othervariable"] = "some other value value";
	   Context.Flash["anothervariable"] = "yet one more";
	}
}
```

In your view you will have the following variables:

* `$myvariable`
* `$othervariable`
* `$anothervariable`

Helpers are also added to allow you to invoke static members on some common types:

* `$Byte`
* `$SByte`
* `$Int16`
* `$Int32`
* `$Int64`
* `$UInt16`
* `$UInt32`
* `$UInt64`
* `$Single`
* `$Double`
* `$Boolean`
* `$Char`
* `$Decimal`
* `$String`
* `$Guid`
* `$DateTime`

This allows you to do useful things like:

```
The Current time is: $DateTime.Now
```

## Accessing the `PropertyBag`

If you want to list the variables in the property bag - then add a reference to the `PropertyBag`:

```csharp
class MyController
{
	public void Index()
	{
		PropertyBag["PropertyBag"] = PropertyBag;
	}
}
```

then in your view `$PropertyBag` is what you want:

```
Property bag variables
#foreach($key in $PropertyBag)
#beforeall

	Name
	value

#each

	$key
	$PropertyBag.get_Item($key)

#afterall

#end
```

## ViewComponent Support

NVelocity allows you to create your own directives, so that's how we introduced components to it. Basically you can use:

* For inline components:

```
#component(ComponentName)
```

* For components with body content (aka block components):

```
#blockcomponent(ComponentName)
  some content
#end
```

ViewComponents have access to the `IRailsContext` so you can access form parameters, session, etc. Sometimes however it's important to specify some parameters.

### Passing Parameters in a Dictionary

Use the name of the component followed by a dictionary string.

```
#component(MyFirstComponent "%{firstParam='some value',anotherParam='other value'}")
```

You can then access the parameters from the component code:

```csharp
public class MyFirstComponent: ViewComponent
{
	public override void Render()
	{
		object param1 = Context.ComponentParameters["firstParam"];
		object param2 = Context.ComponentParameters["anotherParam"];

		...
	}
}
```

### Key/Value Pairs

In this case you need to use the keyword `with` followed by a sequence of key/value pairs:

```
#component(ComponentName with "name=john" "address=some address")
```

You're free to use interpolation as well

```
#component(ComponentName with "name=${customer.name}")
```

You can gain access to the component parameters using the Context.ComponentParameters too.

### Data Type Handling

Every data type is supported. However literal values will be automatically converted to text. If you want to specify a different type, create a varible on NVelocity or use some structure data available on the view.

The parameter value below will be converted to string

```
#component(ComponentName with "age=1")
```

The parameter value below will remain an Int32

```
#set($age = 27)
#component(ComponentName with "age=${age}")
```

### A Simple Example

The view snippet:

```
#blockcomponent(SecurityComponent with "role=admin")
  this will only be rendered if the current user is in the specified role
#end
```

The component code:

```csharp
using Castle.MonoRail.Framework;

namespace WebApp
{
	public class SecurityComponent : ViewComponent
	{
		bool shouldRender;

		public override void Initialize()
		{
			String role = ComponentParameter["role"] as String;
			shouldRender = RailsContext.User.IsInRole( role );
 		}

		public override void Render()
		{
			if (shouldRender) Context.RenderBody();
 		}
	}
}
```