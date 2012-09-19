using System;
using System.Collections;
using System.Collections.Generic;
using Castle.MonoRail.OData.Internal;
using FluentAssertions;
using Microsoft.Data.Edm;

namespace Castle.MonoRail.Extension.OData3.Tests.Stubs
{
	public class StubCallbacks
	{
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _intercept;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _authorize;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>> _authorizeMany;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _view;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>> _viewMany;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _created;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _updated;
		private List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>> _removed;
		private List<Tuple<IEdmType, string, IEnumerable<Tuple<Type, object>>, object>> _invoked;
		protected Func<bool, string> _negotiate = (v) => "";
		
		public ProcessorCallbacks callbacks;

		public StubCallbacks()
		{
			_intercept = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_authorize = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_authorizeMany = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>>();
			_view = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_viewMany = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>>();
			_created = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_updated = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_removed = new List<Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>>();
			_invoked = new List<Tuple<IEdmType, string, IEnumerable<Tuple<Type, object>>, object>>();

			callbacks = new ProcessorCallbacks(
				(rt, ps, item) =>
					{
						_intercept.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return null;
					},
				(rt, ps, item) =>
					{
						_intercept.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return null;
					},
				(rt, ps, item) =>
					{
						_authorize.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return true;
					},
				(rt, ps, items) =>
					{
						_authorizeMany.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>(rt, ps, items));
						return true;
					},
				(rt, ps, item) =>
					{
						_view.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return true;
					},
				(rt, ps, items) =>
					{
						_viewMany.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, IEnumerable>(rt, ps, items));
						return true;
					},
				(rt, ps, item) =>
					{
						_created.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return true;
					},
				(rt, ps, item) =>
					{
						_updated.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return true;
					},
				(rt, ps, item) =>
					{
						_removed.Add(new Tuple<IEdmType, IEnumerable<Tuple<Type, object>>, object>(rt, ps, item));
						return true;
					},
				(rt, ps, action) =>
					{
						_invoked.Add(new Tuple<IEdmType, string, IEnumerable<Tuple<Type, object>>, object>(rt, action, ps, null));
						return null;
					},
				_negotiate);

		}

		public void AuthorizeSingleWasCalled(int howManyTimes)
		{
			_authorize.Should().HaveCount(howManyTimes);
		}
		public void AuthorizeManyWasCalled(int howManyTimes)
		{
			_authorizeMany.Should().HaveCount(howManyTimes);
		}
		public void ViewSingleWasCalled(int howManyTimes)
		{
			_view.Should().HaveCount(howManyTimes);
		}
		public void ViewManyWasCalled(int howManyTimes)
		{
			_viewMany.Should().HaveCount(howManyTimes);
		}
		public void CreateWasCalled(int howManyTimes)
		{
			_created.Should().HaveCount(howManyTimes);
		}
		public void UpdateWasCalled(int howManyTimes)
		{
			_updated.Should().HaveCount(howManyTimes);
		}
		public void RemoveWasCalled(int howManyTimes)
		{
			_removed.Should().HaveCount(howManyTimes);
		}
		
	}
}
