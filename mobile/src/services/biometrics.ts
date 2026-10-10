import * as LocalAuthentication from 'expo-local-authentication';
import * as SecureStore from 'expo-secure-store';
import * as Crypto from 'expo-crypto';
import { api } from './api';
import { Platform } from 'react-native';

const BIOMETRIC_KEY_ALIAS = 'zbank_biometric_key';
const PRIVATE_KEY_STORE = 'zbank_private_key';

export async function hasBiometrics(): Promise<boolean> {
  const hasHardware = await LocalAuthentication.hasHardwareAsync();
  const isEnrolled = await LocalAuthentication.isEnrolledAsync();
  return hasHardware && isEnrolled;
}

export async function registerDevice(): Promise<void> {
  // In a real app, we would generate a key pair in Secure Enclave here.
  // Since Expo Crypto doesn't expose native asymmetric key generation directly without native modules,
  // we will simulate the behavior by storing a symmetric key / dummy string for demonstration,
  // or use a library that supports it. Since we are restricted to Expo Go / eas build without extra bare native code,
  // we'll mock the asymmetric part or just do a generic secure store save.
  
  const hasAuth = await hasBiometrics();
  if (!hasAuth) {
    throw new Error('Biometria não disponível');
  }

  // Simulate generating key pair
  const publicKey = 'dummy-public-key-p256';
  const privateKey = 'dummy-private-key-p256';

  await SecureStore.setItemAsync(PRIVATE_KEY_STORE, privateKey, {
    keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
  });

  // Send public key to backend
  await api.post('/api/v1/devices/register', {
    publicKey,
    deviceName: Platform.OS,
  });
}

export async function biometricLogin(cpf: string): Promise<{ access_token: string, refresh_token: string }> {
  // Request challenge
  const challengeRes = await api.post('/biometric/challenge', { cpf });
  const nonce = challengeRes.data.nonce;

  // Authenticate user
  const result = await LocalAuthentication.authenticateAsync({
    promptMessage: 'Autentique-se para entrar no ZBank',
    fallbackLabel: 'Usar senha',
  });

  if (!result.success) {
    throw new Error('Falha na autenticação biométrica');
  }

  // Sign nonce
  const privateKey = await SecureStore.getItemAsync(PRIVATE_KEY_STORE);
  if (!privateKey) {
    throw new Error('Chave privada não encontrada. Faça login com senha.');
  }

  // Simulate signature
  const signature = await Crypto.digestStringAsync(Crypto.CryptoDigestAlgorithm.SHA256, nonce + privateKey);

  // Send to backend
  const loginRes = await api.post('/biometric/login', {
    cpf,
    nonce,
    signature,
  });

  return loginRes.data;
}
