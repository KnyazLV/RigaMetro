using System.Collections;

namespace RigaMetro.Web.Models.ViewModels.Schedule;

public class HourlySchedule : IReadOnlyDictionary<int, IReadOnlyList<int>> {
    private readonly Dictionary<int, List<int>> _storage = Initialise();
    
    public IEnumerable<int> Keys                  => _storage.Keys;
    public IEnumerable<IReadOnlyList<int>> Values => _storage.Values;
    public int Count                              => _storage.Count;
    public IReadOnlyList<int> this[int hour]      => _storage[hour];

    public void Add(DateTime arrival) {
        _storage[arrival.Hour].Add(arrival.Minute);
    }

    public void Add(int hour, int minute) {
        ValidateHour(hour);
        ValidateMinute(minute);
        _storage[hour].Add(minute);
    }
    

    public bool ContainsKey(int hour) => _storage.ContainsKey(hour);
    public bool TryGetValue(int hour, out IReadOnlyList<int> minutes)
    {
        var ok = _storage.TryGetValue(hour, out var list);
        minutes = list ?? [];
        return ok;
    }
    
    public IEnumerator<KeyValuePair<int, IReadOnlyList<int>>> GetEnumerator()
        => _storage.Select(p => KeyValuePair.Create(p.Key, (IReadOnlyList<int>)p.Value)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static Dictionary<int, List<int>> Initialise() {
        var dict = new Dictionary<int, List<int>>(24);
        for (var h = 0; h < 24; h++) dict[h] = [];
        return dict;
    }

    private static void ValidateHour(int hour) {
        if (hour is < 0 or > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be in range 0–23.");
    }
    private static void ValidateMinute(int minute) {
        if (minute is < 0 or > 59)
            throw new ArgumentOutOfRangeException(nameof(minute), "Minute must be in range 0–59.");
    }
}
