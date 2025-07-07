namespace RigaMetro.Web.Models.ViewModels;

public class TrainViewModel {
    public string TrainID { get; set; }
    public string LineID { get; set; }
    public string TrainName { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan StartWorkTime { get; set; }
    public TimeSpan EndWorkTime { get; set; }
}