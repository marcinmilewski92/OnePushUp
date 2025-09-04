#if ANDROID
using Android.Content;

namespace OneActivity.Core.Platforms.Android;

public interface IAlarmScheduler
{
    void RestoreNotificationsAfterReboot(Context context);
    void RescheduleForTomorrow(Context context);
}
#endif
