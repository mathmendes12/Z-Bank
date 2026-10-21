import { useState } from 'react';
import { View, Text, TextInput, StyleSheet } from 'react-native';
import { useRouter, useLocalSearchParams } from 'expo-router';
import { api } from '../src/services/api';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';

export default function Register() {
  const params = useLocalSearchParams<{ inviteCode?: string }>();
  const [inviteCode, setInviteCode] = useState(params.inviteCode || '');
  const [cpf, setCpf] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const router = useRouter();
  const colors = useTokens();

  const handleRegister = async () => {
    if (!inviteCode || !cpf || !password) {
      setError('Preencha todos os campos');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const res = await api.post('/api/v1/auth/register', {
        invitationCode: inviteCode.trim(),
        cpf: cpf.replace(/\D/g, ''),
        password,
      });

      const { setupToken } = res.data;
      if (setupToken) {
        router.push({ pathname: '/setup-totp', params: { setupToken } });
      } else {
        router.push('/login');
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Erro ao criar conta. Verifique os dados.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const generateFictionalCPF = () => {
    // Generates a mock 11-digit string for testing
    const randomDigits = Math.floor(100000000 + Math.random() * 900000000).toString();
    setCpf(`${randomDigits}00`);
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.md }}>
        Cadastro
      </Text>
      <TextInput
        style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
        placeholder="Código do Convite"
        placeholderTextColor={colors.textPrimary + '80'}
        value={inviteCode}
        onChangeText={setInviteCode}
        autoCapitalize="characters"
      />
      <TextInput
        style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
        placeholder="CPF (apenas números)"
        placeholderTextColor={colors.textPrimary + '80'}
        value={cpf}
        onChangeText={setCpf}
        keyboardType="numeric"
      />
      <Button onPress={generateFictionalCPF} label="Gerar CPF Fictício" />
      <TextInput
        style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginVertical: spacing.sm }]}
        placeholder="Senha"
        placeholderTextColor={colors.textPrimary + '80'}
        secureTextEntry
        value={password}
        onChangeText={setPassword}
      />
      {error ? <Text style={{ color: colors.error, marginBottom: spacing.sm }}>{error}</Text> : null}
      <Button onPress={handleRegister} label={loading ? "Criando..." : "Criar Conta"} />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  input: { borderWidth: 1, padding: 12, borderRadius: 8 },
});
