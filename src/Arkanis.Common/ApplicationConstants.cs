namespace Arkanis.Common;

using NodaMoney;

public static class ApplicationConstants
{
    public const string InstanceId = "91df1d7c-b7fe-4a0f-91e3-22d94cd50ab5";

    public const string LocalAppDataDirectoryName = "ArkanisOverlay";

    public const string CurrencyName = "Alpha United Earth Credits";
    public const string CurrencyAbbr = "aUEC";

    public static readonly CurrencyInfo GameCurrency = CurrencyInfo.Create("UEC") with
    {
        EnglishName = CurrencyName,
        MinorUnit = MinorUnit.Zero,
        IsIso4217 = false,
        Number = short.MaxValue - 1,
        InternationalSymbol = CurrencyAbbr,
        IntroducedOn = new DateTime(2528, 08, 20),
    };

    public static readonly TimeSpan GameTimeOffset = TimeSpan.FromTicks(TimeSpan.TicksPerDay * (long)(365.25 * 930));

    public static readonly string LocalAppDataPath = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        LocalAppDataDirectoryName
    );

    public static readonly DirectoryInfo LocalAppDataDir = Directory.CreateDirectory(LocalAppDataPath);

    static ApplicationConstants()
        => CurrencyInfo.Register(GameCurrency);
}
