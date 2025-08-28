namespace OnePushUp.Data;

// Generic activity entry replacing pushup-specific naming.
// Mapped to existing DB table/columns via DbContext to preserve data.
public class ActivityEntry
{
    public Guid Id { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public int Quantity { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
