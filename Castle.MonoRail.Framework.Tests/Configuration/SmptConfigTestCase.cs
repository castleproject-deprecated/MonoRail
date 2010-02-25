// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Framework.Tests.Configuration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml;
	using Castle.MonoRail.Framework.Configuration;
	using NUnit.Framework;

	[TestFixture]
	public class SmtpConfigTestCase
	{
		private SmtpConfig DeserializeSmtpConfigFromXml(string configXml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(configXml);
			SmtpConfig config = new SmtpConfig();
			config.Deserialize(doc.DocumentElement);
			return config;
		}

		[Test]
		public void WhenConfigIsEmpty_DefaultValuesGetUsed()
		{
			string configXml = @"<monorail></monorail>";

			SmtpConfig config = DeserializeSmtpConfigFromXml(configXml);

			Assert.AreEqual("localhost", config.Host, "Host should be 'localhost' by when not speficied.");
			Assert.AreEqual(25, config.Port, "Port should be 25 by when not speficied.");
			Assert.IsNull(config.Username, "Username should be null by when not speficied.");
			Assert.IsNull(config.Password, "Password should be null by when not speficied.");
			Assert.IsFalse(config.UseSsl, "UseSsl should be false by when not speficied.");
		}

		[Test]
		public void WhenSpecifyingHostAndPort_TheyGetDeserialized()
		{
			string configXml = @"
<monorail 
	smtpHost=""smtp@samplehost.com"" 
	smtpPort=""112233"" 
	>
</monorail>";

			SmtpConfig config = DeserializeSmtpConfigFromXml(configXml);

			Assert.AreEqual("smtp@samplehost.com", config.Host);
			Assert.AreEqual(112233, config.Port);
		}

		[Test]
		public void WhenSpecifyingUsernameAndPassword_TheyGetDeserialized()
		{
			string configXml = @"
<monorail 
	smtpHost=""host"" 
	smtpPort=""123112233"" 
	smtpUsername=""username@samplehost.com"" 
	smtpPassword=""MySecretPassword22"" 
	>
</monorail>";

			SmtpConfig config = DeserializeSmtpConfigFromXml(configXml);

			Assert.AreEqual("username@samplehost.com", config.Username);
			Assert.AreEqual("MySecretPassword22", config.Password);
		}

		[Test]
		public void WhenSslIsFalseInXml_UseSslIsFalse()
		{
			string configXml = @"
<monorail 
	smtpHost=""host"" 
	smtpPort=""123"" 
	smtpUsername=""username"" 
	smtpPassword=""password"" 
	smtpUseSsl=""false""
	>
</monorail>";

			SmtpConfig config = DeserializeSmtpConfigFromXml(configXml);

			Assert.IsFalse(config.UseSsl);
		}

		[Test]
		public void WhenSslIsTrueInXml_UseSslIsTrue()
		{
			string configXml = @"
<monorail 
	smtpHost=""host"" 
	smtpPort=""123"" 
	smtpUsername=""username"" 
	smtpPassword=""password"" 
	smtpUseSsl=""true""
	>
</monorail>";

			SmtpConfig config = DeserializeSmtpConfigFromXml(configXml);

			Assert.IsTrue(config.UseSsl);
		}

	}
}
