# Tarefa em Andamento — Correção e Endurecimento da Fase 1

## Objetivo
Corrigir as vulnerabilidades de segurança (Argon2id, segredo JWT, autorização de dispositivos, proteção de TOTP e replay de biometria) e as quebras de contrato entre o app mobile Expo e a API backend da Fase 1, garantindo que os testes unitários e de integração fiquem verdes e a jornada do usuário funcione de ponta a ponta.

## O que já foi feito
- Revisão completa de segurança e arquitetura da Fase 1 realizada.
- Vulnerabilidades identificadas, falsificadas e reportadas.
- Plano tático aprovado pelo dono.
- Branch `fix/fase-1-security-and-contracts` criada a partir de `origin/master` (`fb1672b`).

## Milestones em Execução
- [ ] Milestone 1.1: Criptografia e Configuração Segura (Argon2id real, chave JWT em configuração)
- [ ] Milestone 1.2: Pipeline JWT e Tratamento de Erros no ASP.NET (AddAuthentication, Authorize em devices, exception handling)
- [ ] Milestone 1.3: Ciclo Seguro de Ativação do TOTP (SetupToken, confirmação antes do login)
- [ ] Milestone 1.4: Desafios Biométricos Anti-Replay (tabela relacional e uso único de Nonce)
- [ ] Milestone 1.5: Alinhamento do App Mobile Expo (casing, fluxo de convite, TOTP e biometria)

## Próximo Passo
Iniciar o Milestone 1.1 com TDD: escrever teste para o hash Argon2id e injeção da chave JWT via configuração.
