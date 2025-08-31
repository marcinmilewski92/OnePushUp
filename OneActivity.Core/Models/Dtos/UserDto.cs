using System;
using OneActivity.Core.Services;

namespace OneActivity.Core.Models.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
}
