namespace RigaMetro.Models;

public class TrainAssignment {
    public int TrainID { get; set; }
    public int ScheduleID { get; set; }
    public DateTime AssignmentDate { get; set; }

    public Train? Train { get; set; }
    public LineSchedule? Schedule { get; set; }
}