using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Assignment3BAD.Models
{
    public class Dish
    {
        public int DishID { get; set; }
        public required string Name { get; set; }

        public int Quantity { get; set; }

    [PositivePrice]
    public decimal Price {get; set;}
    
    [JsonIgnore]
    public DateTime StartTime { get; set; }

        [JsonIgnore]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("startTime")]
        public string StartTimeFormatted => StartTime.ToString("ddMMyyyy HHmm");

        [JsonPropertyName("endTime")]
        public string EndTimeFormatted => EndTime.ToString("ddMMyyyy HHmm");

        [JsonIgnore]
        public Cook? Cook { get; set; }
        public int CookID { get; set; }

        public ICollection<Order>? Orders { get; set; }

        public ICollection<DishOrder>? DishOrders { get; set; }
    }

    // Custom validation attribute to ensure price is non-negative
    public class PositivePrice : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            // Check if the value is a decimal and if it's negative
            if (value is decimal price && price < 0)
            {
                return new ValidationResult("Price must be a non-negative value.");
            }
            return ValidationResult.Success;
        }
    }
}
