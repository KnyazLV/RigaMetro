namespace RigaMetro.Models;

public class ScheduleStop {
    public int ScheduleID { get; set; }
    public int StationOrder { get; set; }
    public int StationID { get; set; }
    public DateTime ArrivalTime { get; set; }
    public DateTime DepartureTime { get; set; }

    public LineSchedule? Schedule { get; set; }
    public Station? Station { get; set; }
}