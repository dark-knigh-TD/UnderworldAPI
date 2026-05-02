using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UnderworldAPI.Sales.Domain.Aggregates.Order;

namespace UnderworldAPI.Sales.Infrastructure.Persistence.Configurations;

public sealed class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
{
     public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever(); // El dominio genera el Id, no la DB

        // Value Object — CustomerId se mapea como owned entity (columna plana)
        builder.OwnsOne(o => o.CustomerId, customerIdBuilder =>
        {
            customerIdBuilder.Property(c => c.Value)
                .HasColumnName("CustomerId")
                .IsRequired();
        });

        builder.Property(o => o.Status)
            .HasConversion<string>()  // Guarda "Pending", "Confirmed" etc. en vez de int
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired(false);

        // Value Object — TotalAmount es calculado, EF Core lo ignora
        builder.Ignore(o => o.TotalAmount);

        // Relación con OrderItems — cascade delete
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para búsquedas frecuentes por CustomerId
        builder.HasIndex(o => new { o.Status })
            .HasDatabaseName("IX_Orders_Status");
    }
}
