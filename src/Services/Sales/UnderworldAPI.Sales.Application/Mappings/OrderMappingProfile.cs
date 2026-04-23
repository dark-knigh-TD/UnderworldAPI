using System;
using AutoMapper;
using UnderworldAPI.Sales.Application.DTOs;
using UnderworldAPI.Sales.Domain.Aggregates.Order;

namespace UnderworldAPI.Sales.Application.Mappings;

public sealed class OrderMappingProfile:Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerId,
                opt => opt.MapFrom(src => src.CustomerId.Value))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount,
                opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency,
                opt => opt.MapFrom(src => src.TotalAmount.Currency));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice,
                opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency,
                opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Total,
                opt => opt.MapFrom(src => src.Total.Amount));
    }
}
