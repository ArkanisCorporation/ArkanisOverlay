namespace Arkanis.Overlay.Common.Abstractions;

using Microsoft.Extensions.Configuration;

public interface ISelfBindableOptions
{
    public string SectionPath { get; }

    public void Bind(IConfiguration configuration)
        => configuration.GetSection(SectionPath)
            .Bind
            (
                this,
                opts => opts.ErrorOnUnknownConfiguration = true);
}
