using System.Reflection;

namespace RepairService.Core;

/// <summary>
/// Версія застосунку, задана властивістю VersionPrefix у Directory.Build.props.
/// </summary>
public static class VersionInfo
{
    /// <summary>Повертає версію збірки без хешу коміту.</summary>
    /// <returns>Рядок версії, наприклад «0.8.0».</returns>
    public static string Current()
    {
        Assembly assembly = typeof(VersionInfo).Assembly;
        AssemblyInformationalVersionAttribute? attribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        string raw = attribute?.InformationalVersion ?? "0.0.0";
        int plus = raw.IndexOf('+', StringComparison.Ordinal);
        return plus < 0 ? raw : raw[..plus];
    }
}
