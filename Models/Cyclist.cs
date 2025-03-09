namespace Assignment3BAD.Models
{
    public class Cyclist
    {
        public int CyclistID { get; set; } // Cyclist's ID
        public required string BikeType { get; set; }
        public required string PhoneNumber { get; set; }

        // Relationer
        public ICollection<Trip>? Trips { get; set; }
        public ICollection<RatingSystem>? RatingSystems { get; set; }
        public ICollection<CyclistStats>? CyclistStats { get; set; }

        // Relation til ApplicationUser
        public string? UserId { get; set; } // Fremmednøgle til ApplicationUser
        public ApplicationUser? User { get; set; } // Navigation property til ApplicationUser
    }
}
