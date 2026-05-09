using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UnderworldAPI.Sales.Application.Auth.Commands.Login;

namespace UnderworldAPI.Sales.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(ISender mediator) : ControllerBase
{
    // POST api/v1/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.IsFailure
            ? Unauthorized(result.Error)
            : Ok(result.Value);
    }
}