namespace Arkanis.Overlay.Common.Errors;

using FluentResults;

public class ExternalAccountError : Error
{
    public ExternalAccountError(string message) : base(message)
    {
    }

    public ExternalAccountError(string message, IError causedBy) : base(message, causedBy)
    {
    }
}

public class ExternalAccountNotFoundError : ExternalAccountError
{
    public ExternalAccountNotFoundError(string message) : base(message)
    {
    }

    public ExternalAccountNotFoundError(string message, IError causedBy) : base(message, causedBy)
    {
    }
}

public class ExternalAccountUnauthorizedError : ExternalAccountError
{
    public ExternalAccountUnauthorizedError(string message) : base(message)
    {
    }

    public ExternalAccountUnauthorizedError(string message, IError causedBy) : base(message, causedBy)
    {
    }
}
