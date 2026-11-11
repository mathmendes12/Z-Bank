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
