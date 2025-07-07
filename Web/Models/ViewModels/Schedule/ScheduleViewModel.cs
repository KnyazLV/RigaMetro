namespace RigaMetro.Web.Models.ViewModels.Schedule;

/// <summary>
/// Stop schedule for one station on line
/// </summary>
public class ScheduleViewModel {
    /// <summary>Key – hour (0‒23), value – minutes arriving </summary>
    public HourlySchedule Clockwise  { get; } = new();
    public HourlySchedule Counterclockwise { get; } = new();
}