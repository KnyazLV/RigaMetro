namespace RigaMetro.Web.Models.ViewModels;

public class MapDataViewModel {
    public List<LineWithStationsViewModel> Lines { get; set; } = new();
    public List<TrainViewModel> Trains { get; set; } = new();
}