using NHttp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers.CSharp
{
	/// <summary>
	/// Script used when a C# script fails to compile.
	/// </summary>
	public class ErrorScript : IScript
	{
		private CompilerErrorCollection _errors;

		/// <summary>
		/// Creates new instance of ErrorScript for the given errors.
		/// </summary>
		/// <param name="errors"></param>
		public ErrorScript(CompilerErrorCollection errors)
		{
			_errors = errors;
		}

		/// <summary>
		/// Renders page showing the errors.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public string Render(HttpRequestEventArgs args)
		{
			var sb = new StringBuilder();

			sb.AppendLine("<!DOCTYPE html>");
			sb.AppendLine("<html>");
			sb.AppendLine("<head>");
			sb.AppendLine("<title>Compilation failure</title>");
			sb.AppendLine("</head>");
			sb.AppendLine("<body>");
			sb.AppendLine("<h1>Error in script</h1>");
			sb.AppendFormat("<pre>");
			foreach (CompilerError error in _errors)
				sb.AppendFormat("{0}\n", error);
			sb.AppendFormat("</pre>");
			sb.AppendLine("</body>");
			sb.AppendLine("</html>");

			return sb.ToString();
		}
	}
}
