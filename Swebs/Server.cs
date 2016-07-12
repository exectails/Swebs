using NHttp;
using Swebs.RequestHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Swebs
{
	public class Server : IDisposable
	{
		public HttpServer HttpServer { get; private set; }
		internal Configuration Conf { get; private set; }
		internal string RootPath { get; private set; }

		public IRequestHandler FileAccessHandler { get; set; }
		public IRequestHandler DirectoryListingHandler { get; set; }
		public IRequestHandler Error404Handler { get; set; }

		public Server(Configuration conf)
		{
			this.HttpServer = new HttpServer();
			this.Conf = conf;

			this.RootPath = Path.GetFullPath(this.Conf.RootPath).NormalizePath();

			this.FileAccessHandler = new FileRequest(this.Conf.FileTypeHandlers.ToDictionary(a => a.Key, b => b.Value));
			this.DirectoryListingHandler = new DirectoryListing(this.RootPath);
			this.Error404Handler = new Error404();
		}

		public void Dispose()
		{
			this.HttpServer.Dispose();
		}

		public void Start()
		{
			this.HttpServer.EndPoint = new IPEndPoint(this.Conf.Host, this.Conf.Port);
			this.HttpServer.RequestReceived += this.OnRequestReceived;
			this.HttpServer.Start();
		}

		private void OnRequestReceived(object sender, HttpRequestEventArgs args)
		{
			var requestPath = args.Request.Path.NormalizePath();
			requestPath = args.Server.UrlDecode(requestPath);
			requestPath = requestPath.Trim('/');

			var localPath = Path.Combine(this.RootPath, requestPath);
			localPath = args.Server.UrlDecode(localPath);
			localPath = localPath.NormalizePath();

			// Check scope
			var fullRequestPath = Path.GetFullPath(localPath).NormalizePath().TrimEnd('/');
			var fullRootPath = Path.GetFullPath(this.RootPath).NormalizePath().TrimEnd('/');
			if (!fullRequestPath.StartsWith(fullRootPath))
			{
				this.Error404Handler.Handle(args, requestPath, localPath);
				return;
			}

			// Check index files
			var fileExists = File.Exists(localPath);
			if (!fileExists)
				fileExists = this.TestIndexNames(this.Conf.IndexNames, ref localPath);

			// Handle request
			if (fileExists)
			{
				this.FileAccessHandler.Handle(args, requestPath, localPath);
			}
			else if (this.Conf.AllowDirectoryListing && Directory.Exists(localPath))
			{
				this.DirectoryListingHandler.Handle(args, requestPath, localPath);
			}
			else
			{
				this.Error404Handler.Handle(args, requestPath, localPath);
			}
		}

		private bool TestIndexNames(IList<string> names, ref string localPath)
		{
			if (Directory.Exists(localPath))
			{
				foreach (var name in this.Conf.IndexNames)
				{
					var indexPath = Path.Combine(localPath, name).Normalize();
					if (File.Exists(indexPath))
					{
						localPath = indexPath;
						return true;
					}
				}
			}

			return false;
		}
	}

	public static class StringExtension
	{
		public static string NormalizePath(this string path)
		{
			return path.Replace('\\', '/');
		}
	}
}
