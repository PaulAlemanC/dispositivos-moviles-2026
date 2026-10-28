using System.Security.Cryptography;
using System.Text;

namespace SistemaAtencionMedica.Api.Helpers.Security;

/// <summary>Cifra y descifra valores de configuración con el esquema <c>ENC:</c>.</summary>
public interface ISecretProtector
{
    /// <summary>Indica si existe una clave maestra configurada.</summary>
    bool HasMasterKey { get; }

    /// <summary>Devuelve <c>ENC:&lt;base64(nonce|tag|cifrado)&gt;</c>.</summary>
    string Encrypt(string plainText);

    /// <summary>Descifra un valor <c>ENC:</c>; si no tiene el prefijo lo devuelve tal cual.</summary>
    string Decrypt(string value);

    /// <summary>Indica si el valor tiene el prefijo <c>ENC:</c>.</summary>
    bool IsProtected(string? value);
}

/// <summary>AES-256-GCM con clave derivada por SHA-256 de <c>Secrets:MasterKey</c>.</summary>
public sealed class SecretProtector : ISecretProtector
{
    public const string Prefix = "ENC:";
    private const int NonceLen = 12;
    private const int TagLen = 16;

    private readonly byte[]? _key;

    public SecretProtector(string? masterKey)
    {
        if (!string.IsNullOrEmpty(masterKey))
        {
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
        }
    }

    public bool HasMasterKey => _key is not null;

    /// <summary>Genera 32 bytes aleatorios en Base64.</summary>
    public static string GenerateKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public bool IsProtected(string? value) =>
        value is not null && value.StartsWith(Prefix, StringComparison.Ordinal);

    public string Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        var key = RequireKey();

        var plain = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceLen);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagLen];

        using var aes = new AesGcm(key, TagLen);
        aes.Encrypt(nonce, plain, cipher, tag);

        var payload = new byte[NonceLen + TagLen + cipher.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceLen);
        Buffer.BlockCopy(tag, 0, payload, NonceLen, TagLen);
        Buffer.BlockCopy(cipher, 0, payload, NonceLen + TagLen, cipher.Length);
        return Prefix + Convert.ToBase64String(payload);
    }

    public string Decrypt(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!IsProtected(value))
        {
            return value;
        }

        var key = RequireKey();

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(value[Prefix.Length..]);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("El valor ENC: no es Base64 válido.", ex);
        }

        if (payload.Length < NonceLen + TagLen)
        {
            throw new CryptographicException("El valor ENC: es demasiado corto.");
        }

        var nonce = payload.AsSpan(0, NonceLen);
        var tag = payload.AsSpan(NonceLen, TagLen);
        var cipher = payload.AsSpan(NonceLen + TagLen);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, TagLen);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    private byte[] RequireKey() =>
        _key ?? throw new InvalidOperationException(
            "No se configuró Secrets:MasterKey. Defínela en User Secrets o con la variable de entorno Secrets__MasterKey.");
}
