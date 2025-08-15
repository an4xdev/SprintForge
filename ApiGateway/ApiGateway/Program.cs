using System.Text;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; });

// TODO: think later on minimal api?
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "https://localhost", Description = "HTTPS Local" },
            new() { Url = "http://localhost", Description = "HTTP Local" }
        };
        return Task.CompletedTask;
    });
});

static string GetRequiredEnvironmentVariable(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    return string.IsNullOrEmpty(value) ? throw new InvalidOperationException($"{name} environment variable is required") : value;
}

var jwtIssuer = GetRequiredEnvironmentVariable("JWT_ISSUER");
var jwtAudience = GetRequiredEnvironmentVariable("JWT_AUDIENCE");
var jwtToken = GetRequiredEnvironmentVariable("JWT_TOKEN");
var redisHost = GetRequiredEnvironmentVariable("REDIS_HOST");
var redisPort = GetRequiredEnvironmentVariable("REDIS_PORT");
var redisConnectionString = $"{redisHost}:{redisPort}";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtToken))
        };
    });

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISendRequestService, SendRequestService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}"));

builder.Services.AddScoped<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});
// https://thecodeman.net/posts/hybrid-cache-in-aspnet-core
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024; // 1 MB
    options.MaximumKeyLength = 512;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(2),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnectionString; });

builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString);

var app = builder.Build();

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SCALAR") == "true")
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "MicroService API Gateway";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health");

app.Run();