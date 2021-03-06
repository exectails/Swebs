﻿using Swebs;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers.CSharp
{
	/// <summary>
	/// C# script request handler.
	/// </summary>
	/// <remarks>
	/// Allows for C# scripts to be called directly, with them creating the
	/// output. Similar in functionality to PHP on Apache.
	/// </remarks>
	public class CSharpScript : IRequestHandler
	{
		private Dictionary<string, IRequestHandler> _cache = new Dictionary<string, IRequestHandler>();
		private Dictionary<string, DateTime> _cacheLast = new Dictionary<string, DateTime>();
		private Regex _referencesRegex = new Regex(@"^\/\/#reference (?<fileName>.*)$", RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// References passed to the compiler.
		/// </summary>
		public HashSet<string> References { get; set; }

		/// <summary>
		/// Creates new instance of CSharpScript, setting some
		/// default references.
		/// </summary>
		public CSharpScript()
		{
			this.References = new HashSet<string>();
			this.References.Add("System.dll");
			this.References.Add("System.Core.dll");
			this.References.Add("System.Data.dll");
			this.References.Add("Microsoft.CSharp.dll");
			this.References.Add("System.Xml.dll");
			this.References.Add("System.Xml.Linq.dll");
		}

		/// <summary>
		/// Handles request by looking up, compiling, and caching the script
		/// and using it to render content to send back to the client.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="requestPath"></param>
		/// <param name="localPath"></param>
		public void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
		{
			var request = args.Request;
			var response = args.Response;

			// Set content type
			response.ContentType = "text/html";

			// Get contents
			var script = this.GetCachedScript(localPath);
			if (script == null)
			{
				var sb = new StringBuilder();

				sb.AppendLine("<!DOCTYPE html>");
				sb.AppendLine("<html>");
				sb.AppendLine("<head>");
				sb.AppendLine("<title>Compilation failure</title>");
				sb.AppendLine("</head>");
				sb.AppendLine("<body>");
				sb.AppendLine("<p>Failed to render page from script.</p>");
				sb.AppendLine("</body>");
				sb.AppendLine("</html>");

				response.Send(sb.ToString());

				return;
			}

			script.Handle(args, requestPath, localPath);
		}

		/// <summary>
		/// Compiles and caches script, returning the cached version if it
		/// exists.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private IRequestHandler GetCachedScript(string filePath)
		{
			filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

			// Try to load script from cache.
			IRequestHandler script = null;
			lock (_cache)
			{
				// Only load cached file if file hasn't changed since
				// last caching.
				DateTime last;
				if (_cacheLast.TryGetValue(filePath, out last) && (!File.Exists(filePath) || File.GetLastWriteTime(filePath) < last))
					_cache.TryGetValue(filePath, out script);
			}

			// If script wasn't found in cache, try to compile it.
			if (script == null)
			{
				// If compilation failed, return whatever was returned
				// (either null or ErrorScript).
				if (!this.GetScript(filePath, out script))
					return script;
			}

			// If a script was successfully found, cache it.
			lock (_cache)
			{
				_cacheLast[filePath] = DateTime.Now;
				_cache[filePath] = script;
			}

			return script;
		}

		/// <summary>
		/// Compiles script and returns it via out parameter. The boolean
		/// return value determines whether compilation was successful,
		/// with no errors and a resulting IScript.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private bool GetScript(string filePath, out IRequestHandler script)
		{
			script = null;

			// Source
			var source = File.ReadAllText(filePath);
			var fileReferences = this.FindReferences(source);

			// References
			var entryAssembly = Assembly.GetEntryAssembly();
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

			// Assemblies referenced in entry
			var referencedAssemblies = entryAssembly.GetReferencedAssemblies().Select(a => loadedAssemblies.SingleOrDefault(b => b.FullName == a.FullName));
			var references = referencedAssemblies.Where(a => a != null && !a.IsDynamic).Select(a => a.Location).ToList();

			// Default references
			foreach (var reference in this.References)
			{
				if (!references.Any(a => a.Contains(reference)))
					references.Add(reference);
			}

			// File references
			foreach (var reference in fileReferences)
			{
				if (!references.Any(a => a.Contains(reference)))
					references.Add(reference);
			}

			// Parameters
			var parameters = new CompilerParameters();
			foreach (var reference in references)
				parameters.ReferencedAssemblies.Add(reference);
			parameters.ReferencedAssemblies.Add(entryAssembly.Location);
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.TreatWarningsAsErrors = false;
			parameters.WarningLevel = 0;
			parameters.IncludeDebugInformation = true;

			// Compile
			var provider = CodeDomProvider.CreateProvider("CSharp");
			var results = provider.CompileAssemblyFromSource(parameters, source);
			if (results.Errors.Count != 0)
			{
				script = new ErrorScript(results.Errors);
				return false;
			}

			var types = results.CompiledAssembly.GetTypes();
			var type = types.FirstOrDefault(a => a.GetInterfaces().Contains(typeof(IRequestHandler)) && !a.IsAbstract);
			if (type == null)
				return false;

			script = Activator.CreateInstance(type) as IRequestHandler;

			return (script != null);
		}

		/// <summary>
		/// Returns all file names the script wants to reference.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		private List<string> FindReferences(string source)
		{
			var result = new List<string>();

			var matches = _referencesRegex.Matches(source);
			foreach (Match match in matches)
				result.Add(match.Groups["fileName"].Value.Trim());

			return result;
		}
	}
}
