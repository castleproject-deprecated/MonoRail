using System;

namespace Castle.MonoRail.Extension.OData3.Tests
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.OData;

	public class StubODataRequest : IODataRequestMessage
	{
		public IEnumerable<KeyValuePair<string, string>> Headers { get; private set; }
		public Uri Url { get; set; }
		public string Method { get; set; }

		public string GetHeader(string headerName)
		{
			throw new NotImplementedException();
		}

		public void SetHeader(string headerName, string headerValue)
		{
			throw new NotImplementedException();
		}

		public Stream GetStream()
		{
			throw new NotImplementedException();
		}
	}

    public class StubODataResponse : IODataResponseMessage
    {
        private NameValueCollection _headers = new NameValueCollection();
		private MemoryStream _stream = new MemoryStream();

	    public StubODataResponse()
	    {
		    this._statusCode = 200;
	    }

	    private int _statusCode;

        public string GetHeader(string headerName)
        {
            return _headers[headerName];
        }

        public void SetHeader(string headerName, string headerValue)
        {
            _headers[headerName] = headerValue;
        }

        public Stream GetStream()
        {
            return _stream;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _headers.AllKeys.Select(key => new KeyValuePair<string, string>(key, _headers[(string) key])); }
        }

        public int StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }

        public override string ToString()
        {
            var content = new StringBuilder();
            content.AppendLine(string.Join(";", _headers.AllKeys.Select(key => key + " " + _headers[key]).ToArray()));
            content.Append(Encoding.UTF8.GetString(_stream.GetBuffer(), 0, (int)_stream.Length));
            return content.ToString();
        }
    }
}