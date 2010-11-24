#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

namespace Castle.MonoRail.Mvc.Rest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Web;
    using Castle.MonoRail;

    [Export]
    public class ContentNegotiator
    {
        private readonly HttpRequestBase _requestBase;

        [ImportingConstructor]
        public ContentNegotiator(HttpRequestBase requestBase)
        {
            _requestBase = requestBase;
        }

        public void Allow(params ContentType[] types)
        {
            throw new NotImplementedException();
        }

        public ActionResult Respond(Action<RequestFormat> eval)
        {
            // Headers we should care about
            // Accept
            // If-Match
            // If-Modified-Since
            // If-None-Match
            // If-Unmodified-Since

            var acceptance = _requestBase.AcceptTypes;
            var types = new Dictionary<ContentType, Func<ActionResult>>();
            var format = new RequestFormat(types);
            eval(format);
            ContentType contentType;

            if (acceptance.Length == 1 && acceptance[0] != "*/*")
            {
                // fx is requesting
                var selector = acceptance[0];
                contentType = new ContentType(selector); // ignoring */* and q value pairs. IOW: naive implementation
            }
            else
            {
                // assumes browser, which is unreliable on accept header. Falls back to html
                contentType = ContentType.Html;
            }

            Func<ActionResult> resultFunc;
            if (types.TryGetValue(contentType, out resultFunc))
            {
                return resultFunc();
            }

            // throw what when content negotion fails?
            return null;
        }
    }

//    public class RespondToAttribute : Attribute
//    {
//    }

    public class RequestFormat
    {
        private readonly Dictionary<ContentType, Func<ActionResult>> _ct2Func;

        public RequestFormat(Dictionary<ContentType, Func<ActionResult>> ct2Func)
        {
            _ct2Func = ct2Func;
        }

        public void Html()
        {
            Add(ContentType.Html, () => new ViewResult());
        }
        public void Html(Func<ActionResult> eval)
        {
            Add(ContentType.Html, eval);
        }

        public void Xml()
        {
            Add(ContentType.Xml, () => new XmlResult());
        }
        public void Xml(Func<ActionResult> eval)
        {
            Add(ContentType.Xml, eval);
        }
        
        public void JSon()
        {
            Add(ContentType.JSon, () => new JSonResult());
        }
        public void JSon(Func<ActionResult> eval)
        {
            Add(ContentType.JSon, eval);
        }

        public void Add(ContentType contentType, Func<ActionResult> eval)
        {
            EnsureCanSetForMimeType(contentType);
            _ct2Func[contentType] = eval;
        }

        private void EnsureCanSetForMimeType(ContentType contentType)
        {
            if (_ct2Func.ContainsKey(contentType))
            {
                throw new InvalidOperationException();
            }
        }
    }
}