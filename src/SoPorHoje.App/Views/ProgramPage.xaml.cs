using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class ProgramPage : ContentPage
{
    public ProgramPage(ProgramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
