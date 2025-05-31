namespace Arkanis.Overlay.Common.UnitTests.Extensions;

using Shouldly;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

public static class TestBedFixtureExtensions
{
    public static T GetRequiredService<T>(this TestBedFixture @this, ITestOutputHelper testOutputHelper)
        where T : class
        => @this.GetService<T>(testOutputHelper).ShouldBeAssignableTo<T>().ShouldNotBeNull();
}
