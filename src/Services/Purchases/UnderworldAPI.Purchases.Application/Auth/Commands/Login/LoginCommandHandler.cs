using System;
using MediatR;
using UnderworldAPI.Sales.Application.Auth.Commands.Login;
using UnderworldAPI.Shared.Domain.Auth;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.Auth.Commands.Login;

internal sealed class LoginCommandHandler(
    ITokenGenerator tokenGenerator
): IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    // Usuarios de prueba — en producción vendrán de la BD
    private static readonly Dictionary<string, (string Password, string Role)> _users = new()
    {
        { "admin@underworld.com",   ("Admin123!", "Admin") },
        { "sales@underworld.com",   ("Sales123!", "Sales") },
        { "manager@underworld.com", ("Manager123!", "Manager") }
    };
    public Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(request.Email, out var userData))
            return Task.FromResult(Result.Failure<LoginResponse>(
                    Error.Validation("Auth", "Invalid email or password.")));

            
        if (userData.Password!= request.Password)
            return Task.FromResult(Result.Failure<LoginResponse>(
                        Error.Validation("Auth", "Invalid email or password.")));

        // Generar token
        var userId = Guid.NewGuid().ToString();
        var token = tokenGenerator.GenerateToken(userId, request.Email, userData.Role);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return Task.FromResult(
            Result.Success(new LoginResponse(
                token,
                request.Email,
                userData.Role,
                expiresAt)));


    }
}

