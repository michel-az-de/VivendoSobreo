using System.Globalization;
using System.Text.Json;
using SoPorHoje.App.Models;
using SQLite;

namespace SoPorHoje.App.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public DatabaseService()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "soporhoje.db3");
    }

    private async Task InitAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            _db = new SQLiteAsyncConnection(_dbPath);
            await _db.CreateTableAsync<UserProfile>();
            await _db.CreateTableAsync<DailyPledge>();
            await _db.CreateTableAsync<DailyReflection>();
            await _db.CreateTableAsync<ChipEarnedEvent>();
            await _db.CreateTableAsync<OnlineMeeting>();

            await SeedMeetingsAsync();
            await SeedReflectionsAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    // --- UserProfile ---
    public async Task<UserProfile?> GetProfileAsync()
    {
        await InitAsync();
        return await _db!.Table<UserProfile>().FirstOrDefaultAsync();
    }

    public async Task SaveProfileAsync(UserProfile profile)
    {
        await InitAsync();
        if (profile.Id == 0)
            await _db!.InsertAsync(profile);
        else
            await _db!.UpdateAsync(profile);
    }

    // --- DailyPledge ---
    public async Task<DailyPledge?> GetTodayPledgeAsync()
    {
        await InitAsync();
        var today = DateTime.Today;
        return await _db!.Table<DailyPledge>()
            .Where(p => p.PledgeDate == today)
            .FirstOrDefaultAsync();
    }

    public async Task SavePledgeAsync(DailyPledge pledge)
    {
        await InitAsync();
        await _db!.InsertAsync(pledge);
    }

    // --- DailyReflection ---
    public async Task<DailyReflection?> GetReflectionAsync(string dateKey)
    {
        await InitAsync();
        return await _db!.Table<DailyReflection>()
            .Where(r => r.DateKey == dateKey)
            .FirstOrDefaultAsync();
    }

    // --- ChipEarnedEvent ---
    public async Task<List<ChipEarnedEvent>> GetAllChipEventsAsync()
    {
        await InitAsync();
        return await _db!.Table<ChipEarnedEvent>().ToListAsync();
    }

    public async Task<ChipEarnedEvent?> GetChipEventAsync(int requiredDays)
    {
        await InitAsync();
        return await _db!.Table<ChipEarnedEvent>()
            .Where(c => c.ChipRequiredDays == requiredDays)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChipEventAsync(ChipEarnedEvent chipEvent)
    {
        await InitAsync();
        if (chipEvent.Id == 0)
            await _db!.InsertAsync(chipEvent);
        else
            await _db!.UpdateAsync(chipEvent);
    }

    // --- OnlineMeeting ---
    public async Task<List<OnlineMeeting>> GetAllMeetingsAsync()
    {
        await InitAsync();
        return await _db!.Table<OnlineMeeting>().ToListAsync();
    }

    // --- Seed ---
    private async Task SeedMeetingsAsync()
    {
        if (await _db!.Table<OnlineMeeting>().CountAsync() > 0) return;

        var meetings = new List<OnlineMeeting>
        {
            M("Grupo Luz do Amanhecer", 126, "06:00", "07:00"),
            M("Grupo Esperança",        62,  "07:00", "08:00"),
            M("Grupo Fé e Ação",        127, "08:00", "09:00"),
            M("Grupo Renascer",         62,  "12:00", "13:00"),
            M("Grupo Serenidade",       127, "19:00", "20:30"),
            M("Grupo Coragem",          20,  "20:00", "21:30"),
            M("Grupo Nova Vida",        127, "22:00", "23:00"),
        };
        await _db.InsertAllAsync(meetings);
    }

    static OnlineMeeting M(string name, int days, string start, string end) => new()
    {
        GroupName = name,
        DaysOfWeekMask = days,
        StartTimeTicks = TimeSpan.Parse(start, CultureInfo.InvariantCulture).Ticks,
        EndTimeTicks = TimeSpan.Parse(end, CultureInfo.InvariantCulture).Ticks,
        MeetingUrl = "https://intergrupos-aa.org.br",
        Platform = "Zoom",
    };

    private async Task SeedReflectionsAsync()
    {
        if (await _db!.Table<DailyReflection>().CountAsync() > 0) return;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("daily_reflections_pt_br.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var items = JsonSerializer.Deserialize<List<ReflectionJson>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items is null) return;

            var reflections = items.Select(i =>
            {
                var date = DateTime.Parse(i.Date, CultureInfo.InvariantCulture);
                return new DailyReflection
                {
                    DateKey = date.ToString("MM-dd", CultureInfo.InvariantCulture),
                    Title = i.Title ?? "",
                    Quote = i.Quote ?? "",
                    Text = i.Text ?? "",
                    Reference = i.Content ?? "",
                };
            }).ToList();

            await _db.InsertAllAsync(reflections);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to seed reflections: {ex.Message}");
        }
    }

    private class ReflectionJson
    {
        public string Date { get; set; } = "";
        public string? Language { get; set; }
        public string? Title { get; set; }
        public string? Quote { get; set; }
        public string? Text { get; set; }
        public string? Content { get; set; }
    }
}
