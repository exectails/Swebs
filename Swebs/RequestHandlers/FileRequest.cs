using NHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers
{
	public class FileRequest : IRequestHandler
	{
		public IDictionary<string, IRequestHandler> _handlers;

		public FileRequest(IDictionary<string, IRequestHandler> handlers)
		{
			_handlers = handlers;
		}

		public void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
		{
			var request = args.Request;
			var response = args.Response;
			var extension = Path.GetExtension(localPath);
			var handler = this.GetFileTypeHandler(extension);

			handler.Handle(args, requestPath, localPath);
		}

		private IRequestHandler GetFileTypeHandler(string extension)
		{
			IRequestHandler result;
			if (_handlers.TryGetValue(extension, out result))
				return result;

			if (_handlers.TryGetValue("", out result))
				return result;

			_handlers.Add("", result = new RawOutput("application/octet-stream"));

			return result;
		}
	}
}
