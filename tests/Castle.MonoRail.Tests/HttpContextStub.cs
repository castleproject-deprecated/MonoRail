#region License
//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Tests
{
    using System.Web;

    internal class HttpContextStub : HttpContextBase
    {
        public class HttpRequestStub : HttpRequestBase
        {
            private string _path;

            public void _SetPath(string v)
            {
                _path = v;
            }
            public override string Path
            {
                get { return _path; }
                    
            }
        }
        public class HttpResponseBaseStub : HttpResponseBase
        {
        }
        public class HttpServerUtilityStub : HttpServerUtilityBase
        {
            public override string HtmlEncode(string s)
            {
                return HttpUtility.HtmlEncode(s);
            }
        }

        public HttpRequestStub RequestStub = new HttpRequestStub();
        public HttpResponseBaseStub ResponseStub = new HttpResponseBaseStub();
        public HttpServerUtilityStub ServerUtilityStub = new HttpServerUtilityStub();

        public override HttpRequestBase Request
        {
            get { return RequestStub; }
        }

        public override HttpResponseBase Response
        {
            get { return ResponseStub; }
        }

        public override HttpServerUtilityBase Server
        {
            get { return ServerUtilityStub; }
        }
    }
}