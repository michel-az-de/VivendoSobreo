using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class PrayersPage : ContentPage
{
    public PrayersPage(PrayersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
