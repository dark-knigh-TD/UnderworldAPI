using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

namespace UnderworldAPI.Purchases.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderItemEntityConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
         builder.ToTable("PurchaseOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.PurchaseOrderId)
            .IsRequired();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

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

        builder.Property(i => i.ReceivedQuantity)
            .IsRequired(false);

        builder.Ignore(i => i.TotalPrice);

        builder.HasIndex(i => i.PurchaseOrderId)
            .HasDatabaseName("IX_PurchaseOrderItems_PurchaseOrderId");

        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_PurchaseOrderItems_ProductId");
    }
}
