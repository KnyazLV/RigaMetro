namespace RigaMetro.Services;

public interface IScheduleService {
    Task GenerateDailyScheduleAsync(string trainId);
}