namespace Arkanis.Overlay.Infrastructure.Data.Converters;

using Domain.Models.Game;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class GameCurrencyValueConverter() : ValueConverter<GameCurrency, int>(
    currency => currency.Amount,
    currency => new GameCurrency(currency)
);
