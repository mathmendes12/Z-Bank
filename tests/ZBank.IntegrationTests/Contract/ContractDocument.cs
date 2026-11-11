using System.Text.Json;
using YamlDotNet.Serialization;

namespace ZBank.IntegrationTests.Contract;

public readonly record struct OperationKey(string Method, string Route)
{
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
