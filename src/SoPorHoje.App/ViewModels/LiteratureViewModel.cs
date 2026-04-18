using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Models;
using SoPorHoje.App.Services;

namespace SoPorHoje.App.ViewModels;

public partial class LiteratureViewModel : BaseViewModel
{
    private readonly LiteratureService _literatureService;

    public LiteratureViewModel(LiteratureService literatureService)
    {
        _literatureService = literatureService;
        Title = "Literaturas";
    }

    [ObservableProperty]
    private string _bookTitle = "";

    [ObservableProperty]
    private string _textNumber = "";

    [ObservableProperty]
    private string _textTitle = "";

    [ObservableProperty]
    private string _shortText = "";

    [ObservableProperty]
    private string _fullText = "";

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _hasText;

    [ObservableProperty]
    private string _progressLabel = "";

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (HasText) return; // já tem texto, não resorteio ao reabrir a tela
        await DrawNewTextAsync();
    }

    [RelayCommand]
    private async Task DrawNewTextAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var text = await _literatureService.DrawNextAsync();
            ApplyText(text);
            IsExpanded = false;

            var (remaining, total) = await _literatureService.GetProgressAsync();
            ProgressLabel = remaining == 0
                ? $"Ciclo completo! Embaralhando novamente... (1/{total})"
                : $"{total - remaining}/{total} textos sorteados neste ciclo";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleExpand() => IsExpanded = !IsExpanded;

    private void ApplyText(LiteratureText? text)
    {
        if (text is null) { HasText = false; return; }
        BookTitle = text.BookTitle;
        TextNumber = $"Cap. {text.TextNumber} — {text.BookTitle}";
        TextTitle = text.Title;
        ShortText = text.ShortText;
        FullText = text.FullText;
        HasText = true;
    }
}
