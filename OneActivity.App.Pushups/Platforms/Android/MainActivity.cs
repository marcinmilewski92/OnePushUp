using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;

namespace OneActivity.App.Pushups;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        OnBackPressedDispatcher.AddCallback(this, new BackCallback(this));
    }

    private sealed class BackCallback(Activity activity) : OnBackPressedCallback(true)
    {
        public override void HandleOnBackPressed()
        {
            activity.MoveTaskToBack(true);
        }
    }
}

