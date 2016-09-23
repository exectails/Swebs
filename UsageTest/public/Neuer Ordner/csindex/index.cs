using Swebs;
using Swebs.RequestHandlers.CSharp;

public class CSharpIndexTest : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestPath, string localPath)
	{
		args.Response.Send("C# index");
	}
}
