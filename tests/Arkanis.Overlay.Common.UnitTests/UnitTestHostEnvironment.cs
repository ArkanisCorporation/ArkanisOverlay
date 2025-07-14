namespace Arkanis.Overlay.Common.UnitTests;

using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

public class UnitTestHostEnvironment : IHostEnvironment
{
    private static readonly string DefaultContentRootPath = Path.GetTempPath();

    public string EnvironmentName { get; set; } = "Test";
    public string ApplicationName { get; set; } = ApplicationConstants.ApplicationName;
    public string ContentRootPath { get; set; } = DefaultContentRootPath;
    public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(DefaultContentRootPath);
}
