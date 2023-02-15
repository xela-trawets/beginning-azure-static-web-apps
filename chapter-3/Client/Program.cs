using Client;
using Client.Services;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using StaticWebAppAuthentication.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.Configuration.AddJsonFile($"appsettings.Development.json", optional: true);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) });
var baseAddress = new Uri(
//	"http://localhost:7071"
	//builder.Configuration["API_Prefix"]	
	builder.HostEnvironment.BaseAddress
	);
Console.WriteLine($"baseAddress <= {baseAddress}");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress});
builder.Services.AddScoped<BlogPostSummaryService>();
builder.Services.AddScoped<BlogPostService>();
builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
