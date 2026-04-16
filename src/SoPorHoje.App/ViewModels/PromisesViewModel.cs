using CommunityToolkit.Mvvm.ComponentModel;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class PromisesViewModel : BaseViewModel
{
    public PromisesViewModel()
    {
        Title = "Promessas";
        Promises = AAContent.Promises.Select(p => new PromiseItem { Text = p }).ToList();
    }

    [ObservableProperty]
    private List<PromiseItem> _promises = new();
}

public class PromiseItem
{
    public string Text { get; set; } = "";
}
