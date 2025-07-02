namespace RigaMetro.Models.ViewModels;

public class MapDataViewModel {
    public List<LineViewModel> Lines { get; set; } = new();
    public List<StationViewModel> Stations { get; set; } = new();
}