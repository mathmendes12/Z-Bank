# Claude Code — instruções deste projeto

As regras de trabalho deste repositório (postura esperada, o que validar, onde salvar cada tipo de
arquivo, o que documentar ao final de uma tarefa, armadilhas conhecidas) estão em **`AGENTS.md`**,
na raiz deste mesmo projeto.

**Leia `AGENTS.md` e siga-o à risca em toda sessão**, especialmente a seção 0.1 (postura de
trabalho), a seção 1 (regras críticas que nunca podem ser violadas) e a seção 4 (documentar ao
final de toda implementação/correção).

**Antes de começar a codar qualquer coisa não-trivial**, passar pelo subagente `architect`
(seção 9): feature nova, módulo novo, mudança que cruza `ZBank.Domain`/`ZBank.Application`/
`ZBank.Infrastructure`, dependência nova ou mudança de schema começam por plano escrito, não por
arquivo aberto.

**Subagentes disponíveis** em `.claude/agents/`: `architect`, `dotnet-specialist`,
`mobile-specialist`, `ui-design-specialist`, `financial-domain-specialist`, `security-specialist`,
`market-data-specialist`. Roster e princípio na seção 0.2 do `AGENTS.md`.
