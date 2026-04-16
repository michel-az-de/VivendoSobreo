using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class OnboardingViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    public OnboardingViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Bem-vindo";
    }

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private bool _isDateSelected;

    partial void OnSelectedDateChanged(DateTime value)
    {
        IsDateSelected = true;
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var profile = new UserProfile
            {
                SobrietyDate = SelectedDate,
                CreatedAt = DateTime.UtcNow
            };

            await _databaseService.SaveProfileAsync(profile);
            await Shell.Current.GoToAsync("//home");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
