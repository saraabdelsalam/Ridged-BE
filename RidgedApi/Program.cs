using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ridged.Application;
using Ridged.Application.Common.Settings;
using Ridged.Infrastructure;
using RidgedApi.Endpoints;
using RidgedApi.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Register Application Layer (MediatR, FluentValidation, etc.)
builder.Services.AddApplicationLayer();

// Register Infrastructure Layer (DbContext, Identity, Repositories, JWT Service)
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed roles on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Ridged.Infrastructure.Extensions.RoleSeeder.SeedRolesAsync(services);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handler (must be early in pipeline)
app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map Auth Endpoints
app.MapAuthEndpoints();

app.Run();