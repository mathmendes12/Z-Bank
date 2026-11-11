import React from 'react';
import { render, screen, waitFor } from '@testing-library/react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import Home from '../../app/home';
import { fetchBalance, fetchStatement } from '../../src/services/account';

jest.mock('../../src/services/account', () => ({
  fetchBalance: jest.fn(),
  fetchStatement: jest.fn(),
}));

jest.mock('../../src/store/authStore', () => ({
  useAuthStore: jest.fn(() => jest.fn()),
}));

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: false } },
});

const renderWithQueryClient = (ui: React.ReactElement) => {
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>
  );
};

describe('Home screen', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    queryClient.clear();
  });

  it('renders balance and statement correctly', async () => {
    (fetchBalance as jest.Mock).mockResolvedValueOnce({ balance: 150050 }); // R$ 1.500,50
    (fetchStatement as jest.Mock).mockResolvedValueOnce({
      entries: [
        {
          id: '1',
          accountId: 'acc1',
          amount: 50000,
          operationType: 0,
          transactionGroupId: 'g1',
          createdAt: '2026-10-10T10:00:00Z',
        },
      ],
    });

    const { getByTestId, queryByTestId, getByText } = await renderWithQueryClient(<Home />);

    expect(getByTestId('balance-loading')).toBeTruthy();
    expect(getByTestId('statement-loading')).toBeTruthy();

    await waitFor(() => {
      expect(queryByTestId('balance-loading')).toBeNull();
    });

    const balanceText = getByTestId('balance-text');
    expect(balanceText.props.children).toMatch(/1\.500,50/);

    const statementList = getByTestId('statement-list');
    expect(statementList).toBeTruthy();
    
    expect(getByText('Depósito')).toBeTruthy();
    const itemAmount = getByText(/\+R\$\s*500,00/);
    expect(itemAmount).toBeTruthy();
  });

  it('displays error messages on fetch failure', async () => {
    (fetchBalance as jest.Mock).mockRejectedValueOnce(new Error('Network error'));
    (fetchStatement as jest.Mock).mockRejectedValueOnce(new Error('Network error'));

    const { queryByTestId, getByText } = await renderWithQueryClient(<Home />);

    await waitFor(() => {
      expect(queryByTestId('balance-loading')).toBeNull();
    });

    expect(getByText('Erro ao carregar saldo.')).toBeTruthy();
    expect(getByText('Erro ao carregar extrato.')).toBeTruthy();
  });
});


