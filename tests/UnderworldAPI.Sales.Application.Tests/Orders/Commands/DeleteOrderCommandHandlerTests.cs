using FluentAssertions;
using NSubstitute;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Application.Orders.Commands.DeleteOrder;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Application.Tests.Orders.Commands;

public class DeleteOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeleteOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithCancelledOrder_ShouldDeleteSuccessfully()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid()).Value;
        order.Cancel();

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new DeleteOrderCommand(order.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _orderRepository.Received(1).DeleteAsync(order, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Order?)null);

        var command = new DeleteOrderCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WithPendingOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid()).Value;
        // Order está en Pending — no se puede borrar

        _orderRepository.GetByIdAsync(order.Id, Arg.Any<CancellationToken>())
            .Returns(order);

        var command = new DeleteOrderCommand(order.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _orderRepository.DidNotReceive().DeleteAsync(
            Arg.Any<Order>(),
            Arg.Any<CancellationToken>());
    }
}