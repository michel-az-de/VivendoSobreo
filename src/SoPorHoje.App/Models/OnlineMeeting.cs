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
    public string MeetingUrl { get; set; } = "https://intergrupos-aa.org.br";
    public string Platform { get; set; } = "Zoom";

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
    public string FormattedTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

    [Ignore]
    public string FormattedDays
    {
        get
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
}
