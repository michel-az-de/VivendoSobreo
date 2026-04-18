using SoPorHoje.App.ViewModels;

namespace SoPorHoje.App.Views;

public partial class LiteraturePage : ContentPage
{
    public LiteraturePage(LiteratureViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LiteratureViewModel vm && !vm.HasText)
            vm.LoadCommand.Execute(null);
    }
}
