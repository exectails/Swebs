using NHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swebs
{
	public interface IRequestHandler
	{
		void Handle(HttpRequestEventArgs args, string requestPath, string localPath);
	}
}
