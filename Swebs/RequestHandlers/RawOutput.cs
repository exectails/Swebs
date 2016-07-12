using NHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers
{
	/// <summary>
	/// Request handler that sends the requested file to the client as is.
	/// </summary>
	public class RawOutput : IRequestHandler
	{
		/// <summary>
		/// The ContentType to set when responding.
		/// </summary>
		public string ContentType { get; private set; }

		/// <summary>
		/// Creatues new RawOutput instance for the given content type.
		/// </summary>
		/// <param name="contentType"></param>
		public RawOutput(string contentType)
		{
			this.ContentType = contentType;
		}

		/// <summary>
		/// Handles request by sending back the request file.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="requestPath"></param>
		/// <param name="localPath"></param>
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
