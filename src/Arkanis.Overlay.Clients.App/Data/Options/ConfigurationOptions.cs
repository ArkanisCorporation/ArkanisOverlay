namespace Arkanis.Overlay.Application.Data.Options;

using System.ComponentModel.DataAnnotations;

public class ConfigurationOptions
{
    public const string Section = "Configuration";

    [Required]
    [RegularExpression(@"^https://.*/$", ErrorMessage = "UexBaseUrl must be a valid URL and end with '/'.")]
    public required string UexBaseUrl { get; set; }
}
