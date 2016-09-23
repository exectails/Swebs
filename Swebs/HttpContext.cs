using System;
using System.Collections.Generic;
using System.Text;

namespace Swebs
{
	public class HttpContext
	{
		internal HttpContext(HttpClient client)
		{
			Request = new HttpRequest(client);
			Response = new HttpResponse(this);
		}

		public HttpRequest Request { get; private set; }

		public HttpResponse Response { get; private set; }
	}
}
