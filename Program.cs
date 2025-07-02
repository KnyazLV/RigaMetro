using Microsoft.EntityFrameworkCore;
using RigaMetro.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MetroConnection");
builder.Services.AddDbContext<MetroDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMvc();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.MapDefaultControllerRoute();

app.Run();