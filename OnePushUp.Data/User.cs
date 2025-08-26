namespace OnePushUp.Data;

public class User
{
    public Guid Id { get; set; }
    public string NickName { get; set; } = string.Empty;
    public ICollection<TrainingEntry>? TrainingEntries { get; set; }
}
