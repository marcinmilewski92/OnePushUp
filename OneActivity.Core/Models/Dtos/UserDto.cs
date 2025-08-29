using System;

namespace OneActivity.Core.Models.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; } = string.Empty;
}
