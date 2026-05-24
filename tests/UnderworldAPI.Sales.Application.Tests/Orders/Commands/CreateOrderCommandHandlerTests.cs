using FluentAssertions;
using NSubstitute;
using UnderworldAPI.Sales.Application.Abstractions;
using UnderworldAPI.Sales.Application.Orders.Commands.CreateOrder;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Shared.Domain.IntegrationEvents;

namespace UnderworldAPI.Sales.Application.Tests.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        // NSubstitute crea mocks automáticamente
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _integrationEventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _handler = new CreateOrderCommandHandler(_orderRepository, _unitOfWork,_integrationEventPublisher);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrderAndReturnId()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [new CreateOrderItemRequest(
                Guid.NewGuid(),
                "Monitor LG 27",
                350.00m,
                "MXN",
                2)]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        // Verifica que se llamaron los métodos del repositorio
        await _orderRepository.Received(1).AddAsync(
            Arg.Any<Order>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());

            // Verifica que se publicó el integration event
            await _integrationEventPublisher.Received(1)
        .PublishOrderCreatedAsync(
            Arg.Any<OrderCreatedIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyCustomerId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.Empty,
            [new CreateOrderItemRequest(
                Guid.NewGuid(),
                "Product A",
                100m,
                "MXN",
                1)]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        // Verifica que NO se llamó al repositorio
        await _orderRepository.DidNotReceive().AddAsync(
            Arg.Any<Order>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCreateOrderWithAllItems()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [
                new CreateOrderItemRequest(Guid.NewGuid(), "Product A", 100m, "MXN", 1),
                new CreateOrderItemRequest(Guid.NewGuid(), "Product B", 200m, "MXN", 2)
            ]
        );

        Order? capturedOrder = null;

        await _orderRepository.AddAsync(
            Arg.Do<Order>(o => capturedOrder = o),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedOrder.Should().NotBeNull();
        capturedOrder!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNegativeUnitPrice_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [new CreateOrderItemRequest(
                Guid.NewGuid(),
                "Product A",
                -50m,
                "MXN",
                1)]
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}