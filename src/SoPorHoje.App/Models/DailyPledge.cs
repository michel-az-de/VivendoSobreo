using SQLite;

namespace SoPorHoje.App.Models;

[Table("daily_pledge")]
public class DailyPledge
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public DateTime PledgeDate { get; set; }
    public DateTime PledgedAt { get; set; }
}
