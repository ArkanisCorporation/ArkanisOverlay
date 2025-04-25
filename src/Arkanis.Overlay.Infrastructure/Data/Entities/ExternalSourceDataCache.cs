namespace Arkanis.Overlay.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExternalSourceDataCache : IDatabaseEntity<string>
{
    public required JsonDocument Content { get; set; }
    public required ServiceAvailableState DataAvailableState { get; set; }
    public required DateTimeOffset CachedUntil { get; set; }

    [Key]
    public required string Id { get; init; }

    internal class Configuration : IEntityTypeConfiguration<ExternalSourceDataCache>
    {
        public void Configure(EntityTypeBuilder<ExternalSourceDataCache> builder)
        {
        }
    }
}
