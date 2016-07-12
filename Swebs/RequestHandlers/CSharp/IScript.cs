using NHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs.RequestHandlers.CSharp
{
	public interface IScript
	{
		string Render(HttpRequestEventArgs args);
	}
}
