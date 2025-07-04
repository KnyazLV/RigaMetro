namespace RigaMetro.Models.ViewModels;

public class TimeBetweenViewModel {
    public string FromStationID { get; set; } = "";
    public string ToStationID { get; set; } = "";
    public int DistanceM     { get; set; }
    public int TimeSeconds   { get; set; }
}