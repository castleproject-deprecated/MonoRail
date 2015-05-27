# SecurityComponent

The SecurityComponent ViewComponent allows you to render the inner content only if the current `IPrincipal` has the role specified.

Suppose a link can only be seen by users that have the `Administrator` role:

```
#blockcomponent(SecurityComponent with "role=Administrator")

  important link here

#end

	... rest of the view
```

:warning: **Do not hide:** Do not base your security on hiding links and buttons. Instead combine this with action and resource protection. Please refer to Authentication/Authorization document for more information.}