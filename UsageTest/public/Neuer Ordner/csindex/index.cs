using Swebs;
using Swebs.RequestHandlers.CSharp;
using System.Text;

public class CSharpIndexTest : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
	{
		var server = args.Context.Server;

		var sb = new StringBuilder();
		sb.AppendLine("C# index<br/>");
		sb.AppendLine("Test 1: " + server.GetLocalPath("Neuer Ordner/csindex/index.cs") + "<br/>");
		sb.AppendLine("Test 2: " + server.GetLocalPath("Neuer Ordner 2/csindex/index.cs") + "<br/>");
		sb.AppendLine("Test 3: " + server.GetLocalPath("css") + "<br/>");
		sb.AppendLine("Test 4: " + server.GetLocalPath("test.txt") + "<br/>");

		args.Response.Send(sb.ToString());
	}
}
