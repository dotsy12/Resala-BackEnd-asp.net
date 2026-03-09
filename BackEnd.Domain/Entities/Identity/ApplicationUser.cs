// Domain/Entities/Identity/ApplicationUser.cs
using BackEnd.Domain.Entities.Identity.AuthenticationHepler;
using Microsoft.AspNetCore.Identity;

namespace BackEnd.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public ICollection<RefreshToken> refreshTokens { get; set; } = new List<RefreshToken>();
    }
}