# View Engines

View engines are responsible for rendering the contents back to the browser. You can create your view engine by simply implementing the interface `IViewEngine`. For example, you can create an XML/XSL view engine, or WML, or whatever you can think of. Notice, however, that the view should be responsible for displaying logic and nothing more, nothing less.

By default the view engines will return pages with the MIME type `text/html`. If you would like to use `application/xml+xhtml` you can set this in your `web.config` file:

```xml
<viewEngine
  viewPathRoot="views"
  xhtmlRendering="true"
  customEngine="Castle.MonoRail.Framework.Views.NVelocity.NVelocityViewEngine, Castle.MonoRail.Framework.Views.NVelocity" />
```

If this attribute is set to `true`, and the user agent says it will accept `application/xml+xhtml` then the page will be returned using that MIME type instead. For browsers such as IE which do not understand the new MIME type, `text/html` will still be used.

## Available View Engines

When working with MonoRail you can chose from a selection of different view engines, each with their own strengths and weaknesses.  Some view engines are released and maintained but the folk who develop MonoRail, while others are provided by third parties.

### Released with MonoRail

* [NVelocity](nvelocity.md)
* [WebForms](webforms.md)
* [Brail](brail.md)
* [AspView](aspview.md)
* [Composite View Engine](composite-view-engine.md)

### Maintained by Third Parties

* [Spark View Engine](http://www.sparkviewengine.com/)

## View Engines Compared

Engine | Language | Compiled | Helpers | ViewComponents
-------|----------|----------|---------|---------------
WebForms | Any .NET language | Yes | Yes | No
NVelocity | Velocity | No | Yes | Yes
Brail | boo | Yes | Yes | Yes

## Creating a View Engine

To implement your own view engine you need to implement the `IViewEngine` interface:

```csharp
/// <summary>
/// Depicts the contract used by the engine
/// to process views, in an independent manner.
/// </summary>
public interface IViewEngine
{
	/// <summary>
	/// Evaluates whether the specified template exists.
	/// </summary>
	/// <returns><c>true</c> if it exists</returns>
	bool HasTemplate(String templateName);

	/// <summary>
	/// Processes the view - using the templateName
	/// to obtain the correct template,
	/// and using the context to output the result.
	/// </summary>
	void Process(IRailsEngineContext context, Controller controller, String templateName);

	///<summary>
	/// Processes the view - using the templateName
	/// to obtain the correct template
	/// and writes the results to the System.TextWriter.
	/// No layout is applied!
	/// </summary>
	void Process(TextWriter output, IRailsEngineContext context, Controller controller, String templateName);

	/// <summary>
	/// Wraps the specified content in the layout using
	/// the context to output the result.
	/// </summary>
	void ProcessContents(IRailsEngineContext context, Controller controller, String contents);
}
```

The view engine implementation is also supposed to invoke a few hooks on the controller instance, namely `PreSendView` and `PostSendView`.