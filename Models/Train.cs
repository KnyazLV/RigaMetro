namespace RigaMetro.Models;

public class Train {
    public int TrainID { get; set; }
    public int LineID { get; set; }
    public string? TrainName { get; set; }
    public bool IsActive { get; set; }

    public Line? Line { get; set; }
    public ICollection<TrainAssignment>? Assignments { get; set; }
}