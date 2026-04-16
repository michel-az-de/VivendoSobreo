using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SoPorHoje.App.ViewModels;

public partial class SOSViewModel : BaseViewModel
{
    public SOSViewModel()
    {
        Title = "SOS";
    }

    [ObservableProperty]
    private bool _isBreathing;

    [ObservableProperty]
    private string _breathingPhase = "";

    [ObservableProperty]
    private int _breathingCounter;

    [ObservableProperty]
    private double _breathingProgress;

    private CancellationTokenSource? _breathingCts;

    public List<string> CopingPhrases { get; } = new()
    {
        "Isso vai passar. Cada momento de resistencia te fortalece.",
        "Voce nao precisa beber hoje. So por hoje.",
        "Ligue para alguem do grupo. Voce nao esta sozinho.",
        "Lembre-se de como voce se sentiu na ultima vez. Vale a pena?"
    };

    [RelayCommand]
    private async Task StartBreathingAsync()
    {
        if (IsBreathing) return;

        _breathingCts = new CancellationTokenSource();
        var token = _breathingCts.Token;

        IsBreathing = true;
        const int totalCycles = 4;
        const int inspireDuration = 4;
        const int holdDuration = 4;
        const int expireDuration = 6;
        const int totalSecondsPerCycle = inspireDuration + holdDuration + expireDuration;
        const int totalSeconds = totalCycles * totalSecondsPerCycle;
        var elapsed = 0;

        try
        {
            for (var cycle = 0; cycle < totalCycles; cycle++)
            {
                // Inspire
                BreathingPhase = "Inspire...";
                for (var s = 0; s < inspireDuration; s++)
                {
                    token.ThrowIfCancellationRequested();
                    BreathingCounter = inspireDuration - s;
                    elapsed++;
                    BreathingProgress = (double)elapsed / totalSeconds;
                    await Task.Delay(1000, token);
                }

                // Hold
                BreathingPhase = "Segure...";
                for (var s = 0; s < holdDuration; s++)
                {
                    token.ThrowIfCancellationRequested();
                    BreathingCounter = holdDuration - s;
                    elapsed++;
                    BreathingProgress = (double)elapsed / totalSeconds;
                    await Task.Delay(1000, token);
                }

                // Expire
                BreathingPhase = "Expire...";
                for (var s = 0; s < expireDuration; s++)
                {
                    token.ThrowIfCancellationRequested();
                    BreathingCounter = expireDuration - s;
                    elapsed++;
                    BreathingProgress = (double)elapsed / totalSeconds;
                    await Task.Delay(1000, token);
                }
            }

            BreathingPhase = "\U0001f60c Muito bem!";
            BreathingCounter = 0;
            BreathingProgress = 1.0;
        }
        catch (OperationCanceledException)
        {
            BreathingPhase = "";
            BreathingCounter = 0;
            BreathingProgress = 0;
        }
        finally
        {
            IsBreathing = false;
            _breathingCts?.Dispose();
            _breathingCts = null;
        }
    }

    [RelayCommand]
    private void StopBreathing()
    {
        _breathingCts?.Cancel();
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CallCVVAsync()
    {
        await Launcher.OpenAsync(new Uri("tel:188"));
    }

    [RelayCommand]
    private async Task OpenHaltCheckAsync()
    {
        await Shell.Current.GoToAsync("halt");
    }
}
