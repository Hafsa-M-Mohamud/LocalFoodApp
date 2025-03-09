using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Assignment3BAD.Models
{
    public class Trip
    {
        public int TripID { get; set; }
        public ICollection<TripStop> Stops { get; set; } // Collection of stops (Pickup/Delivery)

        [JsonIgnore]
        public Cyclist? Cyclist { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }

        public int OrderID { get; set; }
        public int CyclistID { get; set; }

        public Trip()
        {
            Stops = new List<TripStop>(); // Initialize the list of stops
        }
    }
}
