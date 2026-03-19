using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Cover.Web;
using Cover.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient(new CookieHandler { InnerHandler = new HttpClientHandler() })
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<CurrencyState>();

var host = builder.Build();
await host.RunAsync();
