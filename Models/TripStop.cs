using System;
using System.Text.Json.Serialization;

namespace Assignment3BAD.Models
{
    public class TripStop
    {
        public int TripStopID { get; set; }
        public int TripID { get; set; } // Foreign key for the Trip

    [JsonIgnore]
    public Trip? Trip { get; set; }

    public string Address { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string StopType { get; set; } = string.Empty; // "Pickup" or "Delivery"

    [JsonPropertyName("TripTime")]
    public string TimeFormatted => Time.ToString("ddMMyyyy HHmm");

    // Tilføj disse properties for at matche controller brug
    public string Location 
    { 
        get => Address;
        set => Address = value;
    }

    public DateTime ArrivalTime
    {
        get => Time;
        set => Time = value;
    }

    public DateTime DepartureTime
    {
        get => Time;
        set => Time = value;
    }
}
}
