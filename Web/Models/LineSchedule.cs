﻿using System.ComponentModel.DataAnnotations;

namespace RigaMetro.Web.Models;

public class LineSchedule {
    [Key] public string ScheduleID { get; set; } = "";
    public string LineID { get; set; } = "";
    public int TripNumber { get; set; }
    public bool IsClockwise { get; set; }
    public TimeSpan StartTime { get; set; }

    public Line? Line { get; set; }
    public ICollection<ScheduleStop>? Stops { get; set; }
    public ICollection<TrainAssignment>? Assignments { get; set; }
}