using CommunityToolkit.Mvvm.Messaging;
using SoPorHoje.App.Messages;
using SoPorHoje.App.Services;

namespace SoPorHoje.App;

public partial class App : Application
{
    private readonly NotificationService _notificationService;

    public App(NotificationService notificationService)
    {
        _notificationService = notificationService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Created += (s, e) =>
        {
            _ = _notificationService.ScheduleDailyNotificationAsync();
        };

        window.Resumed += (s, e) =>
        {
            WeakReferenceMessenger.Default.Send(new AppResumedMessage());
            _ = _notificationService.ScheduleDailyNotificationAsync();
        };

        return window;
    }
}
