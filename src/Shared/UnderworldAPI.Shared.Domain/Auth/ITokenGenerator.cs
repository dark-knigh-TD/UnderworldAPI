using System;

namespace UnderworldAPI.Shared.Domain.Auth;

public interface ITokenGenerator
{
    string GenerateToken(string userId, string email, string role);
}
