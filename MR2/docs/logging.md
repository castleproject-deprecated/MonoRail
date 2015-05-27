# Logging

You can supply an implementation of `ILoggingFactory` so MonoRail can use logger for its internal pieces and allow you to use the `Controller.Logger`.

Use the `services` node with the key `Custom`. The example below uses `log4net`. Make sure you have a `log4net.config` on the root directory that configures log4net.

```xml
<monorail>
  <services>
    <service
      id="Custom"
      interface="Castle.Core.Logging.ILoggerFactory, Castle.Core"
      type="Castle.Services.Logging.Log4netIntegration.Log4netFactory, Castle.Services.Logging.Log4netIntegration" />
  </services>
</monorail>
```