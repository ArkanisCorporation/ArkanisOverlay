namespace Arkanis.Overlay.Common.UnitTests.Extensions;

using Abstractions;

public static class IDependencyInjectionTestBedExtensions
{
    public static T GetRequiredService<T>(this IDependencyInjectionTestBed @this)
        where T : class
        => @this.Fixture.GetRequiredService<T>(@this.OutputHelper);
}
