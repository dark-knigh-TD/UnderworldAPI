using FluentAssertions;
using NSubstitute;
using UnderworldAPI.Purchases.Application.Abstractions;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;

namespace UnderworldAPI.Purchases.Application.Tests.PurchaseOrders.Commands;

public class CreatePurchaseOrderCommandHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreatePurchaseOrderCommandHandler _handler;

    public CreatePurchaseOrderCommandHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreatePurchaseOrderCommandHandler(
            _purchaseOrderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePurchaseOrderAndReturnId()
    {
        var command = new CreatePurchaseOrderCommand(
            Guid.NewGuid(),
            "First order",
            [new CreatePurchaseOrderItemRequest(
                Guid.NewGuid(),
                "Teclado Mecánico",
                150.00m,
                "MXN",
                5)]
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        await _purchaseOrderRepository.Received(1).AddAsync(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptySupplierId_ShouldReturnFailure()
    {
        var command = new CreatePurchaseOrderCommand(
            Guid.Empty,
            null,
            [new CreatePurchaseOrderItemRequest(
                Guid.NewGuid(), "Product A", 100m, "MXN", 1)]
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();

        await _purchaseOrderRepository.DidNotReceive().AddAsync(
            Arg.Any<PurchaseOrder>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCreateOrderWithAllItems()
    {
        var command = new CreatePurchaseOrderCommand(
            Guid.NewGuid(),
            null,
            [
                new CreatePurchaseOrderItemRequest(Guid.NewGuid(), "Product A", 100m, "MXN", 1),
                new CreatePurchaseOrderItemRequest(Guid.NewGuid(), "Product B", 200m, "MXN", 3)
            ]
        );

        PurchaseOrder? captured = null;
        await _purchaseOrderRepository.AddAsync(
            Arg.Do<PurchaseOrder>(po => captured = po),
            Arg.Any<CancellationToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Items.Should().HaveCount(2);
    }
}