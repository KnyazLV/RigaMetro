namespace RigaMetro.Web.Models;

public class ScheduleStop {
    public string ScheduleID { get; set; } = "";
    public int StationOrder { get; set; }
    public string StationID { get; set; } = "";
    public DateTime ArrivalTime { get; set; }
    public DateTime DepartureTime { get; set; }

    public LineSchedule? Schedule { get; set; }
    public Station? Station { get; set; }
}