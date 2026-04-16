using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class PromisesPage : ContentPage
{
    public PromisesPage(PromisesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
