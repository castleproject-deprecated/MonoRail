# Brail

Pros:

* Use the wrist friendly and feature-rich [Boo language](http://boo.codehaus.org/) for templates.
* Compiled (good performance!)

Cons:

* Requires additional assemblies
* Python syntax (which one can consider as a pro)

Brail is a view engine for MonoRail which allows you to use the same framework and Boo language on both ends of the application. You write the business logic using Boo (or any other .NET language), and then you write the views in Boo. No need for a mental switch or learn another templating language.

## Getting Started

Brail includes many changes to the way that you normally write code in Boo. Brail views are scripts files that ends with "`*.brail`" or "`*.brailjs`". The most significant change is that Brail does not use whitespace to control blocks. This mean that the following statement in Boo:

```
if someCondition:
   DoAction()
```

Will look like this in Brail:

```
if someCondition:
   DoAction()
end
```

Blocks are controlled starting with a colon and ending by "end". While this is probably the most significant change from Boo, there are many other things that Brail does for you so you would get a scripting experience while working in a compiled language. Read on and discover what makes Brail so special.

### Assemblies

First of all you must reference the following assemblies. Copy them to the bin folder if you are not using Visual Studio.

* `Castle.MonoRail.Views.Brail.dll`
* `Boo.Lang.dll`
* `Boo.Lang.Compiler.dll`
* `Boo.Lang.Extensions.dll`
* `Boo.Lang.Parser.dll`
* `anrControls.Markdown.dll`

The `Boo.*` files are required for the language support. The `anrControl.Markdown.dll` is required for support [Markdown](http://daringfireball.net/projects/markdown/) formatting.

### MonoRail ViewEngine Configuration

Edit your `web.config` file to include the following line (in the monoRail config section, of course):

```xml
<viewEngine
	viewPathRoot="Views"
	customEngine="Castle.MonoRail.Views.Brail.BooViewEngine, Castle.MonoRail.Views.Brail" />
```

And that is it, you are now capable of using Boo scripts to write views in MonoRail! Now that you can use it, let's see what you can do with it.

Before we get to how to use it, I must send huge thanks to the Boo Community, for being so helpful.

If your view directory is in the web directory, it's wise to not allow the files to be read through HTTP. So under the `system.web/httpHandlers` configuration you should add the following:

```xml
<add verb="*" path="*.brail" type="System.Web.HttpForbiddenHandler"/>
<add verb="*" path="*.brailjs" type="System.Web.HttpForbiddenHandler"/>
```

### Using It

First, Brail scripts are not normal Boo programs, they require that you have at least one statement at the global namespace, which is what MonoRail will end up calling when your view is invoked. Assuming that you already have a controller, and that you've setup your environment correctly, you can simply do this:

```
Hello, World!
```

Brail will take this script, compile it and send its output to your browser. The nice thing about it is that if you would change it to say:

```
Hi, World!
```

You will instantly get a the updated view, this is highly important to development scenario.

**Developer's feature:** This is a developer feature, meant to ease development. Using it on a production machine would cause recompiling of the scripts and cause an assembly leak until the application domain is restarted. On a developer machine, this should rarely be a problem, on a production machine, if frequent changes are made to the scripts, this can cause problems.

You can also use parameters that the controller has put in the `PropertyBag` or `Flash`, like this:

```
Hi, ${User}!
```

If you are wondering how does it work behind the scenes: the script is loaded and compiled. There is some magic there that sends anything not wrapped in `<% %>` directly to the user (which will also expand `${arg}` expression to their values). The compiled code is cached, so you pay the compilation cost only once.

```html
<html>
	<head>
		<title>${title}</title>
	</head>
	<body>
	     <p>The following items are in the list:</p>
	     <ul><%for element in list:	output "<li>${element}</li>"%></ul>
	     <p>I hope that you would like Brail</p>
	</body>
</html>
```

The output of this program (assuming list is (1,2,3) and title is "Demo" ) would be:

```html
<html>
	<head>
		<title>Demo</title>
	</head>
	<body>
     <p>The following items are in the list:</p>
     <ul><li>1</li><li>2</li><li>3</li></ul>
	</body>
</html>
```

And the rendered HTML will look like this:

```
----
The following items are in the list:
- 1
- 2
- 3
-----
```

## Principal of Least Surprise

On general, since NVelocity is the older view engine for now, I have tried to copy as much behavior as possible from NVelocityViewEngine. If you've a question about how Brail works, you can usually rely on the NVelocity behavior. If you find something different, that is probably a bug, so tell us about it.

## Configuration

The default configuration should suffice for most cases, but if you want to change the configuration, you can do so by adding a configuration section handler to the web.config file:

```xml
<configSections>
  <section name="Brail" type="Brail.BrailConfigurationSection, Brail" />
</configSections>
```

Here is the default configuration for Brail:

```xml
<Brail
	debug="false"
	saveToDisk="false"
	saveDirectory="Brail_Generated_Code"
	batch="true"
	commonScriptsDirectory="CommonScripts">

	<reference assembly="My.Assembly.Name"/>
	<import  namespace="My.Assembly.Name"/>
</Brail>
```

Option | Description | Default value | Possible values
-------|-------------|---------------|----------------
debug | Generate debug or retail code | false | true - generate debug code<br/>false - generate retial code
saveToDisk | Save the generated assemblies to disk - useful if you want to know what is going on behind the scenes. | false | true - save assemblies to disk<br/>false - use entirely in memory
saveDirectory | The directory to save the generated assemblies to.<br/>The path can be relative or absolute, if relative, the default ASP.Net bin path will be used.<br/>If the directory does not exist, it will be created. | "Brail_Generated_Code" | Any valid path
batch | Batch compilation, compile all the scripts in one folder to a single assembly.<br/>This does not work recursively. | true | true - all scripts in a folder will be compiled to a single assembly<br/>false - each script will be compiled to its own assembly
commonScriptsDirectory | The directory where all the common scripts are. This can be a relative or absolute path, if relative, the Views directory of the application will be used as the base.<br/>If the directory does not exist, it will not be created. | "CommonScripts" | Any valid path
reference element, assembly attribute | This tells Brails that all your scripts should reference the specified assembly or assemblies. This allows strong typing in the views and avoids the cost of reflection. | none | The assembly attribute must contain a valid assembly name that is reachable to the application by using `Assembly.Load()`. Usually this means that it's located in the bin directory of the application.
import element, namespace attribute | This tells Brails that all your scripts should import the specified namespace or namespaces. This allows shorter naming in the script. | none | Any valid namespace

## Code Separators

Brail supports two code separators `<% %>` and `<?brail ?>`, I find that `<% %>` is usually easier to type, but `<?brail ?>` allows you to have valid XML in the views, which is important for some use cases. Anything outside a `<?brail ?>` or `<% %>` is sent to the output. `${user.Id}` can be used for string interpolation.

:warning: **Choose one:** The code separator types cannot be mixed. Only one type of separators must be used per file.

## Output Methods

Since most of the time you will want to slice and dice text to serve the client, you need some special tools to aid you in doing this. Output methods are methods that are decorated by `[Html]` / `[Raw]` / `[MarkDown]` attributes. An output method return value is transformed according to the specified attribute that has been set on it, for instance, consider the `[Html]` attribute:

```
<%
[Html]
def HtmlMethod():
	return "Some text that will be <html> encoded"
end
%>
${HtmlMethod()}
```

The output of the above script would be:

```
Some text that will be <html> encoded
```

The output of a method with `[Raw]` attribute is the same as it would've without it (it's supplied as a `NullObject` for the common case) but the output of the `MarkDown` attribute is pretty interesting. Here is the code:

```
<%
[MarkDown]
def MarkDownOutput():
	return "[Ayende Rahien](http://www.ayende.com/), __Rahien__."
end
%>
${MarkDownOutput()}
```

And here is the output:

```
<p><a href="http://www.ayende.com/">Ayende Rahien</a>, <strong>Rahien</strong>.</p>
```

Markdown is very interesting and I suggest you read about its usage.

## HTML Encoding

Brail also supports HTML encoding as part of its syntax to protect against HTML-injection attacks. Instead of using the $ (dollar) notation the exclamation mark is used to reference items in the property bag or flash like so:

```
!{model.Variable}
```

This should be used anywhere where a user is able to modify the underlying data which isn't being intercepted and parsed before being sent to the server.

## Using Variables

A controller can send the view variables, and the Boo script can reference them very easily:

```
My name is ${name}
<ul>
<%
for element in list:
    output "<li>${element}</li>"
end
%>
</ul>
```

Brail has all the normal control statements of Boo , which allows for very easy way to handle such things as:

```
<% output AdminMenu(user) if user.IsAdministrator %>
```

This will send the menu to the user only if he is administrator.

One thing to note about this is that we are taking the variable name and trying to find a matching variable in the property bag that the controller has passed. If the variable does not exist, this will cause an error, so pay attention to that. You can test that a variable exists by calling the `IsDefined()` method.

```
<%
if IsDefined("error"):
	output error
end
%>
```

Or, using the much clearer syntax of "?variable" name:

```
<%
output ?error
%>
```

The syntax of "`?variable`" name will return an `IgnoreNull` proxy, which can be safely used for `null` propagation, like this:

```
<%
# will output an empty string, and not throw a null reference exception
output ?error.Notes.Count
%>
```

This feature can make it easier to work with optional parameters, and possible null properties. Do note that it will work only if you get the parameter from the property bag using the "`?variableName`" syntax. You can also use this using string interpolation, like this:

* Simple string interpolation: `${?error}`
* And a more complex example: `${?error.Notes.Count}`

In both cases, if the error variable does not exists, nothing will be written to the output.

## Sub Views

There are many reasons that you may want to use a sub view in your views and there are several ways to do that in Brail. The first one is to simply use the common functionality. This gives a good solution in most cases (see below for a more detailed discussion of common scripts).

The other ways is to use a true sub view, in Brail, you do that using the `OutputSubView()` method:

```
Some html:
<?brail OutputSubView("/home/menu")?>
<br/>some more html
```

You need to pay attention to two things here:

The rules for finding the sub view are as followed:

* If the sub view start with a '/' : then the sub view is found using the same algorithm you use for `RenderView()`
* If the sub view doesn't start with a '/' : the sub view is searched starting from the current script directory.

A sub view inherits all the properties from its parent view, so you have access to anything that you want there.

You can also call a sub view with parameters, like you would call a function, you do it like this:

```
<?brail OutputSubView("/home/menu", { "var": value, "second_var": another_value } ) ?>
```

Pay attention to the brackets, what we have here is a dictionary that is passed to the `/home/menu` view. From the sub view itself, you can just access the variables normally. This variables, however, are not inherited from views to sub views.

## Importing Content From Files

Occasionally a need will arise to include a file "as-is" in the output, this may be a script file, or a common html code, and the point is not to interpret it, but simply send it to the user. In order to do that, you simply need to do this:

```
${System.IO.File.OpenText("some-file.js").ReadToEnd()}
```

Of course, this is quite a bit to write, so you can just put an import at the top of the file and then call the method without the namespace:

```
<%
import System.IO
%>
${File.OpenText("some-file.js").ReadToEnd()}
```

## Principle of Least Surprise

On general, since NVelocity is the older view engine for now, I have tried to copy as much behavior as possible from NVelocityViewEngine. If you've a question about how Brail works, you can usually rely on the NVelocity behavior. If you find something different, that is probably a bug, so tell us about it.

## Common Scripts

In many cases, you'll have common functionality that you'll want to share among all views. Just drop the file in the `CommonScripts` directory - (most often, this means that you will drop your common functionality to `Views\CommonScripts`) - and they will be accessible to any script under the site.

The language used to write the common scripts is the white space agnostic derivative of Boo, not the normal one. This is done so you wouldn't have white spacing sensitivity in one place and not in the other.

The common scripts are normal Boo scripts and get none of the special treatment that the view scripts gets. An error in compiling one of the common scripts would cause the entire application to stop.

Here is an example of a script that sits on the `CommonScripts` and how to access it from a view:

`Views\CommonScripts\SayHello.boo` - The common script

```
def SayHello(name as string):
	return "Hello, ${name}"
end
```

`Views\Home\HelloFromCommon.boo` - The view using the common functionality

```
<%
output SayHello("Ayende")
%>
```

The output from the script:

```
Hello, Ayende
```

## Symbols and Dictionaries

Quite often, you need to pass a string to a method, and it can get quite cumbersome to understand when you have several such parameters. Brail supports the notion of symbols, which allows to use an identifier when you need to pass a string. A symbol is recognized by a preceding '`@`' character, so you can use this syntax:

```
<%
output SayHello( @Ayende )
%>
```

And it will work exactly as if you called `SayHello( "Ayende" )`. The difference is more noticable when you take into account methods that take components or dictionary parameters, such as this example:

```
<%
component Grid, {@source: users, @columns: [@Id, @Name] }
%>
```

Using a symbol allows a much nicer syntax than using the string alternative:

```
<%
component Grid, {"source: users, "columns": ["Id", "Name"] }
%>
```

## Layouts

Using layouts is very easy, it is just a normal script that outputs `ChildOutput` property somewhere, here is an example:

```
Header
${ChildOutput}
Footer
```

## Performance

If you want to use Brail for 10 million transactions a day, I would suggest measuring first, but in general, it should be good for most of what you throw at it.

Batch compilation should reduce compile time and memory size. The code is not interpreted, it's statically compiled (very similar to how ASP.NET does it) and run whenever a request comes in. Currently there is no further reason to complicate the code until someone actually needs it. The second time that a request comes in for a page, it's already compiled and can immediately serve the request.

A change in a single file will cause a separate assembly to be loaded, and all future requests will go the the new assembly immediately. Be aware that a large number of changes in an application will cause an assembly leak, since the assemblies cannot be unloaded until the AppDomain is unloaded. This isn't a problem in production scenarios, and on a development machine, the usual IIS application resets should take care of it.

If you think that reflection kills your performance, make sure to reference your relevant assemblies and use casting to the appropriate types when applicable. Another option would be to improve dynamic dispatch, but that would wait until there is a true need for it.

### Referencing Assemblies

Consider the following code:

```
<%
for user in users:
	output "<p>${user.Name} - ${user.Email}</p>"
end
%>
```

Looks simple, right? The problem is that Brail doesn't really know what `user` is, so it uses reflection to get the values of the `Name` and `Email` properties. This is, of course, quite expensive in performance terms. What is the solution? You need to tell Brail to add a reference to the assembly where User is defined. You can do that by adding this line to your `web.config` file (see the full configuration section below for more details).

```xml
<Brail>
     <reference assembly="assembly.with.user.object" />
</Brail>
```

And then, in your view code, you write:

```
<%
import My.Models
%>
<!-- lots of html code ->
<%
for user as User in users:
	output "<p>${user.Name} - ${user.Email}</p>"
end
%>
```

You can also use this out side of loops, in order to get strong typing (and the associated performance benefits):

```
<%
import My.Models
%>
<!-- lots of html code ->
<%
# define a parameter called user of type User
user as User = GetParameter("user")
# now it uses strong typing, instead of reflection
output "<p>${user.Name} - ${user.Email}</p>"
%>
```

With this simple change, you've completely eliminated the use of reflection and probably increased by a fair margin your application performance. However, because it significantly increases the complexity of developing the views, it is not really the recommended approach. If you run into a situation where the cost of reflection in the views is a significant one, there are other options, which involve improving dynamic dispatch inside of Brail, bringing you the benefits without the cost. This is not implemented currently, because so far we have not run into a situation where this was a problem that warranted the additional complexity.

:warning: **Side-effects:** Beyond making the view code more complex, this can affect the ability to use some of the nicer abilities on Brail, such as ignoring null references using the `IgnoreNull` proxy.

### Auto Imports

As you can imagine, it can get tiresome to specify the default imports all over the place, Brail supports automatic imports from the configuration. All you need to do is specify the following in the `web.config` file:

```xml
<Brail>
     	<import namespace="My.Models"/>
</Brail>
```

And it will be added for you by Brail.

## ViewComponent Support

Brail supports the following syntax for ViewComponents:

```
<%
component MyViewComponent
%>
```

The above will call `MyViewComponent` and send any output from the component to the browser.

You can also use view components with arguments. Those arguments are passed via a dictionary (Hash table), like this:

```
<%
component MyViewComponentWithParams, {"arg" : "value" }
%>
```

If you want to pass a body to the component, just use the normal colon + indent to do so:

```
<% component MyViewComponentWithBody: %>
html content that will be sent to the user if the component will call the RenderBody() method
<% end %>
```

The contents of a component is evaluated when you call `RenderBody`, so if you will call `RenderBody` multiple times, you will send the output of the component's body multiple times as well.

### Sections

You can also use sections in Brail. Sections are what a way to pass templates to the component in a fine grained manner. Here is a simple example:

```
<%
component Grid:
	section Header:
	%>
		<th>Id</th>
		<th>Name</th>
	<%
	end
	section Item:
	%>
		<td>${item.Id}</td>
		<td>${item.Name}</td>
	<%
	end
end %>
```

## Troubleshooting

Brail will throw an exception for any compilation errors, which will include the reason for the error as well as the transformed source code that caused the error, you can use that in order to find out what went wrong.

One thing to be aware of with batch compilation is that if one of your scripts has an error, it will cause the entire batch to fail. Each script in the directory will first try the batch option, and when that fails, it will compile itself as a stand-alone assembly. This can be bad for performance if there are a lot of scripts in a directory. However, a second request for such a script would be served from memory, so it's not too bad.

While it should be possible to debug the views scripts (add `System.Diagnostics.Debugger.Break()` instead of a breakpoint), I don't recommend it. There is a quite a bit of compiler magic behind Boo as it is, and Brail does its fair share as well. It's likely that you won't have a good experience.

## How Brail Works

First of all let's understand where Brail lives. Brail is a View Engine for the Castle MonoRail web development framework. MonoRail is MVC framework for ASP.Net that allows true Separation of Concerns between your business logic and your UI code.

Brail comes into play when it's time to write your UI code, the idea is that instead of using a templating framework, like NVelocity or StringTemplate, you can use a bona fide programming language with all the benefits that this implies. The down side of this approach is that programming languages usually have very strict rules about how you can write code and that is usually the exact opposite of what you want when you write a web page. You want a language that wouldn't get in your way. This is where Brail comes into play.

Brail is based on Boo, a programming language for the .Net Framework which has a very unorthodox view of the place of the user in the language design. Boo allows you to plug your own steps into the compiler pipeline, rip out whatever you don't like, put in things that you want, etc. This means that it packs a pretty punch when you need it. The options are nearly limitless. But enough raving about Boo, it is Brail that you are interested in. What Brail does is to allow you to write your web pages in Boo, in a very relaxed and free way. After you write the code, Brail takes over and transforms it to allow you to run this code. The Brail syntax and options are documented, so we assume that you are already familiar with it.

We need to understand what MonoRail does when it receive a request:

* The user's browser sends a request to the server: `GET: /admin/users/list.rails`
* The ASP.NET runtime passes the request to MonoRail's ProcessEngine, which loads the appropriate controller and after the controller has finished running, it will call to the appropriate view.
* MonoRail's ProcessEngine calls to Brail passing the current context, the controller and a template name which will usually will look like this: "`admin/users/list`"
* Brail processes the request and writes the results back to the user.

### Processing Requests

MonoRail receives a request, calls the appropriate controller and then calls to the view engine with the current context, the controller and the view that needs to be displayed. Brail then takes over and does the following:

* Check if the controller has defined a layout and if it has, pipe the view's output through the layout's output. (The layout is compiled the same way a view is)
* Get the compiled version of a view script by:
* Checking if the script is already in the cache. The cache is a hash table `["Full file name of view" : compiled type of the view]`
* If the script is already in the cache but the type is null this means that the view has changed, so we compile just this script again.
* Instantiate the type and run the instance, which will send the script output to the user.

A few things about changes in the views: Brail currently allows instantaneous replacement of views, layout and common scripts by watching over the Views directory and recompiling the file when necessary, since this is a developer only feature, I'm not worrying too much about efficiency / memory. I'm just invalidating the cache entry or recompiles the common scripts. Be aware that making a change to the Common Scripts would invalidate all the compiled views & layouts in memory and they would all have to be compiled again. This is done since you can't replace an assembly reference in memory.

The interesting stuff is most happening when Brail is compiling a script. For reference, Brail usually will try to compile all the scripts in a directory (but does not recurse to the child directories) in one go, since this is more efficient in regard to speed / memory issues. Occasionally it will compile a script by itself, usually when it has been changed after its directory has been compiled or if the default configuration has been changed. There isn't much difference between compiling a single file and compiling a bunch of them, so I'm just going to ignore that right now and concentrate on compiling a single script. Brail's scripts are actually a Boo file that is being transformed by custom steps that plug into the Boo compiler.

### Compiling Scripts

Here is what happens when Brail needs to compile a script:

* Creating an instance of BooCompiler, and telling if to compile to memory or to file (configuration option).
* Adding a reference to the following assemblies: Brail, Castle.MonoRail.Framework, the compiled Common Script assembly and any assembly that the user referenced in the configuration file.
* Adding imports that were defined in the configuration
* Run a very simple pre processor on the file, to convert file with <% %> or <?brail ?> to a valid boo script.
* Remove the default Boo's namespace (this is done because common names such as list, date were introduced including the default namespace and that meant that you couldn't use that as a parameter to the view).
* Replace any variable reference that has unknown source with a call to `GetParameter(variableName)` which would use the controller's PropertyBag to get it. `GetParameter()` throws if it can't find a valid parameter, by the way. The reasoning is that this way you won't get null reference exceptions if you are trying to do something like: `date.ToString("dd/mm/yyyy")` and the controller didn't pass the date. Since debugging scripts is a pain, this gives you a much clearer message.
* Then the real transformation begins. Any Brail script is turned into a subclass of the BrailBase class, which provides basic services to the script and allow the engine to output the results to the user. What is happening is that any "free" code, code that isn't in a class / method is moved to a `Run()` method on the subclass. Any methods are moved to the subclass, so they are accessible from the `Run()` method. Anything else is simply compiled normally.

When Brail receive a request for a view it looks it up as described above (from cache/compiled, etc). A new instance of the view is created and its `Run()` method is called. All the output from the script is sent to the user (directly or via the layout wrapping it.)

### BrailBase Class

The BrailBase class has several useful method and properties:

* `ChildOutput` - Layouts are scripts that are using their `ChildOutput` property to wrap their output around the child output. This works as follows, a layout is created, and its `ChildOutput` is set to a view's output, the view is then run. After the view has run, the layout is run and has access to the view's layout.
* `IsDefined(parameterName)` - Check if a parameter has been passed, this allows you to bypass `GetParameter()` throwing if nothing has been passed.
* `OutputSubView()` - Output another view.

You can check the source [here](https://svn.castleproject.org/svn/castle/trunk/MonoRail/Castle.MonoRail.Views.Brail/BrailBase.cs) for the full details.