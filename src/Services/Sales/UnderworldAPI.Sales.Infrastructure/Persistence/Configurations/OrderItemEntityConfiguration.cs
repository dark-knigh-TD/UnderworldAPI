using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnderworldAPI.Sales.Domain.Aggregates.Order;

namespace UnderworldAPI.Sales.Infrastructure.Persistence.Configurations;

public sealed class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.OrderId)
            .IsRequired();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        // Value Object Money — mapeado como owned entity (dos columnas: Amount + Currency)
        builder.OwnsOne(i => i.UnitPrice, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("UnitPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(i => i.Quantity)
            .IsRequired();

        // Total es calculado — EF Core lo ignora
        builder.Ignore(i => i.Total);

        builder.HasIndex(i => i.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");

        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");
    }
}
