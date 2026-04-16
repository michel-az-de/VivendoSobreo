using CommunityToolkit.Mvvm.ComponentModel;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class JustForTodayViewModel : BaseViewModel
{
    public JustForTodayViewModel()
    {
        Title = "Só Por Hoje";
        Items = AAContent.JustForToday.Select((t, i) => new JustForTodayItem
        {
            Number = i + 1,
            Text = t
        }).ToList();
    }

    [ObservableProperty]
    private List<JustForTodayItem> _items = new();
}

public class JustForTodayItem
{
    public int Number { get; set; }
    public string Text { get; set; } = "";
}
