namespace Arkanis.Overlay.LocalLink.Models.Commands;

internal class TestCommand : LocalLinkCommandBase
{
    public string TestPropertyString { get; set; } = string.Empty;
    public int TestPropertyInt { get; set; } = 42;
}
