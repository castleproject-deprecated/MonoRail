# WebForms

Pros:

* Familiar .NET Syntax
* Set of useful Web Controls

Cons:

* Does not implement or support MVC logic, so it's easy to implement more logic than view logic on the web form, leading to scattered logic
* Limitations apply, see the WebForms View Engine documentation
* Although there is an open-source implementation of it, it is not patent-free and might be discontinued or become unsupported/incompatible in the long term.

With WebForms you can use all your existing skills to develop MonoRail applications, however its integration with MonoRail can be quite tricky. The reason for this is that in MonoRail (or any other MVC framework) the controller should be the first entity invoked. For WebForms, the page is everything (the controller, the view and sometimes the model).

Let's take a look at an example of perfectly decent controller code and explain when it does not integrate with WebForms.

```csharp
public class AccountController : SmartDispatcherController
{
	public void Index()
	{
		// empty, just render the index.aspx view so the user
		// can fill the form
	}

	public void Save(String name, String email)
	{
		if (name.Length == 0 || email.Length == 0)
		{
			Flash["error"] = "Please, fill both Name and Email";
			RenderView("index");
			return;
		}

		AccountServices.Create(name, email);

		RenderView("success");
	}
}
```

And here is the source code of the page:

```html
<form runat="server">
  Name: <asp:TextBox ID="name" Runat="server" />
  Email: <asp:TextBox ID="email" Runat="server" />
  <asp:Button ID="Save" Text="Save" Runat="server" />
</form>
```

When the Webform is rendered, the form action will point to the original url, which is `account/index.rails`. But we want to execute the `Save` action instead... what to do?

Well, one approach is to handle the Save click on the server side:

```
<form runat="server">
  <asp:Button ID="Save" Text="Save" Runat="server" OnClick="OnSave" />
</form>
```

In the WebForm code, you can implement the `IControllerAware` interface so you have access to the controller instance. This means that in the `OnSave` event handler, we can delegate the execution back to the controller:

```csharp
public class Index : System.Web.UI.Page, IControllerAware
{
    private Controller _controller;

    public void SetController(Controller controller)
    {
        _controller = controller;
    }

    public void OnSave(object sender, EventArgs args)
    {
        _controller.Send("Save");
    }
}
```

While this works, if the `Save` action is completed successfully it will send back the `success.aspx` page. The problem here is that WebForm will try to populate the controls tree with the view state on the request, which will fail.

"What if we just change the form action through javascript?", you might inquire. Well, the same view state problem will happen. However, we can substitute the `RenderView` with `Redirect` and then everything works. But of course, there's a cost involved.

In this same sample, if the `Save` action decided that there was missing data and used `RenderView("index")` then you're in big trouble. That's because `Index.aspx` will be reprocessed, which it will interpret as a post back and so will invoke the button event handler again, which will delegate to the controller again, and so on.

So, many simple scenarios can get really hard with WebForms.

## Layouts

To use MonoRail layouts with the WebForms View engine, you must create an ordinary aspx file on the layouts folder. You must use `MasterPageBase` as the base class, otherwise you will have a view state name clash.

The following is an example of a layout:

```
<%@ Page Inherits="Castle.MonoRail.Framework.Views.Aspx.MasterPageBase" %>
<%@ Register tagprefix="rails" namespace="Castle.MonoRail.Framework.Views.Aspx" assembly="Castle.MonoRail.Framework" %>
Different master page
<p><rails:Contents id="contents" runat="server" /></p>
Footer
```

The control `Contents` outputs the the view content.