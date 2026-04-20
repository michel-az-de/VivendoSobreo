using Plugin.LocalNotification;

namespace SoPorHoje.App.Services;

public class NotificationService
{
    private readonly IQuoteEngine _quoteEngine;
    private const int DailyNotificationId = 1001;

    public NotificationService(IQuoteEngine quoteEngine)
    {
        _quoteEngine = quoteEngine;
    }

    /// <summary>
    /// Agenda (ou reagenda) a notificação diária com uma frase aleatória da literatura.
    /// Deve ser chamado no startup e no resume do app.
    /// </summary>
    public async Task ScheduleDailyNotificationAsync()
    {
        try
        {
            // Solicita permissão (Android 13+) se ainda não foi concedida
            var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (!granted) return;

            var quote = await _quoteEngine.GetNextQuoteAsync();

            // Agenda para amanhã às 08:00 (ou hoje às 08:00 se antes das 08:00)
            var scheduledTime = DateTime.Today.AddHours(8);
            if (scheduledTime <= DateTime.Now)
                scheduledTime = scheduledTime.AddDays(1);

            // Cancela agendamento anterior para evitar duplicatas
            LocalNotificationCenter.Current.Cancel(DailyNotificationId);

            var request = new NotificationRequest
            {
                NotificationId = DailyNotificationId,
                Title = "Só Por Hoje 💙",
                Description = quote.Text,
                Subtitle = quote.Source,
                BadgeNumber = 1,
                Android = new AndroidOptions
                {
                    ChannelId = "soporhoje_daily",
                    Priority = AndroidNotificationPriority.High,
                    IsProgressBarIndeterminate = false,
                },
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = scheduledTime,
                    RepeatType = NotificationRepeat.No,
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotificationService] Erro ao agendar: {ex.Message}");
        }
    }
}
