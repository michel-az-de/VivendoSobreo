using CommunityToolkit.Mvvm.ComponentModel;

namespace SoPorHoje.App.Models;

public partial class DayFilter : ObservableObject
{
    public string Label { get; set; } = "";

    /// <summary>-1 = Hoje (todos os dias agrupados), 0=Dom, 1=Seg ... 6=Sáb</summary>
    public int DayIndex { get; set; }

    [ObservableProperty]
    private bool _isSelected;
}
