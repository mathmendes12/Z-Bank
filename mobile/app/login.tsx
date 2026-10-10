import { useState } from 'react';
import { View, Text, TextInput, StyleSheet } from 'react-native';
import { api } from '../src/services/api';
import { biometricLogin, registerDevice, hasBiometrics } from '../src/services/biometrics';
import { useAuthStore } from '../src/store/authStore';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';
import * as SecureStore from 'expo-secure-store';

export default function Login() {
  const [cpf, setCpf] = useState('');
  const [password, setPassword] = useState('');
  const [totp, setTotp] = useState('');
  const [step, setStep] = useState<'credentials' | 'totp'>('credentials');
  const [error, setError] = useState('');
  
  const login = useAuthStore((state) => state.login);
  const colors = useTokens();

  const handleLogin = async () => {
    try {
      setError('');
      const res = await api.post('/api/v1/auth/login', { cpf, password });
      
      const { access_token, refresh_token } = res.data;
      await login(access_token, refresh_token);
      
      const isRegistered = await SecureStore.getItemAsync('device_registered');
      if (!isRegistered && await hasBiometrics()) {
        try {
          await registerDevice();
          await SecureStore.setItemAsync('device_registered', 'true');
        } catch (e) {
          console.log('Biometric registration failed', e);
        }
      }
    } catch (err) {
      setError('Credenciais inválidas');
    }
  };

  const handleBiometric = async () => {
    try {
      setError('');
      const data = await biometricLogin(cpf);
      await login(data.access_token, data.refresh_token);
    } catch (err: any) {
      setError(err.message || 'Erro no login biométrico');
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.md }}>
        Login
      </Text>
      
      {step === 'credentials' && (
        <>
          <TextInput
            style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
            placeholder="CPF"
            placeholderTextColor={colors.textPrimary + '80'}
            value={cpf}
            onChangeText={setCpf}
          />
          <TextInput
            style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
            placeholder="Senha"
            placeholderTextColor={colors.textPrimary + '80'}
            secureTextEntry
            value={password}
            onChangeText={setPassword}
          />
          {error ? <Text style={{ color: colors.error, marginBottom: spacing.sm }}>{error}</Text> : null}
          <Button onPress={handleLogin} label="Entrar" />
          
          <View style={{ marginTop: spacing.md }}>
            <Button onPress={handleBiometric} label="Login Biomêtrico" />
          </View>
        </>
      )}

      {step === 'totp' && (
        <>
          <TextInput
            style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm }]}
            placeholder="Código TOTP"
            placeholderTextColor={colors.textPrimary + '80'}
            value={totp}
            onChangeText={setTotp}
          />
          <Button onPress={() => {}} label="Validar TOTP" />
        </>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  input: { borderWidth: 1, padding: 12, borderRadius: 8 },
});
