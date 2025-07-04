namespace RigaMetro.Models.ViewModels;

public class LineStationViewModel {
    public string LineID { get; set; } = "";
    public string StationID { get; set; } = "";
    public int StationOrder { get; set; }
    
    public string LineName { get; set; } = "";
    public string LineColor { get; set; } = "";
    
    public string StationName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
