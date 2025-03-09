using System.Text.Json.Serialization;

namespace Assignment3BAD.Models
{
    public class RatingSystem
    {
        public int RatingID { get; set; } // This will be the primary key

        public int DeliveryRating { get; set; }
        public int FoodRating { get; set; }

        [JsonIgnore]
        public Cook? Cook { get; set; }

        [JsonIgnore]
        public Customer? Customer { get; set; }

        [JsonIgnore]
        public Cyclist? Cyclist { get; set; }
        public int CookID { get; set; }
        public int CustomerID { get; set; }

        public int CyclistID { get; set; }
    }
}
