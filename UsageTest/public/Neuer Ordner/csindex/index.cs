using NHttp;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CSharpIndexTest : IScript
{
	public string Render(HttpRequestEventArgs args)
	{
		return "C# index";
	}
}
