using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ZBank.IntegrationTests.Contract;

public class ApiContractConformanceTests
{
    // Rotas já no contrato mas ainda não na master (vivem em feat/fase-2-ledger).
    // Ao rebasear a Fase 2, Pending_list_has_no_stale_entries obriga a esvaziar esta lista.
    private static readonly HashSet<OperationKey> PendingInCode = new()
    {
    };

    [Fact]
    public async Task Routes_and_methods_match_the_contract()
    {
        var contract = ContractDocument.Load();
        using var factory = new WebApplicationFactory<Program>();
        var code = await ApiEndpoints.Read(factory.Services);

        var expected = contract.Keys.Where(k => !PendingInCode.Contains(k)).ToHashSet();
        var missingInCode = expected.Except(code.Keys).Select(k => k.ToString()).OrderBy(x => x).ToList();
        var missingInContract = code.Keys.Except(contract.Keys).Select(k => k.ToString()).OrderBy(x => x).ToList();

        Assert.True(missingInCode.Count == 0,
            "No contrato mas não no código: " + string.Join(", ", missingInCode));
        Assert.True(missingInContract.Count == 0,
            "No código mas não no contrato: " + string.Join(", ", missingInContract));
    }

    [Fact]
    public async Task Authentication_requirement_matches_the_contract()
    {
        var contract = ContractDocument.Load();
        using var factory = new WebApplicationFactory<Program>();
        var code = await ApiEndpoints.Read(factory.Services);

        var divergences = contract
            .Where(c => code.TryGetValue(c.Key, out var requiresAuth) && requiresAuth != c.Value)
            .Select(c => $"{c.Key}: contrato exige auth={c.Value}, código exige auth={code[c.Key]}")
            .OrderBy(x => x)
            .ToList();

        Assert.True(divergences.Count == 0, string.Join(Environment.NewLine, divergences));
    }

    [Fact]
    public async Task Pending_list_has_no_stale_entries()
    {
        using var factory = new WebApplicationFactory<Program>();
        var code = await ApiEndpoints.Read(factory.Services);

        var stale = PendingInCode.Where(code.ContainsKey).Select(k => k.ToString()).ToList();

        Assert.True(stale.Count == 0,
            "Já existem no código; remova de PendingInCode: " + string.Join(", ", stale));
    }
}
