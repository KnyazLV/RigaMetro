namespace RigaMetro.Web.Models.ViewModels.Admin;

public class LineStatisticsViewModel {
    public string LineID { get; set; } = "";
    public string LineName { get; set; } = "";
    public string LineColor { get; set; } = "";
    public int StationCount { get; set; }
    public double TotalDistanceKm { get; set; }
    public int DailyTripsCount { get; set; }
    public int AssignedTrainsCount { get; set; }
}