using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class ChipsPage : ContentPage
{
    private readonly ChipsViewModel _viewModel;

    public ChipsPage(ChipsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadChipsCommand.Execute(null);
    }
}
