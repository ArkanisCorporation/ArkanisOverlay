namespace Arkanis.Overlay.Components.Helpers;

public static class RsiReferrals
{
    private static readonly Random Randomness = new();

    private static readonly string[] Codes = ["STAR-VS5N-GKW4", "STAR-YYM7-4CKD"];

    public static string RandomCode
        => Codes[Randomness.Next(Codes.Length)];

    public static string RandomReferralLink
        => $"https://www.robertsspaceindustries.com/enlist?referral={RandomCode}";
}
