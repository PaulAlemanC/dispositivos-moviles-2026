using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SistemaAtencionMedica.Api.Services;

public class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 60;

    public bool HasSecureKey() => !string.IsNullOrWhiteSpace(Key) && Encoding.UTF8.GetByteCount(Key) >= 32;
}
