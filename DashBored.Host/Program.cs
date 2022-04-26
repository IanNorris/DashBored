using DashBored.Host;
using DashBored.Host.Data;
using DashBored.PluginApi;
using Microsoft.AspNetCore.Hosting.Server;

var pluginLoader = new PluginLoader();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<PluginLoader>();
builder.Services.AddSingleton<SecretService>();
builder.Services.AddSingleton<PageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Start();

var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
await app.Services.GetRequiredService<PageService>().LoadPages(serviceProvider);

await app.WaitForShutdownAsync();