# Programmatic Configuration

To perform programmatic configuration you will need to have a `Global` class in your project, such as the code-behind for a Global.asax.  Modify the `Global` class to implement the `IMonoRailConfigurationEvents` interface which provides a `Configure` method with the following signature:

```csharp
public void Configure(IMonoRailConfiguration configuration)
{
}
```

Inside this method you can access the MonoRail configuration object which you can modify to customize configuration.

**More Information:** For further information on features exposed by the `IMonoRailConfiguration` interface see the API documentation or delve into the intellisense on the configuration object in your IDE.}

## Controllers

Controller types must be registered with MonoRail to be used by the framework. To register controllers programatically use the ControllersConfig

```csharp
public void Configure(IMonoRailConfiguration configuration)
{
    var controllersConfig = configuration.ControllersConfig;

    //Add controllers from the current assembly.
    controllersConfig.AddAssembly(Assembly.GetExecutingAssembly());

    //Add controllers from other assembly.
    controllersConfig.AddAssembly(Assembly.Load("ControllerAssembly"));
}
```

## Extensions

TODO

## Routing

TODO

## Scaffold

TODO

## Services

TODO

## Url

TODO

## View Components

TODO

## View Engines

TODO