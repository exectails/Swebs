using NHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers.CSharp
{
	/// <summary>
	/// Interface for C# scripts in the web folder to inherit from.
	/// </summary>
	public interface IScript
	{
		string Render(HttpRequestEventArgs args);
	}
}
