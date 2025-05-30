namespace Arkanis.Overlay.Common.UnitTests.Abstractions;

using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public interface IDependencyInjectionTestBed
{
    TestBedFixture Fixture { get; }
    ITestOutputHelper OutputHelper { get; }
}
