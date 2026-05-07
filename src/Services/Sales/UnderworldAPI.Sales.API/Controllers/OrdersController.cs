using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnderworldAPI.Sales.Application.Orders.Commands.CreateOrder;
using UnderworldAPI.Sales.Application.Orders.Commands.DeleteOrder;
using UnderworldAPI.Sales.Application.Orders.Commands.UpdateOrder;
using UnderworldAPI.Sales.Application.Orders.Queries.GetAllOrders;
using UnderworldAPI.Sales.Application.Orders.Queries.GetOrderById;

namespace UnderworldAPI.Sales.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/orders")]
    [Authorize] // Requiere autenticación token para todas las acciones
    public sealed class OrdersController (ISender mediator) : ControllerBase
    {
        // GET api/v1/orders
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAllOrdersQuery(), cancellationToken);

            return result.IsFailure
                ? BadRequest(result.Error)
                : Ok(result.Value);
        }

        // GET api/v1/orders/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

            return result.IsFailure
                ? NotFound(result.Error)
                : Ok(result.Value);
        }

        // POST api/v1/orders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command,CancellationToken cancellationToken)
        {
                var result = await mediator.Send(command, cancellationToken);

                return result.IsFailure
                    ? BadRequest(result.Error)
                    : CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value); 
            
        }


        // PUT api/v1/orders/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateOrderCommand(id, request.Action),
                     cancellationToken);

             return result.IsFailure
            ? BadRequest(result.Error)
            : NoContent();
        }

         // DELETE api/v1/orders/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteOrderCommand(id), cancellationToken);

            return result.IsFailure
                ? BadRequest(result.Error)
                : NoContent();
        }




    }
}

// Request separado para Update — evita exponer el Command directo al contrato HTTP
public sealed record UpdateOrderRequest(string Action);