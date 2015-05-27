# Extensions

From MonoRail beta 5 you can use the new Extensions support which allow you to plug existing extensions or develop your own extensions. This document should clarify why extensions were introduced, a list of out-of-box extensions and how to create your own extensions.

The user and development community on MonoRail is quite active, and consequently we receive lots of suggestion about new interesting features. Some of the features are unarguably helpful for the almost every user, however most of them are only useful to some specific users/scenarios. So implementing the new feature directly into the core framework would not make much sense as they usually increase the request flow overhead and the users that are not using the feature would be penalized.

Extensions were introduced to allow the framework to be extended easily and extensions to be reused.

## Creating Custom Extensions

Creating an extension is a fairly simple task. Just create a new public class and implement the `IMonoRailExtension` or extend from `AbstractMonoRailExtension`. The latter is simpler.

After that you can override the methods you want. Usually your extension will read some custom configuration from the MonoRailConfiguration to configure itself if necessary and then perform some action when one of the hook methods is invoked.

MonoRail must be told that an extension exists. You can do that by using the `extensions` node in the configuration file. For more on that, check the MonoRail Configuration Reference document.

## Built In Extensions

With the beta 5 version we shipped two extensions. As they can be helpful for some scenarios, they were also developed to serve as sample extensions, so the code is very simple.

### Custom Session Extension

The Custom Session extension allow you to plug a custom implementation for the session available on `IRailsEngineContext`. This can be useful if you have requirements to implement on a session that ASP.NET Session strategies can not fulfil.

In order to use this extension you must provide an implementation of `ICustomSessionFactory` which would be responsible to create your own session implementation. A really naive implementation could map some cookie value to an instance of `Hashtable`, for instance.

Extensions were introduced to allow the framework to be extended easily and extensions to be reused.

You need to install the extension using the extensions node, as usual, and also provide the attribute `customsession` to inform the type that implements `ICustomSessionFactory` as follows:

```xml
<monorail customsession="Type name that implements ICustomSessionFactory">
	<extensions>
		<extension
			type="Castle.MonoRail.Framework.Extensions.Session.CustomSessionExtension, Castle.MonoRail.Framework" />
	</extensions>
</monorail>
```

### Exception Chaining Extension

The Exception Chaining extension allow you to execute one or more steps in response to an exception thrown by an action. The steps are called Exception Handlers and must implement `IExceptionHandler` (or `IConfigurableHandler` if they need external configuration).

The `IExceptionHandler` interface is very straighforward, it simply dictates the contract for processing the exception information. As they are chained you must be good and check if there's a next handler available and if so, delegate the invocation to it. It would be also a good behavior if your handler implementation doesn't throw exceptions at all.

It is also important to note that you can use the `AbstractExceptionHandler` to save you some few types.

The `IConfigurableHandler` is just an increment the the previous interface for those handlers that require configuration information. The `Configure` method is invoked as soon as the handler is instantiated and its node on the configuration is passed.

The extension does not do much more than delegating the execution to the installed handlers. You can create handlers to provide actions and even introduce new semantics. As the handlers are chained together, you can even implement a handler that decides if the execution chain should continue or stop right there. For example, suppose you want that only exceptions that extends `SqlException` be e-mailed to you. In this case you could write this simple handler:

```csharp
public class MyFilterHandler : AbstractExceptionHandler
{
	public override void Process(IRailsEngineContext context, IServiceProvider serviceProvider)
	{
		if (context.LastException is SqlException)
		{
			InvokeNext(context, serviceProvider);
		}
	}
}
```

And of course, register this handler before others.

#### The EmailHandler

MonoRail comes with just one exception handler: `EmailHandler`. This handler e-mails the exception details and the environment details to a specified e-mail address.

This handler requires the attribute `mailto` and you can optionally inform the `mailfrom` as well. Also, you must provide the `smtpHost` in the configuration -- see MonoRail Configuration Reference for more details on this one.

```xml
<monorail smtpHost="my.smtp.server">
	<extensions>
		<extension
		type="Castle.MonoRail.Framework.Extensions.ExceptionChaining.ExceptionChainingExtension, Castle.MonoRail.Framework" />
	</extensions>
	<exception>
		<exceptionHandler
			mailTo="lazydeveloper@mycompany.com" mailFrom="angry.client@client.com"
			type="Castle.MonoRail.Framework.Extensions.ExceptionChaining.EmailHandler, Castle.MonoRail.Framework" />
	</exception>
</monorail>
```

You need to install the extension using the `extensions` node, as usual, and also provide the node exception to list the handlers you want to install. Please note that they will be installed and chained in the same order they were declared.

```xml
<monorail customsession="Type name that implements ICustomSessionFactory">
	<extensions>
		<extension
			type="Castle.MonoRail.Framework.Extensions.ExceptionChaining.ExceptionChainingExtension, Castle.MonoRail.Framework" />
	</extensions>
	<exception>
		<exceptionHandler type="type that implements IExceptionHandler" />
	</exception>
</monorail>
```