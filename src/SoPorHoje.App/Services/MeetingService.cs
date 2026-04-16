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
}
