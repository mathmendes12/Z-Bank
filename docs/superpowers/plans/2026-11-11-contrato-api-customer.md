# Contrato da API Customer — Plano 1 (contrato + lint + conformidade etapa 1)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publicar o contrato OpenAPI 3.1 da API Customer (11 rotas), com lint no CI e um teste que quebra se rotas, métodos ou exigência de autenticação do código divergirem do contrato.

**Architecture:** O YAML em `docs/api/` é a fonte de verdade (spec-first). Spectral valida o YAML. Um teste em `ZBank.IntegrationTests` sobe a API com `WebApplicationFactory`, lê os endpoints registrados (`EndpointDataSource`, `IAuthorizeData`, política de fallback) e compara com o YAML. As rotas de conta (Fase 2) estão no contrato mas ainda não na `master`; ficam numa lista explícita de pendências que o próprio teste obriga a esvaziar quando o código as ganhar.

**Tech Stack:** OpenAPI 3.1 (YAML), Spectral CLI 6.15.0, xUnit, `Microsoft.AspNetCore.Mvc.Testing`, YamlDotNet 16.3.0.

**Fora deste plano (planos próprios depois):** correções 1–8 da seção 6 do spec, etapa 2 do conformidade (schemas), geração de tipos TS no mobile, rebase da Fase 2.

## Global Constraints

Copiadas do spec `docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md`:

- Versão: `/api/v1` no path. Incompatível → `/v2`.
- Auth: protegido por padrão (`security` global). Rotas públicas declaram `security: []`.
- Dinheiro: `int64` em centavos; negativo = débito. Nunca float nem string decimal.
- IDs `uuid`; datas `date-time` em UTC (`Z`); JSON em `camelCase`; enums como string.
- Servidor rejeita campos desconhecidos na requisição (`additionalProperties: false` nos requests).
- Paginação por cursor: `?limit&cursor` → `{ items, nextCursor }`; `limit` padrão 20, máx 100; sem `total`.
- Erros: `application/problem+json` (RFC 9457) + extensão `code` em `SCREAMING_SNAKE`; validação traz `errors: [{ field, code }]`.
- `POST` que mexe em dinheiro exige `Idempotency-Key` (nenhuma rota atual usa).
- `/health` fica fora do contrato `v1`.
- AGENTS.md: documentos em `docs/`; ADR em `docs/adr/NNNN-titulo.md` (contexto, decisão, alternativas, consequências); nunca trabalhar direto na `main`; teste existente não se altera para passar.
- Commits terminam com a linha `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.
- Branch deste plano: `docs/contrato-api-customer` (já existe, contém o spec).

## File Structure

| Arquivo | Ação | Responsabilidade |
|---|---|---|
| `docs/api/customer.openapi.yaml` | criar | Contrato (fonte de verdade) |
| `docs/api/.spectral.yaml` | criar | Regras do lint |
| `docs/api/README.md` | criar | Convenções em texto, lista de rotas públicas, como alterar o contrato |
| `docs/adr/0005-contrato-api-customer.md` | criar | Decisão spec-first |
| `tests/ZBank.IntegrationTests/Contract/ContractDocument.cs` | criar | Lê o YAML e expõe `(método, rota, exigeAuth)` |
| `tests/ZBank.IntegrationTests/Contract/ApiEndpoints.cs` | criar | Lê os endpoints registrados no código e expõe a mesma tupla |
| `tests/ZBank.IntegrationTests/Contract/ApiContractConformanceTests.cs` | criar | Compara os dois |
| `tests/ZBank.IntegrationTests/ZBank.IntegrationTests.csproj` | modificar | YamlDotNet e cópia do YAML para a saída |
| `.github/workflows/ci.yml` | modificar | Job `contract` com Spectral |
| `docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md` | modificar | Registrar que auth é verificada via metadados do endpoint |

Ponto compartilhado entre tarefas: o tipo `OperationKey` (record) definido na Tarefa 2.

---

### Task 1: Contrato OpenAPI + lint

**Files:**
- Create: `docs/api/customer.openapi.yaml`
- Create: `docs/api/.spectral.yaml`

**Interfaces:**
- Produces: arquivo `docs/api/customer.openapi.yaml` com 11 operações (`method + path`) e `security` global `bearerAuth`; rotas públicas com `security: []`: `POST /invitations/validate`, `/auth/register`, `/auth/login`, `/auth/refresh`, `/auth/biometric/challenge`, `/auth/biometric/login`. Protegidas: `POST /auth/totp/setup`, `/auth/totp/confirm`, `/devices/register`, `GET /accounts/me/balance`, `/accounts/me/statement`.

- [ ] **Step 1: Criar as regras do lint**

`docs/api/.spectral.yaml`:

```yaml
extends: ["spectral:oas"]
rules:
  # Não exigimos metadados de organização num contrato interno
  info-contact: off
  info-license: off
  license-url: off
```

- [ ] **Step 2: Rodar o lint e ver falhar (arquivo não existe)**

Run: `npx --yes @stoplight/spectral-cli@6.15.0 lint docs/api/customer.openapi.yaml --ruleset docs/api/.spectral.yaml --fail-severity=warn`
Expected: erro de arquivo não encontrado, exit code diferente de 0.

- [ ] **Step 3: Escrever o contrato**

`docs/api/customer.openapi.yaml`:

```yaml
openapi: 3.1.0
info:
  title: ZBank Customer API
  version: 1.0.0
  description: >-
    Contrato da API do app cliente do ZBank. Este arquivo é a fonte de verdade: o código se
    conforma a ele, e um teste de integração quebra se divergirem. Convenções transversais
    estão em docs/api/README.md.
servers:
  - url: /api/v1
tags:
  - name: Invitations
    description: Validação de convites de cadastro.
  - name: Auth
    description: Cadastro, TOTP, login e renovação de sessão.
  - name: Devices
    description: Dispositivos confiáveis do cliente.
  - name: Accounts
    description: Saldo e extrato da conta.
security:
  - bearerAuth: []
paths:
  /invitations/validate:
    post:
      operationId: validateInvitation
      tags: [Invitations]
      summary: Valida um código de convite
      description: Rota pública. Código inválido ou já usado responde 200 com isValid=false.
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/ValidateInvitationRequest' }
      responses:
        '200':
          description: Resultado da validação.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/ValidateInvitationResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '429': { $ref: '#/components/responses/TooManyRequests' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/register:
    post:
      operationId: registerUser
      tags: [Auth]
      summary: Cadastra um usuário com convite válido
      description: Devolve um setupToken de curta duração, usado só para configurar o TOTP.
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/RegisterRequest' }
      responses:
        '200':
          description: Usuário criado.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/RegisterResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '422': { $ref: '#/components/responses/Unprocessable' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/totp/setup:
    post:
      operationId: setupTotp
      tags: [Auth]
      summary: Gera o segredo TOTP do usuário autenticado
      description: Exige o setupToken (Bearer) emitido no cadastro.
      responses:
        '200':
          description: Segredo gerado; o TOTP só é ativado após a confirmação.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/SetupTotpResponse' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/totp/confirm:
    post:
      operationId: confirmTotp
      tags: [Auth]
      summary: Confirma o TOTP com um código de 6 dígitos
      description: Exige o setupToken (Bearer). Código inválido responde 422 com code INVALID_TOTP.
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/ConfirmTotpRequest' }
      responses:
        '204':
          description: TOTP ativado.
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '422': { $ref: '#/components/responses/Unprocessable' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/login:
    post:
      operationId: login
      tags: [Auth]
      summary: Login com CPF, senha e código TOTP
      description: Rota pública, com limite de taxa.
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/LoginRequest' }
      responses:
        '200':
          description: Sessão criada.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/LoginResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '429': { $ref: '#/components/responses/TooManyRequests' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/refresh:
    post:
      operationId: refreshTokens
      tags: [Auth]
      summary: Renova o par de tokens
      description: O usuário é derivado do refresh token, nunca informado pelo cliente.
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/RefreshRequest' }
      responses:
        '200':
          description: Novo par de tokens.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/RefreshResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/biometric/challenge:
    post:
      operationId: createBiometricChallenge
      tags: [Auth]
      summary: Emite um nonce de uso único para o login biométrico
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/BiometricChallengeRequest' }
      responses:
        '200':
          description: Nonce emitido.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/BiometricChallengeResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        default: { $ref: '#/components/responses/InternalError' }
  /auth/biometric/login:
    post:
      operationId: biometricLogin
      tags: [Auth]
      summary: Login com assinatura do dispositivo sobre o nonce
      security: []
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/BiometricLoginRequest' }
      responses:
        '200':
          description: Sessão criada.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/LoginResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        default: { $ref: '#/components/responses/InternalError' }
  /devices/register:
    post:
      operationId: registerDevice
      tags: [Devices]
      summary: Registra a chave pública do dispositivo do usuário autenticado
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/RegisterDeviceRequest' }
      responses:
        '200':
          description: Dispositivo registrado.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/RegisterDeviceResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        default: { $ref: '#/components/responses/InternalError' }
  /accounts/me/balance:
    get:
      operationId: getBalance
      tags: [Accounts]
      summary: Saldo atual da conta do usuário autenticado
      responses:
        '200':
          description: Saldo em centavos.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/BalanceResponse' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '404': { $ref: '#/components/responses/NotFound' }
        default: { $ref: '#/components/responses/InternalError' }
  /accounts/me/statement:
    get:
      operationId: getStatement
      tags: [Accounts]
      summary: Extrato paginado por cursor, do mais recente para o mais antigo
      parameters:
        - $ref: '#/components/parameters/Limit'
        - $ref: '#/components/parameters/Cursor'
      responses:
        '200':
          description: Página do extrato.
          content:
            application/json:
              schema: { $ref: '#/components/schemas/StatementResponse' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '404': { $ref: '#/components/responses/NotFound' }
        default: { $ref: '#/components/responses/InternalError' }
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      description: accessToken, ou setupToken nas rotas de configuração de TOTP.
  parameters:
    Limit:
      name: limit
      in: query
      description: Itens por página. Acima de 100 responde 400.
      schema: { type: integer, minimum: 1, maximum: 100, default: 20 }
    Cursor:
      name: cursor
      in: query
      description: Valor opaco devolvido em nextCursor na página anterior.
      schema: { type: string }
  responses:
    BadRequest:
      description: Requisição inválida (validação).
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/ValidationProblem' }
    Unauthorized:
      description: Credencial ausente, inválida ou expirada.
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/Problem' }
    NotFound:
      description: Recurso não encontrado.
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/Problem' }
    Unprocessable:
      description: Regra de negócio violada; o campo code identifica qual.
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/Problem' }
    TooManyRequests:
      description: Limite de taxa excedido.
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/Problem' }
    InternalError:
      description: Erro inesperado. Resposta genérica, sem detalhe interno.
      content:
        application/problem+json:
          schema: { $ref: '#/components/schemas/Problem' }
  schemas:
    Cents:
      type: integer
      format: int64
      description: Valor em centavos. Negativo é débito, positivo é crédito.
      examples: [-1050]
    OperationType:
      type: string
      enum: [Deposit, Transfer, Withdrawal]
    Problem:
      type: object
      description: Erro no formato RFC 9457, com a extensão code.
      required: [type, title, status, code]
      properties:
        type: { type: string, examples: ['https://zbank/errors/account-not-found'] }
        title: { type: string }
        status: { type: integer }
        detail: { type: string }
        code:
          type: string
          description: Identificador estável em SCREAMING_SNAKE; o app traduz por ele.
          examples: [ACCOUNT_NOT_FOUND]
        traceId: { type: string }
    ValidationProblem:
      allOf:
        - $ref: '#/components/schemas/Problem'
        - type: object
          required: [errors]
          properties:
            errors:
              type: array
              items:
                type: object
                required: [field, code]
                properties:
                  field: { type: string }
                  code: { type: string }
    ValidateInvitationRequest:
      type: object
      additionalProperties: false
      required: [code]
      properties:
        code: { type: string }
    ValidateInvitationResponse:
      type: object
      required: [isValid]
      properties:
        isValid: { type: boolean }
    RegisterRequest:
      type: object
      additionalProperties: false
      required: [invitationCode, cpf, password]
      properties:
        invitationCode: { type: string }
        cpf: { type: string, description: CPF do cliente }
        password: { type: string, format: password }
    RegisterResponse:
      type: object
      required: [userId, setupToken]
      properties:
        userId: { type: string, format: uuid }
        setupToken: { type: string }
    SetupTotpResponse:
      type: object
      required: [totpSecret]
      properties:
        totpSecret: { type: string }
    ConfirmTotpRequest:
      type: object
      additionalProperties: false
      required: [totpCode]
      properties:
        totpCode: { type: string, pattern: '^[0-9]{6}$' }
    LoginRequest:
      type: object
      additionalProperties: false
      required: [cpf, password, totpCode]
      properties:
        cpf: { type: string }
        password: { type: string, format: password }
        totpCode: { type: string, pattern: '^[0-9]{6}$' }
    LoginResponse:
      type: object
      required: [accessToken, refreshToken, deviceId]
      properties:
        accessToken: { type: string }
        refreshToken: { type: string }
        deviceId: { type: string, format: uuid }
    RefreshRequest:
      type: object
      additionalProperties: false
      required: [deviceId, refreshToken]
      properties:
        deviceId: { type: string, format: uuid }
        refreshToken: { type: string }
    RefreshResponse:
      type: object
      required: [accessToken, refreshToken]
      properties:
        accessToken: { type: string }
        refreshToken: { type: string }
    BiometricChallengeRequest:
      type: object
      additionalProperties: false
      required: [deviceId]
      properties:
        deviceId: { type: string, format: uuid }
    BiometricChallengeResponse:
      type: object
      required: [nonce]
      properties:
        nonce: { type: string }
    BiometricLoginRequest:
      type: object
      additionalProperties: false
      required: [cpf, deviceId, nonce, signature]
      properties:
        cpf: { type: string }
        deviceId: { type: string, format: uuid }
        nonce: { type: string }
        signature: { type: string }
    RegisterDeviceRequest:
      type: object
      additionalProperties: false
      required: [publicKey, deviceName]
      properties:
        publicKey: { type: string }
        deviceName: { type: string }
    RegisterDeviceResponse:
      type: object
      required: [deviceId]
      properties:
        deviceId: { type: string, format: uuid }
    BalanceResponse:
      type: object
      required: [balance]
      properties:
        balance: { $ref: '#/components/schemas/Cents' }
    LedgerEntry:
      type: object
      required: [id, amount, operationType, transactionGroupId, createdAt]
      properties:
        id: { type: string, format: uuid }
        amount: { $ref: '#/components/schemas/Cents' }
        operationType: { $ref: '#/components/schemas/OperationType' }
        transactionGroupId: { type: string, format: uuid }
        createdAt: { type: string, format: date-time }
    StatementResponse:
      type: object
      required: [items, nextCursor]
      properties:
        items:
          type: array
          items: { $ref: '#/components/schemas/LedgerEntry' }
        nextCursor:
          type: [string, 'null']
          description: Nulo quando não há mais páginas.
```

- [ ] **Step 4: Rodar o lint e ver passar**

Run: `npx --yes @stoplight/spectral-cli@6.15.0 lint docs/api/customer.openapi.yaml --ruleset docs/api/.spectral.yaml --fail-severity=warn`
Expected: `No results with a severity of 'warn' or higher found!` e exit code 0.
Se aparecer algum aviso: corrigir o YAML. Não relaxar regras no `.spectral.yaml` além das três já desligadas.

- [ ] **Step 5: Commit**

```bash
git add docs/api/customer.openapi.yaml docs/api/.spectral.yaml
git commit -m "docs(api): contrato OpenAPI 3.1 da API Customer e regras do Spectral

Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
```

---

### Task 2: Teste de conformidade, etapa 1 (rotas, métodos e autenticação)

**Files:**
- Modify: `tests/ZBank.IntegrationTests/ZBank.IntegrationTests.csproj`
- Create: `tests/ZBank.IntegrationTests/Contract/ContractDocument.cs`
- Create: `tests/ZBank.IntegrationTests/Contract/ApiEndpoints.cs`
- Create: `tests/ZBank.IntegrationTests/Contract/ApiContractConformanceTests.cs`

**Interfaces:**
- Consumes: `docs/api/customer.openapi.yaml` da Tarefa 1, copiado para `<saída>/contract/customer.openapi.yaml`.
- Produces:
  - `public readonly record struct OperationKey(string Method, string Route)` com `override string ToString() => $"{Method} {Route}"`, em `ZBank.IntegrationTests.Contract`.
  - `ContractDocument.Load() : IReadOnlyDictionary<OperationKey, bool>` (valor = exige autenticação).
  - `ApiEndpoints.Read(IServiceProvider services) : Task<IReadOnlyDictionary<OperationKey, bool>>` (valor = exige autenticação).

- [ ] **Step 1: Adicionar YamlDotNet e a cópia do contrato no csproj**

Em `tests/ZBank.IntegrationTests/ZBank.IntegrationTests.csproj`, dentro do primeiro `<ItemGroup>` de pacotes, depois da linha do `xunit.runner.visualstudio`, adicionar:

```xml
    <PackageReference Include="YamlDotNet" Version="16.3.0" />
```

E antes do `</Project>` final, adicionar:

```xml
  <ItemGroup>
    <None Include="..\..\docs\api\customer.openapi.yaml" Link="contract\customer.openapi.yaml" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```

- [ ] **Step 2: Escrever o teste (falha por não compilar)**

`tests/ZBank.IntegrationTests/Contract/ApiContractConformanceTests.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ZBank.IntegrationTests.Contract;

public class ApiContractConformanceTests
{
    // Rotas já no contrato mas ainda não na master (vivem em feat/fase-2-ledger).
    // Ao rebasear a Fase 2, Pending_list_has_no_stale_entries obriga a esvaziar esta lista.
    private static readonly HashSet<OperationKey> PendingInCode = new()
    {
        new("GET", "/api/v1/accounts/me/balance"),
        new("GET", "/api/v1/accounts/me/statement"),
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
```

- [ ] **Step 3: Rodar e ver falhar**

Run: `dotnet test tests/ZBank.IntegrationTests --filter "FullyQualifiedName~ApiContractConformanceTests"`
Expected: erro de compilação, `OperationKey`, `ContractDocument` e `ApiEndpoints` não existem.

- [ ] **Step 4: Implementar o leitor do contrato**

`tests/ZBank.IntegrationTests/Contract/ContractDocument.cs`:

```csharp
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
```

- [ ] **Step 5: Implementar o leitor dos endpoints do código**

`tests/ZBank.IntegrationTests/Contract/ApiEndpoints.cs`:

```csharp
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
```

- [ ] **Step 6: Rodar e ver passar**

Run: `dotnet test tests/ZBank.IntegrationTests --filter "FullyQualifiedName~ApiContractConformanceTests"`
Expected: 3 aprovados. Se `Routes_and_methods_match_the_contract` listar rotas, conferir se o contrato ou o código está errado antes de mexer em qualquer um dos dois.

- [ ] **Step 7: Verificar que o teste realmente detecta divergência (mutação manual)**

1. Em `docs/api/customer.openapi.yaml`, apague a linha `      security: []` da operação `login`.
2. Rode o mesmo comando do Step 6. Expected: `Authentication_requirement_matches_the_contract` FALHA com `POST /api/v1/auth/login: contrato exige auth=True, código exige auth=False`.
3. Desfaça a mudança (`git checkout docs/api/customer.openapi.yaml`) e rode de novo. Expected: 3 aprovados.

- [ ] **Step 8: Commit**

```bash
git add tests/ZBank.IntegrationTests/ZBank.IntegrationTests.csproj tests/ZBank.IntegrationTests/Contract
git commit -m "test(api): conformidade etapa 1 entre contrato OpenAPI e endpoints (rotas, metodos e auth)

Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
```

---

### Task 3: README, ADR 0005, job de CI e ajuste do spec

**Files:**
- Create: `docs/api/README.md`
- Create: `docs/adr/0005-contrato-api-customer.md`
- Modify: `.github/workflows/ci.yml`
- Modify: `docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md`

**Interfaces:**
- Consumes: nomes de arquivos e comandos das Tarefas 1 e 2.

- [ ] **Step 1: Escrever o README das convenções**

`docs/api/README.md`:

````markdown
# API Customer — contrato

O contrato é `customer.openapi.yaml` (OpenAPI 3.1). **Ele é a fonte de verdade**: o código se
conforma a ele, não o contrário. Decisão registrada em `docs/adr/0005-contrato-api-customer.md`.

## Como alterar a API

1. Altere `customer.openapi.yaml` primeiro.
2. Rode o lint: `npx --yes @stoplight/spectral-cli@6.15.0 lint docs/api/customer.openapi.yaml --ruleset docs/api/.spectral.yaml --fail-severity=warn`
3. Rode o conformidade: `dotnet test tests/ZBank.IntegrationTests --filter "FullyQualifiedName~ApiContractConformanceTests"`
4. Ajuste o código até os dois passarem.

Mudança **compatível**: campo opcional novo, endpoint novo. Mudança **incompatível** (remover ou
renomear campo, mudar tipo, mudar enum): exige `/v2`.

## Convenções

| Tema | Regra |
|---|---|
| Versão | `/api/v1` no path |
| Autenticação | Protegido por padrão. Rota pública declara `security: []` e consta na lista abaixo |
| Dinheiro | `int64` em centavos; negativo é débito. Nunca float nem string decimal |
| IDs e datas | `uuid`; `date-time` em UTC (`Z`); data sem hora usa `date` |
| JSON | `camelCase`; enums como string |
| Evolução | Cliente ignora campos novos na resposta; servidor rejeita campos desconhecidos na requisição |
| Paginação | Cursor: `?limit&cursor` devolve `{ items, nextCursor }`. `limit` padrão 20, máximo 100 (acima, 400). Mais recente primeiro. Cursor opaco. Sem `total` |
| Erros | `application/problem+json` (RFC 9457) com `code` estável em `SCREAMING_SNAKE`. Validação traz `errors: [{ field, code }]`. O app traduz por `code`, nunca por `detail` |
| Status | 400 validação, 401 sem identidade, 403 sem permissão, 404, 409 conflito, 422 regra de negócio, 429 limite de taxa |
| Idempotência | `POST` que mexe em dinheiro exige o header `Idempotency-Key` (uuid); a mesma chave devolve a resposta original. Nenhuma rota atual usa |

## Rotas públicas

`POST /invitations/validate`, `POST /auth/register`, `POST /auth/login`, `POST /auth/refresh`,
`POST /auth/biometric/challenge`, `POST /auth/biometric/login`.

`/auth/totp/setup` e `/auth/totp/confirm` exigem o `setupToken` emitido no cadastro.
`/health` fica fora do contrato `v1`.

## Estado de conformidade

O contrato descreve o estado-alvo. Divergências conhecidas do código atual estão na seção 6 de
`docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md`. A etapa 1 do teste de
conformidade (rotas, métodos, autenticação) já é exigida; a etapa 2 (schemas) é ligada junto de
cada correção.
````

- [ ] **Step 2: Escrever o ADR**

`docs/adr/0005-contrato-api-customer.md`:

```markdown
# ADR 0005 — Contrato da API Customer em OpenAPI, spec-first

**Status:** aceito
**Data:** 2026-11-11

## Contexto

Backend e app mobile evoluem em paralelo e já divergem: `mobile/src/services/account.ts` ignora a
paginação do extrato e recebe `operationType` como número solto; os formatos de erro variam entre
controllers e o `GlobalExceptionHandlerMiddleware`; a Fase 1 mudou contratos (TOTP, biometria) sem
um documento único que os descrevesse. O app é distribuído por OTA, então versões antigas ficam em
uso e mudança incompatível custa caro.

## Decisão

1. O contrato é `docs/api/customer.openapi.yaml` (OpenAPI 3.1), escrito à mão. É a fonte de verdade.
2. Convenções transversais (dinheiro em centavos, cursor, `problem+json` com `code`, autenticação
   por padrão, idempotência) ficam em `docs/api/README.md`.
3. Duas travas: lint com Spectral no CI, e teste de conformidade em `ZBank.IntegrationTests` que
   compara rotas, métodos e exigência de autenticação com os endpoints registrados no código. A
   autenticação é lida dos metadados do endpoint porque o gerador nativo do .NET não emite
   `security` a partir de `[Authorize]`. A comparação de schemas entra depois, junto de cada
   correção.
4. Paginação do extrato por cursor, decidida antes de o app publicar um consumo de `page`/`limit`.

## Alternativas consideradas

- **Code-first (gerar o OpenAPI do código):** menos trabalho inicial, mas o contrato só reflete o que
  já foi codado e não previne divergência.
- **Markdown em `docs/api/`:** legível, sem validação automática nem geração de tipos; envelhece.
- **Paginação `page`/`limit`:** simples, mas instável com escrita concorrente (item duplicado ou
  perdido ao entrar lançamento novo) e lenta com histórico grande. Trocar depois obrigaria a manter
  os dois formatos por causa das versões do app em campo.
- **Envelope de erro próprio:** sem suporte pronto em ferramentas; `problem+json` é padrão e nativo
  no ASP.NET Core.

## Consequências

- Toda mudança de API começa por um diff no YAML.
- O contrato descreve o estado-alvo; as divergências do código atual (seção 6 do spec) viram tarefas
  próprias, três delas de segurança (formato de erro sem vazar exceção, rate limit, `refresh` sem
  `userId` do cliente).
- As rotas de conta (Fase 2) constam no contrato mas ficam numa lista de pendências do teste até a
  Fase 2 ser rebaseada na `master`.
- Os tipos TypeScript do mobile passam a ser gerados do YAML em um plano posterior.
```

- [ ] **Step 3: Adicionar o job de lint ao CI**

Em `.github/workflows/ci.yml`, adicionar ao final do arquivo (mesma indentação dos jobs `backend` e `mobile`):

```yaml

  contract:
    name: Contrato da API (Spectral)
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: "24"
      - name: Lint OpenAPI
        run: npx --yes @stoplight/spectral-cli@6.15.0 lint docs/api/customer.openapi.yaml --ruleset docs/api/.spectral.yaml --fail-severity=warn
```

O job `backend` já roda `dotnet test` na solução inteira, então o teste de conformidade entra no CI sem mudança adicional. Confirme que o arquivo continua YAML válido:

Run: `npx --yes js-yaml .github/workflows/ci.yml > /dev/null && echo ok`
Expected: `ok`

- [ ] **Step 4: Registrar no spec que a autenticação é verificada via metadados do endpoint**

Em `docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md`, na seção 3, substituir o item 2 inteiro por:

```markdown
2. **Teste de conformidade** em `ZBank.IntegrationTests`, sobe a API com `WebApplicationFactory`.
   O `AddOpenApi()` do .NET não emite `security` a partir de `[Authorize]`, então:
   - Etapa 1 (liga já): lê os endpoints registrados (`EndpointDataSource`, `IAuthorizeData`,
     `IAllowAnonymous` e política de fallback) e confere que toda rota+método do contrato existe no
     código e vice-versa, e que a exigência de autenticação bate. Rotas ainda ausentes na `master`
     (conta, Fase 2) ficam numa lista de pendências que o próprio teste obriga a esvaziar.
   - Etapa 2 (liga junto de cada correção): compara schemas, `required` e enums contra o
     `/openapi/v1.json` gerado.
```

- [ ] **Step 5: Rodar tudo que este plano entrega**

Run: `npx --yes @stoplight/spectral-cli@6.15.0 lint docs/api/customer.openapi.yaml --ruleset docs/api/.spectral.yaml --fail-severity=warn`
Expected: sem avisos, exit 0.

Run: `dotnet test tests/ZBank.IntegrationTests --filter "FullyQualifiedName~ApiContractConformanceTests"`
Expected: 3 aprovados.

Run: `dotnet build --configuration Release`
Expected: build bem-sucedido, sem erros.

- [ ] **Step 6: Commit**

```bash
git add docs/api/README.md docs/adr/0005-contrato-api-customer.md .github/workflows/ci.yml docs/superpowers/specs/2026-11-11-contrato-api-customer-design.md
git commit -m "docs(api): README do contrato, ADR 0005, job de lint no CI e ajuste do spec

Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
```

---

## Self-Review

**Cobertura do spec (seções 2, 3, 4, 5):**
- Seção 2 (arquivos): YAML (T1), README e ADR 0005 (T3). ✔
- Seção 3 item 1 (lint no CI): T1 regras + T3 job. ✔ Item 2 etapa 1: T2. Etapa 2 e item 3 (mobile): fora deste plano, declarado no topo. ✔
- Seção 4 (convenções): refletidas no YAML (centavos, cursor, `problem+json`+`code`, `additionalProperties: false`, auth global) e no README. Idempotência declarada no README, sem rota que a use. ✔
- Seção 5 (11 rotas): todas no YAML da T1; `/health` excluído pelo filtro de prefixo `/api/v1/`. ✔
- Seções 6 e 7: itens 2–5 e 7 fora do plano (próximos planos); o item 1 da seção 7 já foi feito.

**Varredura de placeholders:** nenhum "TBD" nem passo sem código. Um comentário com nome de teste errado foi corrigido na própria revisão.

**Consistência de tipos:** `OperationKey(string Method, string Route)` definido em `ContractDocument.cs` (T2 Step 4) e usado em `ApiEndpoints.cs` e nos testes; `ContractDocument.Load()` e `ApiEndpoints.Read(IServiceProvider)` batem com as chamadas dos testes.

**Pontos de atenção na execução:**
- O Spectral roda via `npx` e precisa de rede; no CI e local.
- `WebApplicationFactory<Program>` sem mocks funciona nos testes existentes (401 sem token), então os leitores de endpoint não exigem banco.
- Na branch da Fase 2, `AccountsController` sem `[Authorize]` fará `Authentication_requirement_matches_the_contract` falhar depois que `PendingInCode` for esvaziada. É o comportamento desejado: força a correção 3 do spec.
