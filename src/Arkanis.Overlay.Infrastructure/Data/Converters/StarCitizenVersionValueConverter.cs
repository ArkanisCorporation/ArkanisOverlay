namespace Arkanis.Overlay.Infrastructure.Data.Converters;

using Domain.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public sealed class StarCitizenVersionValueConverter() : ValueConverter<StarCitizenVersion, string>(
    version => version.Version,
    version => StarCitizenVersion.Create(version)
);
