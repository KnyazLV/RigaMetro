using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Resources;
using RigaMetro.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("MetroConnection");
builder.Services.AddDbContext<MetroDbContext>(options =>
    options.UseNpgsql(connectionString));

// Authentication Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Localization Configuration
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization(options => {
    options.DataAnnotationLocalizerProvider = (type, factory) => {
        var assemblyName = new AssemblyName(typeof(RigaMetro.Services.SharedResource).GetTypeInfo().Assembly.FullName);
        return factory.Create("SharedResource", assemblyName.Name);
    };
});
builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new List<CultureInfo> {
        new("ru-RU"),
        new("en-US"),
        new("lv-LV")
    };
    options.DefaultRequestCulture = new RequestCulture("en-US", "en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});

// Business Services
builder.Services.AddScoped<DistanceSeeder>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

// View Engine Configuration
builder.Services.Configure<RazorViewEngineOptions>(options => {
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Web/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Web/Views/Shared/{0}.cshtml");
});

var app = builder.Build();

// Environment-specific Configuration
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware Pipeline
app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);
app.UseAuthentication();
app.UseAuthorization();


// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

// Database Seeding
using (var scope = app.Services.CreateScope()) {
    var seeder = scope.ServiceProvider.GetRequiredService<DistanceSeeder>();
    await seeder.SeedAsync();
}

app.Run();
