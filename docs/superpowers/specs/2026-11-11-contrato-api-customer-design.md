# Contrato da API Customer — Design

Data: 2026-11-11. Status: aprovado em conversa; aguardando revisão do arquivo.

## 1. Objetivo

Definir um contrato formal, versionado e verificável para a `ZBank.Api.Customer`, de modo que
backend e app mobile deixem de divergir (hoje `mobile/src/services/account.ts` e o
`AccountsController` já divergem) e que mudança incompatível não entre por acidente.

Escopo: a API Customer inteira (Fase 1 + endpoints de conta da Fase 2). Fora: API Backoffice,
SDKs em outras linguagens, portal de documentação, ETags, webhooks.

## 2. Formato e arquivos

OpenAPI 3.1 em YAML, **spec-first**: o arquivo é a fonte de verdade e o código se conforma a ele.

| Arquivo | Papel |
|---|---|
| `docs/api/customer.openapi.yaml` | Contrato (fonte de verdade) |
| `docs/api/README.md` | Convenções transversais em texto + lista de rotas públicas |
| `docs/adr/0005-contrato-api-customer.md` | Decisão spec-first, alternativas, consequências (0003 está livre/reservado) |

## 3. Verificação

1. **Lint** da spec com Spectral como passo do CI.
2. **Teste de conformidade** em `ZBank.IntegrationTests`: sobe a API com `WebApplicationFactory`,
   obtém `/openapi/v1.json` (gerado por `AddOpenApi()`) e compara com o YAML.
   - Etapa 1 (liga já): toda rota+método do contrato existe no código e vice-versa; toda rota sem
     `security: []` exige autenticação no código.
   - Etapa 2 (liga junto de cada correção): schemas, `required` e enums.
3. **Mobile:** tipos TS gerados com `openapi-typescript` a partir do YAML, substituindo os tipos
   escritos à mão em `mobile/src/services/`.

## 4. Convenções

| Tema | Regra |
|---|---|
| Versão | `/api/v1` no path. Incompatível → `/v2` |
| Auth | Protegido por padrão (`security` global). Públicas declaram `security: []`; política de fallback exigindo usuário autenticado no .NET |
| Dinheiro | `int64` em centavos; negativo = débito. Nunca float nem string decimal |
| IDs / datas | `uuid`; `date-time` em UTC (`Z`); data sem hora usa `date` |
| JSON | `camelCase`; enums como string (`"Deposit"`), nunca número |
| Evolução | Cliente ignora campos novos na resposta; servidor rejeita campos desconhecidos na requisição |
| Paginação | **Cursor**: `?limit&cursor` → `{ items, nextCursor }`. `limit` padrão 20, máx 100 (acima → 400). Ordem: mais recente primeiro, desempate por `id`. Cursor opaco. Sem `total`. Toda lista usa `items` |
| Erros | `application/problem+json` (RFC 9457) + extensão `code` estável (`SCREAMING_SNAKE`); validação traz `errors: [{ field, code }]`; 500 genérico com `code: INTERNAL_ERROR` e `traceId`. Cliente traduz por `code`, nunca por `detail` |
| Status | 400 validação, 401 sem identidade, 403 sem permissão, 404, 409 conflito, 422 regra de negócio, 429 limite de taxa |
| Idempotência | `POST` que mexe em dinheiro exige `Idempotency-Key` (uuid); mesma chave devolve a resposta original. Declarada agora, usada a partir de transferência/Pix |

## 5. Inventário de endpoints

| # | Rota | Auth | Request | 200 |
|---|---|---|---|---|
| 1 | `POST /invitations/validate` | pública | `{ code }` | `{ isValid }` |
| 2 | `POST /auth/register` | pública | `{ invitationCode, cpf, password }` | `{ userId, setupToken }` |
| 3 | `POST /auth/totp/setup` | Bearer (setupToken) | — | `{ totpSecret }` |
| 4 | `POST /auth/totp/confirm` | Bearer (setupToken) | `{ totpCode }` | vazio/ok; falha → 422 `INVALID_TOTP` |
| 5 | `POST /auth/login` | pública | `{ cpf, password, totpCode }` | `{ accessToken, refreshToken, deviceId }` |
| 6 | `POST /auth/refresh` | pública | `{ deviceId, refreshToken }` | `{ accessToken, refreshToken }` |
| 7 | `POST /auth/biometric/challenge` | pública | `{ deviceId }` | `{ nonce }` |
| 8 | `POST /auth/biometric/login` | pública | `{ cpf, deviceId, nonce, signature }` | `{ accessToken, refreshToken, deviceId }` (mesmo `LoginResponse` do login) |
| 9 | `POST /devices/register` | Bearer | `{ publicKey, deviceName }` | `{ deviceId }` |
| 10 | `GET /accounts/me/balance` | Bearer | — | `{ balance }` |
| 11 | `GET /accounts/me/statement` | Bearer | `?limit&cursor` | `{ items: LedgerEntry[], nextCursor }` |

`LedgerEntry` (DTO próprio, sem `accountId`): `id`, `amount` (Cents), `operationType`
(`Deposit` | `Transfer` | `Withdrawal`), `transactionGroupId`, `createdAt`.

`/health` fica fora do contrato `v1` (sonda de infraestrutura).

## 6. Divergências entre o código atual e o contrato

O contrato descreve o **estado-alvo**. Cada item abaixo vira tarefa própria, e a etapa 2 do teste
de conformidade é ligada junto de cada correção, para o CI não nascer quebrado.

| # | Divergência | Correção |
|---|---|---|
| 1 | `GlobalExceptionHandlerMiddleware` devolve `exception.Message` ao cliente, formato `{ statusCode, message }` | `problem+json` com `code`; mensagem genérica no 500; sem texto de exceção |
| 2 | `/invitations/validate` e `/auth/login` sem rate limit (enumeração/força bruta) | Rate limit nessas rotas (**segurança**) |
| 3 | `AddAuthorization()` sem política de fallback; `AccountsController` sem `[Authorize]` | Fallback exigindo usuário autenticado; `[AllowAnonymous]` explícito nas públicas |
| 4 | `totp/confirm` devolve 200 com `isSuccess=false` | 422 `INVALID_TOTP` |
| 5 | `invitations/validate` devolve 400 para código inválido | 200 `{ isValid: false }` |
| 6 | Extrato expõe a entidade de domínio `LedgerEntry` | DTO próprio |
| 7 | Extrato com `page`/`limit` (padrão 50) | Cursor, padrão 20, máx 100 |
| 8 | `/auth/refresh` recebe `userId` do cliente | Derivar usuário do refresh token (**segurança**) |

Itens 1, 2 e 8 são de segurança e devem passar pelo `security-specialist`.

## 7. Ordem de execução

1. Mergear a Fase 1 na `master` (feito localmente em `9aa698b`, ainda sem push).
2. Contrato (YAML, README, ADR 0005) + lint + teste de conformidade etapa 1.
3. Rebasear `feat/fase-2-ledger` sobre a `master` e regenerar a migration da Fase 2 depois de
   `AddBiometricChallenges`.
4. Correções 1–8 da seção 6, cada uma com seu teste e ligando a etapa 2 do conformidade.
5. Geração de tipos TS no mobile e remoção dos tipos manuais.

## 8. Riscos e pontos em aberto

- Cursor muda o formato do extrato; o app publicado via OTA ainda não consome `page`/`limit`
  (`account.ts` já os ignora), então a troca agora é barata. Depois de publicado, não seria.
- Enum de `operationType` tirado de `OperationType.cs` da branch da Fase 2 (valores 1–3);
  se ganhar valores, o contrato muda de forma compatível (valor novo no enum exige cliente tolerante).
