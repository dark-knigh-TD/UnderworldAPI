using System;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.DeletePurchaseOrder;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

namespace UnderworldAPI.Purchases.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/purchase-orders")]
[Authorize] 
public sealed class PurchaseOrdersController(ISender mediator) : ControllerBase
{
     // GET api/v1/purchase-orders
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllPurchaseOrdersQuery(), cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : Ok(result.Value);
    }

    // GET api/v1/purchase-orders/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetPurchaseOrderByIdQuery(id),
            cancellationToken);

        return result.IsFailure
            ? NotFound(result.Error)
            : Ok(result.Value);
    }

    // POST api/v1/purchase-orders
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : CreatedAtAction(
                nameof(GetById),
                new { id = result.Value },
                result.Value);
    }

    // PUT api/v1/purchase-orders/{id}
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdatePurchaseOrderCommand(id, request.Action),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : NoContent();
    }

     // DELETE api/v1/purchase-orders/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DeletePurchaseOrderCommand(id),
            cancellationToken);

        return result.IsFailure
            ? BadRequest(result.Error)
            : NoContent();
    }

}

public sealed record UpdatePurchaseOrderRequest(string Action);
