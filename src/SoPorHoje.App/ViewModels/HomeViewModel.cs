using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Constants;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly ChipService _chipService;
    private readonly MeetingService _meetingService;
    private UserProfile? _profile;

    public HomeViewModel(DatabaseService databaseService, ChipService chipService, MeetingService meetingService)
    {
        _databaseService = databaseService;
        _chipService = chipService;
        _meetingService = meetingService;
        Title = "So Por Hoje";
    }

    // --- Sobriety ---
    [ObservableProperty]
    private int _soberDays;

    [ObservableProperty]
    private string _soberMonths = "";

    [ObservableProperty]
    private string _currentChipEmoji = "";

    [ObservableProperty]
    private string _currentChipColor = "";

    [ObservableProperty]
    private string _currentChipName = "";

    // --- Pledge ---
    [ObservableProperty]
    private bool _hasPledgedToday;

    [ObservableProperty]
    private string _personalReason = "";

    [ObservableProperty]
    private bool _isReasonEditing;

    // --- Reflection ---
    [ObservableProperty]
    private string _reflectionTitle = "";

    [ObservableProperty]
    private string _reflectionQuote = "";

    [ObservableProperty]
    private string _reflectionText = "";

    [ObservableProperty]
    private string _reflectionReference = "";

    [ObservableProperty]
    private bool _isReflectionExpanded;

    // --- Meetings ---
    [ObservableProperty]
    private string _liveMeetingName = "";

    [ObservableProperty]
    private bool _hasLiveMeeting;

    [ObservableProperty]
    private string _nextMeetingName = "";

    [ObservableProperty]
    private int _nextMeetingMinutes;

    [ObservableProperty]
    private bool _hasNextMeeting;

    // --- Progress ---
    [ObservableProperty]
    private double _progressToNextChip;

    [ObservableProperty]
    private string _nextChipName = "";

    [ObservableProperty]
    private int _daysToNextChip;

    // --- Serenity Prayer ---
    [ObservableProperty]
    private string _serenityPrayer = AAContent.Prayers[0].Text;

    [ObservableProperty]
    private string _liveMeetingEndTime = "";

    // --- Live meeting URL for join command ---
    private string _liveMeetingUrl = "";

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Profile & sober days
            _profile = await _databaseService.GetProfileAsync();
            if (_profile is null)
            {
                await Shell.Current.GoToAsync("onboarding");
                return;
            }

            SoberDays = _profile.SoberDays;
            SoberMonths = $"{SoberDays / 30} meses";
            PersonalReason = _profile.PersonalReason ?? "";

            // Chip info
            var currentChip = _chipService.GetCurrentChip(SoberDays);
            if (currentChip is not null)
            {
                CurrentChipEmoji = currentChip.Emoji;
                CurrentChipColor = currentChip.ChipColor;
                CurrentChipName = currentChip.Name;
            }

            // Next chip progress
            var nextChipDef = _chipService.GetNextChipDef(SoberDays);
            if (nextChipDef.HasValue)
            {
                var next = nextChipDef.Value;
                NextChipName = next.Name;
                DaysToNextChip = next.Days - SoberDays;

                var prevDays = currentChip?.RequiredDays ?? 0;
                var range = next.Days - prevDays;
                ProgressToNextChip = range > 0 ? (double)(SoberDays - prevDays) / range : 0;
            }

            // Pledge
            var pledge = await _databaseService.GetTodayPledgeAsync();
            HasPledgedToday = pledge is not null;

            // Reflection
            var dayOfYear = DateTime.Now.DayOfYear;
            var reflection = await _databaseService.GetReflectionAsync(dayOfYear);
            if (reflection is not null)
            {
                ReflectionTitle = reflection.Title.Trim('\u201C', '\u201D', '\u201E', '"', '"', ' ');
                ReflectionQuote = $"\u201C{reflection.Quote.Trim('\u201C', '\u201D', '\u201E', '"', '"', ' ')}\u201D";
                ReflectionText = reflection.Text;
                ReflectionReference = reflection.Reference;
            }

            // Meetings
            await LoadMeetingsDataAsync();

            // Check for uncelebrated chip
            var uncelebrated = await _chipService.CheckAndRecordNewChipAsync(SoberDays);
            if (uncelebrated is not null)
            {
                await _chipService.MarkCelebrationShownAsync(uncelebrated.ChipRequiredDays);
                // Celebration popup can be triggered here via messaging or event
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadMeetingsDataAsync()
    {
        var liveMeeting = await _meetingService.GetLiveMeetingAsync();
        HasLiveMeeting = liveMeeting is not null;
        if (liveMeeting is not null)
        {
            LiveMeetingName = liveMeeting.GroupName;
            LiveMeetingEndTime = liveMeeting.EndTime.ToString(@"hh\:mm");
            _liveMeetingUrl = liveMeeting.MeetingUrl;
        }

        var nextMeeting = await _meetingService.GetNextMeetingAsync();
        HasNextMeeting = nextMeeting is not null;
        if (nextMeeting is not null)
        {
            NextMeetingName = nextMeeting.GroupName;
            NextMeetingMinutes = nextMeeting.MinutesUntilStart ?? 0;
        }
    }

    [RelayCommand]
    private async Task MakePledgeAsync()
    {
        if (HasPledgedToday) return;

        var pledge = new DailyPledge
        {
            PledgeDate = DateTime.Today,
            PledgedAt = DateTime.UtcNow
        };

        await _databaseService.SavePledgeAsync(pledge);
        HasPledgedToday = true;
    }

    [RelayCommand]
    private async Task SaveReasonAsync()
    {
        if (_profile is null) return;

        _profile.PersonalReason = PersonalReason;
        await _databaseService.SaveProfileAsync(_profile);
        IsReasonEditing = false;
    }

    [RelayCommand]
    private void ToggleReasonEdit()
    {
        IsReasonEditing = !IsReasonEditing;
    }

    [RelayCommand]
    private void ToggleReflection()
    {
        IsReflectionExpanded = !IsReflectionExpanded;
    }

    [RelayCommand]
    private async Task OpenSOSAsync()
    {
        await Shell.Current.GoToAsync("sos");
    }

    [RelayCommand]
    private async Task GoToLiteratureAsync()
    {
        await Shell.Current.GoToAsync("literature");
    }

    [RelayCommand]
    private async Task JoinLiveMeetingAsync()
    {
        if (!string.IsNullOrEmpty(_liveMeetingUrl))
        {
            try
            {
                await Launcher.OpenAsync(new Uri(_liveMeetingUrl));
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Erro", "Não foi possível abrir o link da reunião.", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task RefreshMeetingsAsync()
    {
        await LoadMeetingsDataAsync();
    }
}
