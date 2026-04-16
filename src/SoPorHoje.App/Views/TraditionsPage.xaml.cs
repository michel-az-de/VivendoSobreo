using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class TraditionsPage : ContentPage
{
    public TraditionsPage(TraditionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
