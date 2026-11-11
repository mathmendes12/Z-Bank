using System.Text.Json;
using YamlDotNet.Serialization;

namespace ZBank.IntegrationTests.Contract;

/// <summary>
/// Chave de operação. Method e Route são comparados ignorando a caixa (o roteamento do ASP.NET
/// é case-insensitive e [controller] expande capitalizado). Só a caixa é ignorada: nomes de
/// parâmetros de path ({id} vs {accountId}) não são normalizados.
/// </summary>
public readonly record struct OperationKey(string Method, string Route)
{
    public bool Equals(OperationKey other) =>
        string.Equals(Method, other.Method, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(Route, other.Route, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Method ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(Route ?? string.Empty));

    public override string ToString() => $"{Method} {Route}";
}

public static class ContractDocument
{
    private static readonly string[] HttpMethods = { "get", "put", "post", "delete", "patch", "head", "options" };

    /// <summary>Operações do contrato; o valor indica se a operação exige autenticação.</summary>
    public static IReadOnlyDictionary<OperationKey, bool> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "contract", "customer.openapi.yaml");
        var yaml = File.ReadAllText(path);

        var parsed = new DeserializerBuilder().Build().Deserialize<object>(yaml);
        var json = new SerializerBuilder().JsonCompatible().Build().Serialize(parsed);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var basePath = root.GetProperty("servers")[0].GetProperty("url").GetString()!.TrimEnd('/');
        var globalSecurity = root.TryGetProperty("security", out var gs) && gs.GetArrayLength() > 0;

        var result = new Dictionary<OperationKey, bool>();
        foreach (var pathItem in root.GetProperty("paths").EnumerateObject())
        {
            foreach (var op in pathItem.Value.EnumerateObject())
            {
                if (!HttpMethods.Contains(op.Name)) continue;

                // security ausente herda o global; security: [] torna a rota pública.
                var requiresAuth = op.Value.TryGetProperty("security", out var sec)
                    ? sec.GetArrayLength() > 0
                    : globalSecurity;

                result[new OperationKey(op.Name.ToUpperInvariant(), basePath + pathItem.Name)] = requiresAuth;
            }
        }
        return result;
    }
}
