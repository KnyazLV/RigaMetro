namespace RigaMetro.Models.ViewModels;
public class StationViewModel {
    public string StationID { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Order { get; set; }
}