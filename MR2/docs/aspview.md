# AspView

AspView is a Visual Studio 2005 friendly ViewEngine implementation. Scripting is done using VisualStudio languages (C# and VB.NET). The views are precompiled or can be compiled on-demand.

## Example Code

Following is a sample view created using AspView:

```
<%@ Page Language="C#" Inherits="Castle.MonoRail.Views.AspView.ViewAtDesignTime" %>
<%@ import namespace="MyOrg.Model"%>
<aspView:properties>
<%
  User currentUser;
%>
</aspView:properties>
Hello <%=currentUser.Shortname%>.

<% if (currentUser.IsAccountExpired) { %>
your account is expired
<% } else { %>
<a href="proceed">proceed</a>
<% } %>

<% foreach (User friend in currentUser.Friends) { %>
  <subView:Friend friend="<%= friend %>"></subView:Friend>
<% } %>

<component:componentname param1="<%=currentUser%>" param2="constant">
  <sectionname>content</sectioname>
</component:componentname>
```

## Configuration

Configuration of the AspView engine is performed in 3 steps.

First, a configuration section definition needs to be added to the configSections node in your web.config.

```xml
<configSections>
...
  <section name="aspview"
           type="Castle.MonoRail.Views.AspView.AspViewConfigurationSection, Castle.MonoRail.Views.AspView" />
...
</configSections>
```

After that a corresponding configuration section should be added to provide settings for AspView.

```xml
<aspview saveFiles="true|false" autoRecompilation="true|false"  debug="true|false">
  <reference assembly="AddYourAssembliesInThisSection.dll"/>
  <reference assembly="System.Core.dll" isfromgac="true"/> <!-- note isfromgac attribute if you need to reference gaced dll -->
</aspview>
```

And finally, MonoRail should be configured to use AspView as its view engine.

```xml
<monoRail>
...
  <viewEngine
    viewPathRoot="Views"
    customEngine="Castle.MonoRail.Views.AspView.AspViewEngine, Castle.MonoRail.Views.AspView" />
...
</monoRail>
```

## Precompilation

When it's time to move to production, you should deploy a precompiled version of your views. In order to do so, you should put VCompile.exe in your site's bin folder, and run it once. A precompiled views assembly named 'CompiledViews.dll' will be created which should then be deployed that to your application's bin folder on the server.

In order to tell AspView to load the views from the precompiled assembly, go to the configuration section of aspview, and set autoRecompilation to 'false':

```xml
<aspview saveFiles="false" autoRecompilation="false" debug="false">
  <reference assembly="AddYourAssembliesInThisSection.dll"/>
</aspview>
```

### Precompiling in Development

If you prefer to, you can use the precompiled mode on your development machine. Just setup a PostBuild event on your site's project, to copy VCompile.exe to your site's bin folder and run it:

```
copy $(SolutionDir)YOUR_SHARED_LIB_FOLDER\vcompile.exe $(TargetDir)
$(TargetDir)vcompile.exe
```

## View Syntax

The following sections detail the each of the elements of an AspView template.

### The Page Directive

This directive is used to define the base class of the view declaring it and to enable intellisense support.  Only one Page directive can be included in a page, however it is not mandatory; if it is omitted then it will default to `Castle.MonoRail.Views.AspView.ViewAtDesignTime`.

```
<% page language="c#" inherits="Castle.MonoRail.Views.AspView.ViewAtDesignTime" %>
```

**The `inherits` attribute:** Note that the `page` directive's `inherits` attribute has special behavior. Please refer to this blog entry [dead link](http://www.kenegozi.com/Blog/2007/12/13/new-stuff-in-aspview.aspx) for now.

It is also possible to use genericized base view page. For example:

```
<% page language="c#" inherits="Castle.MonoRail.Views.AspView.ViewAtDesignTime<IStronglyTypedPropertiesWrapper>" %>
```

This makes the `StronglyTypedPropertiesWrapper` instance accessible using the `view` property as follows:

```
<%=view.MyProperty%>
```

**IStronglyTypedPropertiesWrapper:** You don't have to provide an instance of IStronglyTypedPropertiesWrapper (or ever implement one) as it is automagically created using the `DictionaryAdapter` which will wrap the PropertyBag and map matching properties which you declared on your `StronglyTypedPropertiesWrapper` type.

### The Import Directive

This directive is used to import a namespace, thereby avoiding the use of fully qualified type names in your template. It's also there to enable intellisense for the imported types.

```
<% import namespace="Namespace.To.Import" %>
```

Multiple `import` directives can be included in your template (one for each namespace you wish to import), however it is perfectly acceptable not to include any `import` directives at all.

### Declaring Properties

Local variables for the view can be declared in the AspView `properties` section.  This can be done in one of two ways.

```
<aspView:properties><%
  PropertyType propertyName;
%></aspView:properties>
```

or

```
<script runat="server" type="aspview/properties">
  PropertyType propertyName;
</script>
```

Note that only one `properties` section can be provided and, if declared, it must follow the `page` or `import` directive. If no `properties` section is provided the generic `view` property can still be used in the view.

Each property in the `properties` section is defined on a separate line and must specify the property's type and name and, optionally, a default value using standard C# notation.

When a property is defined in the `properties` section an attempt is made to map it to a corresponding element in the PropertyBag.  This match is made an a case insensitive manner.  If no match is found then the property will be initialized with the default for that type unless a value is specifically defined in the property declaration.

**C# 3 Syntax:** It is not possible to define properties using the C# 3 'var' syntax; a type name must be provided.

### Using Properties

Once properties have been declared they can be used in the body of an AspView template where the value of the property will be dynamically substituted for the property declaration when the template is rendered.

**The `view` Property:** A special property named `view` is always available and is mapped to the PropertyBag.

The raw value of a property can be emitted using the following syntax:

```
<%=propertyName%>
```

In the event, however, that the value of a property requires HTML encoding to safely display on a browser a different syntax is required.  There are three different ways in which this can be accomplished:

```
<%#propertyName%>
```

or

```
<%=(propertyName)%>
```

or

```
${propertyName}
```

### View Components

MonoRail view components can be used in an AspView template and are declared using the following syntax:

```
<component:viewcomponentname></component:viewcomponentname>
```

The name of the view component is not case sensitive. The opening tag must have a corresponding closing tag (self closing tags are not permitted) and the closing tag can on the same line or a subsequent line.

#### Passing Values

Values can be passed to view components using natural HTML/XML approach for string literals by specifying an attribute where its name corresponds to the property in the view component and the attribute value contains the value to pass.

```
<component:viewcomponentname propertyname="literal property value">
</component:viewcomponentname>
```

Note that property names are not case sensitive.

In addition to literal values, variables can also be passed to view components:

```
<component:viewcomponentname propertyname="<%=something.Wise">">
</component:viewcomponentname>
```

#### The Component Body

A view component can have a body if the view component supports it. The body is simply included between the view component start and end tags.

```
<component:viewcomponentname>
body contents
</component:viewcomponentname>
```

#### Component Sections

If the view component body supports sections then they can be declared using the `section` tag:

```
<component:viewcomponentname propertyname="<%=something.Wise">">
<section:sectionname>section contents</section:sectionname>
</component:viewcomponentname>
```

Note that section name of a section tag is case sensitive.

### Embedding Script

Scripts can also be included in an AspView template.

```
<script runat="server">
string DoubleIt(string toBeDoubled) { return string.Format("{0}{0}",toBeDoubled); }
</script>

<%=DoubleIt("me")%>
```

:warning: **Avoid Complex Logic!:** Don't use complex logic in embedded scripts. Logic in a view is considered bad practice as you can't unit test it.

Note that embedded scripts may work with view extension methods (not tested yet).

## Faking Masterpages

AspView enables fake masterpage support so you can use the `<asp:content/>` and `<asp:contentplaceholder/>` tags, with the autocompletion goodness that it provides.

Behind the scene theses tags are substituted with capturefor viewcomponent and raw output.

~/views/layouts/html.master:

```
<%@ master language="c#"%>
<html>
<head>
<asp:contentplaceholder id="htmlhead" runat="server"></asp:contentplaceholder>
</head>
<body>
<asp:contentplaceholder id="htmlbody" runat="server"></asp:contentplaceholder>
</body>
</html>
```

~/views/home/index.aspx:

```
<%@ page language="c#" masterpagefile="~/views/layouts/html.master"%>
<asp:content contentplaceholderid="htmlhead" runat="server">
<!-- something that get in the head -->
</asp:content>
<asp:content contentplaceholderid="htmlbody" runat="server">
<!-- something that get in the body -->
</asp:content>
```

The `masterpagefile` attribute in the page or masterpage (when nested) is only here to fake designer support, at runtime only the layout specified in the controller or the action is applied.