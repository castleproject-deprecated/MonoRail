# Custom Bindable Parameters

When an action is invoked on a controller that extends `SmartDispatcherController`, it inspects the parameters for attributes that implement `IParameterBinder`. The `DataBindAttribute`, for example, is one of these attributes.

This allows you to create binding logic, or add validation or anything that you want. You just need to create an attribute that applies to method arguments and implements the `IParameterBinder`.