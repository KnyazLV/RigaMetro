using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MetroConnection");
builder.Services.AddDbContext<MetroDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMvc();
builder.Services.AddScoped<DistanceSeeder>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.Configure<RazorViewEngineOptions>(options => {
    options.ViewLocationFormats.Clear();
    options.ViewLocationFormats.Add("/Web/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/Web/Views/Shared/{0}.cshtml");
});


var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.MapDefaultControllerRoute();

using (var scope = app.Services.CreateScope()) {
    var seeder = scope.ServiceProvider.GetRequiredService<DistanceSeeder>();
    await seeder.SeedAsync();
}

app.Run();