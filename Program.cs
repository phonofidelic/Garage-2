using System.Globalization;
using Garage_2;
using Garage_2.Data;
using Garage_2.Interfaces;
using Garage_2.Models.Entities;
using Garage_2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GarageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GarageContext") ?? throw new InvalidOperationException("Connection string 'GarageContext' not found.")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GarageContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddOptions<GarageConfig>()
    .BindConfiguration(nameof(GarageConfig))
    .ValidateDataAnnotations()
    .ValidateOnStart()
    .PostConfigure(options =>
    {
        // Set system culture from config
        CultureInfo systemCulture = new(options.SystemCulture);
        CultureInfo.DefaultThreadCurrentCulture = systemCulture;
        CultureInfo.DefaultThreadCurrentUICulture = systemCulture;
    });

builder.Services.AddScoped<IVehicleSearchService, VehicleSearchService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IParkingSessionService, ParkingSessionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Vehicles}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

await DbInitializer.SeedAsync(app);

app.Run();
