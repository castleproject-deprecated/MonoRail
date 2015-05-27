# Windsor Integration

By enabling `Windsor Container` integration, your controllers, filters and ViewComponents might request dependencies or configurations that can be satisfied by the container, thus leading your design to a loosely coupled state.

:warning: **Usage:** Once the integration is set you must register MVC components the same way as for ordinary container components components one could register:

* controllers
* viewcomponents (To look up a ViewComponent on the view you must use the key used to register the component on the container instead of the ViewComponent name)
* filters
* dynamic action providers

:bulb: **You can use any IoC container with MonoRail:** MonoRail is not in any way tied to Castle Windsor. Integration with Windsor comes out of the box, but MonoRail has no dependency on Windsor whatsoever, so you can freely use any other container of your choice.

## Benefits

Once the integration is set you can take advantage of all the benefits offered by an Inversion of Control container.

Suppose you have a controller that receives file uploads. Where should it store the files? Make it configurable.

On the controller:

```csharp
public class ImageGalleryController : Controller
{
	...

	public ImageGalleryController(String imageDirectory)
	{
		this.imageDirectory = imageDirectory;
	}

	...
}
```

Let's assume that this controller was registered on the container with the key `imagegallery.controller`. On the configuration section:

```xml
<castle>

  <components>

	<component id="imagegallery.controller">
	  <parameters>
		<imageDirectory>C:\mytempdir\safedir</imageDirectory>
	  </parameters>
	</component>

  <components>

</castle>
```

Your controller could assume a default directory and allow it to be overriden as well:

```csharp
public class ImageGalleryController : Controller
{
	private String imageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

	public ImageGalleryController()
	{
	}

	public string ImageDirectory
	{
		get { return imageDirectory; }
		set { imageDirectory = value; }
	}

	...
}
```

The previous configuration for the component is still valid. But now it is optional as the controller can live without it.

## Implementation strategy

The implementation of the component resolution is following this pattern:

```csharp
if (kernel.HasComponent(componentType))
{
    return (T)container.Resolve(componentType);
}
else
{
    base.MethodName(...method call arguments...);
}
```