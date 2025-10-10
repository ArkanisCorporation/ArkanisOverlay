namespace Arkanis.Overlay.Common.Exceptions;

public class ExternalApiException(string message, Exception? innerException) : Exception(message, innerException);
