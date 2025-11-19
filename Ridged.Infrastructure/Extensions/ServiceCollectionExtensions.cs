using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ridged.Application.Common.Interfaces;
using Ridged.Domain.Entities;
using Ridged.Infrastructure.Context;
using Ridged.Infrastructure.Repository;
using Ridged.Infrastructure.Services;

namespace Ridged.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("AppConnection"),
                    x => x.MigrationsAssembly("Ridged.Infrastructure")
                );
            });

            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentityCore<User>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Sign in settings
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<IdentityRole<int>>>();

            // Register password hasher
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Register services
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
