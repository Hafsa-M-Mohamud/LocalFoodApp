using Microsoft.AspNetCore.Identity;

namespace Assignment3BAD.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Role { get; set; }

        // Relation til Cook
        public Cook? Cook { get; set; }

        // Relation til Cyclist
        public Cyclist? Cyclist { get; set; }
    }
}
