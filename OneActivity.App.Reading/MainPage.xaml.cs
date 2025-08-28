using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace OneActivity.App.Reading;

public partial class MainPage : ContentPage
{
    private NavigationManager? Navigation =>
        blazorWebView?.Handler?.MauiContext?.Services.GetService<NavigationManager>();

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnSwipedLeft(object sender, SwipedEventArgs e)
    {
        var nav = Navigation;
        if (nav == null) return;
        var uri = new Uri(nav.Uri);
        var next = uri.AbsolutePath switch
        {
            "/" => "/stats",
            "/stats" => "/settings",
            _ => "/"
        };
        nav.NavigateTo(next);
    }

    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        var nav = Navigation;
        if (nav == null) return;
        var uri = new Uri(nav.Uri);
        var next = uri.AbsolutePath switch
        {
            "/" => "/settings",
            "/stats" => "/",
            _ => "/stats"
        };
        nav.NavigateTo(next);
    }
}

