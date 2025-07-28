using Azure.Storage;
using Azure.Storage.Blobs;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// MVC + attribute routing
builder.Services.AddControllersWithViews();

// CosmosClient
builder.Services.AddSingleton(sp =>
{
    var connStr = builder.Configuration["CosmosDb:ConnectionString"];
    return new CosmosClient(connStr);
});

// CosmosDbService (client, databaseName, containerName)
builder.Services.AddSingleton(sp =>
{
    var cfg = builder.Configuration.GetSection("CosmosDb");
    var client = sp.GetRequiredService<CosmosClient>();
    return new CosmosDbService(
        client,
        cfg["DatabaseName"]!,
        cfg["ContainerName"]!
    );
});

// Blob Storage
builder.Services.AddSingleton<AzureStorageService>();

var app = builder.Build();

// Proxy headers para Azure App Service
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

// Seguridad con encabezados HTTP
app.Use(async (context, next) =>
{
    const string csp =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' https://fonts.googleapis.com; " +
        "img-src 'self' data:; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    context.Response.Headers["Content-Security-Policy"] = csp;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=()";
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

    await next();
});

// pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "shortlink",
    pattern: "{shortCode}",
    defaults: new { controller = "Download", action = "DownloadPage" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
