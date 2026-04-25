using System.ComponentModel.DataAnnotations;

namespace IpBlockApi.Models.Dtos;

public sealed class BlockCountryRequest
{
    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = "";
}
