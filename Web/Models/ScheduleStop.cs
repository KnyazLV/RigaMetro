namespace RigaMetro.Web.Models;

public class ScheduleStop {
    public string ScheduleID { get; set; } = "";
    public int StationOrder { get; set; }
    public string StationID { get; set; } = "";
    public TimeSpan ArrivalTime { get; set; }
    public TimeSpan DepartureTime { get; set; }

    public LineSchedule? Schedule { get; set; }
    public Station? Station { get; set; }
}