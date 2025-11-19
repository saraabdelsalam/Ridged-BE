using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Ridged.Domain.Enums;

namespace Ridged.Infrastructure.Extensions
{
    public static class RoleSeeder {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            string[] roleNames = Enum.GetNames(typeof(UserRole));

            foreach ( var roleName in roleNames){
                if(! await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }
    }
}