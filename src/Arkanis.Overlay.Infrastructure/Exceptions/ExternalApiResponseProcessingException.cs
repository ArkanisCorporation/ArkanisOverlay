namespace Arkanis.Overlay.Infrastructure.Exceptions;

public class ExternalApiResponseProcessingException(string message, Exception? innerException = null)
    : ExternalApiException(message, innerException);
