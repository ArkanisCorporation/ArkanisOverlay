namespace Arkanis.Overlay.Application;

public static class Constants
{
    public const string InstanceId = "91df1d7c-b7fe-4a0f-91e3-22d94cd50ab5";
    public const string LocalAppDataDirectoryName = "ArkanisOverlay";

    public const string WindowClass = "CryENGINE";
    public const string WindowName = "Star Citizen "; // the trailing space is NOT a typo!!

    public static readonly string LocalAppDataPath = System.IO.Path.Join
    (
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LocalAppDataDirectoryName
    );
}
