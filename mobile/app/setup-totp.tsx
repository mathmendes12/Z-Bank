import { useState, useEffect } from 'react';
import { View, Text, TextInput, StyleSheet } from 'react-native';
import { useRouter, useLocalSearchParams } from 'expo-router';
import { api } from '../src/services/api';
import { Button } from '../src/design-system/Button';
import { useTokens } from '../src/design-system/useTokens';
import { spacing } from '../src/design-system/tokens';

export default function SetupTotp() {
  const params = useLocalSearchParams<{ setupToken: string }>();
  const [totpSecret, setTotpSecret] = useState('');
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const router = useRouter();
  const colors = useTokens();

  useEffect(() => {
    async function initTotp() {
      if (!params.setupToken) {
        setError('Token de ativação ausente. Faça o cadastro novamente.');
        return;
      }
      try {
        const res = await api.post(
          '/api/v1/auth/totp/setup',
          {},
          { headers: { Authorization: `Bearer ${params.setupToken}` } }
        );
        setTotpSecret(res.data.totpSecret);
      } catch (err: any) {
        setError('Erro ao iniciar configuração do 2FA.');
      }
    }
    initTotp();
  }, [params.setupToken]);

  const handleConfirmTotp = async () => {
    if (!code || code.length < 6) {
      setError('Informe o código de 6 dígitos gerado no seu autenticador.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      await api.post(
        '/api/v1/auth/totp/confirm',
        { totpCode: code.trim() },
        { headers: { Authorization: `Bearer ${params.setupToken}` } }
      );
      setSuccess(true);
    } catch (err: any) {
      setError('Código inválido. Tente novamente com o código atual do app.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={[styles.container, { backgroundColor: colors.background, padding: spacing.lg }]}>
      <Text style={{ color: colors.textPrimary, fontSize: 24, marginBottom: spacing.sm }}>
        Configurar Autenticação (2FA)
      </Text>
      <Text style={{ color: colors.textPrimary + 'B0', marginBottom: spacing.md, lineHeight: 20 }}>
        Adicione a chave abaixo no seu aplicativo autenticador (Google Authenticator, Microsoft Authenticator ou 1Password):
      </Text>

      {totpSecret ? (
        <View style={[styles.secretBox, { borderColor: colors.primary }]}>
          <Text selectable style={{ color: colors.primary, fontSize: 18, fontWeight: 'bold', letterSpacing: 2 }}>
            {totpSecret}
          </Text>
        </View>
      ) : null}

      {!success ? (
        <>
          <TextInput
            style={[styles.input, { borderColor: colors.primary, color: colors.textPrimary, marginVertical: spacing.md }]}
            placeholder="Código de 6 dígitos"
            placeholderTextColor={colors.textPrimary + '80'}
            value={code}
            onChangeText={setCode}
            keyboardType="numeric"
            maxLength={6}
          />
          {error ? <Text style={{ color: colors.error, marginBottom: spacing.sm }}>{error}</Text> : null}
          <Button onPress={handleConfirmTotp} label={loading ? 'Validando...' : 'Confirmar e Ativar 2FA'} />
        </>
      ) : (
        <View style={{ marginTop: spacing.md }}>
          <Text style={{ color: colors.primary, fontSize: 18, marginBottom: spacing.md }}>
            2FA ativado com sucesso!
          </Text>
          <Button onPress={() => router.push('/login')} label="Ir para o Login" />
        </View>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, justifyContent: 'center' },
  secretBox: {
    padding: 16,
    borderRadius: 8,
    borderWidth: 1,
    alignItems: 'center',
    marginVertical: 12,
  },
  input: { borderWidth: 1, padding: 12, borderRadius: 8, textAlign: 'center', fontSize: 20, letterSpacing: 4 },
});
