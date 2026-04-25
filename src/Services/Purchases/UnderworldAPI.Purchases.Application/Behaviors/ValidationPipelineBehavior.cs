using System;
using FluentValidation;
using MediatR;
using UnderworldAPI.Shared.Domain.Results;

namespace UnderworldAPI.Purchases.Application.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
            .ToList();

        if (failures.Count != 0)
            return CreateFailureResult<TResponse>(failures.First());

        return await next();
    }

    private static T CreateFailureResult<T>(Error error) where T : Result
    {
        var resultType = typeof(T);

        if (resultType == typeof(Result))
            return (T)(object)Result.Failure(error);

        var valueType = resultType.GetGenericArguments()[0];
        var method = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(valueType);

        return (T)method.Invoke(null, [error])!;
    }
}
