using Swebs;
using Swebs.RequestHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UsageTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var webConf = new Configuration();
			webConf.Port = 8181;
			//webConf.SourcePaths.Add("public2");
			webConf.SourcePaths.Add("public");
			webConf.AllowDirectoryListing = true;

			var server = new Server(webConf);
			server.HttpServer.RequestReceived += (s, e) => Console.WriteLine("[{0}] - {1}", e.Request.HttpMethod, e.Request.Path);
			server.HttpServer.UnhandledException += (s, e) => Console.WriteLine("[Error] {0}", e.Exception);
			server.Start();

			//Process.Start("http://127.0.0.1:8181");

			Console.WriteLine("Press [Return] to close.");
			Console.ReadLine();
		}
	}
}
