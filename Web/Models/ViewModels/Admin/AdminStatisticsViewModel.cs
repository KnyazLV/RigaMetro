namespace RigaMetro.Web.Models.ViewModels.Admin;

public class AdminStatisticsViewModel {
    public int TotalLines { get; set; }
    public int TotalStations { get; set; }
    public int TotalTrains { get; set; }
    public int ActiveTrains { get; set; }
    public double TotalNetworkDistanceKm { get; set; }
    public int TotalDailyTrips { get; set; }

    public string TripsPerLineHourlyJson { get; set; } = "";
    public List<LineStatisticsViewModel> LineStatistics { get; set; }
}