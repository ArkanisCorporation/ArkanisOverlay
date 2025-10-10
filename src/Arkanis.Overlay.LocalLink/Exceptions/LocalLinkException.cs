namespace Arkanis.Overlay.LocalLink.Exceptions;

public class LocalLinkException(string message, Exception? innerException = null) : Exception(message, innerException);

public class LocalLinkConnectionException(string message, Exception? innerException = null) : Exception(message, innerException);

public class LocalLinkSendException(string message, Exception? innerException = null) : LocalLinkConnectionException(message, innerException);

public class LocalLinkReceiveException(string message, Exception? innerException = null) : LocalLinkConnectionException(message, innerException);
