namespace Arkanis.Overlay.Infrastructure.Exceptions;

public class ObjectMappingException(string message, Exception? exception) : Exception(message, exception);
