using Microsoft.EntityFrameworkCore;
using RigaMetro.Data;
using RigaMetro.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MetroConnection");
builder.Services.AddDbContext<MetroDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMvc();
builder.Services.AddScoped<DistanceSeeder>();

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


app.Run();