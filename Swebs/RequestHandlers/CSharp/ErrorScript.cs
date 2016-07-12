using NHttp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers.CSharp
{
	public class ErrorScript : IScript
	{
		private CompilerErrorCollection _errors;

		public ErrorScript(CompilerErrorCollection errors)
		{
			_errors = errors;
		}

		public string Render(HttpRequestEventArgs args)
		{
			var sb = new StringBuilder();

			sb.AppendFormat("<pre>");
			foreach (CompilerError error in _errors)
			{
				sb.AppendFormat("{0}\n", error);
			}
			sb.AppendFormat("</pre>");

			return sb.ToString();
		}
	}
}
