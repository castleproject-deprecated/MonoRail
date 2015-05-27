# AJAX and JSON

## AJAX

AJAX is short for "Asynchronous JavaScript and XML". It's a technique for doing background requests to the web server. Since the requests are done in the background, a full-page refresh is not required. The AJAX request can bring back a small payload -- perhaps just a small, updated section of the HTML page. Smaller payloads mean less work for the webserver and the absence of a full-page refresh gives the user a better browser experience.

Despite it's name, AJAX does not require JavaScript nor XML. At the heart of an AJAX request is simple text. However, JavaScript is often used as the scripting language to make an AJAX request and process the results.

### Installing Scripts

Since MonoRail uses the prototype js framework, you need to make the prototype JavaScript available to your page. This can be accomplished via the AjaxHelper.

```
$AjaxHelper.InstallScripts()
```

This will render a javascript tag into your page which will point to a built in set of additional scripts which manage automate further integration.

```html
<script type="text/javascript" src="/MonoRail/Files/AjaxScripts.rails"></script>
```

At present, MonoRail uses the [prototype.js](http://www.prototypejs.org/) framework to make AJAX integration easy.

### Javascript in the View

The javascript code to make and AJAX call uses the Ajax.Request object provided by the [prototype.js](http://www.prototypejs.org/) framework.

```html
<script language="text/javascript">
  function UpdateWeatherData()
  {
    new Ajax.Request('$UrlHelper.For("%{action='GetWeatherData'}")',
      {
        method: 'get',
        parameters: {
          zipCode: $('zipcode').value
          },
        onSuccess: showResult,
        onFailure: showMessage
      });
  }

  function showResult(transport)
  {
    $('weather').innerHTML = transport.responseText;
  }

  function showMessage(transport)
  {
    alert('An error occurred during the AJAX request.');
  }
</script>
```

The call above is using the [UrlHelper] to build the URL for the AJAX call. The zipCode parameter is populated from a textfield named "zipcode" from a form in the HTML of the view. The onSuccess variable defines the function to be called on successful completion of the AJAX request. Likewise, the onFailure function will be called if there is a problem making the request.

In the example above, the response text returned from the AJAX request is used to update element of the HTML with the id "weather" (which could be a DIV or SPAN tag).

**AjaxHelper:** The [AjaxHelper](ajaxhelper.md) provides many useful methods to simplify AJAX development in your views.

## Ajax Helper

[AjaxHelper](ajaxhelper.md)

### An AJAX Action

The action that would support the AJAX request from the sample above would look something like the following.

```csharp
public class WeatherDataController : SmartDispatcherController
{
    public void GetWeatherData(string zipCode)
    {
        // get weather data for zip code and place results
        // in the PropertyBag

        CancelLayout();
        RenderView("weather");
    }
}
```

This method expects a string parameter named zipCode. When called, this method returns a snippet of HTML that represents the weather for the specific zip code.  This is the content that would be received by the AJAX request.

## JSON

JSON stands for "Javascript Object Notation". It's a lightweight interchange format used to represent simple JavaScript objects as text. MonoRail supports both receiving a JSON object in an AJAX request as well as returning a JSON object to an AJAX request.

MonoRail has built-in support for both sending and receiving JSON objects.

### Sending JSON from a View

Let's assume we have a simple JavaScript class that contains some data:

```html
<script language="text/javascript">
  function UserData(name, age)
  {
    this.Name = name;
    this.Age = age;
  }
</script>
```

We can use the AjaxRequest object to send an entire instance of the UserData class to an action method on a controller. The following JavaScript sample demonstrates the creating a UserData object and sending it as JSON via an AJAX request to the GetUpdatedUserData action.

```javascript
<script language="text/javascript">
  function UpdateUserData()
  {
    var userdata = new UserData('Patrick Steele', 41);
    new Ajax.Request('$UrlHelper.For("%{action='GetUpdatedUserData'}")',
      {
        method: 'get',
        parameters: {
          ud: Object.toJSON(userdata)
          },
        onSuccess: showResult,
        onFailure: showMessage
      });
  }

  function showResult(transport)
  {
    $('userinfo').innerHTML = transport.responseText;
  }

  function showMessage(transport)
  {
    alert('An error occurred during the AJAX request.');
  }
</script>
```

### Receiving JSON in a Action

While MonoRail's DataBinder is used to populate objects from data found inside an HTTP request, the JSONBinder is used to populate objects from JSON data transmitted in an HTTP request.

For JSON binding to take place a C# class that has the same signature as the JavaScript class must be present so it can be bound to:

```csharp
// NOTE: Use VS2008 property syntax
public class UserData
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

Note that the C# class must have read/write properties that correspond to all of the JavaScript object properties that are to be bound.

To bind the JSON data in the request to a property on the controller's action we use the JSONBinder attribute.

```csharp
public void UserController : SmartDispatcherController
{
    public void GetUpdatedUserData([JSONBinder("ud")]UserData data)
    {
      // use "data" as any other C# object
    }
}
```

The above snippet tells MonoRail that an HTTP request coming to the GetUpdatedUserData action containing a data element called "ud" should be mapped via JSON to an instance of the UserData class.

### Sending JSON from an Action

Sending the JSON version of a C# class or struct is accomplished in a similar fashion to receiving a JSON object.  The following snippet returns an instance of the UserData class defined earlier in this topic as JSON.

```csharp
[return:JSONReturnBinder]
public UserData GetData()
{
    UserData fromCsharp = new UserData();
    fromCsharp.Age = 32;
    fromCsharp.Name = "Bob Smith";

    return fromCsharp
}
```

The JSONReturnBinder attribute instructs MonoRail to serialize the resulting return value as a JSON object rather than rendering a view.

### Receiving JSON in a View

The prototype framework has a built-in method for recreating the JSON object in the browser:

```html
<script language="text/javascript">
  function showResult(transport)
  {
    var newData = transport.responseText.evalJSON();
  }
</script>
```

In the interest of brevity, only the AJAX onSuccess handler is shown.