using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ridged.Domain.Entities;
using Ridged.Domain.Enums;
using Ridged.Infrastructure.Context;

namespace Ridged.Infrastructure.Data
{
    public static class SellerSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<User> userManager,
            IConfiguration configuration,
            ILogger? logger = null)
        {
            // Get configuration values
            var sellerEmail = configuration["DefaultAccounts:Seller:Email"] ?? "seller@ridged.com";
            var password = configuration["DefaultAccounts:Seller:Password"];

            // Validate password is provided
            if (string.IsNullOrEmpty(password))
            {
                logger?.LogWarning("Seller password not configured. Skipping seller account creation.");
                return;
            }

            // Check if seller already exists
            var existingSeller = await userManager.FindByEmailAsync(sellerEmail);

            if (existingSeller != null)
            {
                logger?.LogInformation("Seller account already exists: {Email}", sellerEmail);
                return;
            }

            // Create new seller account
            var seller = new User
            {
                FirstName = "Sara",
                LastName = "Seller",
                Email = sellerEmail,
                UserName = sellerEmail,
                EmailConfirmed = true,
                Role = UserRole.Seller,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // UserManager.CreateAsync will automatically hash the password using PBKDF2
            var result = await userManager.CreateAsync(seller, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(seller, UserRole.Seller.ToString());
                logger?.LogInformation("✅ Seller account created successfully: {Email}", sellerEmail);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger?.LogError("❌ Failed to create seller account: {Errors}", errors);
            }
        }
    }
}