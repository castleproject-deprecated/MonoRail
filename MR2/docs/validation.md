# Validation

The Validator component is used to validate your objects. It uses an attribute driven syntax making it easy to start adding validation to your classes.

Here is a sample domain object:

```csharp
namespace GettingStartedSample.DomainObjects
{
	using System;
	using Castle.Components.Validator;

	public class Person
	{
		private String _name;
		public String Name
		{
			get { return _name; }
			set { _name = value; }
		}
	}
}
```

Here is a simple test to show its use:

```csharp
namespace GettingStartedSample.DomainObjects.Tests
{
	using System;
	using NUnit.Framework;
	using Castle.Components.Validator;

	[TestFixture]
	public class PersonTests
	{
		[Test]
		public void Should_be_invalid_if_name_is_empty()
		{
			ValidatorRunner runner = new ValidatorRunner(new CachedValidationRegistry());
			Person p = new Person()
			Assert.IsNull(p.Name);
			Assert.IsFalse(runner.IsValid(p));
		}
	}
}
```