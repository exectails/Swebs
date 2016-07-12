using NHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers
{
	public class RawOutput : IRequestHandler
	{
		public string ContentType { get; private set; }

		public RawOutput(string contentType)
		{
			this.ContentType = contentType;
		}

		public void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
		{
			var request = args.Request;
			var response = args.Response;

			// Set content type
			response.ContentType = this.ContentType;

			// Send file
			using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var output = response.OutputStream)
			{
				fs.CopyTo(output);
			}
		}
	}
}
