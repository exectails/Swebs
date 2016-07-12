using NHttp;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Class1 : IScript
{
	public string Render(HttpRequestEventArgs args)
	{
		var sb = new StringBuilder();
		Func<string, StringBuilder> echo = sb.Append;

		echo("test o.o");

		return sb.ToString();
	}
}
