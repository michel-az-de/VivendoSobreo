using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class MeetingsPage : ContentPage
{
    private readonly MeetingsViewModel _viewModel;

    public MeetingsPage(MeetingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadMeetingsCommand.Execute(null);
    }
}
