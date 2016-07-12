using Swebs.RequestHandlers;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Swebs
{
	public class Configuration
	{
		public IPAddress Host { get; set; }
		public int Port { get; set; }

		public string RootPath { get; set; }
		public bool AllowDirectoryListing { get; set; }

		public List<string> IndexNames { get; set; }

		public Dictionary<string, IRequestHandler> FileTypeHandlers { get; set; }

		public Configuration()
		{
			this.Host = IPAddress.Any;
			this.Port = 80;

			this.RootPath = "./";
			this.AllowDirectoryListing = true;

			this.IndexNames = new List<string>();
			this.IndexNames.Add("index.htm");
			this.IndexNames.Add("index.html");

			this.FileTypeHandlers = new Dictionary<string, IRequestHandler>();

			this.FileTypeHandlers.Add("", new RawOutput("application/octet-stream"));

			this.FileTypeHandlers.Add(".jpg", new RawOutput("image/jpeg"));
			this.FileTypeHandlers.Add(".jpeg", new RawOutput("image/jpeg"));
			this.FileTypeHandlers.Add(".gif", new RawOutput("image/gif"));
			this.FileTypeHandlers.Add(".png", new RawOutput("image/png"));
			this.FileTypeHandlers.Add(".ico", new RawOutput("image/x-icon"));

			this.FileTypeHandlers.Add(".htm", new RawOutput("text/html"));
			this.FileTypeHandlers.Add(".html", new RawOutput("text/html"));
			this.FileTypeHandlers.Add(".js", new RawOutput("application/javascript"));
			this.FileTypeHandlers.Add(".json", new RawOutput("application/json"));
			this.FileTypeHandlers.Add(".css", new RawOutput("text/css"));

			this.FileTypeHandlers.Add(".xml", new RawOutput("text/xml"));
			this.FileTypeHandlers.Add(".txt", new RawOutput("text/plain"));
		}

		public IRequestHandler GetFileTypeHandler(string extension)
		{
			IRequestHandler result;
			if (this.FileTypeHandlers.TryGetValue(extension, out result))
				return result;

			if (this.FileTypeHandlers.TryGetValue("", out result))
				return result;

			this.FileTypeHandlers.Add("", result = new RawOutput("application/octet-stream"));

			return result;
		}
	}
}
