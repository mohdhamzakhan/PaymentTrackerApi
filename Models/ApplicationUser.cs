using Microsoft.AspNetCore.Identity;

namespace PaymentTrackerApi.Models
{
    /// <summary>
    /// Extends the built-in Identity user with the extra fields we need.
    /// Role membership itself is handled by ASP.NET Identity (AspNetUserRoles),
    /// not by a single "Role" column here - a user could technically hold
    /// more than one role, though in practice you'll usually assign one.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
