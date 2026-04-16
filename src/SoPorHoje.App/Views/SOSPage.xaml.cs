using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class SOSPage : ContentPage
{
    public SOSPage(SOSViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
