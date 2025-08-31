namespace OneActivity.Core.Services;

public interface IGenderService
{
    Gender Current { get; }
    void Set(Gender gender);
    event Action? GenderChanged;
}

