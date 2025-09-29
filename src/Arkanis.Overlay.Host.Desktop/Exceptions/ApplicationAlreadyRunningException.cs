namespace Arkanis.Overlay.Host.Desktop.Exceptions;

using Common;

public class ApplicationAlreadyRunningException() : Exception($"Another instance of {ApplicationConstants.ApplicationName} is already running.");
