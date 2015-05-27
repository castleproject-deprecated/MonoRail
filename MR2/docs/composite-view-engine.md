# Composite View Engine

The Composite View Engine just checks whether the view selected to be rendered is a `.vm` file or `.aspx` file. It then delegates the process to the correct view engine instance: AspNetWebForm or NVelocity.

To enable the Composite View Engine, configure your `web.config` `viewEngine` node as follows:

```xml
<viewEngine
  viewPathRoot="views"
  customEngine="Castle.MonoRail.Framework.Views.CompositeView.CompositeViewEngine, Castle.MonoRail.Framework.Views.CompositeView" />
```