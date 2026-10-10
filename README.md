# Z Bank

Banco digital fictício para portfólio, construído como se um dia pudesse virar um banco real:
ledger de partidas dobradas, concorrência tratada de verdade, biometria com chaves no dispositivo,
e dados reais de mercado (CDI, Selic, Tesouro Direto, B3) por trás de simuladores.

> ⚠️ **Projeto de demonstração.** Nenhum valor é real, não há vínculo com nenhuma instituição
> financeira, e nenhum dado pessoal real é coletado.

Em desenvolvimento, fase a fase — este README cresce junto. GIF do app, badges de CI/cobertura,
vídeo de demonstração, link da vitrine e credenciais de teste entram a partir da Fase 4
(demonstração no ar).

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core (.NET 10), C#, Clean Architecture |
| Banco de dados | PostgreSQL + EF Core |
| App do cliente | React Native (Expo, TypeScript, Expo Router) |
| Gerenciador da equipe | Blazor Web App (Interactive Server) |
| Infra local | Docker Compose (PostgreSQL + Mailpit) |
| CI | GitHub Actions |

Stack completa e arquitetura de módulos: ver spec do MVP e `AGENTS.md`.

## Estrutura

```
zbank/
├── src/            # ZBank.Domain, Application, Infrastructure, Api.Customer, Backoffice, Worker, MerchantSimulator
├── mobile/         # App Expo (cliente)
├── tests/          # Testes de Domain, Application e integração
├── infra/          # Docker Compose (dev) e config de VPS (produção, a partir da Fase 4)
├── docs/           # ADRs e guias
└── .github/workflows/
```

## Rodando localmente (backend)

```bash
docker compose -f infra/docker-compose.yml up -d   # Postgres + Mailpit
dotnet build
dotnet test
dotnet run --project src/ZBank.Api.Customer         # http://localhost:5000, GET /health
```

## Rodando no celular Android (sem cabo USB)

O app é testado continuamente num Android físico, sempre pela internet — instalação e
atualização nunca usam `adb`/USB.

### Pré-requisitos (uma vez só)

1. Criar uma conta grátis em [expo.dev](https://expo.dev).
2. Dentro de `mobile/`: `npx eas-cli login`, depois `npx eas-cli init` — isso cria o projeto no
   EAS e imprime um `projectId`. Colar esse ID em `mobile/app.config.ts`, no campo
   `extra.eas.projectId` (a CLI não edita `app.config.ts` automaticamente).
3. Instalar a [Dev Tunnels CLI](https://learn.microsoft.com/azure/developer/dev-tunnels/get-started)
   da Microsoft e autenticar: `devtunnel user login`.
4. Copiar `mobile/.env.example` para `mobile/.env` (fica fora do git).

### A cada sessão de desenvolvimento

1. Subir a API local (porta fixa `5000`, ver seção acima).
2. Criar/retomar o túnel persistente apontando pra porta 5000:
   ```bash
   devtunnel host -p 5000 --allow-anonymous
   ```
   Copiar a URL HTTPS impressa e colar em `mobile/.env` (`EXPO_PUBLIC_API_URL=...`). Uma vez
   criado com um nome fixo (`devtunnel create zbank-api` antes do `host`), a URL não muda entre
   sessões.
3. Dentro de `mobile/`: `npm start` (roda `expo start --tunnel`) — o QR code/link que aparece é
   só pra desenvolvimento ao vivo (Fast Refresh), não pra instalar o app.

### Instalar o app no celular (uma vez, ou quando mudar algo nativo)

```bash
cd mobile
npm run build:dev   # eas build --profile development --platform android
```

Ao terminar, o terminal mostra um link/QR code — abrir no celular e permitir "instalar de fontes
desconhecidas" quando pedido. **Nunca `expo run:android`, nunca `adb`.**

### Publicar uma mudança sem reinstalar (OTA)

Mudança só em JS/TS/assets (não em módulo nativo nem em `app.config.ts`):

```bash
cd mobile
npm run update:dev   # eas update --channel development
```

O app instalado recebe a atualização na próxima abertura. Build novo só é necessário quando
mudar algo nativo (biblioteca nativa nova, permissão, config de `app.config.ts`) — a política
`runtimeVersion: { policy: "fingerprint" }` garante que o app nunca aceita uma atualização
incompatível com o build instalado.

### Biometria (a partir da Fase 1)

Biometria com chaves nativas (Keystore) não funciona no Expo Go — exige o development build
acima. Pra testar: nas configurações do Android, cadastrar ao menos uma digital/face antes de
abrir o app.

### Cota do EAS

O plano grátis do EAS tem limite mensal de builds. Preferir sempre `npm run update:*` (OTA) e só
rodar `npm run build:*` quando a mudança exigir (módulo nativo novo).

## Testes

```bash
dotnet test              # backend (xUnit)
cd mobile && npm test     # app (Jest)
```

## Decisões técnicas

Registradas em `docs/adr/` conforme acontecem — ver `AGENTS.md` seção 2.

## Fora do escopo

Ver seção 10 da spec do MVP: sem detecção de fraude/ML, sem PIX ou integração financeira real,
sem análise de crédito real, sem rotativo/juros de cartão, sem renegociação de dívida, sem
day trade/derivativos/cripto, sem rotinas de LGPD, sem acesso de cliente pela web.
