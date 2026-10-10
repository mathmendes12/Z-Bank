# Regras de Trabalho — Z Bank

> Este arquivo vale para **qualquer agente de IA** trabalhando neste repositório (Claude Code,
> Kimi Code, Antigravity, ou outro). Leia antes de executar qualquer ação de implementação,
> correção ou deploy. Adaptado do `AGENTS.md` do projeto Site Zephyr, enxugado para o contexto
> deste projeto: um portfólio solo, sem produção real de dinheiro, trabalhado em fases com
> aprovação explícita do dono a cada fase (ver seção 11 da spec do MVP).

---

## 0. Regra de ouro da raiz do projeto

**Não criar arquivos `.md` soltos na raiz do repositório.** Guia, registro de decisão ou doc de
referência vai em **`docs/`** (ADR em `docs/adr/`, diagramas em `docs/diagrams/`, guias
operacionais em `docs/guias/`). As únicas exceções são `AGENTS.md` e `CLAUDE.md` — ficam na raiz
por exigência técnica: é onde Claude Code procura essas instruções automaticamente, e onde Kimi
Code e Antigravity também olham por convenção (`AGENTS.md` na raiz).

Mesma lógica vale para as pastas de subagente:

- `Claude Code` lê `.claude/agents/`
- `Kimi Code` lê `.agents/agents/` (descoberta automática de agentes do projeto)
- `Antigravity` lê `.agents/agents/` (carregamento JIT em memória via `define_subagent`)

Os arquivos devem ficar **espelhados** entre as duas pastas (mesmo conteúdo, mesmas regras — um
arquivo por subagente em cada local). São arquivos de configuração de ferramenta, não
documentação — não contêm regra própria, só herdam deste `AGENTS.md`. Se um subagente mudar, editar
nas duas pastas na mesma sessão — divergência entre pastas é exatamente o que gera comportamento
diferente dependendo de qual ferramenta abrir o projeto.

## 0.1 Postura de trabalho

"Sênior" aqui não é adjetivo no prompt — é comportamento verificável. O que se espera:

- **Entender o problema antes de propor a solução.** Se o pedido ataca o sintoma, dizer qual é a
  causa **antes** de implementar — e então fazer o que o dono decidir.
- **Discordar quando for o caso.** Concordância automática transforma decisão ruim em código
  pronto e testado. Divergência se declara na hora, com o motivo, **uma vez**. Depois de decidido,
  executa sem reabrir o assunto.
- **Nomear o tradeoff.** Toda escolha custa algo. Recomendação sem custo declarado é propaganda,
  não engenharia.
- **Menor mudança que resolve.** Sem abstração especulativa, sem camada "pro caso de", sem opção
  de configuração que ninguém pediu. Generalizar na terceira repetição, não na primeira.
- **Separar o que foi verificado do que é hipótese.** "Rodei e passou" e "deve funcionar" são
  afirmações diferentes e se escrevem diferente.
- **Branches:** nunca trabalhar direto na `main`. Cada fase (seção 11 da spec) nasce em uma branch
  própria (`fase-N-<nome>` ou `feature/<nome>`) a partir da `main` atualizada; merge só depois do
  "ok" do dono na revisão daquela fase.
- **Peso proporcional à reversibilidade.** Migration, mudança de schema do ledger, deploy em
  produção e reescrita de histórico merecem cuidado que um ajuste de estilo não merece.
- **Dizer o que ficou de fora.** Ao entregar: o que não foi feito, o que pode ter quebrado, qual é
  o próximo passo natural.
- **Sem elogio ao pedido, sem repetir a pergunta de volta.** A resposta começa pela resposta.
- **Pensar em quem mantém isso em seis meses.**

## 0.2 Subagentes

Em `.claude/agents/`, `.agents/agents/` (espelhado, seção 0):

| Subagente | Quando | Regras em |
|---|---|---|
| `architect` | **Antes de codar**: feature nova, módulo novo, mudança que cruza `Domain`/`Application`/`Infrastructure`, dependência nova, mudança de schema | seção 9 |
| `dotnet-specialist` | Arquitetura em camadas C# (`ZBank.Domain`/`Application`/`Infrastructure`/`Api.Customer`/`Backoffice`/`Worker`), EF Core, disciplina xUnit/TDD | seção 1.1 |
| `mobile-specialist` | App Expo/React Native: navegação, dados (TanStack Query), armazenamento seguro, biometria com chaves nativas, push, testes Jest/RTL | seção 1.1 |
| `ui-design-specialist` | Design system do app (seção 7 da spec), telas do Backoffice Blazor, tradução de referência visual em UI real, motion/animação | seção 9.1 |
| `financial-domain-specialist` | Ledger de partidas dobradas, boleto FEBRABAN, Tabela Price/SAC, CET, IOF/IR regressivos, calendário de dias úteis — correção dos cálculos, não a UI em volta | seção 7 |
| `security-specialist` | Revisão defensiva de segurança (auth, idempotência, concorrência de saldo, mascaramento de dado, separação cliente/equipe, quatro olhos) | seção 8 |
| `market-data-specialist` | Integração com SGS/Bacen, Tesouro Transparente, COTAHIST da B3, provedor de cotação intradiária — coleta, fixtures de teste, tratamento de falha/dado obsoleto | seção 9.2 |

**Princípio: o subagente não contém regra própria.** Ele herda a postura da seção 0.1 e as regras
das seções deste arquivo. Regra nova se escreve aqui, não dentro do arquivo do subagente.

**Usar e abusar dos subagentes.** Em toda tarefa não-trivial, antes de partir pro trabalho inline,
avaliar explicitamente se algum subagente da tabela se aplica — e se aplicar, delegar pra ele em
vez de fazer tudo direto na conversa principal. Por padrão inclinar pra delegar, não pra fazer tudo
sozinho.

## 1. Antes de começar

- **Confira `docs/TAREFA_EM_ANDAMENTO.md`** (quando existir) antes de assumir que uma tarefa
  começa do zero — pode haver uma sessão anterior interrompida no meio de uma fase.
- **Worktree:** antes de escrever qualquer arquivo, confirmar que a sessão está isolada em um
  worktree/branch próprio da tarefa (`git worktree add` ou equivalente) — nunca commitar direto na
  pasta onde outra sessão possa estar "ao vivo" ao mesmo tempo.
- Regras críticas do projeto (nunca violar), vindas da spec do MVP:
  1. **Dinheiro é sempre `long` em centavos.** Proibido `float`/`double` em qualquer cálculo ou
     campo monetário, no backend ou no app.
  2. **Lançamento do ledger é imutável.** Nunca `UPDATE`/`DELETE` em lançamento gravado — correção
     é lançamento novo em sentido inverso, com referência ao original.
  3. **Invariante global:** soma de todos os lançamentos do sistema é sempre zero. Mudança que
     toque o ledger só é considerada concluída com o teste do invariante passando.
  4. **Nunca alteração destrutiva em banco** (migration que apaga dado, `DROP`, reset de ambiente
     de amigos) sem confirmação explícita do dono.
  5. **Nunca expor segredo, token, chave privada ou dado de cartão em claro** em código, log,
     commit ou documentação — inclusive durante verificação (comando que devolve só
     booleano/hash, nunca `cat`/`grep` ecoando o valor).
  6. **`ZBank.Domain` não referencia nenhuma biblioteca de infraestrutura.** Toda integração
     externa (boleto, PIX, SMS, verificação de documento, dado de mercado) é interface em
     `Application` com implementação simulada em `Infrastructure`.
  7. **Taxa, alíquota e parâmetro regulatório (IOF, IR, multa, mora, CET, limites) nunca vêm
     hardcoded** — são configuráveis no gerenciador, porque a legislação muda.
  8. **CVV nunca é armazenado.** Número completo do cartão fica criptografado, separado do resto
     do registro.
  9. Ao mexer no backend (.NET), validar com `dotnet build` + `dotnet test` nas camadas afetadas.
  10. Ao mexer no app Expo, validar com `npm test` + `tsc --noEmit` (mais `eslint` quando
      configurado).
  11. Ao mexer no Backoffice (Blazor), build limpo não prova componente interativo funcionando —
      rodar e clicar de verdade antes de considerar concluído (mesma armadilha de UI da seção 9.1).

## 1.1 TDD

Backend (.NET) usa **xUnit**; app Expo usa **Jest** + Testing Library (RN); testes de integração
do backend usam **Testcontainers** com PostgreSQL real (nunca mock de banco para o ledger).

- **Feature ou fix novo segue TDD**: escrever o teste (vermelho) antes do código, implementar até
  passar (verde), refatorar com o teste passando. Ver o vermelho é passo obrigatório, não
  formalidade.
- **Teste existente não se altera pra passar.** Teste e código discordando → hipótese padrão é que
  o código está errado. Alterar ou remover teste antigo é decisão do dono, nunca da sessão.
- Bug corrigido nasce com teste que reproduz o bug.
- **Cálculo financeiro** (Price, SAC, CET, boleto, rendimento, IOF/IR) é validado contra valor de
  referência calculado à parte, não só contra o próprio código — ver seção 7.
- Teste de concorrência (duas transferências simultâneas na mesma conta) e teste do invariante
  (ledger soma zero) são obrigatórios antes de uma feature que move dinheiro ser considerada
  concluída.
- Teste de idempotência (mesma requisição enviada duas vezes) é obrigatório em todo endpoint que
  move dinheiro.

## 1.2 Deploy (a partir da Fase 4)

- **O agente não roda deploy em produção sozinho sem aprovação prévia.** Apresentar o que vai
  subir (commits/diff), aguardar o "ok" explícito do dono, só então rodar o script de deploy na VPS
  da OVHcloud.
- Migration em produção roda **antes** de liberar a nova versão (seção 8 da spec), nunca depois.
- Ambiente "amigos" e ambiente "vitrine" são bancos separados — confirmar qual ambiente antes de
  qualquer comando que toque dado (reset, seed, migration manual).
- Segredo de produção (connection string, chave JWT, VAPID key, credencial de backup) vive em
  variável de ambiente / secret da VPS, nunca em arquivo versionado.

## 2. Onde cada coisa vive

Estrutura completa já definida na spec do MVP (seção 3). Pontos que valem repetir:

- `ZBank.Domain`: entidades e regras puras, sem EF Core, sem ASP.NET, sem HTTP.
- `ZBank.Application`: casos de uso e interfaces (`IBoletoRegistrar`, `ITopUpProvider`,
  `IIndexProvider`, `ITreasuryProvider`, `IQuoteProvider`, `IDocumentVerifier`, etc.) — a fronteira
  onde todo serviço externo vira simulável.
- `ZBank.Infrastructure`: EF Core, simuladores, e-mail, push, PDF — implementa as interfaces acima.
- Decisão estrutural aceita (mudança de camada, dependência nova, troca de biblioteca) vira ADR em
  `docs/adr/NNNN-titulo.md` (formato: contexto, decisão, alternativas consideradas, consequências).

## 3. Durante uma tarefa longa — handoff entre ferramentas

`docs/TAREFA_EM_ANDAMENTO.md` descreve o que está no meio do caminho **agora**, não o que já foi
entregue — serve pra continuar numa sessão nova, possivelmente em outra ferramenta (ex.: o Claude
Code atingiu o limite de uso no meio de uma implementação, o dono abre Kimi Code ou Antigravity na
mesma pasta e ele lê esse arquivo pra continuar de onde parou, sem reperguntar contexto já
decidido).

- Ao iniciar uma tarefa não-trivial (mais de uma sessão provável): escrever objetivo, o que já foi
  feito, decisões tomadas e o próximo passo.
- Atualizar a cada etapa relevante concluída.
- Ao terminar: resumo final conforme seção 4, e limpar o arquivo de volta para "Nada em andamento".
- Tarefa curta/trivial não precisa desse arquivo.

## 4. Ao final de toda implementação ou correção

1. Resumir para o dono: o que mudou, arquivos/módulos tocados, como testar — seguindo a seção 11
   da spec do MVP (fim de fase = plano cumprido, critério de pronto demonstrado, aguardar
   aprovação antes de seguir pra próxima fase).
2. Mudança estrutural relevante (feature nova, decisão de arquitetura, mudança de infra) ganha ADR
   em `docs/adr/` (seção 2) — isso substitui changelog separado; o histórico do git cobre o resto.
3. Se a mudança resolve algo pendente ou abre um pendente novo, registrar em
   `docs/PENDENCIAS.md` (criar na primeira vez que fizer sentido — não precisa existir desde o
   início).

## 5. Armadilhas conhecidas (crescer esta lista conforme aparecerem)

- **Ponto flutuante em dinheiro.** `decimal` já é proibido por regra (seção 1), mas atenção
  especial em cálculo de juros/rendimento que soma muitas parcelas pequenas — arredondar só no
  lugar certo (ver regra de arredondamento documentada nos testes de Price/SAC/CET), nunca a cada
  passo intermediário.
- **Lock de conta em transferência.** Duas contas envolvidas sempre travadas em ordem determinística
  de ID (não ordem de chegada da requisição) para não gerar deadlock sob concorrência.
- **Fonte de dado de mercado indisponível.** Nunca inventar valor — usar o último dado válido,
  registrar a falha, alertar a equipe (seção 9.2). Testes nunca dependem de API externa de verdade.
- **Expo Go vs. development build.** A partir da Fase 1 (biometria com chaves nativas), o app para
  de funcionar no Expo Go — qualquer mudança que adicione módulo nativo exige novo build EAS, não
  só `eas update`.
- **Mais de uma sessão (Claude Code/Kimi Code/Antigravity) pode estar "ao vivo" na mesma pasta
  principal do repositório ao mesmo tempo.** Um commit concorrente de outra ferramenta pode cair em
  cima do que a sessão atual está fazendo, sem aviso — a mesma árvore de trabalho não distingue
  quem está escrevendo. **Regra:** qualquer sessão que for mexer em código (não só ler/investigar)
  cria o próprio `git worktree add .worktrees/<nome-da-tarefa> -b <branch>` antes de começar e
  trabalha isolada lá (seção 1) — a pasta principal fica reservada pra leitura/coordenação. Antes
  de dar push numa branch, conferir `git log --oneline -5` e validar que todo commit listado é
  mesmo da própria sessão (autor, mensagem, diff).

## 6. Perguntar antes de agir quando...

- A ação for destrutiva ou difícil de reverter (migration que apaga dado, reset de ambiente,
  force-push, deploy em produção).
- Faltar um contexto que só o dono tem (ex.: "essa regra de negócio já foi decidida ou é nova?").
- O pedido for ambíguo o suficiente pra duas interpretações razoáveis levarem a resultados bem
  diferentes — igual à seção 11, regra 3 da spec do MVP.
- O pedido não vier com detalhe técnico suficiente pra decidir sozinho. Perguntar via
  `AskUserQuestion` (opções objetivas) **antes** de acionar `architect` ou qualquer subagente de
  implementação.

## 7. Correção de cálculos financeiros

Vale para ledger, boleto, cartão/fatura, empréstimo, financiamento e investimentos — qualquer
código que produza um número que representa dinheiro ou uma regra regulatória.

1. **Nunca inventar fórmula, alíquota ou tabela.** Tabela Price, SAC, CET, mod10/11 do boleto,
   fator de vencimento, base 252, IOF/IR regressivo — saem da definição regulatória real ou da
   spec, nunca de memória aproximada.
2. **Todo cálculo tem um valor de referência independente** calculado fora do código (planilha,
   calculadora financeira, exemplo oficial) antes de o teste ser escrito — o teste prova contra
   esse valor, não contra o próprio código que se está escrevendo.
3. **Regra de arredondamento é documentada** no teste, não só aplicada — centavo que "sobra" numa
   parcela da Tabela Price precisa de regra explícita de para onde vai.
4. **Taxa/alíquota/parâmetro é dado, não constante de código** (regra 7 da seção 1) — teste cobre o
   cálculo parametrizado, não um número fixo.
5. **Boleto:** 44 dígitos do código de barras, 47 da linha digitável, módulo 10 e 11, fator de
   vencimento considerando o reinício de fevereiro/2025 — cada um com teste unitário próprio.

**Subagente:** `financial-domain-specialist` (seção 0.2) — entrega a fórmula/algoritmo com
teste e valor de referência, não só o código.

## 8. Revisão de segurança

Escopo é **defensivo**: encontrar e corrigir falha no próprio código. Não escrever exploit nem
testar contra sistema de terceiro.

1. **Fluxo de 3 fases: identificar → falsificar → reportar.** Tentar honestamente provar que um
   candidato a vulnerabilidade **não** é explorável neste contexto antes de reportar.
2. **Reportar só com confiança alta** e severidade real (ex.: "um cliente comum consegue ver saldo
   de outra conta", não "boa prática"). Dívida técnica sem vetor não é vulnerabilidade.
3. **Todo achado tem `arquivo:linha` e o caminho do input até o dano.**
4. **Nunca ecoar segredo em claro**, inclusive durante a própria verificação.
5. **Atenção redobrada em:** idempotência de operação monetária, lock de concorrência em saldo,
   separação completa entre identidade de cliente e de equipe (token de um nunca abre rota do
   outro), mascaramento de dado pessoal por padrão no Backoffice, princípio dos quatro olhos em
   estorno/ajuste/desbloqueio/limite, dado de cartão (nunca CVV armazenado, PAN criptografado
   separado).
6. **Reportar antes de corrigir.** Correção que toca autenticação, autorização, ou o ledger espera
   confirmação do dono.
7. Correção de segurança entra pelo TDD da seção 1.1: teste que reproduz o abuso primeiro.

**Subagente:** `security-specialist` (seção 0.2).

## 9. Arquitetura — antes de começar a codar

Vale para feature nova, módulo novo, mudança que cruza camadas, dependência nova, mudança de
schema, e qualquer pedido no formato "preciso construir X".

1. **Plano antes de código.** Tarefa não-trivial começa por plano escrito — exatamente o que a
   seção 11 da spec do MVP já pede fase a fase.
2. **Duas opções no mínimo, com o custo de cada uma, e uma recomendação explícita.**
3. **Fatiar em partes verificáveis independentes**, cada uma com um comando de validação.
4. **O plano nomeia as fronteiras de teste** que o TDD da seção 1.1 vai usar.
5. **Decisão estrutural aceita vira ADR** em `docs/adr/` (seção 2).
6. **O arquiteto não escreve código de produção.** Entrega plano; implementar é etapa separada.

## 9.1 Design de UI/UX

Vale pra design system do app (seção 7 da spec do MVP), telas do app, e telas do Backoffice Blazor.

1. **Referência manda sobre gosto pessoal.** Havendo mockup/imagem/link de referência, observar a
   estrutura real primeiro, listar elementos concretos, só depois implementar.
2. **Paleta e tokens oficiais** (seção 7.1 da spec: `#111111`/`#C6F432`/`#F4F4F0`, contraste AA em
   todos os tokens de sucesso/erro/alerta, nos dois modos) por padrão — divergir é decisão a
   declarar, não escolha silenciosa. Lima elétrico nunca como texto sobre fundo claro.
3. **Validar rodando de verdade**, nunca só build/typecheck limpo — sobretudo em Blazor Interactive
   Server e em animação/gesto no app (Reanimated/Gesture Handler), onde build limpo não prova
   comportamento em runtime.
4. **Regra de ouro do dinheiro (seção 7.3 da spec):** tela comum pode ter atualização otimista;
   operação com dinheiro nunca — só mostra concluído depois da confirmação real do backend.

**Subagente:** `ui-design-specialist` (seção 0.2).

## 9.2 Dados de mercado

Vale pra qualquer integração com SGS/Bacen, Tesouro Transparente, arquivos da B3 (COTAHIST,
composição de índice) ou provedor de cotação intradiária.

1. **Toda fonte externa passa por interface** (`IIndexProvider`, `ITreasuryProvider`,
   `IQuoteProvider`) — o app e o Backoffice nunca chamam a API externa direto, só o `ZBank.Worker`.
2. **Fonte indisponível → último dado válido + registro de falha + alerta**, nunca valor inventado.
3. **Testes nunca dependem da API externa de verdade** — usam fixture gravada (resposta real
   salva), para o teste não quebrar quando o Bacen/B3 ficar fora do ar ou mudar formato sem avisar.
4. **Todo dado coletado grava data e fonte** junto do valor — necessário pro painel de saúde dos
   dados de mercado no Backoffice (seção 6.2 da spec).

**Subagente:** `market-data-specialist` (seção 0.2).
