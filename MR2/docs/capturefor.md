# CaptureFor

With the CaptureFor ViewComponent you can define specific data sections on your view and use them on the layout.

Suppose each view can optionally define a javascript block. This block needs to go under the head node on the html page, but it is declared on the layout view. In this case you can use capturefor and define the javascript inside it:

```
#capturefor(javascript)

  javascript code here

#end

	... rest of the view
```

The inner content will be available in a variable named `$javascript` to be used on your layout:

```html
<html>
	<head>

	<script type="javascript">
	$!javascript
	</script>

	</head>

	... rest of the layout view
```