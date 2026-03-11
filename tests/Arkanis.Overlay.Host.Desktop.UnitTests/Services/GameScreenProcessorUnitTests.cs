namespace Arkanis.Overlay.Host.Desktop.UnitTests.Services;

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Desktop.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

[TestSubject(typeof(GameScreenProcessor))]
[SuppressMessage("Usage", "xUnit1046:Avoid using TheoryDataRow arguments that are not serializable")]
public class GameScreenProcessorUnitTests
{
    public static DirectoryInfo SourceDirectory { get; set; } = new(@"D:\Git\github\ArkanisCorporation\ArkanisOverlay\GameScreens");

    public GameScreenProcessor Subject { get; set; } = new(null!, null!, NullLogger<GameScreenProcessor>.Instance);

    public static TheoryDataRow<FileInfo>[] SourceFiles
        => SourceDirectory
            .GetFiles()
            .Where(f => !f.Name.Contains('_'))
            .Select(f => new TheoryDataRow<FileInfo>(f))
            .ToArray();

    [Theory]
    [MemberData(nameof(SourceFiles))]
    public void ProcessImage(FileInfo imageFile)
    {
        // Arrange
        var image = Image.FromFile(imageFile.FullName) as Bitmap;
        image.ShouldNotBeNull();

        // Act
        Subject.ProcessImage(image, imageFile.FullName);

        // Assert
    }
}
