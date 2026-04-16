using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class ChipsViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly ChipService _chipService;

    public ChipsViewModel(DatabaseService databaseService, ChipService chipService)
    {
        _databaseService = databaseService;
        _chipService = chipService;
        Title = "Fichas";
    }

    [ObservableProperty]
    private ObservableCollection<SobrietyChip> _chips = new();

    [ObservableProperty]
    private int _earnedCount;

    [ObservableProperty]
    private int _totalCount = 10;

    [ObservableProperty]
    private double _overallProgress;

    [RelayCommand]
    private async Task LoadChipsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var profile = await _databaseService.GetProfileAsync();
            if (profile is null) return;

            var chipList = await _chipService.GetChipsStatusAsync(profile.SoberDays);
            Chips = new ObservableCollection<SobrietyChip>(chipList);

            EarnedCount = chipList.Count(c => c.IsEarned);
            OverallProgress = TotalCount > 0 ? (double)EarnedCount / TotalCount : 0;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
