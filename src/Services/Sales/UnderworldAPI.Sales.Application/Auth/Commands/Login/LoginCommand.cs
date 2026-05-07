using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Sales.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(
    string Token,
    string Email,
    string Role,
    DateTime ExpiresAt
);