#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Routing.Tests
{
	using Castle.MonoRail.Routing;
	using Castle.MonoRail.Routing.Tests.Stubs;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class RouteMatchingTests
	{
		[Test]
		public void LiteralMatching_DoubleTerms_GeneratesCorrectUri()
		{
			const string path = "/something/else";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something/else");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/else");
		}
		
		[Test]
		public void VPath_LiteralMatching_DoubleTerms_GeneratesCorrectUri()
		{
			const string path = "/something/else";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/app/something/else", "/app");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/app/something/else");
		}

		[Test]
		public void LiteralAndGreedy_GreedyIsOptionalByDefault_GeneratesCorrectUri()
		{
			const string path = "/something/**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something");
		}

		[Test]
		public void LiteralAndGreedy_GreedyIsOptionalByDefaultButMatchesTrailingFwdSlash_GeneratesCorrectUri()
		{
			const string path = "/something/**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something/");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/");
		}

		[Test]
		public void LiteralAndGreedy_GreedyIsOptionalByDefaultButForDotMatchForCharIsRequired2_GeneratesCorrectUri()
		{
			const string path = "/something.**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something.");
			data.Uri.OriginalString.Should().Be("http://localhost:3333/something.");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something");
		}

		[Test]
		public void LiteralAndGreedy_GreedyIsOptionalByDefaultButForDotMatchForCharIsRequired3_GeneratesCorrectUri()
		{
			const string path = "/something.**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something.pdf");
			data.Uri.OriginalString.Should().Be("http://localhost:3333/something.");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something");
		}

		[Test]
		public void LiteralAndGreedy_GreedyMatch1Segment_GeneratesCorrectUri()
		{
			const string path = "/something/**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something/some");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/");
		}

		[Test]
		public void LiteralAndGreedy_GreedyMatch2Segments_GeneratesCorrectUri()
		{
			const string path = "/something/**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/something/some/1/");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/");
		}

		[Test]
		public void VPath_LiteralAndGreedy_GreedyMatch2Segments_GeneratesCorrectUri()
		{
			const string path = "/something/**";
			_router.Match(path, new DummyHandlerMediator());

			var data = _router.TryMatch("/app/something/some/1/", "/app");
			data.Uri.OriginalString.Should().Be("http://localhost:3333/app/something/");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/app/something/");
		}

		[Test]
		public void NestedRouteWithGreedyTerm_GeneratesCorrectUri()
		{
			_router.Match("/something",
				config =>
				{
					config.Invariables(dc => dc.Controller("Something"));
					config.Match("/$metadata", rc => rc.Invariables(dc => dc.Action("Metadata")));
					config.Match("/**");
				});

			var data = _router.TryMatch("/something/$metadata");
			data.Uri.OriginalString.Should().Be("http://localhost:3333/something/$metadata");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/$metadata");

			data = _router.TryMatch("/something/Products(0)");
			data.Uri.OriginalString.Should().Be("http://localhost:3333/something/");
			data.Uri.AbsoluteUri.Should().Be("http://localhost:3333/something/");
		}
	}
}
