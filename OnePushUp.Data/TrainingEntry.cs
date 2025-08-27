namespace OnePushUp.Data;

public class TrainingEntry
{
    public Guid Id { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public int NumberOfRepetitions { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
