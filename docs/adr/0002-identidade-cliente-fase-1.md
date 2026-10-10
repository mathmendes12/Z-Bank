# ADR 0002 — Identidade do Cliente (Fase 1)

**Status:** aceito
**Data:** 2026-10-10

## Contexto

A Fase 1 foca no fluxo "Zero Trust" de biometria, gestão de sessão e cadastro condicionado a convite. Antes de iniciarmos a implementação, precisamos definir como o material criptográfico (Argon2id, TOTP) será armazenado e como rastrearemos os tokens de renovação (Refresh Tokens) com vínculo a um dispositivo específico.

## Decisões

1. **Credenciais Desacopladas (Tabela `Credentials`):** Material sensível (senhas com hash Argon2id, segredos TOTP) ficarão em uma tabela separada do modelo `User`. O `User` trafegará limpo pelas camadas, reduzindo a chance de vazamento acidental (log ou serialização) das credenciais.
2. **Sessões e Refresh Tokens no PostgreSQL (Tabela `Sessions`):** Optou-se por utilizar o banco relacional para persistir as sessões (UserId, DeviceId, Token, Expiração) em vez de introduzir uma infraestrutura nova de cache (Redis) logo na Fase 1. Isso simplifica o deploy inicial e garante auditoria consistente e transacional para o login e detecção de reuso de tokens.
3. **Argon2id como Hashing Padrão:** Utilizar Argon2id para senhas devido à sua defesa "memory-hard" contra GPUs/ASICs, seguindo diretrizes atuais da OWASP e o prêmio no PHC.

## Consequências

- A validação de login e checagem do TOTP exigirá uma query extra ou um `Include`/`JOIN` explícito para carregar a entidade `Credential`.
- O registro na tabela de Sessão deve marcar a revogação explícita de "famílias" de tokens quando a rotação for violada (Reuso de Refresh Token).
- A tabela `Sessions` será utilizada como âncora para os alertas de "Login em novo dispositivo".
