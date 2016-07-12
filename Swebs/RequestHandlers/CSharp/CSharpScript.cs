using NHttp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
		private Dictionary<string, IScript> _cache = new Dictionary<string, IScript>();

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
			this.References.Add("Swebs.dll");
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
			string contents = "";
			if (script != null)
				contents = script.Render(args);
			else
				contents = "Failed to render page from script.";

			// Send
			using (var output = response.OutputStream)
			{
				var bytes = Encoding.UTF8.GetBytes(contents);
				output.Write(bytes, 0, bytes.Length);
			}
		}

		/// <summary>
		/// Compiles and caches script, returning the cached version if it
		/// exists.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private IScript GetCachedScript(string filePath)
		{
			filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

			IScript script;
			lock (_cache)
				_cache.TryGetValue(filePath, out script);

			if (script == null)
				script = GetScript(filePath);

			if (script == null)
				return null;

			lock (_cache)
				_cache[filePath] = script;

			return script;
		}

		/// <summary>
		/// Compiles script and returns it. Returns ErrorScript if
		/// compilation fails, and null if file doesn't contain any class
		/// that implements IScript.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		private IScript GetScript(string filePath)
		{
			var entryAssembly = Assembly.GetEntryAssembly();
			var asmPath = entryAssembly.Location;
			var asmDir = Path.GetDirectoryName(asmPath);

			var parameters = new CompilerParameters();
			foreach (var reference in this.References)
				parameters.ReferencedAssemblies.Add(reference);
			parameters.ReferencedAssemblies.Add(asmPath);
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.TreatWarningsAsErrors = false;
			parameters.WarningLevel = 0;
			parameters.IncludeDebugInformation = true;

			// Compile
			var provider = CodeDomProvider.CreateProvider("CSharp");
			var results = provider.CompileAssemblyFromFile(parameters, filePath);
			if (results.Errors.Count != 0)
				return new ErrorScript(results.Errors);

			var types = results.CompiledAssembly.GetTypes();
			var type = types.FirstOrDefault(a => a.GetInterfaces().Contains(typeof(IScript)) && !a.IsAbstract);
			if (type == null)
				return null;

			var script = Activator.CreateInstance(type) as IScript;

			return script;
		}
	}
}
