namespace Arkanis.Overlay.Infrastructure.Exceptions;

public class ExternalApiException(string message, Exception? innerException) : Exception(message, innerException);
