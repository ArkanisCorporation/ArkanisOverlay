namespace Arkanis.Overlay.Application.Workers;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

public static class ProcessExitWatcher
{
    /// <summary>
    /// The maximum amount of time to wait for the process to exit.
    /// Will loop in order to be able to respond to cancellation requests.
    /// </summary>
    private const uint DelayWaitMilliseconds = 100;

    public static Task WaitForProcessExitAsync(uint processId, CancellationToken cancellationToken = default)
        => Task.Run(
            () =>
            {
                var processHandle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE, false, processId);
                if (processHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to open process handle.");
                }

                try
                {
                    while (true)
                    {
                        // var waitResult = PInvoke.WaitForSingleObject(processHandle, PInvoke.INFINITE);
                        var waitResult = PInvoke.WaitForSingleObject(processHandle, DelayWaitMilliseconds);
                        if (waitResult == WAIT_EVENT.WAIT_OBJECT_0)
                        {
                            Console.WriteLine("Process has exited.");
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    PInvoke.CloseHandle(processHandle);
                }
            },
            cancellationToken
        );
}
