using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SoPorHoje.App.Models;
using SQLite;

namespace SoPorHoje.App.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    private const int MeetingsDataVersion = 4;
    private const int ReflectionsDataVersion = 4;
    private const int LiteraturesDataVersion = 2;

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
            await _db.CreateTableAsync<LiteratureText>();
            await _db.CreateTableAsync<MoodEntry>();

            await SeedMeetingsAsync();
            await SeedReflectionsAsync();
            await SeedLiteraturesAsync();

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

    // --- LiteratureText ---
    public async Task<List<LiteratureText>> GetAllLiteratureTextsAsync(string? bookId = null)
    {
        await InitAsync();
        if (bookId is null)
            return await _db!.Table<LiteratureText>().ToListAsync();
        return await _db!.Table<LiteratureText>()
            .Where(t => t.BookId == bookId)
            .ToListAsync();
    }

    public async Task<List<(string, string, int)>> GetLiteratureBookSummariesAsync()
    {
        await InitAsync();
        var all = await _db!.Table<LiteratureText>().ToListAsync();
        return all
            .GroupBy(t => t.BookId)
            .Select(g => (g.Key, g.First().BookTitle, g.Count()))
            .ToList();
    }

    // --- DailyReflection ---
    public async Task<DailyReflection?> GetReflectionAsync(int dayOfYear)
    {
        await InitAsync();
        return await _db!.Table<DailyReflection>()
            .Where(r => r.DayOfYear == dayOfYear)
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

    // --- MoodEntry ---
    public async Task SaveMoodEntryAsync(MoodEntry entry)
    {
        await InitAsync();
        entry.EntryDate = entry.EntryDate.Date; // normalise to midnight
        var existing = await _db!.Table<MoodEntry>()
            .Where(m => m.EntryDate == entry.EntryDate)
            .FirstOrDefaultAsync();
        if (existing is not null)
        {
            existing.MoodEmoji = entry.MoodEmoji;
            existing.MoodLabel = entry.MoodLabel;
            await _db.UpdateAsync(existing);
        }
        else
        {
            await _db.InsertAsync(entry);
        }
    }

    public async Task<MoodEntry?> GetMoodEntryAsync(DateTime date)
    {
        await InitAsync();
        var d = date.Date;
        return await _db!.Table<MoodEntry>()
            .Where(m => m.EntryDate == d)
            .FirstOrDefaultAsync();
    }

    // --- DailyReflection (all) ---
    public async Task<List<DailyReflection>> GetAllReflectionsAsync()
    {
        await InitAsync();
        return await _db!.Table<DailyReflection>().ToListAsync();
    }

    // --- OnlineMeeting ---
    public async Task<List<OnlineMeeting>> GetAllMeetingsAsync()
    {
        await InitAsync();
        return await _db!.Table<OnlineMeeting>().ToListAsync();
    }

    // --- Seed Meetings from JSON ---
    private async Task SeedMeetingsAsync()
    {
        var currentVersion = Preferences.Get("meetings_data_version", 0);
        if (currentVersion >= MeetingsDataVersion) return;

        try
        {
            // Drop old data
            await _db!.DeleteAllAsync<OnlineMeeting>();

            using var stream = await FileSystem.OpenAppPackageFileAsync("online_meetings.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<MeetingsJson>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true
            });

            if (data?.Reunioes is null) return;

            var meetings = new List<OnlineMeeting>();
            foreach (var reuniao in data.Reunioes)
            {
                var dayBit = DayNameToBitmask(reuniao.DiaSemana);
                foreach (var sessao in reuniao.Sessoes)
                {
                    meetings.Add(new OnlineMeeting
                    {
                        GroupName = reuniao.NomeGrupo,
                        DaysOfWeekMask = dayBit,
                        StartTimeTicks = TimeSpan.Parse(sessao.HorarioInicio, CultureInfo.InvariantCulture).Ticks,
                        EndTimeTicks = TimeSpan.Parse(sessao.HorarioFim, CultureInfo.InvariantCulture).Ticks,
                        MeetingUrl = sessao.Url,
                        Platform = sessao.Aplicativo,
                        Location = reuniao.Localizacao ?? "",
                        Observations = reuniao.Observacoes ?? "",
                        SessionType = sessao.Tipo,
                    });
                }
            }

            await _db.InsertAllAsync(meetings);
            Preferences.Set("meetings_data_version", MeetingsDataVersion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to seed meetings: {ex.Message}");
        }
    }

    private static int DayNameToBitmask(string diaSemana) => diaSemana.ToLowerInvariant() switch
    {
        "domingo" => 1 << 0,
        "segunda-feira" => 1 << 1,
        "terça-feira" => 1 << 2,
        "terca-feira" => 1 << 2,
        "quarta-feira" => 1 << 3,
        "quinta-feira" => 1 << 4,
        "sexta-feira" => 1 << 5,
        "sábado" => 1 << 6,
        "sabado" => 1 << 6,
        _ => 0
    };

    // --- Seed Reflections ---
    private async Task SeedReflectionsAsync()
    {
        var currentVersion = Preferences.Get("reflections_data_version", 0);
        if (currentVersion >= ReflectionsDataVersion)
        {
            if (await _db!.Table<DailyReflection>().CountAsync() > 0) return;
        }
        else
        {
            await _db!.DropTableAsync<DailyReflection>();
            await _db.CreateTableAsync<DailyReflection>();
            Preferences.Set("reflections_data_version", ReflectionsDataVersion);
        }

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

            var reflections = items.Select(i => new DailyReflection
            {
                DayOfYear = i.DayOfYear,
                Title = i.Title ?? "",
                Quote = i.Quote ?? "",
                Text = i.Text ?? "",
                Reference = i.Reference ?? "",
            }).ToList();

            await _db.InsertAllAsync(reflections);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to seed reflections: {ex.Message}");
        }
    }

    // --- DTOs ---
    private class ReflectionJson
    {
        [JsonPropertyName("day_of_year")]
        public int DayOfYear { get; set; }
        public string? Date { get; set; }
        public string? Title { get; set; }
        public string? Quote { get; set; }
        public string? Text { get; set; }
        public string? Reference { get; set; }
    }

    // --- Seed Literaturas ---
    private async Task SeedLiteraturesAsync()
    {
        var currentVersion = Preferences.Get("literatures_data_version", 0);
        if (currentVersion >= LiteraturesDataVersion)
        {
            if (await _db!.Table<LiteratureText>().CountAsync() > 0) return;
        }
        else
        {
            await _db!.DropTableAsync<LiteratureText>();
            await _db.CreateTableAsync<LiteratureText>();
            Preferences.Set("literatures_data_version", LiteraturesDataVersion);
        }

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("literaturas.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<LiteraturasJson>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data?.Livros is null) return;

            var rows = new List<LiteratureText>();
            foreach (var livro in data.Livros)
            {
                foreach (var texto in livro.Textos)
                {
                    rows.Add(new LiteratureText
                    {
                        BookId    = livro.Id,
                        BookTitle = livro.Titulo,
                        TextNumber = texto.Id,
                        Title     = texto.Titulo,
                        ShortText = texto.TextoResumido,
                        FullText  = texto.TextoCompleto,
                    });
                }
            }

            await _db.InsertAllAsync(rows);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to seed literaturas: {ex.Message}");
        }
    }

    private class LiteraturasJson
    {
        public int Versao { get; set; }
        public List<LivroJson> Livros { get; set; } = new();
    }

    private class LivroJson
    {
        public string Id { get; set; } = "";
        public string Titulo { get; set; } = "";
        public List<TextoJson> Textos { get; set; } = new();
    }

    private class TextoJson
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        [JsonPropertyName("texto_resumido")]
        public string TextoResumido { get; set; } = "";
        [JsonPropertyName("texto_completo")]
        public string TextoCompleto { get; set; } = "";
    }

    private class MeetingsJson
    {
        [JsonPropertyName("reunioes")]
        public List<ReuniaoJson> Reunioes { get; set; } = new();
    }

    private class ReuniaoJson
    {
        [JsonPropertyName("nome_grupo")]
        public string NomeGrupo { get; set; } = "";
        [JsonPropertyName("dia_semana")]
        public string DiaSemana { get; set; } = "";
        [JsonPropertyName("localizacao")]
        public string? Localizacao { get; set; }
        [JsonPropertyName("observacoes")]
        public string? Observacoes { get; set; }
        [JsonPropertyName("sessoes")]
        public List<SessaoJson> Sessoes { get; set; } = new();
    }

    private class SessaoJson
    {
        [JsonPropertyName("horario_inicio")]
        public string HorarioInicio { get; set; } = "";
        [JsonPropertyName("horario_fim")]
        public string HorarioFim { get; set; } = "";
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "";
        [JsonPropertyName("aplicativo")]
        public string Aplicativo { get; set; } = "";
        [JsonPropertyName("url")]
        public string Url { get; set; } = "";
    }
}
