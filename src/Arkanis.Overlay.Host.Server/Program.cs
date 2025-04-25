using Arkanis.Overlay.Host.Server.Components;
using Arkanis.Overlay.Host.Server.Services;
using Arkanis.Overlay.Infrastructure;
using Arkanis.Overlay.Infrastructure.Data;
using Arkanis.Overlay.Infrastructure.Data.Extensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddHttpClient();

builder.Services.AddMemoryCache();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services
    .AddServerOverlayControls()
    .AddInfrastructure()
    .AddInfrastructureConfiguration(builder.Configuration);

var app = builder.Build();

await app.MigrateDatabaseAsync<OverlayDbContext>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
