using DashBored.Host;
using DashBored.Host.Data;

var pluginLoader = new PluginLoader();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<PluginLoader>();
builder.Services.AddSingleton<SecretService>();
builder.Services.AddSingleton<PageService>();

var app = builder.Build();

await app.Services.GetRequiredService<PageService>().LoadPages();

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
