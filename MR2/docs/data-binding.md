# Data Binding

MonoRail is able to bind simple values and complex objects. Both approaches are described in the sections below.

## The SmartDispatchController

The `SmartDispatcherController` extends `Controller` class adding support for parameter binding. This allows you to bind parameters from form elements to your action arguments. Overloads are also supported. MonoRail will invoke the action that it can supply more parameters.

### Simple Parameter Binding

Consider the following html form:

```html
<form action="/User/Search.rails" method="post">
	Name: <input type="text" name="name" />
	Email: <input type="text" name="email" />
	Country:
	<select name="country">
		<option	value="44">England</option>
		<option	value="55">Brazil</option>
	</select>
	<input type="submit" value="Search" />
</form>
```

When this form is submitted, the following entries will be present on the Form dictionary:

* name
* email
* country

The standard way of getting those values on the controller is to use one of the dictionaries:

* Params : Has query string, form and environment entries
* Form : Has only form entries (method post)
* Query : Has only query string entries

Having said that your action code could be the following:

```csharp
using Castle.MonoRail.Framework;

public class UserController : Controller
{
	public void Search()
	{
		String name = Form["name"];
		String email = Form["email"];
		String country = Form["country"];

		// Perform search ...
	}
}
```

Now if you switch to `SmartDispatcherController` you would be able to use the following simpler code instead:

```csharp
using Castle.MonoRail.Framework;

public class UserController : SmartDispatcherController
{
	public void Search(string name, string email, string country)
	{
		// Perform search ...
	}
}
```

The `SmartDispatcherController` is able to perform conversions (more on that below). In this case if the value is not present (ie. it was not submitted), the argument will assume a default value. However, if the value was submitted, but could not be converted, an exception will be thrown and the action will not be invoked.

**Empty strings:** Since the RC2 release empty strings are converted to null strings.

### DateTime Properties

To bind DateTime fields you can pass a single value or multiple values. Each of them will be a part of the `DateTime` struct. For example, using a single value:

```html
<form action="SaveValues.rails" method="post">
	<input type="text" name="dob" value="1/1/2000"/>
</form>
```

Using multiple values:

```html
<form action="SaveValues.rails" method="post">
	<input type="text" name="dobday" value="16" />
	<input type="text" name="dobmonth" value="7"/>
	<input type="text" name="dobyear" value="1979" />
	<input type="text"name="dobhour" value="4" />
	<input type="text" name="dobminute" value="0" />
	<input type="text" name="dobsecond" value="0" />
</form>
```

Regardless of the form approach, the controller action parameter will be the same:

```
using Castle.MonoRail.Framework;

public class UserController : SmartDispatcherController
{
	public void SaveValues(DateTime dob)
	{
		...
	}
}
```

### Nullable Support

Nullables data types are also supported. They will only be populated if the values are present on the form and in non-empty fields.

### Array Support

Arrays are also supported on the controller side. You can use two naming approaches on the form elements to make it work.
The first approach is to repeat the element name. For example:

```html
<form action="SaveValues.rails" method="post">
	<input type="text" name="name" value="1" />
	<input type="text" name="name" value="2" />
	<input type="text" name="name" value="3" />
	<input type="text" name="name" value="4" />
	<input type="text" name="name" value="5" />
</form>
```

The second approach is to use the indexed value notation. The index value is meaningless to MonoRail, but it must be unique per element name. For example:

```html
<form action="SaveValues.rails" method="post">
	<input type="text" name="name[0]" value="1" />
	<input type="text" name="name[1]" value="2" />
	<input type="text" name="name[2]" value="3" />
	<input type="text" name="name[3]" value="4" />
	<input type="text" name="name[4]" value="5" />
</form>
```

On the controller side, the parameter will be the same independently of the approach in use. All you need to do is to use an array type:

```csharp
using Castle.MonoRail.Framework;

public class UserController : SmartDispatcherController
{
	public void SaveValues(string[] name)
	{
		 ...
	}
}
```

## Custom Binding

### The DataBind Attribute

If instead of working with flat values you want to populate an object, this is also possible using the `DataBindAttribute`.

The `DataBindAttribute` uses the `Castle.Component.Binder` to instantiate and populate the target type. Simple values, nested objects and arrays are supported. As with simple binding, a name convention must be used on the form elements, so the binder can do its work.

First of all you must use a prefix which is required to avoid name clashing. It is as giving the form elements a name space. The form below uses product as a prefix:

```html
<form method="post" action="create.rails">
	<input type="text" name="product.id" />
	<input type="text" name="product.name" />
	<input type="checkbox" name="product.inStock" id="" value="true" />
</form>
```

On the controller action you must specify the prefix as the argument to the `DataBindAttribute`:

```csharp
using Castle.MonoRail.Framework;

public class ProductController : SmartDispatcherController
{
	public void Create([DataBind("product")] Product prod)
	{
	}
}
```

**Parameter name:** The parameter name (in the case above `prod`) is not used by the binder and have no relation with the prefix.

The binding of values happens with writable properties only. Fields are never used. The `Product` class used on the example above would be:

```csharp
public class Product
{
	private int id;
	private String name;
	private bool inStock;

	public int Id
	{
		get { return id; }
		set { id = value;}
	}

	public string Name
	{
		get { return name; }
		set { name = value; }
	}

	public bool InStock
	{
		get { return inStock; }
		set { inStock = value; }
	}
}
```

**Parameterless constructor:** Your class must have a default parameterless constructor.

### Nested Objects

Nested objects are supported with no deep limit. Suppose the `Product` class above included a `SupplierInfo`:

```csharp
public class Product
{
	private SupplierInfo supplierInfo;

	// others fields omitted

	public SupplierInfo SupplierInfo
	{
		get { return supplierInfo; }
		set { supplierInfo = value; }
	}

	// others properties omitted
}
```

The declaration of `SupplierInfo` follows. Note that it uses different types, including an enumerator.

```csharp
public enum WeightUnit
{
	Kilos,
	Pounds
}

public class SupplierInfo
{
	private String brand;
	private float weight;
	private WeightUnit weightUnit;
	private int warrantyInMonths;

	public string Brand
	{
		get { return brand; }
		set { brand = value; }
	}

	public float Weight
	{
		get { return weight; }
		set { weight = value; }
	}

	public WeightUnit WeightUnit
	{
		get { return weightUnit; }
		set { weightUnit = value; }
	}

	public int WarrantyInMonths
	{
		get { return warrantyInMonths; }
		set { warrantyInMonths = value; }
	}
}
```

When adding elements on the form, all you have to care is to include the property name. For the case above it would be:

```html
<form method="post" action="create.rails">
	<input type="text" name="product.id" />
	<input type="text" name="product.name" />
	<input type="checkbox" name="product.inStock" id="" value="true" />

	<input type="text" name="product.supplierinfo.brand" />
	<input type="text" name="product.supplierinfo.Weight" />

	<select name="product.supplierinfo.WeightUnit">
		<option value="Kilos">In Kg</option>
		<option value="Pounds">In Pounds</option>
	</select>

	<input type="text" name="product.supplierinfo.WarrantyInMonths" />
</form>
```

The rule is `prefixname.propertyname1.propertyname2...`. The binder is not case sensitive.

### Object Array Support

There are two situations for array support. First, suppose instead of populating a single `Product` you would want to populate a sequence of them. This demands two changes in the example we have seen so far.

First the form elements must use the indexed notation discussed earlier:

```html
<form method="post" action="create.rails">
	<input type="text" name="product[0].id" />
	<input type="text" name="product[0].name" />
	<input type="checkbox" name="product[0].inStock" id="" value="true" />

	<input type="text" name="product[1].id" />
	<input type="text" name="product[1].name" />
	<input type="checkbox" name="product[1].inStock" id="" value="true" />
</form>
```

Second, on the controller you must declare the parameter as an array of `Product`s:

```csharp
using Castle.MonoRail.Framework;

public class ProductController : SmartDispatcherController
{
	public void Create([DataBind("product")] Product[] prods)
	{
	}
}
```

The rule is `prefixname[uniqueindex].propertyname1.propertyname2...`. The index must be a number, and the same number identifies the same instance.

Another situation is when one or more properties of the binding target are arrays. This case is also supported and not different from what we have seen.

Being practical, suppose the Product class in the example above included a `Category` array.

```csharp
public class Product
{
	private Category[] categories;

	// others fields omitted

	public Category[] Categories
	{
		get { return categories; }
		set { categories = value; }
	}

	// others properties omitted
}
```

It could also be an array of simple values like `string s` or `int s` and the solution would be the same. The declaration of the `Category` follows:

```csharp
public class Category
{
	private String name;

	public string Name
	{
		get { return name; }
		set { name = value; }
	}
}
```

One more time the solution lies on the element names on the form. The property name must be used in the indexed notation:

```html
<form method="post" action="create.rails">
	<input type="text" name="product.id" />
	<input type="text" name="product.name" />
	<input type="checkbox" name="product.inStock" id="" value="true" />

	<input type="checkbox" name="product.categories[0].name" value="Kitchen"/>
	<input type="checkbox" name="product.categories[1].name" value="Bedroom"/>
	<input type="checkbox" name="product.categories[2].name" value="Living-room" />
</form>
```

The rule in this case would be `prefixname.propertyname1[uniqueindex].propertyname2...`.

### Generic Lists

Generic Lists are supported and the behavior is the same of the arrays, except the property declaration ofcourse:

```csharp
public class Product
{
	private List<Category> categories;

	// others fields omitted

	public List<Category> Categories
	{
		get { return categories; }
		set { categories = value; }
	}

	// others properties omitted
}
```

### Setting the Binding Source

By default the binder will use the `Params` collection as source of information to bind data. You can define that it should use the `QueryString` or `Form` post data instead. To do that use the `From` property exposed by the `DataBindAttribute`.

This is a recommend practice for performance and to prevent people from easily override form parameters. For example:

```csharp
using Castle.MonoRail.Framework;

public class ProductController : SmartDispatcherController
{
	public void Create([DataBind("product", From=ParamStore.Form)] Product product)
	{
		...
	}
}
```

### Defining Accessible Properties

As the `DataBindAttribute` usually acts on domain model classes, you might not want that all properties be "bindable". Suppose you are binding a `User` class. Sensitive properties might allow overriding the password, roles, access levels or audit information. For these cases you can use `Allow` and `Exclude` properties of `DataBindAttribute`.

The values for these properties are a comma separated list of property names, including the prefix. For example:

```csharp
public class AccountController : SmartDispatcherController
{
	public void CreateAccount([DataBind("account", Allow="account.Name, account.Email, account.Password")] Account account)
	{
		...
	}
}
```

This indicates that you only want to allow the `Name`, `Email` and `Password` properties to bound with the values from the request. All other properties will be ignored.

The `Exclude` property is the inverse. It prevents the properties indicated from being used, and allows all others.

There is no depth limit. You should be able to `allow` or `exclude` properties in any level of the object graph. For example:

```csharp
public class AccountController : SmartDispatcherController
{
	public void CreateAccount([DataBind("account", Allow="account.Name, account.Address, account.Address.Street")] Account account)
	{
		...
	}
}
```

### Binding Errors

Binding errors might occur, like invalid dates or problems in data conversion. When using simple binding, an exception will be thrown. When using the `DataBindAttribute`, however, no exception will be thrown.

To access the error information, use the `GetDataBindErrors` method:

```csharp
public class AccountController : SmartDispatcherController
{
	public void CreateAccount([DataBind("account")] Account account)
	{
		ErrorList errors = GetDataBindErrors(account);
		...
	}
}
```

The `ErrorList` implements the `ICollection` so you can enumerate the problems. You can also check if some specific property could not be converted. For example:

```csharp
public class AccountController : SmartDispatcherController
{
	public void CreateAccount([DataBind("account")] Account account)
	{
		ErrorList errors = GetDataBindErrors(account);

		if (errors.Contains("DateOfBirth"))
		{
			Flash["error"]= errors["DateOfBirth"].ToString(); // Or Exception
			RedirectToAction("New", Params);
		}

		...
	}
}
```

### BindObject and BindObjectInstance

You do not need to always use parameters to have an object bound. The methods `BindObject` and `BindObjectInstance`, exposed by the `SmartDispatcherController`, allow you to have the same functionality. The benefit is that not in every case you want to perform the bindings. For example:

```csharp
public class AccountController : SmartDispatcherController
{
	public void CreateAccount(bool acceptedConditions)
	{
		if(acceptedConditions)
		{
			Account account = (Account)BindObject(ParamStore.Form, typeof(Account),"account");
			...
		}

		...
	}
}
```

### Supported Types

The following types are natively supported by the `DataBinder` component:

Type name | Note
----------|-----
String | Empty fields are converted to null strings
All types where IsPrimitive returns true | -
Enum | It is converted using the name or value. Flags are also supported
Decimal | -
Guid | -
DateTime | The implementation checks for the key plus `day, month, year, hour, minute` and `second`. If none of these elements are found, it falls back to use `DateTime.Parse` on the value associated with the key.
Array | -
Generic Lists | -
HttpPostedFile | -
TypeConverter | If the type is not within the range above, the converter checks for a `TypeConverter` associated with it that is able to convert from a string.

### FormHelper

The `FormHelper` was created to act together with the binder. It is able to create form elements with the right names and to obtain the existing value (if possible) saving you from populating the elements manually.

For more on the `FormHelper` visit the Helpers documentation pages.

## JSON Binding

MonoRail also provides support for JSON data binding through the `JSONBinder` and `JSONReturnBinder`.  For further information see the [AJAX and JSON](ajax-and-json.md) topic in this documentation.