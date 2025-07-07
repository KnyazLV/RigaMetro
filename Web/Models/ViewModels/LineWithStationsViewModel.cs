namespace RigaMetro.Web.Models.ViewModels;

public class LineWithStationsViewModel {
    public string LineID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public List<StationViewModel> Stations { get; set; } = new();
    public string ClockwiseTerminal { get; set; } = "";
    public string CounterclockwiseTerminal { get; set; } = "";
}