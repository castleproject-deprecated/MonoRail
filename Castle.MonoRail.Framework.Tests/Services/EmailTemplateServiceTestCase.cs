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

using System.Linq;

namespace Castle.MonoRail.Framework.Tests.Services
{
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using NUnit.Framework;
	using Rhino.Mocks;
	using Test;
	using Is=Rhino.Mocks.Constraints.Is;

	[TestFixture]
	public class EmailTemplateServiceTestCase
	{
		private readonly MockRepository mockRepository = new MockRepository();
		private EmailTemplateService service;
		private IViewEngineManager viewEngineManagerMock;
		private StubEngineContext engineContext;
		private DummyController controller;
		private ControllerContext controllerContext;

		[SetUp]
		public void Init()
		{
			viewEngineManagerMock = mockRepository.DynamicMock<IViewEngineManager>();

			service = new EmailTemplateService(viewEngineManagerMock);

			engineContext = new StubEngineContext(null, null, null, null);
			controller = new DummyController();
			controllerContext = new ControllerContext();
		}

		[Test]
		public void RenderMailMessage_BackwardCompatibility_PassOnControllerContext()
		{
			const string templateName = "welcome";

			using(mockRepository.Record())
			{
				Expect.Call(viewEngineManagerMock.HasTemplate("mail\\" + templateName)).Return(true);

				viewEngineManagerMock.Process(templateName, null, engineContext, controller, controllerContext);
				LastCall.Constraints(
					Is.Equal("mail\\" + templateName),
					Is.Anything(),
					Is.Same(engineContext),
					Is.Same(controller),
					Is.Same(controllerContext));
			}

			using(mockRepository.Playback())
			{
				service.RenderMailMessage(templateName, engineContext, controller, controllerContext, false);
			}
		}

		[Test]
		public void RenderMailMessage_BackwardCompatibility_UsesTemplateNameAsItIsIfStartsWithSlash()
		{
			const string templateName = "/emailtemplates/welcome";

			using(mockRepository.Record())
			{
				Expect.Call(viewEngineManagerMock.HasTemplate(templateName)).Return(true);

				viewEngineManagerMock.Process(templateName, null, engineContext, controller, controllerContext);
				LastCall.Constraints(
					Is.Equal(templateName),
					Is.Anything(),
					Is.Same(engineContext),
					Is.Same(controller),
					Is.Same(controllerContext));
			}

			using(mockRepository.Playback())
			{
				service.RenderMailMessage(templateName, engineContext, controller, controllerContext, false);
			}
		}

		[Test]
		public void RenderMailMessage_InvokesViewEngineManager()
		{
			const string templateName = "welcome";
			var parameters = new Hashtable();

			using(mockRepository.Record())
			{
				Expect.Call(viewEngineManagerMock.HasTemplate("mail\\" + templateName)).Return(true);

				viewEngineManagerMock.Process(templateName, "layout", null, null);
				LastCall.Constraints(
					Is.Equal("mail\\" + templateName),
					Is.Equal("layout"),
					Is.Anything(),
					Is.Anything());
			}

			using(mockRepository.Playback())
			{
				service.RenderMailMessage(templateName, "layout", parameters);
			}
		}

		[Test]
		public void RenderMailMessage_MessageIsConstructedCorrectly()
		{
			const string templateName = "welcome";
			var parameters = new Hashtable();

			using(mockRepository.Record())
			{
				Expect.Call(viewEngineManagerMock.HasTemplate("mail\\" + templateName)).Return(true);

				Expect.Call(() => viewEngineManagerMock.Process(templateName, "layout", null, null))
					.Constraints(
						Is.Equal("mail\\" + templateName),
						Is.Equal("layout"),
						Is.Anything(),
						Is.Anything())
					.Do(new Render(RendersEmail));
			}

			using(mockRepository.Playback())
			{
				var message = service.RenderMailMessage(templateName, "layout", parameters);

				Assert.AreEqual("hammett@noemail.com", message.To[0].Address);
				Assert.AreEqual("copied@noemail.com", message.CC[0].Address);
				Assert.AreEqual("bcopied@noemail.com", message.Bcc[0].Address);
				Assert.AreEqual("contact@noemail.com", message.From.Address);
				Assert.AreEqual("Hello!", message.Subject);
				Assert.AreEqual("This is the\r\nbody\r\n", message.Body);
				Assert.AreEqual(1, message.Headers.Count);
#if !DOTNET40
				Assert.AreEqual("Test Reply", message.ReplyTo.DisplayName);
				Assert.AreEqual("replyto@noemail.com", message.ReplyTo.Address);
#else
				Assert.AreEqual(1, message.ReplyToList.Count());
				Assert.AreEqual("Test Reply", message.ReplyToList.First().DisplayName);
				Assert.AreEqual("replyto@noemail.com", message.ReplyToList.First().Address);
#endif
			}
		}

		[Test]
		public void RenderMailMessage_MessageFormatIsHtmlEvenIfHtmlTagHasAttributes()
		{
			const string templateName = "welcome";
			var parameters = new Hashtable();

			using (mockRepository.Record())
			{
				Expect.Call(viewEngineManagerMock.HasTemplate("mail\\" + templateName)).Return(true);

				Expect.Call(() => viewEngineManagerMock.Process(templateName, "layout", null, null))
					.Constraints(
						Is.Equal("mail\\" + templateName),
						Is.Equal("layout"),
						Is.Anything(),
						Is.Anything())
					.Do(new Render(RendersHtmlEmail));
			}

			using (mockRepository.Playback())
			{
				var message = service.RenderMailMessage(templateName, "layout", parameters);

				Assert.IsTrue(message.IsBodyHtml);
			}
		}

		public delegate void Render(string templateName, string layoutName,
		                            TextWriter output, IDictionary<string, object> parameters);

		public static void RendersEmail(string templateName, string layoutName, TextWriter output,
		                                IDictionary<string, object> parameters)
		{
			output.WriteLine("to: hammett@noemail.com");
			output.WriteLine("cc: copied@noemail.com");
			output.WriteLine("bcc: bcopied@noemail.com");
			output.WriteLine("from: contact@noemail.com");
			output.WriteLine("reply-to: Test Reply <replyto@noemail.com>");
			output.WriteLine("subject: Hello!");
			output.WriteLine("X-something: Mime-super-content");
			output.WriteLine("This is the");
			output.WriteLine("body");
		}

		public static void RendersHtmlEmail(string templateName, string layoutName, TextWriter output,
										IDictionary<string, object> parameters)
		{
			output.WriteLine("to: hammett@noemail.com");
			output.WriteLine("cc: copied@noemail.com");
			output.WriteLine("bcc: bcopied@noemail.com");
			output.WriteLine("from: contact@noemail.com");
			output.WriteLine("reply-to: Test Reply <replyto@noemail.com>");
			output.WriteLine("subject: Hello!");
			output.WriteLine("X-something: Mime-super-content");
			output.WriteLine("<html lang=\"en-US\" xml:lang=\"en-US\" xmlns=\"http://www.w3.org/1999/xhtml\">");
			output.WriteLine("<head>Html message test</head>");
			output.WriteLine("<body>This is the body</body>");
			output.WriteLine("</html>");
		}

		private class DummyController : Controller
		{
		}
	}
}