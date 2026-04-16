using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SoPorHoje.App.Constants;

namespace SoPorHoje.App.ViewModels;

public partial class HaltCheckViewModel : BaseViewModel
{
    public HaltCheckViewModel()
    {
        Title = "H.A.L.T.";
    }

    [ObservableProperty]
    private bool _isHExpanded;

    [ObservableProperty]
    private bool _isAExpanded;

    [ObservableProperty]
    private bool _isLExpanded;

    [ObservableProperty]
    private bool _isTExpanded;

    [RelayCommand]
    private void ToggleItem(string index)
    {
        switch (index)
        {
            case "0": IsHExpanded = !IsHExpanded; break;
            case "1": IsAExpanded = !IsAExpanded; break;
            case "2": IsLExpanded = !IsLExpanded; break;
            case "3": IsTExpanded = !IsTExpanded; break;
        }
    }
}
