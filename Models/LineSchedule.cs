using System.ComponentModel.DataAnnotations;

namespace RigaMetro.Models;

public class LineSchedule {
    [Key]
    public int ScheduleID { get; set; }
    public int LineID { get; set; }
    public int TripNumber { get; set; }
    public bool IsClockwise { get; set; }
    public DateTime StartTime { get; set; }

    public Line? Line { get; set; }
    public ICollection<ScheduleStop>? Stops { get; set; }
    public ICollection<TrainAssignment>? Assignments { get; set; }
}