namespace Arkanis.Overlay.UI.Utils;

using ReactiveUI;

public class FakeScreen : IScreen
{
    public static readonly FakeScreen Instance = new();

    private FakeScreen()
    {
    }

    public RoutingState Router { get; } = new();
}
