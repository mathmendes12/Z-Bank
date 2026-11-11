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
