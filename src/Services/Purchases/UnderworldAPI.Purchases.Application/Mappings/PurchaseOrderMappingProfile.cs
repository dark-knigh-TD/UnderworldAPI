using System;
using AutoMapper;
using UnderworldAPI.Purchases.Application.DTOs;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

namespace UnderworldAPI.Purchases.Application.Mappings;

public sealed class PurchaseOrderMappingProfile:Profile
{
    public PurchaseOrderMappingProfile()
    {
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(dest => dest.SupplierId,
                opt => opt.MapFrom(src => src.SupplierId.Value))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalAmount,
                opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency,
                opt => opt.MapFrom(src => src.TotalAmount.Currency));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(dest => dest.UnitPrice,
                opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency,
                opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Total,
                opt => opt.MapFrom(src => src.TotalPrice.Amount));
    }
}
