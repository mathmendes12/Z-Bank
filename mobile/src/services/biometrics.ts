import * as LocalAuthentication from 'expo-local-authentication';
import * as SecureStore from 'expo-secure-store';
import { api } from './api';
import { Platform } from 'react-native';

const PRIVATE_KEY_STORE = 'zbank_ecdsa_private_key';
const DEVICE_ID_STORE = 'zbank_device_id';

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let binary = '';
  for (let i = 0; i < bytes.byteLength; i++) {
    binary += String.fromCharCode(bytes[i]);
  }
  return btoa(binary);
}

function base64ToArrayBuffer(base64: string): ArrayBuffer {
  const binaryString = atob(base64);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes.buffer;
}

export async function hasBiometrics(): Promise<boolean> {
  const hasHardware = await LocalAuthentication.hasHardwareAsync();
  const isEnrolled = await LocalAuthentication.isEnrolledAsync();
  return hasHardware && isEnrolled;
}

export async function registerDevice(accessToken: string): Promise<string> {
  const hasAuth = await hasBiometrics();
  if (!hasAuth) {
    throw new Error('Biometria não disponível ou não configurada no dispositivo.');
  }

  // Generate NIST P-256 ECDSA key pair
  const keyPair = await globalThis.crypto.subtle.generateKey(
    { name: 'ECDSA', namedCurve: 'P-256' },
    true,
    ['sign', 'verify']
  );

  const spkiBuffer = await globalThis.crypto.subtle.exportKey('spki', keyPair.publicKey);
  const publicKeyBase64 = arrayBufferToBase64(spkiBuffer);

  const pkcs8Buffer = await globalThis.crypto.subtle.exportKey('pkcs8', keyPair.privateKey);
  const privateKeyBase64 = arrayBufferToBase64(pkcs8Buffer);

  await SecureStore.setItemAsync(PRIVATE_KEY_STORE, privateKeyBase64, {
    keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
  });

  const res = await api.post(
    '/api/v1/devices/register',
    {
      publicKey: publicKeyBase64,
      deviceName: `${Platform.OS} device`,
    },
    {
      headers: { Authorization: `Bearer ${accessToken}` },
    }
  );

  const deviceId = res.data.deviceId;
  await SecureStore.setItemAsync(DEVICE_ID_STORE, deviceId);
  return deviceId;
}

export async function biometricLogin(cpf: string): Promise<{ accessToken: string; refreshToken: string }> {
  const deviceId = await SecureStore.getItemAsync(DEVICE_ID_STORE);
  if (!deviceId) {
    throw new Error('Dispositivo não cadastrado para biometria. Faça login com senha e TOTP primeiro.');
  }

  const challengeRes = await api.post('/api/v1/auth/biometric/challenge', { deviceId });
  const nonce = challengeRes.data.nonce;

  const authResult = await LocalAuthentication.authenticateAsync({
    promptMessage: 'Autentique-se para entrar no ZBank',
    fallbackLabel: 'Usar senha',
  });

  if (!authResult.success) {
    throw new Error('Falha na autenticação biométrica.');
  }

  const privateKeyBase64 = await SecureStore.getItemAsync(PRIVATE_KEY_STORE);
  if (!privateKeyBase64) {
    throw new Error('Chave privada biométrica não encontrada.');
  }

  const privateKeyBuffer = base64ToArrayBuffer(privateKeyBase64);
  const privateKey = await globalThis.crypto.subtle.importKey(
    'pkcs8',
    privateKeyBuffer,
    { name: 'ECDSA', namedCurve: 'P-256' },
    false,
    ['sign']
  );

  const nonceBytes = new TextEncoder().encode(nonce);
  const signatureBuffer = await globalThis.crypto.subtle.sign(
    { name: 'ECDSA', hash: { name: 'SHA-256' } },
    privateKey,
    nonceBytes
  );
  const signatureBase64 = arrayBufferToBase64(signatureBuffer);

  const loginRes = await api.post('/api/v1/auth/biometric/login', {
    cpf: cpf.replace(/\D/g, ''),
    deviceId,
    nonce,
    signature: signatureBase64,
  });

  return loginRes.data;
}
