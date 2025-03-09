using System.Text.Json.Serialization;

namespace Assignment3BAD.Models
{
public class Order
{
    public int OrderID{get; set;}

    private DateTime _orderTime;

    [JsonIgnore]
    public DateTime OrderTime 
    {
        get => _orderTime;
        set => _orderTime = value;
    }

    [JsonPropertyName("orderTime")]
    public string OrderTimeFormatted 
    {
        get => OrderTime.ToString("ddMMyyyy HHmm");
        set => _orderTime = DateTime.ParseExact(value, "ddMMyyyy HHmm", 
            System.Globalization.CultureInfo.InvariantCulture);
    }

        [JsonIgnore]
        public Customer? Customer { get; set; }
        public int CustomerID { get; set; }

        [JsonIgnore]
        public Dish? Dish { get; set; }
        public int DishID { get; set; }

        public ICollection<DishOrder>? DishOrders { get; set; }
    }
}
