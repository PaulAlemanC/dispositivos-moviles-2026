namespace SistemaAtencionMedica.Api.Helpers.Security;

public static class SecureConfiguration
{
    /// <summary>
    /// Descifra todos los valores <c>ENC:</c> y los publica en una fuente en memoria
    /// agregada al final, de modo que tengan la máxima precedencia.
    /// </summary>
    /// <returns>Cantidad de valores descifrados.</returns>
    public static int DecryptProtectedValues(this IConfigurationManager config, ISecretProtector protector)
    {
        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in config.AsEnumerable())
        {
            if (protector.IsProtected(value))
            {
                overrides[key] = protector.Decrypt(value!);
            }
        }

        if (overrides.Count > 0)
        {
            config.AddInMemoryCollection(overrides);
        }

        return overrides.Count;
    }

    /// <summary>
    /// Imprime (solo en Development) los valores de las rutas protegidas en su forma cifrada.
    /// Nunca muestra texto plano. Pensado para usarse bajo <c>#if DEBUG</c>.
    /// </summary>
    public static void PrintEncryptedSnapshot(
        this IConfiguration config,
        IHostEnvironment env,
        ISecretProtector protector,
        IReadOnlyCollection<string> protectedPaths)
    {
        if (!env.IsDevelopment() || !protector.HasMasterKey)
        {
            return;
        }

        Console.WriteLine("[Secrets] Snapshot cifrado de configuración protegida:");
        foreach (var path in protectedPaths)
        {
            var section = config.GetSection(path);
            var entries = new List<KeyValuePair<string, string?>>();
            if (section.Value is not null)
            {
                entries.Add(new KeyValuePair<string, string?>(path, section.Value));
            }
            else
            {
                entries.AddRange(section.AsEnumerable(makePathsRelative: false).Where(kv => kv.Value is not null));
            }

            foreach (var (key, value) in entries)
            {
                Console.WriteLine($"  {key} = {protector.Encrypt(value!)}");
            }
        }
    }
}
