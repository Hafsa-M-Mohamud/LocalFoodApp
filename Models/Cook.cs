namespace Assignment3BAD.Models
{
    public class Cook
    {
        public int CookID { get; set; }
        public required string Name { get; set; }
        public required string PhoneNumber { get; set; }

        //public required string CPR {get; set;} // removed CPR for M2
        public required string PhysicalAddress { get; set; }

        public required bool PassedCourse { get; set; }

        public ICollection<Dish>? Dishes { get; set; }
        public ICollection<RatingSystem>? RatingSystems { get; set; }

        // Relation til ApplicationUser
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

    }
}

