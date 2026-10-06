namespace Arkanis.Overlay.Components.Exceptions;

public class JsInteropException : Exception
{
    public JsInteropException(string? message) : base(message)
    {
    }

    public JsInteropException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
