using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UnderworldAPI.Sales.Application.Mappings;
using UnderworldAPI.Sales.Application.Orders.Queries.GetAllOrders;
using UnderworldAPI.Sales.Domain.Aggregates.Order;
using UnderworldAPI.Sales.Domain.Ports.Repositories;
using UnderworldAPI.Sales.Domain.ValueObjects;

namespace UnderworldAPI.Sales.Application.Tests.Orders.Queries;

public class GetAllOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

       _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<OrderMappingProfile>(),
            NullLoggerFactory.Instance
             ).CreateMapper();

        _handler = new GetAllOrdersQueryHandler(_orderRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WithOrders_ShouldReturnAllOrders()
    {
        // Arrange
        var money = Money.Create(100m, "MXN").Value;

        var order1 = Order.Create(Guid.NewGuid()).Value;
        order1.AddItem(Guid.NewGuid(), "Product A", money, 1);

        var order2 = Order.Create(Guid.NewGuid()).Value;
        order2.AddItem(Guid.NewGuid(), "Product B", money, 2);

        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Order> { order1, order2 });

        // Act
        var result = await _handler.Handle(
            new GetAllOrdersQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        _orderRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Order>());

        // Act
        var result = await _handler.Handle(
            new GetAllOrdersQuery(),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}