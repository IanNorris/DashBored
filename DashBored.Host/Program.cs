using DashBored.Host;
using DashBored.Host.Data;
using DashBored.Host.Models;
using DashBored.PluginApi;
using Newtonsoft.Json;

var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashBored");
Directory.CreateDirectory(folderPath);

var serverConfigPath = Path.Combine(folderPath, "Server.json");

if (!File.Exists(serverConfigPath))
{
	Console.Error.WriteLine($"Server config file {serverConfigPath} does not exist.");
	Environment.Exit(1);
}

var serverConfigContent = await File.ReadAllTextAsync(serverConfigPath);
var serverConfig = JsonConvert.DeserializeObject<Server>(serverConfigContent);

var pluginLoader = new PluginLoader();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel( k =>
{
	k.ListenAnyIP(serverConfig.Port, lo =>
	{
		if (serverConfig.Https)
		{
			lo.UseHttps();
		}
	});
});

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