using SQLite;

namespace SoPorHoje.App.Models;

[Table("user_profile")]
public class UserProfile
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime SobrietyDate { get; set; }
    public string? PersonalReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public int SoberDays => Math.Max(0, (int)(DateTime.Today - SobrietyDate.Date).TotalDays);
}
