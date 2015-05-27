# Legacy Routing

:warning: **This style of Routing has been superseded in favor of our [new Routing engine](routing.md).**

MonoRail supports simple URL rewrites based on regular expressions. However, to use it for nicer URLs you must allow the ASP.NET ISAPI to handle all file extensions, which has a performance penalty.

Another approach is to use an ISAPI filter that is able to rewrite URLs based on pattern matching. `Mod_Rewrite` is one example of an ISAPI filter which performs URL rewriting. By using a ISAPI filter to rewrite URLs, no wildcard mapping needs to be configured to direct all requests to the ASP.NET ISAPI. Even though the ASP.NET ISAPI will process requests for static files this is not ideal for a production site because of the performance penalty.

Other URL rewriting solutions are available by different companies, some of them are free. ISAPI_Rewrite Lite is one example.

## Configuration

1 - Depending on how you intend to use routing will determine how it needs to be configured. If you will always be using filenames you only need map the specific extensions you want to work with (such as `.aspx`, `.rails`, `.content`, `.article`, etc). However, if you intend on routing directories (such as `http://localhost/myapp/somedir/`) then you will need to map everything (`*.*`) in IIS to `aspnet_isapi.dll` which causes IIS to redirect every request to the ASP.NET ISAPI (be aware of the consequences).

2 - Create a routing element under the monorail element in your `web.config` as shown below. Routing rules will be evaluated in a top-down order until a match is found. If there are no matches the request will continue as normal with the requested URL.

```xml
<monorail>
	<routing>
		<rule>
			<pattern>(/blog/posts/)(\d+)/(\d+)/(.)*$</pattern>
			<replace><![CDATA[ /blog/view.rails?year=$2&month=$3 ]]]]></replace>
		</rule>
		<rule>
			<pattern>(/news/)(\d+)/(\d+)/(.)*$</pattern>
			<replace><![CDATA[ /news/view.rails?year=$2&month=$3 ]]]]></replace>
		</rule>
	</routing>
</monorail>
```

3 - Add the routing module to the `httpModules` element of `system.web` in your `web.config`. Ensure that the routing module is listed before the `monorail` module as shown below.

```xml
	<system.web>
		<httpHandlers>
			<add verb="*" path="*.rails"
			  type="Castle.MonoRail.Engine.MonoRailHttpHandlerFactory, Castle.MonoRail.Engine" />
		</httpHandlers>

		<httpModules>
			<!-- NOTE: Routing MUST come before the monorail module -->
			<add name="routing" type="Castle.MonoRail.Framework.RoutingModule, Castle.MonoRail.Framework" />
			<add name="monorail" type="Castle.MonoRail.Framework.EngineContextModule, Castle.MonoRail.Framework" />
		</httpModules>
	</system.web>
```

The regular expressions are compiled, therefore performance should be acceptable. If no matches are found then the request is processed as it would be without routing.

This example routing rule defines that a request for the URL `/blog/posts/2000/11/anything` will be processed as if it was `/blog/view.rails?year=2000&month=11`:

```xml
<pattern>(/blog/posts/)(\d+)/(\d+)/(.)*$</pattern>
<replace><![CDATA[ /blog/view.rails?year=$2&month=$3 ]]></replace>
```

## Root Directory Mapping Workaround

If you do not want to setup a wildcard mapping just to get a default document for your root directory you can use these steps:

Create an empty file in your root directory that is mapped to `aspnet_isapi.dll` and is a configured as a default document in IIS. For example, create a file named `index.rails` or `default.aspx` depending on your configuration.

Then create a new routing rule in your web.config as shown below.

```xml
	<monorail>
		<routing>
			<rule>
				<pattern>^(/index.rails)(.)*$</pattern>
				<replace><![CDATA[ /Controller/Action.rails?$2 ]]></replace>
			</rule>
		</routing>
	</monorail>
```

**Limited usage:** This work around is only feasible for root directories because the default document file must be created in each directory which is not practical for URLs that contain dynamic strings.

## Another Approach

David Moore has sent an interesting approach he uses:

> I really like mod_rewrite, but as you can see it requires 3rd party components also.
>
> The easiest thing is to use what you already have at your fingertips with IIS, ASP.NET and MonoRail itself, and route all requests through ASP.NET. Then, you can use the MonoRail URL rewriting features within `web.config`.
>
> Now, performance-wise, you don't want to route requests through the ASP.NET/MonoRail stack for things such as `.css` files, image resources etc. What I do for this is that I map the URLs for all static resources to another virtual directory which is purely IIS serving up these files.
>
> I.e. I make the `$static` variable available in all my NVelocity templates, and if I insert an image it looks like this: `<img src="$static/images/image.gif"/>` as a simple example.
>
> This currently resolves to `http://localhost/static/images/image.gif`, with `/static` being a virtual directory, and within there all my static content (css, javascript, images etc).
>
> This is very scalable in that in a production environment, we can change our `$static` to point to `http://static.ourdomain.com` and have light and fast web servers such as lighttpd or just a vanilla IIS serving up our static content fast, easing the load on a dedicated application server.
>
> So in summary, I evaluated `mod_rewrite`-style alternatives and went with the simple solution built into MonoRail, and implemented a good practice of having a separate URL for static content which is going to make our application more scalable in the future anyway.

## Additional Information

Josh wrote a very interesting post about MonoRail and [Url rewriting](http://joshrobb.com/blog/2007/04/26/monorail-url-rewriting/).

Colin Ramsay wrote about [rewriting options for IIS](http://colinramsay.co.uk/2007/04/17/url-rewriting-options-on-iis/).