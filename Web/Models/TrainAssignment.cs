﻿namespace RigaMetro.Web.Models;

public class TrainAssignment {
    public string TrainID { get; set; }
    public string ScheduleID { get; set; }

    public Train? Train { get; set; }
    public LineSchedule? Schedule { get; set; }
}