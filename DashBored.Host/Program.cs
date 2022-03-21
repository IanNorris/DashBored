using DashBored.Host;
using DashBored.Host.Data;
using DashBored.Host.Models;
using Newtonsoft.Json;

var pluginLoader = new PluginLoader();
pluginLoader.LoadPlugins();

var builder = WebApplication.CreateBuilder(args);

var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashBored");
var configPath = Path.Combine(folderPath, "Layout.json");
var secretsPath = Path.Combine(folderPath, "Secrets.json");

if(!File.Exists(configPath))
{
	Console.Error.WriteLine($"Layout file {configPath} does not exist.");
	Environment.Exit(1);
}

var fileContent = File.ReadAllText(configPath);

var layout = JsonConvert.DeserializeObject<Layout>(fileContent);

var page = new Page(layout, pluginLoader);

var pages = new List<Page>()
{
	page
};

var pageService = new PageService(pages);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<PageService>(pageService);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
