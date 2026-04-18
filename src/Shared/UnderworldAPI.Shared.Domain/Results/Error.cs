using System;

namespace UnderworldAPI.Shared.Domain.Results;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Errores genéricos reutilizables
    public static readonly Error NullValue = new(
        "General.NullValue",
        "A null value was provided.");

    public static Error NotFound(string entity, Guid id) => new(
        $"{entity}.NotFound",
        $"{entity} with Id '{id}' was not found.");

    public static Error Conflict(string entity, string detail) => new(
        $"{entity}.Conflict",
        detail);

    public static Error Validation(string field, string detail) => new(
        $"Validation.{field}",
        detail);

    public override string ToString() => $"{Code}: {Description}";
}
