import { View, Text, StyleSheet, FlatList, ActivityIndicator } from 'react-native';
import { useQuery } from '@tanstack/react-query';
import { useAuthStore } from '../src/store/authStore';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing, typography, radii } from '../src/design-system/tokens';
import { fetchBalance, fetchStatement, LedgerEntry } from '../src/services/account';

const formatCurrency = (cents: number) => {
  return (cents / 100).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
};

export default function Home() {
  const logout = useAuthStore((state) => state.logout);
  const colors = useTokens();

  const { data: balanceData, isLoading: isLoadingBalance, error: balanceError } = useQuery({
    queryKey: ['balance'],
    queryFn: fetchBalance,
  });

  const { data: statementData, isLoading: isLoadingStatement, error: statementError } = useQuery({
    queryKey: ['statement'],
    queryFn: fetchStatement,
  });

  const renderItem = ({ item }: { item: LedgerEntry }) => (
    <View style={[styles.entryCard, { backgroundColor: colors.surface }]}>
      <View style={styles.entryHeader}>
        <Text style={[typography.body, { color: colors.textPrimary }]}>
          {item.operationType === 0 ? 'Depósito' : 'Transferência'}
        </Text>
        <Text
          style={[
            typography.body,
            { color: item.operationType === 0 ? colors.success : colors.textPrimary, fontWeight: '700' },
          ]}
        >
          {item.operationType === 0 ? '+' : '-'}{formatCurrency(item.amount)}
        </Text>
      </View>
      <Text style={[typography.caption, { color: colors.textSecondary }]}>
        {new Date(item.createdAt).toLocaleDateString('pt-BR')}
      </Text>
    </View>
  );

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <View style={styles.header}>
        <Text style={[typography.title, { color: colors.textPrimary, marginBottom: spacing.md }]}>
          Seu Saldo
        </Text>
        {isLoadingBalance ? (
          <ActivityIndicator testID="balance-loading" color={colors.primary} />
        ) : balanceError ? (
          <Text style={{ color: colors.error }}>Erro ao carregar saldo.</Text>
        ) : (
          <Text testID="balance-text" style={[typography.display, { color: colors.textPrimary }]}>
            {formatCurrency(balanceData?.balance ?? 0)}
          </Text>
        )}
      </View>

      <View style={styles.statementContainer}>
        <Text style={[typography.title, { color: colors.textPrimary, marginBottom: spacing.md }]}>
          Extrato
        </Text>
        {isLoadingStatement ? (
          <ActivityIndicator testID="statement-loading" color={colors.primary} />
        ) : statementError ? (
          <Text style={{ color: colors.error }}>Erro ao carregar extrato.</Text>
        ) : (
          <FlatList
            testID="statement-list"
            data={statementData?.entries ?? []}
            keyExtractor={(item) => item.id}
            renderItem={renderItem}
            contentContainerStyle={{ paddingBottom: spacing.xl }}
            ListEmptyComponent={
              <Text style={{ color: colors.textSecondary }}>Nenhum lançamento encontrado.</Text>
            }
          />
        )}
      </View>

      <Button onPress={logout} label="Sair" />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, paddingTop: 64 },
  header: { marginBottom: spacing.xl },
  statementContainer: { flex: 1, marginBottom: spacing.md },
  entryCard: {
    padding: spacing.md,
    borderRadius: radii.md,
    marginBottom: spacing.sm,
  },
  entryHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: spacing.xs,
  },
});
