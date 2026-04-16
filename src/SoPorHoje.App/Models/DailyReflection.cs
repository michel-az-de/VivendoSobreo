using SQLite;

namespace SoPorHoje.App.Models;

[Table("daily_reflection")]
public class DailyReflection
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed(Unique = true)]
    public string DateKey { get; set; } = "";
    public string Title { get; set; } = "";
    public string Quote { get; set; } = "";
    public string Text { get; set; } = "";
    public string Reference { get; set; } = "";
}
