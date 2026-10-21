# ADR 0004 — Endurecimento de Segurança e Contratos de Identidade (Fase 1)

**Status:** aceito
**Data:** 2026-10-21

## Contexto

A revisão de segurança e arquitetura da Fase 1 identificou vulnerabilidades críticas no fluxo de
identidade:
1. `SetupTotp` aceitava `UserId` sem autenticação, permitindo account takeover ao sobrescrever o
   segredo de qualquer conta.
2. `RegisterDevice` aceitava `UserId` sem autenticação, permitindo registrar chave pública em nome de
   terceiros e marcar o dispositivo como confiável.
3. Desafios biométricos (`Nonce`) não eram persistidos nem validados contra reuso, permitindo ataques
   de repetição (replay attack).
4. `PasswordHasher` configurava `Argon2Type.DataIndependentAddressing` (Argon2i) em vez de
   `HybridAddressing` (Argon2id).
5. Segredo JWT estava fixo no código.
6. O app mobile não enviava o código de convite no cadastro, não configurava TOTP no primeiro acesso e
   utilizava dados mockados incompatíveis com a validação ECDSA do backend.

## Decisões

1. **Ativação Segura de TOTP com `SetupToken`:**
   - O cadastro de usuário emite um `SetupToken` (JWT de curta duração com claim `scope: "totp_setup"`).
   - Os endpoints `POST /api/v1/auth/totp/setup` e `POST /api/v1/auth/totp/confirm` exigem autenticação
     via Bearer Token.
   - O segredo só é ativado no banco (`TotpEnabled = true`) após o usuário enviar um código válido de 6
     dígitos no endpoint `confirm`.
2. **Proteção de Dispositivos Confiáveis:**
   - `POST /api/v1/devices/register` agora exige autenticação JWT e extrai a identidade do usuário
     diretamente das claims autenticadas (`NameIdentifier`/`sub`), rejeitando IDs fornecidos no corpo.
3. **Prevenção de Replay em Biometria:**
   - Criada a entidade e tabela `BiometricChallenges` com TTL de 2 minutos.
   - O login biométrico recupera o desafio gerado para o dispositivo e marca `IsUsed = true`
     atomicamente antes de validar a assinatura ECDSA.
4. **Criptografia Padrão:**
   - `PasswordHasher` atualizado para `Argon2Type.HybridAddressing` (Argon2id).
   - Segredo JWT lido de `IConfiguration`.
   - Implementação de par de chaves NIST P-256 e formato SPKI + IEEE P1363 no app mobile.

## Consequências

- Nenhuma rota de configuração sensível ou vinculação de hardware pode ser chamada anonimamente.
- O login exige comprovadamente 2FA ativo para todos os usuários cadastrados.
- O app possui a tela `setup-totp.tsx` conduzindo o usuário diretamente da criação de conta para a
  validação do código antes do primeiro login.
