using SQLite;

namespace SoPorHoje.App.Models;

[Table("chip_earned")]
public class ChipEarnedEvent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ChipRequiredDays { get; set; }
    public DateTime EarnedAt { get; set; }
    public bool CelebrationShown { get; set; }
}
