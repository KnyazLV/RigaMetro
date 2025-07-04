namespace RigaMetro.Models.ViewModels;

public class LineWithStationsViewModel {
    public string LineID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    
    // Станции строго в порядке следования по линии
    public List<StationViewModel> Stations { get; set; } = new();
}