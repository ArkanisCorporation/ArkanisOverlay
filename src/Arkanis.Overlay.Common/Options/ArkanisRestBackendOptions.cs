namespace Arkanis.Overlay.Common.Options;

using Abstractions;

public class ArkanisRestBackendOptions : ISelfBindableOptions
{
    public Uri BaseAddress { get; set; } = new("https://api.arkanis.cc");

    public string SectionPath
        => "Backend:Rest";
}
