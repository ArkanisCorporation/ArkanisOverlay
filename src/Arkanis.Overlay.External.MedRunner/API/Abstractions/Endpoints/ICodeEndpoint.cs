namespace Arkanis.Overlay.External.MedRunner.API.Abstractions.Endpoints;

using Models;

/// <summary>
///     Endpoints for interacting with promotional codes.
/// </summary>
public interface ICodeEndpoint
{
    /// <summary>
    ///     Gets the redeemed codes for the current user.
    /// </summary>
    /// <returns>An API response containing a list of redeemed promotional codes.</returns>
    Task<ApiResponse<List<PromotionalCode>>> GetRedeemedCodesAsync();

    /// <summary>
    ///     Redeems the specified promotional code for the current user.
    /// </summary>
    /// <param name="code">The promotional code to redeem.</param>
    /// <returns>An API response indicating success or failure.</returns>
    Task<ApiResponse<string>> RedeemAsync(string code);
}
