namespace Assignment3BAD.Models
{
    public class CyclistStats
    {
        public int CyclistStatsID { get; set; }
        public int CyclistID { get; set; }
        public string? Month { get; set; }
        public string? MonthlyHours { get; set; }
        public string? MonthlyEarning { get; set; }

        public required Cyclist Cyclist { get; set; }
    }
}
