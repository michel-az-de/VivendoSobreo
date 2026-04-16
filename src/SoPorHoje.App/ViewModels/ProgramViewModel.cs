using CommunityToolkit.Mvvm.Input;

namespace SoPorHoje.App.ViewModels;

public partial class ProgramViewModel : BaseViewModel
{
    public ProgramViewModel()
    {
        Title = "Programa";
    }

    [RelayCommand]
    private async Task GoToStepsAsync()
    {
        await Shell.Current.GoToAsync("steps");
    }

    [RelayCommand]
    private async Task GoToTraditionsAsync()
    {
        await Shell.Current.GoToAsync("traditions");
    }

    [RelayCommand]
    private async Task GoToPromisesAsync()
    {
        await Shell.Current.GoToAsync("promises");
    }

    [RelayCommand]
    private async Task GoToJustForTodayAsync()
    {
        await Shell.Current.GoToAsync("justfortoday");
    }

    [RelayCommand]
    private async Task GoToPrayersAsync()
    {
        await Shell.Current.GoToAsync("prayers");
    }

    [RelayCommand]
    private async Task GoToHaltCheckAsync()
    {
        await Shell.Current.GoToAsync("halt");
    }
}
