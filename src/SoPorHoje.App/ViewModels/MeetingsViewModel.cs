using System.Collections.ObjectModel;
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
        Title = "Reunioes";
    }

    [ObservableProperty]
    private ObservableCollection<OnlineMeeting> _meetings = new();

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var list = await _meetingService.GetSortedMeetingsAsync();
            Meetings = new ObservableCollection<OnlineMeeting>(list);
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
        await Launcher.OpenAsync(new Uri(meeting.MeetingUrl));
    }

    [RelayCommand]
    private async Task OpenLinkAsync(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        await Launcher.OpenAsync(new Uri(url));
    }
}
