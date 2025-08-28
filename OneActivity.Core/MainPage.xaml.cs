using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace OneActivity.Core;

public partial class MainPage : ContentPage
{
    private NavigationManager? Navigation =>
        blazorWebView?.Handler?.MauiContext?.Services.GetService<NavigationManager>();

    public MainPage()
    {
        InitializeComponent();
    }
}
