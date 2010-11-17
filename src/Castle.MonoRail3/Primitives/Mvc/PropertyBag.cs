namespace Castle.MonoRail3.Primitives.Mvc
{
	using System.Collections.Generic;
	using System.Dynamic;

	public class PropertyBag : DynamicObject
	{
		private Dictionary<string, object> data = new Dictionary<string,object>();

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (!data.ContainsKey(binder.Name)) 
				result = null;
			else
				result = data[binder.Name];

			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			data[binder.Name] = value;

			return true;
		}
	}
}
