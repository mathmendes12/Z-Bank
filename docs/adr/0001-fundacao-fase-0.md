# ADR 0001 — Fundação da Fase 0

**Status:** aceito
**Data:** 2026-10-09

## Contexto

Fase 0 exige: solution .NET em Clean Architecture, app Expo, Docker Compose, CI, design system
base, e toda a configuração de desenvolvimento no Android físico pela nuvem (sem USB). Algumas
decisões de ferramenta/estrutura precisavam ser fechadas antes de codar.

## Decisões

1. **`app.config.ts` em vez de `app.json`** no app Expo. A spec exige ler
   `EXPO_PUBLIC_API_URL` dinamicamente por ambiente (dev/preview/produção); `app.config.ts`
   permite isso em tempo de avaliação. Custo: `eas init` não edita esse arquivo automaticamente
   (precisa colar o `projectId` à mão) — documentado no README.
2. **Dev Tunnels (Microsoft) em vez de ngrok** pra expor a API local. Alternativa descartada:
   ngrok free não dá URL persistente entre reinícios (precisa colar nova URL no app a cada
   sessão). Dev Tunnels dá URL estável com `devtunnel create` nomeado, de graça, vinculado à
   conta Microsoft do dono.
3. **Porta fixa 5000, só HTTP localmente** (sem perfil `https` em `launchSettings.json`). O
   túnel já termina TLS — manter HTTPS redundante localmente só adicionaria certificado
   autoassinado pra gerenciar sem ganho real. `UseHttpsRedirection()` removido do pipeline por
   esse motivo.
4. **`Microsoft.OpenApi` fixado em `2.12.2`** (última da série 2.x) nos projetos
   `ZBank.Api.Customer`, `ZBank.MerchantSimulator` e `ZBank.IntegrationTests`. A versão
   transitiva do template (`2.0.0`, trazida por `Microsoft.AspNetCore.OpenApi 10.0.8`) tem
   vulnerabilidade alta conhecida (GHSA-v5pm-xwqc-g5wc). A série `3.x` quebra o source generator
   do `Microsoft.AspNetCore.OpenApi` (API `IOpenApiMediaType.Example` ficou somente leitura) —
   por isso o pin ficou na última versão compatível da série 2.x, não na mais recente.
5. **Todos os 7 projetos de `src/` e os 3 de `tests/` criados já na Fase 0**, mesmo os que ainda
   não têm lógica (`Backoffice`, `Worker`, `MerchantSimulator`) — evita retrabalho de estrutura
   nas fases seguintes. `ZBank.MerchantSimulator` não referencia `ZBank.Application`: ele
   representa um lojista externo e vai falar com a API por HTTP, não por chamada direta.

## Consequências

- Variável de ambiente nova (`EXPO_PUBLIC_API_URL`) documentada em `mobile/.env.example`.
- README ganhou a seção completa "Rodando no celular Android" com o passo a passo de Dev
  Tunnels + EAS.
- Projetos `Backoffice`/`Worker`/`MerchantSimulator` compilam mas não fazem nada ainda — ficam
  como esqueleto até a fase que os usa.
