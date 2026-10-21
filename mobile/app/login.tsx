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
  const [loading, setLoading] = useState(false);

  const login = useAuthStore((state) => state.login);
  const colors = useTokens();

  const handleNextStep = () => {
    if (!cpf || !password) {
      setError('Informe CPF e senha para continuar.');
      return;
    }
    setError('');
    setStep('totp');
  };

  const handleLogin = async () => {
    if (!totp || totp.length < 6) {
      setError('Informe o código TOTP de 6 dígitos.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const res = await api.post('/api/v1/auth/login', {
        cpf: cpf.replace(/\D/g, ''),
        password,
        totpCode: totp.trim(),
      });

      const { accessToken, refreshToken } = res.data;
      await login(accessToken, refreshToken);

      const isRegistered = await SecureStore.getItemAsync('device_registered');
      if (!isRegistered && (await hasBiometrics())) {
        try {
          await registerDevice(accessToken);
          await SecureStore.setItemAsync('device_registered', 'true');
        } catch (e) {
          console.log('Biometric registration optional step skipped or failed', e);
        }
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Credenciais ou código TOTP inválidos.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleBiometric = async () => {
    if (!cpf) {
      setError('Informe seu CPF para iniciar o login biométrico.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const data = await biometricLogin(cpf);
      await login(data.accessToken, data.refreshToken);
    } catch (err: any) {
      const msg = err.response?.data?.message || err.message || 'Erro no login biométrico.';
      setError(msg);
    } finally {
      setLoading(false);
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
            keyboardType="numeric"
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
          <Button onPress={handleNextStep} label="Avançar (2FA)" />

          <View style={{ marginTop: spacing.md }}>
            <Button onPress={handleBiometric} label={loading ? 'Autenticando...' : 'Login Biométrico'} />
          </View>
        </>
      )}

      {step === 'totp' && (
        <>
          <Text style={{ color: colors.textPrimary + 'B0', marginBottom: spacing.sm }}>
            Digite o código de 6 dígitos do seu autenticador:
          </Text>
          <TextInput
            style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginBottom: spacing.sm, textAlign: 'center', fontSize: 20, letterSpacing: 4 }]}
            placeholder="000000"
            placeholderTextColor={colors.textPrimary + '80'}
            value={totp}
            onChangeText={setTotp}
            keyboardType="numeric"
            maxLength={6}
          />
          {error ? <Text style={{ color: colors.error, marginBottom: spacing.sm }}>{error}</Text> : null}
          <Button onPress={handleLogin} label={loading ? 'Validando...' : 'Entrar'} />

          <View style={{ marginTop: spacing.md }}>
            <Button onPress={() => setStep('credentials')} label="Voltar" />
          </View>
        </>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  input: { borderWidth: 1, padding: 12, borderRadius: 8 },
});
