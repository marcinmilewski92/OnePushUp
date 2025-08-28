using System;

namespace OnePushUp.Models.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string NickName { get; set; } = string.Empty;
}

