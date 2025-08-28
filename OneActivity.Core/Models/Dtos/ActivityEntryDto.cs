using System;

namespace OnePushUp.Models.Dtos;

public class ActivityEntryDto
{
    public Guid Id { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public int Quantity { get; set; }
    public Guid UserId { get; set; }

    public string FormattedDate => DateTime.ToLocalTime().ToString("MMM dd, yyyy - h:mm tt");
}

