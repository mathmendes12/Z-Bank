import { View, Text, StyleSheet } from 'react-native';
import { useAuthStore } from '../src/store/authStore';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';

export default function Home() {
  const logout = useAuthStore((state) => state.logout);
  const colors = useTokens();

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.md }}>
        Olá, você está logado!
      </Text>
      <Button onPress={logout} label="Sair" />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center', alignItems: 'center' },
});
