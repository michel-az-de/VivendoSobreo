using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class JustForTodayPage : ContentPage
{
    public JustForTodayPage(JustForTodayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
