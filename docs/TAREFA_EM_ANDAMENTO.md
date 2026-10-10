# Fase 1: Identidade do Cliente (Plano Tático)

## 1. Arquitetura

### 1.1 Modelos de Domínio (Domain Models)
*   `Invitation`: `Id`, `Code`, `Status` (PENDING, USED, EXPIRED), `UsedByUserId`
*   `User`: `Id`, `Cpf` (Fictício), `Status` (UNDER_ANALYSIS, APPROVED, BLOCKED), `CreatedAt`
*   `Credential`: `UserId`, `PasswordHash` (Argon2id), `TotpSecret`, `TotpEnabled`
*   `Device`: `Id`, `UserId`, `PublicKey` (para biometria), `DeviceName`, `IsTrusted`, `CreatedAt`
*   `Session`: `Id`, `UserId`, `DeviceId`, `RefreshTokenHash`, `ExpiresAt`, `IsRevoked`

### 1.2 Endpoints (REST)
*   `POST /api/v1/invitations/validate` -> Verifica código.
*   `POST /api/v1/auth/register` -> Recebe (InviteCode, Cpf, Password). Cria usuário UNDER_ANALYSIS, Credential e consome Invite.
*   `POST /api/v1/auth/totp/setup` -> Gera TOTP secret para o usuário (requer autenticação temporária de setup).
*   `POST /api/v1/auth/login` -> Recebe (Cpf, Password, TotpCode). Retorna JWTs e DeviceId temporário.
*   `POST /api/v1/devices/register` -> Recebe PublicKey. Vincula ao DeviceId (requer login completo).
*   `POST /api/v1/auth/biometric/challenge` -> Inicia login biométrico, retorna `Nonce`.
*   `POST /api/v1/auth/biometric/login` -> Recebe (Cpf, DeviceId, NonceAssinado). Valida assinatura com PublicKey. Retorna JWTs.
*   `POST /api/v1/auth/refresh` -> Rotaciona tokens.

### 1.3 Banco de Dados (PostgreSQL)
Tabelas relacionais refletindo o Domínio. Ledger imutável não é o foco aqui (será na Fase de Transações), mas as tabelas `Session` e `Device` guardam rastros de auditoria rigorosos.

### 1.4 Integração com Frontend Nativo
O App (iOS/Android) deve gerar o Cpf (Mock) offline. Na primeira vez, usuário informa senha+TOTP. O App usa `SecureEnclave`/`Android Keystore` para gerar o KeyPair (NIST P-256), guarda a privada no hardware e manda a pública para o backend via `/api/v1/devices/register`. Nas próximas vezes, assina o Challenge.

## 2. Decisões Estruturais (Trade-offs)

### Decisão 1: Estrutura de Credenciais (Senha e TOTP)
*   **Opção A (Acoplada):** Colunas `PasswordHash`, `TotpSecret` direto na tabela `Users`.
    *   *Custo:* Menos complexidade. Uma junção a menos no banco.
    *   *Risco:* A tabela de usuários é frequentemente consultada por outros serviços (ex: extrato, perfil). Maior risco de vazar dados sensíveis em logs ou serializações acidentais.
*   **Opção B (Desacoplada):** Tabela separada `Credentials` vinculada por `UserId`.
    *   *Custo:* Necessidade de um JOIN ou query extra no login.
    *   *Vantagem:* Isolamento de segurança. O modelo `User` fica limpo. Pode ter permissões de banco restritas para a tabela `Credentials`.

**Recomendação:** **Opção B**. Sistemas financeiros exigem defesa em profundidade. O Domínio de Usuário não precisa trafegar material de segurança.

### Decisão 2: Rastreamento de Refresh Tokens e Dispositivos
*   **Opção A (Redis/Stateless):** JWT stateless onde o Refresh Token tem um ID verificado no Redis.
    *   *Vantagem:* Rápido, TTL nativo.
    *   *Custo:* Adiciona infraestrutura extra logo na Fase 1.
*   **Opção B (Relacional/Sessions):** Tabela `Sessions` no PostgreSQL relacionando `Device` e `User`, com verificação de reuso (Family of tokens) atualizando a linha ou marcando `IsRevoked`.
    *   *Vantagem:* Simplicidade infraestrutural inicial, registro durável que amarra bem com a regra de negócio de "notificação de novo device".

**Recomendação:** **Opção B**. Manter em DB relacional para Phase 1 minimiza o overhead de infraestrutura (só Postgres no início) e garante consistência transacional para auditorias de segurança e detecção de reuso.

## 3. Fatiamento em Partes Verificáveis (Milestones)

*   **Milestone 1.1: Convites e Cadastro.** (Endpoints `/validate` e `/register`).
    *   *Critério:* Só cadastra com convite válido, gasta o convite, usuário nasce `UNDER_ANALYSIS`, senha hasheada com Argon2id.
*   **Milestone 1.2: Login Básico e 2FA.** (Endpoints setup TOTP e `/login`).
    *   *Critério:* Login só ocorre com Senha + TOTP válidos. Retorna JWT access e refresh.
*   **Milestone 1.3: Sessão, Rotação e Devices.** (Endpoint `/refresh` e tabela `Sessions`).
    *   *Critério:* Refresh token expira os anteriores. Se reusado, revoga a família de tokens ativa. Identifica login em dispositivo novo (gancho para notificação futura).
*   **Milestone 1.4: Biometria e Hardware Key.** (`/challenge`, `/biometric/login`, `/devices/register`).
    *   *Critério:* Cadastro correto da Public Key. Validação da assinatura do Nonce criptográfico, entregando JWT apenas em caso de sucesso da validação.

## 4. Fronteiras de Teste (TDD)

*   **`InvitationRegistrationFlowTests`:** Verifica o ciclo de vida do convite e transição para cadastro. Testes negativos para reuso e convites inválidos.
*   **`AuthenticationDomainTests`:** Mock do DB. Injeta hashers Argon2 e TotpValidators. Testa sucesso e falhas da entidade Credential.
*   **`SessionManagerTests`:** Testa estritamente a rotação de tokens JWT, detecção de reuso de Refresh Token e vinculação correta ao `DeviceId`.
*   **`BiometricCryptoChallengeTests`:** Testa o algoritmo de geração de Nonce e verificação de assinatura ECDSA/RSA (mockando a chave pública enviada pelo cliente), garantindo que payloads modificados ou assinaturas falsas são fortemente rejeitados.
