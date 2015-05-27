# Unit Testing

Previously, MonoRail used the ASP.NET infrastructure to run a web site and run tests against it. This worked for simple scenarios but didn't scale well or allow access to all aspects of the `RailsEngineContext`.

During RC2, `BaseControllerTest` was added to the trunk. `BaseControllerTest` exposed all of the properties of the MonoRail pipeline so that you can now inspect values as if running in a web context. This allowed us to mock the `RailsEngineContext` and inject mocks into the pipeline.

## The TestSupport Assembly

`Castle.MonoRail.TestSupport` was created to enable easy testing of MonoRail projects. Classes in this namespace are for performing tests on MonoRail Controllers. It exposes the `PropertyBag`, `Flash` and `Session` dictionaries so you can write assertions for their contents. You also have access to `MockRailsEngineContext` via the `Context` property to insert values to be used during the Controller execution.

In order to use the ASP.NET runtime for deprecated `AbstractMRTestCase`, the assembly `Castle.MonoRail.TestSupport.dll` must be registered in the GAC. If you have installed Castle using the MSI distribution this was already done for you. Otherwise, execute:

```
> gacutil /i Castle.MonoRail.TestSupport
```

## Setting Up a Test Project

To set up a test project perform the following steps:

* Create a `Class Library` project (usually you are going to use the same solution of the web project)
* Add references to:
  * `nunit.framework.dll`
  * `Castle.MonoRail.Framework.dll`
  * `Castle.MonoRail.TestSupport.dll`
* Create test case classes extending `BaseControllerTest`
* Call the `PrepareController` method passing `Controller Instance`, `Area`, `Controller Name` and `Action Name`.
* Make `Assertion` calls on `PropertyBag` or other parts of the `Controller` that is under test. You can access the `RailsEngineContext` from `BaseControllerTest.Context`.

### A simple example from MonoRail test case

The following class is a snippet of one of the MonoRail test cases:

```csharp
[TestFixture]
public class BasicFunctionalityTestCase : BaseControllerTest
{
	[Test]
	public void SimpleControllerAction()
	{
		SimpleController simpleController = new SimpleController();
		PrepareController(simpleController, "areaName", "simplecontroller", "index");

		simpleController.Index();

		// Some Assertions here
	}

	[Test]
	public void Flash()
	{
		SimpleController simpleController = new SimpleController();
		PrepareController(simpleController, "areaName", "simplecontroller", "someotheraction");

		simpleController.SomeOtherAction();

		Assert.IsNotNull(simpleController.Flash["someothervalue"]);
	}

	[Test]
	public void Redirect()
	{
		SimpleController simpleController = new SimpleController();
		PrepareController(simpleController, "areaName", "simplecontroller", "redirectaction");

		simpleController.RedirectAction();

		Assert.IsNotNull("controller/action.rails", Response.RedirectedTo);
	}

	[Test]
	public void PropertyBag()
	{
		SimpleController simpleController = new SimpleController();
		PrepareController(simpleController, "areaName", "simpleController", "propertybagaction");

		simpleController.PropertyBagAction();

		Assert.IsNotNull(simpleController.PropertyBag["somevalue"]);
	}

	[Test]
	public void CurrentUser()
	{
		SimpleController simpleController = new SimpleController();
		PrepareController(simpleController, "areaName", "simpleController", "currentuseraction");

		simpleController.CurrentUserAction();

		Assert.IsNotNull(Context.CurrentUser);
	}
}
```

For more documentation, please see the `Castle.MonoRail.TestSupport` namespace classes.