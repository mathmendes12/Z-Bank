using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ZBank.IntegrationTests.Contract;

public static class ApiEndpoints
{
    private const string Prefix = "/api/v1/";

    /// <summary>Endpoints /api/v1 registrados; o valor indica se exigem autenticação.</summary>
    public static async Task<IReadOnlyDictionary<OperationKey, bool>> Read(IServiceProvider services)
    {
        var dataSource = services.GetRequiredService<EndpointDataSource>();
        var policyProvider = services.GetRequiredService<IAuthorizationPolicyProvider>();
        var hasFallbackPolicy = await policyProvider.GetFallbackPolicyAsync() is not null;

        var result = new Dictionary<OperationKey, bool>();
        foreach (var endpoint in dataSource.Endpoints.OfType<RouteEndpoint>())
        {
            var route = "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/');
            if (!route.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;
            if (methods is null) continue;

            var anonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
            var requiresAuth = !anonymous &&
                (endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Any() || hasFallbackPolicy);

            foreach (var method in methods)
                result[new OperationKey(method.ToUpperInvariant(), route)] = requiresAuth;
        }
        return result;
    }
}
