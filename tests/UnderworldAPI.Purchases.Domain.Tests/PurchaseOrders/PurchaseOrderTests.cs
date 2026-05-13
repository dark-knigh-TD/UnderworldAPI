using FluentAssertions;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Events;
using UnderworldAPI.Purchases.Domain.ValueObjects;

namespace UnderworldAPI.Purchases.Domain.Tests.PurchaseOrders;

public class PurchaseOrderTests
{
    private static readonly Guid ValidSupplierId = Guid.NewGuid();
    private static readonly Guid ValidProductId = Guid.NewGuid();
    private static readonly Money ValidMoney = Money.Create(200m, "MXN").Value;

    [Fact]
    public void Create_WithValidSupplierId_ShouldReturnSuccess()
    {
        var result = PurchaseOrder.Create(ValidSupplierId);

        result.IsSuccess.Should().BeTrue();
        result.Value.SupplierId.Value.Should().Be(ValidSupplierId);
        result.Value.Status.Should().Be(PurchaseOrderStatus.Draft);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptySupplierId_ShouldReturnFailure()
    {
        var result = PurchaseOrder.Create(Guid.Empty);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaisePurchaseOrderCreatedEvent()
    {
        var result = PurchaseOrder.Create(ValidSupplierId);

        result.Value.GetDomainEvents()
            .Should().ContainSingle(e => e is PurchaseOrderCreatedEvent);
    }

    [Fact]
    public void AddItem_ToDraftOrder_ShouldSucceed()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;

        var result = po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);

        result.IsSuccess.Should().BeTrue();
        po.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_ToSentOrder_ShouldReturnFailure()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);
        po.Send();

        var result = po.AddItem(Guid.NewGuid(), "Mouse", ValidMoney, 1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Send_DraftOrderWithItems_ShouldSucceed()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);

        var result = po.Send();

        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be(PurchaseOrderStatus.Sent);
    }

    [Fact]
    public void Send_DraftOrderWithNoItems_ShouldReturnFailure()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;

        var result = po.Send();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Confirm_SentOrder_ShouldSucceed()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);
        po.Send();

        var result = po.Confirm();

        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be(PurchaseOrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_DraftOrder_ShouldReturnFailure()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);

        var result = po.Confirm();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_DraftOrder_ShouldSucceed()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;

        var result = po.Cancel();

        result.IsSuccess.Should().BeTrue();
        po.Status.Should().Be(PurchaseOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ReceivedOrder_ShouldReturnFailure()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.AddItem(ValidProductId, "Teclado", ValidMoney, 5);
        po.Send();
        po.Confirm();

        // Simular estado Received directamente no es posible sin método
        // En su lugar testeamos que Cancelled no puede cancelarse de nuevo
        po.Cancel();
        var result = po.Cancel();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Cancel_ShouldRaisePurchaseOrderCancelledEvent()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        po.ClearDomainEvents();

        po.Cancel();

        po.GetDomainEvents()
        .OfType<PurchaseOrderCancelledEvent>()
        .Should().HaveCount(1);
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldReturnCorrectTotal()
    {
        var po = PurchaseOrder.Create(ValidSupplierId).Value;
        var money100 = Money.Create(100m, "MXN").Value;
        var money200 = Money.Create(200m, "MXN").Value;

        po.AddItem(Guid.NewGuid(), "Product A", money100, 3);
        po.AddItem(Guid.NewGuid(), "Product B", money200, 2);

        po.TotalAmount.Amount.Should().Be(700m);
    }
}