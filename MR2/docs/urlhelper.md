# UrlHelper

The `UrlHelper` exposes methods to easily build URL's and HTML anchors in your views. The UrlHelper uses context-sensitive information (such as the currently configured MonoRail extension) to build proper URLs.

## Generating URLs

The `For` method generates a URL from the specified parameters. For example:

```
$UrlHelper.For("%{action='Save'}")
```

This generates a complete URL for a controller and and action. With no controller specified, the controller from the current context is used. The action was defined in the parameter list as "`Save`".

All of the `UrlHelper` methods accept a set of parameters to control the building of the URL. The list of possible parameters are defined below:

Parameter | Definition
----------|-----------
`area` | The area of the controller. The default is the current area.
`controller` | The controller for the URL. The default is the current controller.
`action` | The controller's action.
`protocol` | The protocol to use.
`port` | The port number to use.
`domain` | The domain of the URL.
`subdomain` | The subdomain.
`appVirtualDir` | The virtual directory.
`extension` | The extension for the URL. If not specified, the configured Monorail extension is used.
`absolutePath` | If set to true, define an absolute path.
`applySubdomain` | If set to true, use the subdomain.
`suffix` | The suffix.

## Generating Links

Use the `Link` method to generate a complete HTML link.

```
$UrlHelper.Link("Edit Trainer", "%{action='Edit', 'querystring='id=$trainer.id'}")
```

The above produces a link that would look like this (assuming the current controller is called "Trainer"):

```html
<a href="/virtualdir/Trainer/Edit.rails?id=45">Edit Trainer</a>
```

In the example above, the extension ".rails" is pulled from the current Monorail configuration.

You may also add a second set of parameters used to add atributes to the generated anchor:

```
$UrlHelper.Link("Edit Trainer", "%{action='Edit', 'querystring='id=$trainer.id'}", "%{target='_new'}")
```

Produces:

```html
<a href="/virtualdir/Trainer/Edit.castle?id=45" target="_new">Edit Trainer</a>
```

## Generating Button Links

The `$UrlHelper.ButtonLink` method works the same way as the `$UrlHelper.Link` method, except an HTML button is generated with some javascript to handle the `onclick` event to navigate to the URL.