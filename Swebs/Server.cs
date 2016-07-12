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
	/// <summary>
	/// Wrapper around HttpServer that manages basic tasks.
	/// </summary>
	public class Server : IDisposable
	{
		/// <summary>
		/// The internal HttpServer.
		/// </summary>
		public HttpServer HttpServer { get; private set; }

		/// <summary>
		/// The server's currently loaded configuration.
		/// </summary>
		internal Configuration Conf { get; private set; }

		/// <summary>
		/// The absolute root path of the web server.
		/// </summary>
		internal string RootPath { get; private set; }

		/// <summary>
		/// The request handler for normal files.
		/// </summary>
		public IRequestHandler FileAccessHandler { get; set; }

		/// <summary>
		/// The request handler for directories.
		/// </summary>
		/// <remarks>
		/// If directory listing is disabled via the configuration,
		/// Error404Handler is called instead.
		/// </remarks>
		public IRequestHandler DirectoryListingHandler { get; set; }

		/// <summary>
		/// The request handler for non-existent or invalid files.
		/// </summary>
		public IRequestHandler Error404Handler { get; set; }

		/// <summary>
		/// Creates new Server instance.
		/// </summary>
		/// <param name="conf"></param>
		public Server(Configuration conf)
		{
			this.Conf = conf;
			this.RootPath = Path.GetFullPath(this.Conf.RootPath).NormalizePath();

			this.FileAccessHandler = new FileRequest(this.Conf.FileTypeHandlers.ToDictionary(a => a.Key, b => b.Value));
			this.DirectoryListingHandler = new DirectoryListing(this.RootPath);
			this.Error404Handler = new Error404();

			this.HttpServer = new HttpServer();
			this.HttpServer.EndPoint = new IPEndPoint(this.Conf.Host, this.Conf.Port);
			this.HttpServer.RequestReceived += this.OnRequestReceived;
		}

		/// <summary>
		/// Disposes used resources.
		/// </summary>
		public void Dispose()
		{
			this.HttpServer.Dispose();
		}

		/// <summary>
		/// Starts HTTP server.
		/// </summary>
		public void Start()
		{
			this.HttpServer.Start();
		}

		/// <summary>
		/// Called when a request comes in.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Checks given path is a directory that contains an index file.
		/// If so, the path is set to that index file's path and true
		/// is returned.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="localPath"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Replaces backward-slashes with forward-slashes.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string NormalizePath(this string path)
		{
			return path.Replace('\\', '/');
		}
	}
}
