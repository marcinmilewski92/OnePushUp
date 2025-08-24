using System;

namespace OnePushUp.Models.Dtos;

public class TrainingEntryDto
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public int NumberOfRepetitions { get; set; }
    public Guid UserId { get; set; }
    
    // For displaying in UI
    public string FormattedDate => DateTime.ToLocalTime().ToString("MMM dd, yyyy - h:mm tt");
}
