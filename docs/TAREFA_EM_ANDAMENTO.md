# Fase 2: Ledger de Lançamentos e Contas - Plano de Arquitetura

Este documento descreve o plano tático para a Fase 2 do MVP, focando no core financeiro (Ledger Imutável, Partidas Dobradas, Saldo e Extrato). Nenhuma linha de código de produção foi escrita nesta etapa.

## 1. Plano de Arquitetura

### Domain Models
*   **Account (Conta):** Entidade vinculada a um `UserId`. Possui `Id`, `UserId`, `Status`, `CreatedAt`. (O saldo é gerenciado através do Ledger).
*   **LedgerEntry (Lançamento):** Registro imutável de movimentação. Propriedades: `Id`, `AccountId`, `Amount` (em centavos, `long`), `OperationType` (Depósito, Transferência, Saque), `CreatedAt`, e `TransactionGroupId` (ID de correlação).
*   **Invariantes de Domínio:** 
    *   *Centavos*: O sistema não conhece pontos flutuantes; tudo é `long`.
    *   *Imutabilidade*: Um `LedgerEntry` não possui método `Update`. Erros são corrigidos via lançamentos reversos.
    *   *Partidas Dobradas (Zero-Sum)*: Cada operação financeira gera pelo menos dois `LedgerEntries` com o mesmo `TransactionGroupId`, onde a soma global das propriedades `Amount` é estritamente igual a zero (ex: Crédito na Conta do Cliente (+1000) e Débito na Conta do Banco/Caixa Central (-1000)).

### Banco de Dados (Relacional)
*   **Tabela `Accounts`**: (Id, UserId, Balance, CreatedAt, UpdatedAt)
*   **Tabela `LedgerEntries`**: (Id, AccountId, Amount, OperationType, TransactionGroupId, CreatedAt)

### Endpoints
*   `GET /api/accounts/me/balance`: Retorna o saldo atual do usuário (em centavos).
*   `GET /api/accounts/me/statement?page=1&limit=20`: Retorna a lista paginada de `LedgerEntries` da conta logada.

### Integração/Fronteira com Frontend
*   O App nativo consome o `balance` na home, fazendo o parse dos centavos (divisão por 100) na camada de UI localmente.
*   O App apresenta o `statement` exibindo débitos em vermelho/negativo e créditos em verde/positivo com base no sinal do `Amount`.

---

## 2. Opções Estruturais (Trade-offs Difíceis)

### Trade-off A: Cálculo do Saldo (Leitura vs Escrita)
**Como manter e expor o saldo da conta garantindo performance e consistência?**

*   **Opção 1 (Event Sourcing "Puro"):** Calcular o saldo on-the-fly (`SELECT SUM(Amount) FROM LedgerEntries WHERE AccountId = X`).
    *   *Custo:* I/O intenso. À medida que o cliente usa a conta, o extrato cresce, degradando o tempo de resposta da Home do App.
    *   *Benefício:* Fonte de verdade única e inquestionável. Sem risco de dessincronização entre Ledger e Saldo em cache.
*   **Opção 2 (Snapshot Transacional em Account):** Manter uma coluna `Balance` na tabela `Accounts`. A cada lançamento, uma transação ACID no banco faz o `INSERT` no Ledger e o `UPDATE` no `Balance`.
    *   *Custo:* Lock na linha da `Account` durante a escrita. Múltiplas transferências concorrentes para a mesma conta criam contenção no banco.
    *   *Benefício:* Leitura de saldo `O(1)`. O tempo de carregamento da Home fica extremamente rápido.
*   **Decisão do Arquiteto:** **Opção 2**. Em bancos de varejo, a proporção de consultas de saldo (reads) em relação a transações (writes) chega a 100:1. A contenção no update da conta é gerenciável no nível atual da arquitetura e o benefício na UX é indispensável.

### Trade-off B: Gerenciamento da Concorrência e Duplicação (Idempotência)
*   **Opção 1:** Deixar o banco de dados falhar no lock ou depender de restrições simples.
*   **Opção 2:** Exigir um `Idempotency-Key` (Header) no fluxo de entrada das transações, salvando a chave atrelada ao `TransactionGroupId`.
    *   *Decisão do Arquiteto:* **Opção 2**. O frontend enviará uma chave de idempotência para evitar que falhas de rede dupliquem lançamentos (ex: clique duplo no botão de depositar).

---

## 3. Fatiamento em Partes Verificáveis (Milestones)

*   **Milestone 2.1 (Infra e Domínio Base):** Modelagem e migrações do BD (`Accounts`, `LedgerEntries`). Repositórios base configurados.
*   **Milestone 2.2 (Core do Ledger):** Criação do `LedgerTransactionService` que aplica a regra de partida dobrada, escreve no diário e atualiza o saldo da conta sob uma mesma Transação do Banco de Dados.
*   **Milestone 2.3 (API de Leitura):** Implementação dos endpoints `GET /balance` e `GET /statement`.
*   **Milestone 2.4 (Setup de Teste/Mock):** Script ou endpoint de uso interno (ex: `POST /internal/seed`) para simular os primeiros depósitos no sistema, validando a tela no frontend nativo sem precisar ainda da "Fase de Cash-in".

---

## 4. Fronteiras de Teste Nomeadas (TDD)

*   **`LedgerEntryDomainTests`**: 
    *   Garante a imutabilidade do registro após a sua instânciação.
    *   Assegura que o tipo numérico respeita limites sem uso de ponto flutuante.
*   **`LedgerTransactionServiceTests` (Invariantes)**: 
    *   Validar que o lançamento é recusado se a soma dos `Amounts` de uma transação não resultar em **ZERO** exato.
    *   Validar que a tentativa de deduzir fundos acima do limite resulta em falha de domínio (`InsufficientFundsException`).
*   **`AccountBalanceConcurrencyTests`**:
    *   Teste de integração garantindo que N requisições de débito/crédito concorrentes deixam o saldo final consistente e o balanço global igual a zero (Testa o mecanismo de lock e transação).
*   **`StatementApiIntegrationTests`**:
    *   Verifica o payload (contrato JSON) e a aplicação da paginação/filtros.
