using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnderworldAPI.Purchases.Domain.Aggregates.PurchaseOrder;

namespace UnderworldAPI.Purchases.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderEntityConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(po => po.SupplierId, supplierIdBuilder =>
        {
            supplierIdBuilder.Property(s => s.Value)
                .HasColumnName("SupplierId")
                .IsRequired();
        });

        builder.Property(po => po.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(po => po.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(po => po.CreatedAt)
            .IsRequired();

        builder.Property(po => po.UpdatedAt)
            .IsRequired(false);

        builder.Ignore(po => po.TotalAmount);

        builder.HasMany(po => po.Items)
            .WithOne()
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(po => po.Status)
            .HasDatabaseName("IX_PurchaseOrders_Status");
    }
}
