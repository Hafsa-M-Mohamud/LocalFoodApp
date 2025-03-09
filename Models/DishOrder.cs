namespace Assignment3BAD.Models
{
    public class DishOrder
    {
        public int DishOrderID { get; set; }
        public int Quantity { get; set; }

        public Dish? Dish { get; set; }
        public int DishID { get; set; }

        public Order? Order { get; set; }

        public int OrderID { get; set; }
    }
}
