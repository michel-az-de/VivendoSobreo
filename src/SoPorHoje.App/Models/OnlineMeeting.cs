using SQLite;

namespace SoPorHoje.App.Models;

[Table("online_meeting")]
public class OnlineMeeting
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string GroupName { get; set; } = "";
    public int DaysOfWeekMask { get; set; }
    public long StartTimeTicks { get; set; }
    public long EndTimeTicks { get; set; }
    public string MeetingUrl { get; set; } = "";
    public string Platform { get; set; } = "Zoom";
    public string Location { get; set; } = "";
    public string Observations { get; set; } = "";
    public string SessionType { get; set; } = "";

    [Ignore]
    public TimeSpan StartTime => TimeSpan.FromTicks(StartTimeTicks);
    [Ignore]
    public TimeSpan EndTime => TimeSpan.FromTicks(EndTimeTicks);

    [Ignore]
    public bool IsLiveNow
    {
        get
        {
            var now = DateTime.Now;
            var bit = 1 << (int)now.DayOfWeek;
            if ((DaysOfWeekMask & bit) == 0) return false;
            var t = now.TimeOfDay;
            if (EndTime < StartTime) // overnight (ex: 22:00→00:00)
                return t >= StartTime || t < EndTime;
            return t >= StartTime && t < EndTime;
        }
    }

    [Ignore]
    public int? MinutesUntilStart
    {
        get
        {
            var now = DateTime.Now;
            var bit = 1 << (int)now.DayOfWeek;
            if ((DaysOfWeekMask & bit) == 0) return null;
            var diff = (int)(StartTime - now.TimeOfDay).TotalMinutes;
            return diff > 0 ? diff : null;
        }
    }

    [Ignore]
    public bool IsTodayMeeting
    {
        get
        {
            var bit = 1 << (int)DateTime.Now.DayOfWeek;
            return (DaysOfWeekMask & bit) != 0;
        }
    }

    [Ignore]
    public string StatusText
    {
        get
        {
            if (IsLiveNow)
                return $"Aberta até {EndTime:hh\\:mm}!!!";
            if (MinutesUntilStart.HasValue)
                return $"Hoje {StartTime:hh\\:mm} às {EndTime:hh\\:mm}";
            return FormattedTime;
        }
    }

    [Ignore]
    public string FormattedTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

    [Ignore]
    public string FormattedDays
    {
        get
        {
            return DaysOfWeekMask switch
            {
                127 => "Todos os dias",
                62 => "Seg–Sex",
                126 => "Seg–Sáb",
                _ => FormatDaysList()
            };
        }
    }

    private string FormatDaysList()
    {
        var days = new[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };
        var result = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if ((DaysOfWeekMask & (1 << i)) != 0)
                result.Add(days[i]);
        }
        return string.Join(", ", result);
    }
}
