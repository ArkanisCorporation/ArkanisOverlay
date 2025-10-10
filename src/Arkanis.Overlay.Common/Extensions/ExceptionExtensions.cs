namespace Arkanis.Overlay.Common.Extensions;

using FluentResults;

public static class ExceptionExtensions
{
    public static Result ToResult(this Exception exception)
        => Result.Fail(exception.ToError());

    public static Result ToResult(this Exception exception, string errorMessage)
        => Result.Fail(exception.ToError(errorMessage));

    public static Error ToError(this Exception exception)
        => new ExceptionalError(exception);

    public static Error ToError(this Exception exception, string errorMessage)
        => new ExceptionalError(errorMessage, exception);
}
