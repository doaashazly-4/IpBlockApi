using System.ComponentModel.DataAnnotations;

namespace IpBlockApi.Models.Dtos;

public sealed class TemporalBlockRequest
{
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = "";

    [Range(1, 1440)]
    public int DurationMinutes { get; set; }
}
