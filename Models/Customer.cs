namespace Assignment3BAD.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public required string Name { get; set; }
        public required string PhysicalAddress { get; set; }
        public required string PhoneNumber { get; set; }

        public required string PaymentOptions { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<RatingSystem>? RatingSystems { get; set; }
    }
}
