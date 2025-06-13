using DatabaseManager.Seeders;
using Microsoft.EntityFrameworkCore;
using SharedObjects.AppDbContext;
using DatabaseManager.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var dbHost = GetRequiredEnvironmentVariable("DB_HOST", builder.Environment.IsProduction());
var dbDatabase = GetRequiredEnvironmentVariable("DB_DATABASE", builder.Environment.IsProduction());
var dbUsername = GetRequiredEnvironmentVariable("DB_USERNAME", builder.Environment.IsProduction());
var dbPassword = GetRequiredEnvironmentVariable("DB_PASSWORD", builder.Environment.IsProduction());


if (builder.Environment.IsDevelopment())
{
    dbHost ??= "database";
    dbDatabase ??= "project";
    dbUsername ??= "postgres";
    dbPassword ??= "P@ssword123!";
}

var connectionString = $"Host={dbHost};Database={dbDatabase};Username={dbUsername};Password={dbPassword}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("DatabaseManager"))
);

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

using var serviceScope = app.Services.CreateScope();
var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

var pendingMigrations = context.Database.GetPendingMigrations().ToList();

if (pendingMigrations.Count != 0)
{
    Console.WriteLine($"Applying pending migrations.");
    foreach (var migration in pendingMigrations)
    {
        Console.WriteLine($"- {migration}");
    }

    try
    {
        context.Database.Migrate();
        Console.WriteLine("Migrations completed successfully.");
        DatabaseSeeder.SeedFromSqlFile(context, Constants.FilePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during migration: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Error details: {ex.InnerException.Message}");
        }
    }
}
else
{
    Console.WriteLine("No pending migrations.");
}

app.MapHealthChecks("/health");

app.Run();
return;

static string? GetRequiredEnvironmentVariable(string name, bool isProduction)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (!string.IsNullOrEmpty(value)) return value;
    if (isProduction)
    {
        throw new InvalidOperationException($"{name} environment variable is required");
    }
    return value;
}
