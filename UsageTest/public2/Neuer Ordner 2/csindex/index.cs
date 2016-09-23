using Swebs;
using Swebs.RequestHandlers.CSharp;
using System.Text;

public class CSharpIndexTest2 : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
	{
		var sb = new StringBuilder();
		sb.AppendLine("C# index<br/>");

		args.Response.Send(sb.ToString());
	}
}
