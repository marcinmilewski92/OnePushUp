using System;

namespace OnePushUp.Models.Dtos;

public class StreakDataDto
{
    public int CurrentStreak { get; set; }
    public int PushupsInCurrentStreak { get; set; }
    public int TotalPushups { get; set; }
    
    // Additional properties that might be useful for display
    public string StreakMessage
    {
        get
        {
            if (CurrentStreak >= 30)
                return "Legendary streak!";
            if (CurrentStreak >= 14)
                return "Amazing streak!";
            if (CurrentStreak >= 7)
                return "Great streak!";
            if (CurrentStreak > 0)
                return "Keep it up!";
                
            return "Start your streak today!";
        }
    }
}
