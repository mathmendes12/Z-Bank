# Instruções deste diretório de subagentes

As regras de trabalho deste repositório estão em `AGENTS.md` na raiz. Os arquivos de subagentes
são mantidos em espelho entre `.claude/agents/` (Claude Code) e `.agents/agents/` (Kimi Code e
Antigravity) — mesmo conteúdo nas duas pastas, ver seção 0 do `AGENTS.md`. Este arquivo não é um
subagente.

Os subagentes reais deste projeto são os arquivos com frontmatter `name`, `description` e `tools`:
`architect`, `dotnet-specialist`, `mobile-specialist`, `ui-design-specialist`,
`financial-domain-specialist`, `security-specialist`, `market-data-specialist`.
