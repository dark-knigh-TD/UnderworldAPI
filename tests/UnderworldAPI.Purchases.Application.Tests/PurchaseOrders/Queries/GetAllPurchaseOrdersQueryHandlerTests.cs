using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UnderworldAPI.Purchases.Application.Mappings;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetAllPurchaseOrders;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Purchases.Domain.ValueObjects;

namespace UnderworldAPI.Purchases.Application.Tests.PurchaseOrders.Queries;

public class GetAllPurchaseOrdersQueryHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IMapper _mapper;
    private readonly GetAllPurchaseOrdersQueryHandler _handler;

    public GetAllPurchaseOrdersQueryHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();

        _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<PurchaseOrderMappingProfile>(),
            NullLoggerFactory.Instance
        ).CreateMapper();

    _handler = new GetAllPurchaseOrdersQueryHandler(_purchaseOrderRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WithPurchaseOrders_ShouldReturnAll()
    {
        // Arrange
        var money = Money.Create(150m, "MXN").Value;

        var po1 = PurchaseOrder.Create(Guid.NewGuid()).Value;
        po1.AddItem(Guid.NewGuid(), "Teclado", money, 2);

        var po2 = PurchaseOrder.Create(Guid.NewGuid()).Value;
        po2.AddItem(Guid.NewGuid(), "Mouse", money, 5);

        _purchaseOrderRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<PurchaseOrder> { po1, po2 });

        // Act
        var result = await _handler.Handle(
            new GetAllPurchaseOrdersQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoPurchaseOrders_ShouldReturnEmptyList()
    {
        // Arrange
        _purchaseOrderRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<PurchaseOrder>());

        // Act
        var result = await _handler.Handle(
            new GetAllPurchaseOrdersQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}