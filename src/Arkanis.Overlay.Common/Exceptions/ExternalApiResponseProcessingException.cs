namespace Arkanis.Overlay.Common.Exceptions;

public class ExternalApiResponseProcessingException(string message, Exception? innerException = null)
    : ExternalApiException(message, innerException);
