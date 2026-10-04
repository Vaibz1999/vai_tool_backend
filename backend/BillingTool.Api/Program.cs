using BillingTool.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// ---------------------------------------------------------
// Configuration
// ---------------------------------------------------------
builder.Configuration.Sources.Clear();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: false)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: false)
    .AddEnvironmentVariables();

// ---------------------------------------------------------
// Controllers
// ---------------------------------------------------------
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------------------------------------------------
// Database - Supabase PostgreSQL
// ---------------------------------------------------------
builder.Services.AddDbContext<BillingDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ---------------------------------------------------------
// CORS
// ---------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("https://billing-frontend.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------------------------------------------------------
// Build application
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// Swagger
// ---------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI();

// ---------------------------------------------------------
// Routing
// ---------------------------------------------------------
app.UseRouting();

// ---------------------------------------------------------
// CORS
// IMPORTANT: Must be before Authorization and Controllers
// ---------------------------------------------------------
app.UseCors("AllowFrontend");

// ---------------------------------------------------------
// Authorization
// ---------------------------------------------------------
app.UseAuthorization();

// ---------------------------------------------------------
// Controllers
// ---------------------------------------------------------
app.MapControllers();

// ---------------------------------------------------------
// Database migrations
// Only run automatically in Development
// ---------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<BillingDbContext>();

    db.Database.Migrate();
}

// ---------------------------------------------------------
// Render PORT
// ---------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrEmpty(port))
{
    app.Run($"http://0.0.0.0:{port}");
}
else
{
    app.Run();
}