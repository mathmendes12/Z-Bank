import { useState } from 'react';
import { View, Text, TextInput, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { api } from '../src/services/api';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';

export default function Register() {
  const [cpf, setCpf] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const router = useRouter();
  const colors = useTokens();

  const handleRegister = async () => {
    try {
      setError('');
      await api.post('/api/v1/auth/register', { cpf, password });
      router.push('/login');
    } catch (err) {
      setError('Erro ao criar conta');
    }
  };

  const generateFictionalCPF = () => {
    setCpf('12345678909'); // Placeholder for fake CPF generator
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.md }}>
        Cadastro
      </Text>
      <TextInput
        style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
        placeholder="CPF"
        placeholderTextColor={colors.textPrimary + '80'}
        value={cpf}
        onChangeText={setCpf}
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
      <Button onPress={handleRegister} label="Criar Conta" />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  input: { borderWidth: 1, padding: 12, borderRadius: 8 },
});
