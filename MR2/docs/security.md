# Security

There are a few security related issues you should consider when configuring your MonoRail application.

First, if your view directory is in the web folder then clients can potentially see the source code of the views, which can expose potentially sensitive information to parties you would prefer not to have access to it. To prevent this, associate the view extension with an `IHttpHandler` that comes with ASP.NET.

Second, if you use the `DataBinder` to populate classes, you might want to provide an `Exclude` or `Allow` list to prevent people from populating properties that are not on the form. Check the `DataBind` documentation for more information.