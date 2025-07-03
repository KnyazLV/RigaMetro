namespace RigaMetro.Models.ViewModels;

public class TimeBetweenViewModel {
    public int FromStationID { get; set; }
    public int ToStationID   { get; set; }
    public int DistanceM     { get; set; }
    public int TimeSeconds   { get; set; }
}