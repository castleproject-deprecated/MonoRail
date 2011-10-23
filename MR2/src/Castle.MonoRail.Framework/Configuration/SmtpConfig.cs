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

namespace Castle.MonoRail.Framework.Configuration
{
	using System;
	using System.Xml;
	using System.Configuration;
	using System.Net.Configuration;
	using System.Web.Configuration;

	/// <summary>
	/// Represents the SMTP configuration
	/// on the configuration file
	/// </summary>
	public class SmtpConfig : ISerializedConfig
	{
		private String host = "localhost";
		private int port = 25;
		private String username;
		private String password;
		private bool useSsl;

		/// <summary>
		/// Deserializes the specified smtp section.
		/// </summary>
		/// <param name="section">The smtp section.</param>
		public void Deserialize(XmlNode section)
		{
			var smtpHostAtt = section.Attributes["smtpHost"];
			var smtpPortAtt = section.Attributes["smtpPort"];
			var smtpUserAtt = section.Attributes["smtpUsername"];
			var smtpPwdAtt = section.Attributes["smtpPassword"];
			var smtpSslAtt = section.Attributes["smtpUseSsl"];

			if (smtpHostAtt != null && smtpHostAtt.Value != String.Empty)
			{
				host = smtpHostAtt.Value;
			}
			if (smtpPortAtt != null && smtpPortAtt.Value != String.Empty)
			{
				port = int.Parse(smtpPortAtt.Value);
			}
			if (smtpUserAtt != null && smtpUserAtt.Value != String.Empty)
			{
				username = smtpUserAtt.Value;
			}
			if (smtpPwdAtt != null && smtpPwdAtt.Value != String.Empty)
			{
				password = smtpPwdAtt.Value;
			}
			if (smtpSslAtt != null && smtpSslAtt.Value != String.Empty)
			{
				useSsl = bool.Parse(smtpSslAtt.Value);
			}
		}

		 /// <summary>
		/// Uses the <c>system.net/mailSettings</c> section of the 
		/// web's config file (web.config) to populate this
		/// <see cref="SmtpConfig"/>
		/// </summary>
		public void ConfigureFromWebConfigFile()
		{
			Configuration config = null;
			if (System.Web.HttpRuntime.AppDomainAppId != null)
			{
				config = WebConfigurationManager.OpenWebConfiguration("~/web.config");
			}

			if (config == null)
			{
				// try to use app.config for current user
				config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);        
			}

			if (config == null)
			{
				// still not found => error!
				throw new ConfigurationErrorsException("Could not find application configuration file");
			}

			ConfigureFromConfig(config);
		}

		/// <summary>
		/// Uses the <c>system.net/mailSettings</c> section of the 
		/// provided <see cref="Castle.MonoRail.Framework.Configuration"/> to populate this
		/// <see cref="SmtpConfig"/>
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		public void ConfigureFromConfig(Configuration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException("configuration");
			}
			MailSettingsSectionGroup mailSettings = configuration.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;

			if (mailSettings != null && mailSettings.Smtp != null && mailSettings.Smtp.Network != null)
			{
				SmtpNetworkElement network = mailSettings.Smtp.Network;
				Host = network.Host;
				Password = network.Password;
				Username = network.UserName;
				Port = network.Port;
			}
			else
			{
				throw new ConfigurationErrorsException("Could not find the system.net/mailSettings/smtp/network element in the application configuration");
			}
		}

		/// <summary>
		/// Gets or sets the smtp host.
		/// </summary>
		/// <value>The host.</value>
		public String Host
		{
			get { return host; }
			set { host = value; }
		}

		/// <summary>
		/// Gets or sets the smtp port.
		/// </summary>
		/// <value>The port.</value>
		public int Port
		{
			get { return port; }
			set { port = value; }
		}

		/// <summary>
		/// Gets or sets the smtp username.
		/// </summary>
		/// <value>The username.</value>
		public String Username
		{
			get { return username; }
			set { username = value; }
		}

		/// <summary>
		/// Gets or sets the smtp password.
		/// </summary>
		/// <value>The password.</value>
		public String Password
		{
			get { return password; }
			set { password = value; }
		}

		/// <summary>
		/// Gets or sets whether to enable SSL.
		/// </summary>
		public bool UseSsl
		{
			get { return useSsl; }
			set { useSsl = value; }
		}
	}
}
