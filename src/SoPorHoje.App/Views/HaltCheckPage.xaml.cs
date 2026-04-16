using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class HaltCheckPage : ContentPage
{
    public HaltCheckPage(HaltCheckViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
