namespace Arkanis.Overlay.Infrastructure.Exceptions;

public class ObjectMappingMissingLinkedRelatedObjectException(string message)
    : ObjectMappingException(message, null);
