namespace SoPorHoje.App.Models;

public class SobrietyChip
{
    public int RequiredDays { get; set; }
    public string Name { get; set; } = "";
    public string Label { get; set; } = "";
    public string ChipColor { get; set; } = "";
    public string BgColor { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string ShortLabel { get; set; } = "";
    public bool IsEarned { get; set; }
    public bool IsCurrent { get; set; }
    public bool CelebrationShown { get; set; }
}
