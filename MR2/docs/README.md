# Castle MonoRail v2 Documentation

<img align="right" src="images/mr-logo.png">

MonoRail is an MVC framework inspired by ActionPack, a paradigm shift to simplicity. Current version is 2.0, released in January 2010.

## What is It

MonoRail is an Model-View-Controller (or MVC) framework. MonoRail differs from the standard WebForms way of development as it enforces separation of concerns; controllers just handle application flow, models represent the data, and the view is just concerned about presentation logic. Consequently, you write less code and end up with a more maintainable application.

## Why Use It

MonoRail is a simplification of the standard ASP.NET WebForms paradigm. By using MonoRail you end up with small controllers and small views, each one having its own distinct concerns.

It also handles binding of data sent from forms, vastly reducing the need for annoying and repetitive code.

MonoRail can be extended in several directions, so it is easier to reuse pieces for different applications, dramatically reducing the time-to-market for a web application.

## Getting Started

Our MonoRail [Getting Started](getting-started.md) guide is the best source of information for newcomers. You will be acquainted with the project and how to use it in small steps. After that you can always consult the documentation for more in-depth information.

## Table of Contents

1. [Introduction](introduction.md)
1. [Getting Started](getting-started.md)
  * [Getting Started with ActiveRecord Integration](getting-started-with-activerecord-integration.md)
  * [Getting Started with Windsor Integration](getting-started-with-windsor-integration.md)
1. [Installation](installation.md)
1. [Configuration](configuration.md)
  * [XML Configuration](xml-configuration.md)
  * [Programmatic Configuration](programmatic-configuration.md)
1. [Controllers](controllers.md)
  * [Data Binding](data-binding.md)
  * [Validation](validation.md)
  * [AJAX and JSON](ajax-and-json.md)
  * [Wizards](wizards.md)
  * [Send Email](send-email.md)
1. [Views](views.md)
1. [View Engines](view-engines.md)
  * [NVelocity](nvelocity.md)
  * [WebForms](webforms.md)
  * [Brail](brail.md)
  * [AspView](aspview.md)
  * [Composite View Engine](composite-view-engine.md)
1. [Layouts](layouts.md)
1. [Rescues](rescues.md)
1. [View Components](view-components.md)
  * [CaptureFor](capturefor.md)
  * [SecurityComponent](securitycomponent.md)
  * [DiggStylePagination](diggstylepagination.md)
1. [Filters](filters.md)
1. [Helpers](helpers.md)
  * [FormHelper](formhelper.md)
  * [UrlHelper](urlhelper.md)
  * [AjaxHelper](ajaxhelper.md)
  * [PaginationHelper](paginationhelper.md)
1. [Authentication and Authorization](authentication-and-authorization.md)
1. [Resources and Localization](resources-and-localization.md)
1. [Unit Testing](unit-testing.md)
1. [Integrations](integrations.md)
  * [ActiveRecord Integration](activerecord-integration.md)
  * [Windsor Integration](windsor-integration.md)
1. Advanced Topics
  * [Security](security.md)
  * [Routing](routing.md)
  * [Legacy Routing](legacy-routing.md)
  * [Transformation Filters](transformation-filters.md)
  * [Dynamic Actions](dynamic-actions.md)
  * [Scaffolding](scaffolding.md)
  * [Extensions](extensions.md)
  * [Service Architecture](service-architecture.md)
  * [Custom Bindable Parameters](custom-bindable-parameters.md)
  * [Using Resources to Store Views](using-resources-to-store-views.md)
  * [Logging](logging.md)