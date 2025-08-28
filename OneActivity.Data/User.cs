namespace OneActivity.Data;

public class User
{
    public Guid Id { get; set; }
    public string NickName { get; set; } = string.Empty;
    public ICollection<ActivityEntry>? ActivityEntries { get; set; }
}

