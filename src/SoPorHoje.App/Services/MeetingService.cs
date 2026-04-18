using SoPorHoje.App.Models;

namespace SoPorHoje.App.Services;

public class MeetingService
{
    private readonly DatabaseService _db;

    public MeetingService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<List<OnlineMeeting>> GetAllMeetingsAsync()
    {
        return await _db.GetAllMeetingsAsync();
    }

    public async Task<OnlineMeeting?> GetLiveMeetingAsync()
    {
        var meetings = await _db.GetAllMeetingsAsync();
        return meetings.FirstOrDefault(m => m.IsLiveNow);
    }

    public async Task<OnlineMeeting?> GetNextMeetingAsync()
    {
        var meetings = await _db.GetAllMeetingsAsync();
        return meetings
            .Where(m => m.MinutesUntilStart.HasValue)
            .OrderBy(m => m.MinutesUntilStart!.Value)
            .FirstOrDefault();
    }

    public async Task<List<OnlineMeeting>> GetSortedMeetingsAsync()
    {
        var meetings = await _db.GetAllMeetingsAsync();
        return meetings
            .OrderByDescending(m => m.IsLiveNow)
            .ThenBy(m => m.StartTimeTicks)
            .ToList();
    }

    public async Task<List<OnlineMeeting>> GetTodayMeetingsSortedAsync()
    {
        var meetings = await _db.GetAllMeetingsAsync();
        var now = DateTime.Now;
        var todayBit = 1 << (int)now.DayOfWeek;
        var currentTime = now.TimeOfDay;
        return meetings
            .Where(m => (m.DaysOfWeekMask & todayBit) != 0)
            .Where(m => m.IsLiveNow || m.EndTime > currentTime)
            .OrderByDescending(m => m.IsLiveNow)
            .ThenByDescending(m => m.GroupName.Contains("Um Dia de Cada Vez", StringComparison.OrdinalIgnoreCase))
            .ThenBy(m => m.StartTimeTicks)
            .ToList();
    }

    public async Task<List<MeetingGroup>> GetAllMeetingsGroupedAsync()
    {
        var meetings = await _db.GetAllMeetingsAsync();
        var now = DateTime.Now;
        var todayBit = 1 << (int)now.DayOfWeek;
        var dayNames = new[] { "Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado" };

        var groups = new List<MeetingGroup>();

        // Start from today, then cycle through the week
        for (int offset = 0; offset < 7; offset++)
        {
            var dayIndex = ((int)now.DayOfWeek + offset) % 7;
            var bit = 1 << dayIndex;
            var dayMeetings = meetings
                .Where(m => (m.DaysOfWeekMask & bit) != 0)
                .OrderByDescending(m => m.GroupName.Contains("Um Dia de Cada Vez", StringComparison.OrdinalIgnoreCase))
                .ThenBy(m => m.StartTimeTicks)
                .ToList();

            if (dayMeetings.Count == 0) continue;

            var label = offset == 0 ? $"Hoje — {dayNames[dayIndex]}" : dayNames[dayIndex];
            groups.Add(new MeetingGroup(label, dayMeetings));
        }

        return groups;
    }
}

public class MeetingGroup : List<OnlineMeeting>
{
    public string DayName { get; }

    public MeetingGroup(string dayName, List<OnlineMeeting> meetings) : base(meetings)
    {
        DayName = dayName;
    }
}
