import { api } from './api';

export type BalanceResponse = {
  balance: number;
};

export type LedgerEntry = {
  id: string;
  accountId: string;
  amount: number;
  operationType: number;
  transactionGroupId: string;
  createdAt: string;
};

export type StatementResponse = {
  entries: LedgerEntry[];
};

export async function fetchBalance(): Promise<BalanceResponse> {
  const response = await api.get('/api/v1/accounts/me/balance');
  return response.data;
}

export async function fetchStatement(): Promise<StatementResponse> {
  const response = await api.get('/api/v1/accounts/me/statement');
  return response.data;
}
