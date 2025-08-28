using System;

namespace OnePushUp.Models.Dtos;

public class StreakDataDto
{
    public int CurrentStreak { get; set; }
    public int QuantityInCurrentStreak { get; set; }
    public int TotalQuantity { get; set; }
    
    // Additional properties that might be useful for display
    public string StreakMessage => CurrentStreak switch
    {
        >= 30 => "Legendary streak!",
        >= 14 => "Amazing streak!",
        >= 7 => "Great streak!",
        > 0 => "Keep it up!",
        _ => "Start your streak today!",
    };
}
