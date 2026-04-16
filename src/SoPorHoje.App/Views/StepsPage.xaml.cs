using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class StepsPage : ContentPage
{
    public StepsPage(StepsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
