using Microsoft.AspNetCore.Identity;
using Ridged.Domain.Enums;

namespace Ridged.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // JWT & Refresh Token
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        // Email Verification
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiryTime { get; set; }
        
        // Password Reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryTime { get; set; }

        // Full name computed property
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
