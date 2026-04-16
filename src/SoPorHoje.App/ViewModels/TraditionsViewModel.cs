using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class TraditionsViewModel : BaseViewModel
{
    public TraditionsViewModel()
    {
        Title = "12 Tradições";

        var items = AAContent.TwelveTraditions.Select((t, i) => new TraditionItem
        {
            Number = i + 1,
            Title = $"Tradição {i + 1}",
            Text = t
        }).ToList();

        Traditions = new ObservableCollection<TraditionItem>(items);
    }

    [ObservableProperty]
    private ObservableCollection<TraditionItem> _traditions = new();

    [RelayCommand]
    private void ToggleTradition(TraditionItem tradition)
    {
        if (tradition is null) return;
        tradition.IsExpanded = !tradition.IsExpanded;
    }
}

public partial class TraditionItem : ObservableObject
{
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";

    [ObservableProperty]
    private bool _isExpanded;
}
