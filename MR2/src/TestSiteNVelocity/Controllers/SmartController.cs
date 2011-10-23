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
	
	public class SmartController : SmartDispatcherController
	{
		public SmartController()
		{
		}

		public void StringMethod(string name)
		{
			RenderText("incoming " + name);
		}

		public void Complex(string strarg, int intarg, String[] strarray)
		{
			RenderText(String.Format("incoming {0} {1} {2}", strarg, intarg, String.Join(",", strarray)));
		}

		public void SimpleBind([DataBind("order")] Order order)
		{
			RenderText(String.Format("incoming {0}", order));
		}

		public void SimpleBindArray([DataBind("orders")] Order[] orders)
		{
			if (orders == null)
			{
				RenderText("Null array shouldn't be returned by databinder");
			}
			else
			{
				RenderText(String.Format("incoming {0}", orders.Length));
			}
		}

		public void ComplexBind([DataBind("order")] Order order, [DataBind("person")] Person person)
		{
			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindExcludePrice([DataBind("order", Exclude="order.Price")] Order order, [DataBind("person")] Person person)
		{
			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindExcludeName([DataBind("order", Exclude="order.Name")] Order order, [DataBind("person")] Person person)
		{
			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void ComplexBindWithPrefix([DataBind("order")] Order order, [DataBind("person")] Person person)
		{
			RenderText(String.Format("incoming {0} {1}", order, person));
		}

		public void FillingBehavior([DataBind("abc")] ClassWithInitializers clazz)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			RenderText(String.Format("incoming {0} {1} {2}", clazz.Name, clazz.Date1.ToShortDateString(), clazz.Date2.ToShortDateString()));
		}

		public void NullableConversion(double? amount)
		{
			RenderText(String.Format("incoming {0} {1}", amount.HasValue, amount));
		}

		public void NullableConversion2([DataBind("mov")] Movement movement)
		{
			RenderText(String.Format("incoming {0} {1}", movement.Name, movement.Amount));
		}

		public void ArrayBinding([DataBind("user")] User2 user)
		{
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

		public void CalculateUtilizationByDay([DataBind("tp1")] TimePoint tp1, [DataBind("tp2")] TimePoint tp2)
		{
			RenderText(tp1.ToString());
			RenderText(tp2.ToString());
		}
	}

	public class ClassWithInitializers
	{
		public ClassWithInitializers()
		{
			Name = "hammett";
			Date1 = DateTime.Now;
			Date2 = DateTime.Now.AddDays(1);
		}

		public string Name { get; set; }

		public DateTime Date1 { get; set; }

		public DateTime Date2 { get; set; }
	}

	public class Order
	{
		private String name;
		private int itemCount;
		private float price;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public int ItemCount
		{
			get { return itemCount; }
			set { itemCount = value; }
		}

		public float Price
		{
			get { return price; }
			set { price = value; }
		}

		public override string ToString()
		{
			return String.Format("{0} {1} {2}", name, itemCount, price);
		}
	}

	public class Person
	{
		String id;
		Contact contact;

		public string Id
		{
			get { return id; }
			set { id = value; }
		}

		public Contact Contact
		{
			get { return contact; }
			set { contact = value; }
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", id, contact);
		}
	}

	public class Contact
	{
		String email, phone;

		public Contact()
		{
		}

		public Contact(string email, string phone)
		{
			this.email = email;
			this.phone = phone;
		}

		public string Email
		{
			get { return email; }
			set { email = value; }
		}

		public string Phone
		{
			get { return phone; }
			set { phone = value; }
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", email, phone);
		}
	}

	public class Movement
	{
		public string Name { get; set; }

		public double? Amount { get; set; }
	}

	public class User2
	{
		String name; int[] roles; int[] permissions;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public int[] Roles
		{
			get { return roles; }
			set { roles = value; }
		}

		public int[] Permissions
		{
			get { return permissions; }
			set { permissions = value; }
		}

		public override string ToString()
		{
			return String.Format("User {0} {1} {2}", name, roles.Length, permissions.Length);
		}
	}

	public class TimePoint : IComparable
	{
		private int _hour;
		private int _minute;
		private int _second;
      
		public TimePoint()
		{
			
		}

		public TimePoint(int hour, int minute, int second)
		{
			_hour = hour;
			_minute = minute;
			_second = second;
		}

		public int Hour
		{
			get { return _hour; }
			set { _hour = value; }
		}

		public int Minute
		{
			get { return _minute; }
			set { _minute = value; }
		}

		public int Second
		{
			get { return _second; }
			set { _second = value; }
		}

		public override string ToString()
		{
			return String.Format(" {0}:{1}:{2} ", _hour, _minute, _second);
		}

		public int CompareTo(object obj)
		{
			throw new NotImplementedException();
		}
	}
}
