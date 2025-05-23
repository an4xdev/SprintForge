using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using AuthService.Services.Auth;
using AuthService.Services.File;
using AuthService.Services.Profile;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Minio;
using SharedObjects.AppDbContext;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

static string GetRequiredEnvironmentVariable(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrEmpty(value))
    {
        throw new InvalidOperationException($"{name} environment variable is required");
    }
    return value;
}


var dbHost = GetRequiredEnvironmentVariable("DB_HOST");
var dbDatabase = GetRequiredEnvironmentVariable("DB_DATABASE");
var dbUsername = GetRequiredEnvironmentVariable("DB_USERNAME");
var dbPassword = GetRequiredEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={dbHost};Database={dbDatabase};Username={dbUsername};Password={dbPassword}";

var jwtIssuer = GetRequiredEnvironmentVariable("JWT_ISSUER");
var jwtAudience = GetRequiredEnvironmentVariable("JWT_AUDIENCE");
var jwtToken = GetRequiredEnvironmentVariable("JWT_TOKEN");

var minioEndpoint = GetRequiredEnvironmentVariable("MINIO_ENDPOINT");
var minioAccessKey = GetRequiredEnvironmentVariable("MINIO_ACCESS_KEY_ID");
var minioSecretKey = GetRequiredEnvironmentVariable("MINIO_SECRET_ACCESS_KEY");
var minioBucket = GetRequiredEnvironmentVariable("MINIO_BUCKET");
var minioUsePathStyle = GetRequiredEnvironmentVariable("MINIO_USE_PATH_STYLE_ENDPOINT");


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtToken)),
            ValidateIssuerSigningKey = true
        };
    });



builder.Services.Configure<MinioSettings>(options =>
{
    options.Endpoint = minioEndpoint;
    options.AccessKey = minioAccessKey;
    options.SecretKey = minioSecretKey;
    options.Bucket = minioBucket;
    options.UsePathStyle = bool.Parse(minioUsePathStyle);
});

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
    return new MinioClient()
        .WithEndpoint(settings.Endpoint)
        .WithCredentials(settings.AccessKey, settings.SecretKey)
        .WithRegion("us-east-1")
        .WithSSL(false)
        .Build();
});

builder.Services.AddScoped<IAuthService, AuthService.Services.Auth.AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IFileService, MinioFileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SCALAR") == "true")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health");

app.Run();