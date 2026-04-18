using SQLite;

namespace SoPorHoje.App.Models;

[Table("literature_text")]
public class LiteratureText
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string BookId { get; set; } = "";

    public string BookTitle { get; set; } = "";

    public int TextNumber { get; set; }

    public string Title { get; set; } = "";

    public string ShortText { get; set; } = "";

    public string FullText { get; set; } = "";
}
