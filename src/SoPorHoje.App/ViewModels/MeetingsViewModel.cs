using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SoPorHoje.App.Messages;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class MeetingsViewModel : BaseViewModel, IRecipient<AppResumedMessage>
{
    private readonly MeetingService _meetingService;
    private List<MeetingGroup> _allGroups = new();

    private static readonly string[] DayNames = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

    public MeetingsViewModel(MeetingService meetingService)
    {
        _meetingService = meetingService;
        Title = "Reuniões";
        WeakReferenceMessenger.Default.Register(this);
        InitDayFilters();
    }

    [ObservableProperty]
    private ObservableCollection<MeetingGroup> _meetingGroups = new();

    [ObservableProperty]
    private ObservableCollection<DayFilter> _dayFilters = new();

    [ObservableProperty]
    private string _todayLabel = "";

    [ObservableProperty]
    private bool _hasMeetings;

    // --- Messenger ---
    public void Receive(AppResumedMessage message)
    {
        LoadMeetingsCommand.Execute(null);
    }

    // --- Day filters ---
    private void InitDayFilters()
    {
        DayFilters.Clear();
        DayFilters.Add(new DayFilter { Label = "Hoje", DayIndex = -1, IsSelected = true });
        for (int i = 0; i < 7; i++)
            DayFilters.Add(new DayFilter { Label = DayNames[i], DayIndex = i });
    }

    [RelayCommand]
    private void SelectDayFilter(DayFilter filter)
    {
        foreach (var f in DayFilters)
            f.IsSelected = false;
        filter.IsSelected = true;
        ApplyFilter(filter.DayIndex);
    }

    private void ApplyFilter(int dayIndex)
    {
        if (dayIndex == -1)
        {
            MeetingGroups = new ObservableCollection<MeetingGroup>(_allGroups);
        }
        else
        {
            var filtered = _allGroups.Where(g => g.DayIndex == dayIndex).ToList();
            MeetingGroups = new ObservableCollection<MeetingGroup>(filtered);
        }
        HasMeetings = MeetingGroups.Count > 0;
    }

    // --- Load ---
    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            TodayLabel = DateTime.Now.ToString("dddd, dd 'de' MMMM", new CultureInfo("pt-BR"));

            _allGroups = await _meetingService.GetAllMeetingsGroupedAsync();

            // Re-apply current filter
            var selected = DayFilters.FirstOrDefault(f => f.IsSelected);
            ApplyFilter(selected?.DayIndex ?? -1);
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
