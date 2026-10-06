namespace Arkanis.Overlay.Common.UnitTests.Abstractions;

using Xunit.Microsoft.DependencyInjection.Abstracts;

public interface IDependencyInjectionTestBed
{
    public TestBedFixture Fixture { get; }
    public ITestOutputHelper OutputHelper { get; }
}
