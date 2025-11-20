using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Ridged.Application;
using Ridged.Application.Common.Settings;
using Ridged.Domain.Entities;
using Ridged.Infrastructure;
using Ridged.Infrastructure.Context;
using Ridged.Infrastructure.Data;
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
builder.Services.AddRateLimiter(options => {
    options.AddSlidingWindowLimiter("Fixed", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(10);
        opt.SegmentsPerWindow = 2;
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// Seed roles on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Ridged.Infrastructure.Extensions.RoleSeeder.SeedRolesAsync(services);
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    await SellerSeeder.SeedAsync(context, userManager, configuration, logger);
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
    
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Map Auth Endpoints
app.MapAuthEndpoints();
app.Run();