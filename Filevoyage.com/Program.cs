using Azure.Storage;
using Azure.Storage.Blobs;
using Filevoyage.com.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// MVC + Attribute routing
builder.Services.AddControllersWithViews();

// CosmosClient
builder.Services.AddSingleton(sp => {
    var connStr = builder.Configuration["CosmosDb__ConnectionString"];
    return new CosmosClient(connStr);
});


// CosmosDbService (client, databaseName, containerName)
builder.Services.AddSingleton(sp => {
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

// Activamos Attribute Routing
app.MapControllers();

// Ruta corta: `https://miapp/{shortCode}` → HomeController.RedirectToBlob
// en Program.cs, justo antes de tu ruta "default"
app.MapControllerRoute(
  name: "shortlink",
  pattern: "{shortCode}",
  defaults: new { controller = "Download", action = "DownloadPage" }
);


// MVC convencional
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
