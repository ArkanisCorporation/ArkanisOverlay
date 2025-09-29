namespace Arkanis.Overlay.Common;

public static class ApplicationConstants
{
    public const string GoogleAnalyticsTrackingId = "G-ND6WBR51VP";

    public const string ApplicationName = "Arkanis Overlay";
    public const string ApplicationSlug = "ArkanisOverlay";

    public const string AppDirectoryName = ApplicationSlug;
    public const string DataDirectoryName = "data";
    public const string LogsDirectoryName = "logs";

    public const string GitHubOwner = "ArkanisCorporation";
    public const string GitHubRepository = "ArkanisOverlay";
    public const string GitHubRepositoryUrl = $"https://github.com/{GitHubOwner}/{GitHubRepository}";

    public const string CurrencyName = "Alpha United Earth Credits";
    public const string CurrencyAbbr = "aUEC";
    public const string CurrencySymbol = "\u00A4";

    public static readonly int GameTimeYearOffset = 930;

    private static readonly string ApplicationDirectoryPath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppDirectoryName
    );

    private static readonly string ApplicationDataDirectoryPath = Path.Join(ApplicationDirectoryPath, DataDirectoryName);
    private static readonly string ApplicationLogsDirectoryPath = Path.Join(ApplicationDirectoryPath, LogsDirectoryName);

    public static readonly DirectoryInfo ApplicationDataDirectory = Directory.CreateDirectory(ApplicationDataDirectoryPath);
    public static readonly DirectoryInfo ApplicationLogsDirectory = Directory.CreateDirectory(ApplicationLogsDirectoryPath);

    public static class Company
    {
        public const string ShortName = "Arkanis";
        public const string FullName = "Arkanis Corporation";
        public const string Slug = "ArkanisCorp";
    }

    public static class Protocol
    {
        private const string AppName = "Overlay";

        public const string Schema = $"{Company.Slug}-{AppName}";

        private static readonly Uri BaseUri = new($"{Schema.ToLowerInvariant()}://");

        public static Uri CreateUriFor(string path)
            => new(BaseUri, path);
    }

    public static class Args
    {
        public static readonly string[] All = [HandleUrl];

        public const string HandleUrl = "handle-url";

        public static class Config
        {
            private const string ConfigurationSectionPath = "CommandLine";

            public static string GetKeyFor(string key)
                => $"{ConfigurationSectionPath}:{key}";
        }
    }
}
