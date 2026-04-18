using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class MeetingsViewModel : BaseViewModel
{
    private readonly MeetingService _meetingService;

    public MeetingsViewModel(MeetingService meetingService)
    {
        _meetingService = meetingService;
        Title = "Reuniões";
    }

    [ObservableProperty]
    private ObservableCollection<MeetingGroup> _meetingGroups = new();

    [ObservableProperty]
    private string _todayLabel = "";

    [ObservableProperty]
    private bool _hasMeetings;

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            TodayLabel = DateTime.Now.ToString("dddd, dd 'de' MMMM", new CultureInfo("pt-BR"));

            var groups = await _meetingService.GetAllMeetingsGroupedAsync();
            MeetingGroups = new ObservableCollection<MeetingGroup>(groups);
            HasMeetings = groups.Count > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task JoinMeetingAsync(OnlineMeeting meeting)
    {
        if (meeting is null || string.IsNullOrEmpty(meeting.MeetingUrl)) return;
        try
        {
            await Launcher.OpenAsync(new Uri(meeting.MeetingUrl));
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Erro", "Não foi possível abrir o link da reunião.", "OK");
        }
    }
}
