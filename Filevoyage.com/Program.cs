using Filevoyage.com.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<AzureStorageService>();
builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var config = builder.Configuration.GetSection("CosmosDb");
    return new CosmosClient(config["Account"], config["Key"]);
});

builder.Services.AddSingleton<CosmosDbService>(serviceProvider =>
{
    var config = builder.Configuration.GetSection("CosmosDb");
    var client = serviceProvider.GetRequiredService<CosmosClient>();
    return new CosmosDbService(client, config["DatabaseName"], config["ContainerName"]);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "shortlink",
        pattern: "{shortCode}",
        defaults: new { controller = "Home", action = "RedirectToBlob" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
