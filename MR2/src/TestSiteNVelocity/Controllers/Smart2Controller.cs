// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace TestSiteNVelocity.Controllers
{
	using System;
	
	using Castle.MonoRail.Framework;

	/// <summary>
	/// Same as <see cref="SmartController"/> but using
	/// BindObject methods
	/// </summary>
	public class Smart2Controller : SmartDispatcherController
	{
		public void SimpleBind()
		{
			var order = (Order) BindObject(typeof(Order), "order");
			RenderText(String.Format("incoming {0}", order));
		}

		public void SimpleBindArray()
		{
			var orders = (Order[]) BindObject(typeof(Order[]), "orders");
			
			if (orders == null)
			{
				RenderText("Null array shouldn't be returned by databinder");
			}
			else
			{
				RenderText(String.Format("incoming {0}", orders.Length));
			}
		}

		public void ComplexBind()
		{
			var order = (Order) BindObject(typeof(Order), "order");
			var person = (Person) BindObject(typeof(Person), "person");

			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindExcludePrice()
		{
			var order = (Order) BindObject(ParamStore.Params, typeof(Order), "order", "order.Price", null);
			var person = (Person) BindObject(typeof(Person), "person");

			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindExcludeName()
		{
			var order = (Order) BindObject(ParamStore.Params, typeof(Order), "order", "order.Name", null);
			var person = (Person) BindObject(typeof(Person), "person");

			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindWithPrefix()
		{
			var order = (Order) BindObject(typeof(Order), "order");
			var person = (Person) BindObject(typeof(Person), "person");

			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void FillingBehavior()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			var clazz = (ClassWithInitializers) BindObject(typeof(ClassWithInitializers), "abc");
			
			RenderText(String.Format("incoming {0} {1} {2}", clazz.Name, clazz.Date1.ToShortDateString(), clazz.Date2.ToShortDateString()));
		}

		public void NullableConversion2()
		{
			var movement = (Movement) BindObject(typeof(Movement), "mov");
			
			RenderText(String.Format("incoming {0} {1}", movement.Name, movement.Amount));
		}

		public void ArrayBinding()
		{
			var user = (User2) BindObject(typeof(User2), "user");

			RenderText(user.ToString());
			
			foreach(var id in user.Roles)
			{
				RenderText(" " + id);
			}
			foreach(var id in user.Permissions)
			{
				RenderText(" " + id);
			}
		}

		public void CalculateUtilizationByDay()
		{
			var tp1 = (TimePoint) BindObject(typeof(TimePoint), "tp1");
			var tp2 = (TimePoint) BindObject(typeof(TimePoint), "tp2");
			
			RenderText(tp1.ToString());
			RenderText(tp2.ToString());
		}
	}
}
