using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OnePushUp.Domain;

public class User
{
    public Guid Id { get; set; }
    [MinLength(1), MaxLength(15)]
    public string NickName { get; set; } = string.Empty;
    public IEnumerable<TrainingEntry>? TrainingEntries { get; set; }
}

