using CommunityToolkit.Mvvm.ComponentModel;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class PrayersViewModel : BaseViewModel
{
    public PrayersViewModel()
    {
        Title = "Oracoes";

        Prayers = AAContent.Prayers.Select(p => new PrayerItem
        {
            Name = p.Name,
            Text = p.Text
        }).ToList();
    }

    [ObservableProperty]
    private List<PrayerItem> _prayers = new();
}

public class PrayerItem
{
    public string Name { get; set; } = "";
    public string Text { get; set; } = "";
}
