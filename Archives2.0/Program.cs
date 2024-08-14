using Archives2._0.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Archives2._0.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});



builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();



builder.Services.AddSingleton<AzureResourceServices>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var clientId = configuration["AzureAd:ClientId"];
    var clientSecret = configuration["AzureAd:ClientSecret"];
    var tenantId = configuration["AzureAd:TenantId"];
    var subscriptionId = "a1d1ccd2-cdb0-4acd-ab00-bd1f77376278"; // replace with your subscription id
    string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=kamvatest;AccountKey=9idC6E+3U5mKPopRDWvy4UtCg8VFGXUAB4Ia3PuTCvlIi3kOSye+SFaoNILWRacxNYiu0MziDDpV+AStQe7WjA==;EndpointSuffix=core.windows.net";

    return new AzureResourceServices(clientId, clientSecret, tenantId, subscriptionId, storageConnectionString);
});











var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var path = context.Request.Path;
        if (path == "/" || path == "/Home/Index")
        {
            context.Response.Redirect("/Home/Dashboard");
            return;
        }
    }
    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "storageDownload",
    pattern: "Storage/Download",
    defaults: new { controller = "Storage", action = "Download" });
app.MapRazorPages();

app.Run();
