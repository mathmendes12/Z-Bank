import { useState } from 'react';
import { View, Text, TextInput, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { api } from '../src/services/api';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';

export default function Index() {
  const [inviteCode, setInviteCode] = useState('');
  const [error, setError] = useState('');
  const router = useRouter();
  const colors = useTokens();

  const handleValidateInvite = async () => {
    try {
      setError('');
      await api.post('/api/v1/invitations/validate', { code: inviteCode });
      router.push('/register');
    } catch (err) {
      setError('Convite inválido ou expirado');
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.md }}>
        Bem-vindo ao ZBank
      </Text>
      <TextInput
        style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
        placeholder="Código do Convite"
        placeholderTextColor={colors.textPrimary + '80'}
        value={inviteCode}
        onChangeText={setInviteCode}
      />
      {error ? <Text style={{ color: colors.error, marginBottom: spacing.sm }}>{error}</Text> : null}
      <Button onPress={handleValidateInvite} label="Entrar com Convite" />
      <View style={{ marginTop: spacing.md }}>
        <Button onPress={() => router.push('/login')} label="Já tenho conta (Login)" />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  input: { borderWidth: 1, padding: 12, borderRadius: 8 },
});
