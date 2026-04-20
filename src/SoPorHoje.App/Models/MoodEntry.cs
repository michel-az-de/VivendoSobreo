using SQLite;

namespace SoPorHoje.App.Models;

[Table("mood_entry")]
public class MoodEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public DateTime EntryDate { get; set; }

    public string MoodEmoji { get; set; } = "";
    public string MoodLabel { get; set; } = "";
}
