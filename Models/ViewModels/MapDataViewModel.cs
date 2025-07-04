namespace RigaMetro.Models.ViewModels;

public class MapDataViewModel {
    public List<LineWithStationsViewModel> Lines { get; set; }
    public List<TimeBetweenViewModel> TimeBetween { get; set; }
    public List<TrainViewModel> Trains { get; set; }
    public List<LineSchedule> Schedules { get; set; }
    public List<ScheduleStop> Stops { get; set; }
    public List<TrainAssignment> Assignments { get; set; }
}