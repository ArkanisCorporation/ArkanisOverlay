namespace Arkanis.Overlay.Infrastructure.Exceptions;

public class ExternalLinkException(string message, Exception? innerException) : ExternalApiException(message, innerException);

public class ExternalLinkAccountNotFoundException(string message, Exception? innerException) : ExternalLinkException(message, innerException);

public class ExternalLinkUnauthorizedException(string message, Exception? innerException) : ExternalLinkException(message, innerException);
