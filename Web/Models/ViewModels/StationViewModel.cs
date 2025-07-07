using RigaMetro.Web.Models.ViewModels.Schedule;

namespace RigaMetro.Web.Models.ViewModels;
public class StationViewModel {
    public string StationID { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Order { get; set; }
    
    /// Key is a LineID, ScheduleViewModel is a Stop Schedule in both directions
    public Dictionary<string, ScheduleViewModel> Schedule { get; set; }
}