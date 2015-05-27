# AjaxHelper

MonoRail supports Ajax by using the `prototype jslib`.

First of all, to use Ajax support you must make the javascript code available to your view:

```
$AjaxHelper.GetJavascriptFunctions()
```

Which will render:

```html
<script type="text/javascript" src="/MonoRail/Files/AjaxScripts.rails"></script>
```

This helper also exposes the `Behaviour` js library. To use it, invoke `GetBehaviourFunctions`:

```
$AjaxHelper.GetBehaviourFunctions()
```

## Common Parameters

The more you know about the prototype library, the better. We recommend the [Developer Notes for prototype.js](http://www.sergiopereira.com/articles/prototype.js.html) although it is a little outdated.

The prototype library has two main classes to perform remote requests:

* `Ajax.Request`: performs a remote invocation and allow you to work on the results with callbacks
* `Ajax.Updater`: extends the `Ajax.Request` and updates a html element with the invocation result.

The following parameters can be used on both `Ajax.Request` and `Ajax.Updater`:

Parameter | Description
----------|------------
`url` | The url to be invoked. You cannot specify parameters (like `url?key=value`). If you need to pass parameters, use the `with` parameter.
`method` | Http method to be used on the invocation. Defaults to '`post`'.
`with` | Defines the parameters to be send with the request. For example `name=hammett&age=27&iscustomer=true`
`form` | If you omit the `with` parameter but include the `form` then it will generate code to serialize the current form. Equivalent to use `with=Form.serialize(this)`.

When you specify the parameter `update` or `success` or `failure`, the `AjaxHelper` will generate an `Ajax.Updater` call. The following parameters apply to it:

Parameter | Description
----------|------------
`update` | Defines the name of the html element that will be updated with the return xml of the request. Usually a `div` is used.
`success / failure` | Defines the name of the html element that will be updated conditionally with the return xml of the request. If the request is successful, the elemented pointed by success will be updated, otherwise it will use the elemented pointed by `failure`.
`evalScripts` | Defines whether the returned xml should have its javascript content evaluated. Defaults to `true`.
`position` | Defines a strategy to insert the resulting xml on the DOM. The supported values are `Before`, `Top`, `Bottom` and `After`.

Callbacks can also be used. The prototype will invoke the specified javascript functions during different steps in the remote invocation.

Parameter | Description
----------|------------
`Loading` | Called when the remote document is being loaded with data by the browser.
`Loaded` | Called when the browser has finished loading the remote document.
`Interactive` | Called when the user can interact with the remote document, even though it has not finished loading.
`Complete` | Called when the `XMLHttpRequest` has completed.
`OnSuccess` | Called when the request was successful (Status code < 500)
`OnFailure` | Called when the request was not successful (Status code >= 500)

You can also specify that a function must be executed before, after or define it as a condition to the Ajax request be issue.

Parameter | Description
----------|------------
`before` | Defines that the specified javascript function must run before the Ajax request is sent.
`after` | Defines that the specified javascript function must run right after the Ajax request is sent.
`condition` | Defines that the specified function must return true to allow the Ajax request to take place.

Note that the `OnSuccess` and `OnFailure` callbacks will include a parameter called request which is the original `XmlHttpRequest` object. Your callback function will need to have a parameter called `request` to operate properly.

## Using It

The best and easiest way to use the `AjaxHelper` is to check the API documentation to identify the method you want. Check its signature. Most of them will have an `IDictionary` parameter. This is an approach to make the ajax usage on views more self-documented.

The method's API documentation should highlight the required parameters or special meanings that a parameter might have. Note that the common parameters discussed above applies to most of the methods on `AjaxHelper`.

The following snippets shows the `AjaxHelper` in action:

```
$AjaxHelper.LinkToRemote("Show server time", "showtime.rails", "%{update='maindiv', OnSuccess='showSuccessMessage(request)'}")
```

## Using the Behavior Library

The `Behaviour` javascript library allow you to use CSS selectors to bind object's events to javascript functions.

The `AjaxHelper` exposes a few methods related to `Behaviour`.

If you change your document object model dynamically you can re-apply the rules defined within `Behavior`. In this case use `ReApply` which renders a script block invoking `Behavior.apply()`.

`Behavior` binds itself to the `window.onload` event. In case you want to use it as well, invoke `AddLoadEvent(String loadFunctionName)`.

### Registering Rules

The following is an example of how to associate javascript functions to events using `Behaviour`:

```javascript
var myrules = {
  'b.someclass' : function(element){
    element.onclick = function(){
      alert(this.innerHTML);
    }
  },
  '#someid u' : function(element){
    element.onmouseover = function(){
      this.innerHTML = "BLAH!";
    }
  }
};
```

If you would prefer to use the helper to generate the source, you can do something like the following:

```
$AjaxHelper.StartBehaviourRegister()
$AjaxHelper.Register("b.someclass", "onclick", "functionName")
$AjaxHelper.Register("#someid u", "onmouseover", "function(){ this.innerHTML = 'BLAH!'; }")
$AjaxHelper.EndBehaviourRegister()
```

You may also consult the API documentation for the `AjaxHelper`.

## Javascript Action Proxies

MonoRail includes the ability to generate a proxy object in javascript to invoke actions on controllers. For example, suppose you have the following controller:

```csharp
using Castle.MonoRail.Framework;

public class AdminController : SmartDispatcherController
{
    public void Index()
    {
    }

    [AjaxAction]
    public void DisableUser(int userId)
    {
        // Do something important here
        RenderText("Done");
    }

    [AjaxAction]
    public void ChangeUserPassword(int userId, string newPassword)
    {
        // Do something important here
        RenderText("Done");
    }
}
```

The methods you want to generate proxies for need to be marked with the attribute `AjaxActionAttribute`.

On the view side, you can use the following methods to generate a javascript block that uses AjaxRequest class to invoke the actions.

* `AjaxHelper.GenerateJSProxy(string proxyName)`
* `AjaxHelper.GenerateJSProxy(string proxyName, string controller)`
* `AjaxHelper.GenerateJSProxy(string proxyName, string area, string controller)`

In the view for the `Index` action of `AdminController` we can generate a proxy:

```
$AjaxHelper.GetJavascriptFunctions()
$AjaxHelper.GenerateJSProxy("myproxy")
```

The `GenerateJSProxy` call will generate a js block that uses `Ajax.Request` to make a remote invocation:

```
<script type="text/javascript" src="/MonoRail/Files/AjaxScripts.rails"></script>
<script type="text/javascript">var myproxy =
{
	DisableUser:
	function(userId, callback)
	{
		var r=new Ajax.Request('/admin/DisableUser.rails',
		{
			parameters: '_=x26userid=' + userId,
			asynchronous: !!callback,
			onComplete: callback
		});
		if(!callback) return r.transport.responseText;
	}
,
	ChangeUserPassword:
	function(userId, newPassword, callback)
	{
		var r=new Ajax.Request('/admin/ChangeUserPassword.rails',
		{
			parameters: '_=&userId=' + userId + '&newPassword=' + newPassword,
			asynchronous: !!callback,
			onComplete: callback
		});
		if(!callback) return r.transport.responseText;
	}
}
</script>
```

As you see it supports synchronous and asynchronous calls. If you specify a callback function it will be async, otherwise synchronous. The use of the remote method becomes natural js code:

```html
<input type="button" onclick="javascript:myproxy.DisableUser($('userid'));" />
```

You may also consult the API documentation for the AjaxHelper.

## LinkToFunction and ButtonToFunction

The `LinkToFunction` and `ButtonToFunction` methods allow the generation of Html elements that once clicked invoke a javascript function. It is very simple and has nothing to do with remote invocations.

You may also consult the API documentation for the AjaxHelper.

## LinkToRemote and ButtonToRemote

The `LinkToRemote` and `ButtonToRemote` generates html elements that once clicked invokes a remote action (usually a controller's action).

The following overloads are supported:

* `ButtonToRemote(string innerContent, string url, IDictionary options)`
* `ButtonToRemote(string innerContent, string url, IDictionary options, IDictionary htmloptions)`
* `LinkToRemote(string name, string url, IDictionary options)`
* `LinkToRemote(string name, string url, IDictionary options, IDictionary htmloptions)`

## Remote Form

The `BuildFormRemoteTag` generates a form element that, instead of sending the content in the normal way, uses Ajax to send the data.

The following overloads are supported:

* `BuildFormRemoteTag(String url, IDictionary options)`
* `BuildFormRemoteTag(IDictionary options)`

## Observers

The observers can be associated with form elements or with the whole form. An ajax invocation is sent when a change is detected.

The following overloads are supported:

* `ObserveField(string fieldId, int frequency, string url, string idOfElementToBeUpdated, string with)`
* `ObserveField(string fieldId, int frequency, string url, IDictionary options)`
* `ObserveField(IDictionary options)`
* `ObserveForm(string formId, int frequency, string url, string idOfElementToBeUpdated, string with)`
* `ObserveForm(string formId, IDictionary options)`
* `ObserveForm(IDictionary options)`

You may also consult the API documentation for the AjaxHelper.

## Periodical Updates

The `PeriodicallyCallRemote` makes remote invocations with a specified frequency.

* `PeriodicallyCallRemote(IDictionary options)`
* `PeriodicallyCallRemote(String url, IDictionary options)`

## Autocompletion

The `AutoComplete` enables a google-style search where partial searches are emitted as you type.

It is highly advisable to carefully read the `script.aculo.us` documentation for the `AutoCompleter` to know about the possible `completionOptions`.

`AjaxHelper` offers the following methods for providing AutoCompletion:

* `InputTextWithAutoCompletion(IDictionary parameters, IDictionary tagAttributes)`
* `InputTextWithAutoCompletion(string inputName, string url, IDictionary tagAttributes, IDictionary completionOptions)`
* `AutoCompleteInputText(string elementId, string url, IDictionary options)`

FormHelper also offers a combination of a input field with databinding behaviour and Ajax Autocomplete behaviour.

* `FormHelper.TextFieldAutoComplete(string target, string url, IDictionary tagAttributes, IDictionary completionOptions)`

This field is created analogous to `InputTextWithAutoCompletion`, but instead of an `inputName`, the target object's property path is specified (i.e. user.Role).

### Usage

Ok, let me directly start with the code:

```
#set ($ajaxOpt = "%{parameters='{user:\'$user.Id\'}', paramName='\'search\''}")
#set ($inputOpt = "%{class='txt'}")
<p>Enter your favorite programming frameworks:</p>

<form action="$UrlHelper.For("%{action='EnterDataResults'}")" method="post">
	<input type="hidden" name="user" value="$user.Id" />
	<table class="blind">
		<tr>
			<td>IoC-Framework</td>
			<td>$FormHelper.TextFieldAutoComplete("preference.ioc", UrlHelper.For("%{action='LookupIoc'}"), inputOpt, ajaxOpt)</td>
		</tr>
		<tr>
			<td>OR/M-Framework</td>
			<td>$FormHelper.TextFieldAutoComplete("preference.orm", Url.ForHelper("%{action='LookupOrm'}"), inputOpt, ajaxOpt)</td>
		</tr>
		<tr>
			<td></td>
			<td><input type="submit" value="Save Data" /></td>
		</tr>
	</table>
</form>
```

Now to the explanation.

The variable `ajaxOpt` shows how to specify options for the Scriptaculous AutoCompleter. The AutoCompleter wants `JSON`, so you need to make sure that string literals are enclosed in quotation marks both in Brail/NVelocity and in the value itself.

In this example, the completion options are used to add a custom parameter to the query, which will be used by the controller called by the AutoCompleter. Additionally the name of the parameter is changed. If it is left unchanged, AutoCompleter would use the name attribute of the input element, in this case `preference.ioc` or `preference.orm`, which is normally unwanted due to databinding that should not occur at this point.

`inputOpt` simply defines options for the input tag and should be straightforward.

The last thing to do on the calling page is to call the helper method.

Next is the AJAX controller:

```
public void LookupIoc([ARFetch("user")]User user, string search)
{
	// perform search and put results into PropertyBag
}
```

And the associated view

```
<ul>
#foreach ($ioc in $frameworks)
	<li>$ioc.Name</li>
#end
</ul>
```

This is straight-forward. Format the results as an unordered list. The AutoCompleter uses this list to build the contents of the completion widget.