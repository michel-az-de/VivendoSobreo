using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class StepsViewModel : BaseViewModel
{
    public StepsViewModel()
    {
        Title = "12 Passos";

        var items = AAContent.TwelveSteps.Select((s, i) => new StepItem
        {
            Number = i + 1,
            Title = s.Title,
            Text = s.Text
        }).ToList();

        Steps = new ObservableCollection<StepItem>(items);
    }

    [ObservableProperty]
    private ObservableCollection<StepItem> _steps = new();

    [RelayCommand]
    private void ToggleStep(StepItem step)
    {
        if (step is null) return;
        step.IsExpanded = !step.IsExpanded;
    }
}

public partial class StepItem : ObservableObject
{
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";

    [ObservableProperty]
    private bool _isExpanded;
}
