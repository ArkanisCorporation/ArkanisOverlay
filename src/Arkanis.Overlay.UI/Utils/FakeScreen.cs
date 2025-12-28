namespace Arkanis.Overlay.UI.ViewModels;

using ReactiveUI;

public class FakeScreen : IScreen
{
    public static readonly FakeScreen Instance = new();

    public RoutingState Router { get; } = new();

    private FakeScreen()
    {
    }
}
