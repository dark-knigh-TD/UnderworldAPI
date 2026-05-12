using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Purchases.Application.Mappings;
using UnderworldAPI.Purchases.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;
using UnderworldAPI.Purchases.Domain.Ports.Repositories;
using UnderworldAPI.Purchases.Domain.ValueObjects;

namespace UnderworldAPI.Purchases.Application.Tests.PurchaseOrders.Queries;

public class GetPurchaseOrderByIdQueryHandlerTests
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IMapper _mapper;
    private readonly GetPurchaseOrderByIdQueryHandler _handler;

    public GetPurchaseOrderByIdQueryHandlerTests()
    {
        _purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();

        _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<PurchaseOrderMappingProfile>(),
            NullLoggerFactory.Instance
        ).CreateMapper();

    _handler = new GetPurchaseOrderByIdQueryHandler(_purchaseOrderRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WithExistingPurchaseOrder_ShouldReturnDto()
    {
        // Arrange
        var po = PurchaseOrder.Create(Guid.NewGuid()).Value;
        var money = Money.Create(200m, "MXN").Value;
        po.AddItem(Guid.NewGuid(), "Teclado", money, 3);

        _purchaseOrderRepository
            .GetByIdAsync(po.Id, Arg.Any<CancellationToken>())
            .Returns(po);

        var query = new GetPurchaseOrderByIdQuery(po.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        //result.Value.Should().BeOfType<PurchaseOrderDto>();
        result.Value.Id.Should().Be(po.Id);
        result.Value.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Handle_WithNonExistentPurchaseOrder_ShouldReturnFailure()
    {
        // Arrange
        _purchaseOrderRepository
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PurchaseOrder?)null);

        var query = new GetPurchaseOrderByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }
}