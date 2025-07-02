namespace RigaMetro.Models.ViewModels;

public class StationViewModel {
    public int StationID { get; set; }
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}