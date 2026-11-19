using IPFront;
using IPFront.Service;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static System.Net.WebRequestMethods;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["WebApiURL:ApiBaseUrl"]
    ?? "https://localhost:7196" ?? throw new InvalidOperationException("ApiBaseUrl is missing in appsettings.json");
Console.WriteLine("apiBaseUrl" + apiBaseUrl);



// Register Typed HttpClients pointing to your saxafon API
builder.Services.AddHttpClient<UserApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ResourceApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ReservationApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<CurrentUserService>();
await builder.Build().RunAsync();
