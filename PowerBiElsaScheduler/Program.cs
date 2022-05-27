using Elsa;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.SqlServer;
using Elsa.WorkflowTesting.Api.Extensions;
using PowerBiElsaScheduler.Models;
using PowerBiElsaScheduler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services
    .Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"))
    .Configure<PowerBiOptions>(builder.Configuration.GetSection("PowerBI"));

builder.Services
    .AddScoped<AzureAdService>()
    .AddScoped<PowerBIService>();

// ELSA Specific configuration
var elsaSection = builder.Configuration.GetSection("Elsa");
builder.Services
    .AddElsa(elsa => elsa
        .UseEntityFrameworkPersistence(ef =>
            ef.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))
        .AddConsoleActivities()
        .AddHttpActivities(elsaSection.GetSection("Server").Bind)
        .AddEmailActivities(elsaSection.GetSection("Smtp").Bind)
        .AddQuartzTemporalActivities()
        .AddWorkflowsFrom<Program>()
    )
    .AddElsaApiEndpoints()
    .AddWorkflowTestingServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles()
    .UseHttpActivities()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        endpoints.MapWorkflowTestHub();

        endpoints.MapFallbackToController("Index", "Elsa");
    });

app.Run();
