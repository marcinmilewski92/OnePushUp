namespace OnePushUp.Domain;

public class TrainingEntry
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public int NumberOfRepetitions { get; set; }
    public Guid UserId { get; set; }
}

