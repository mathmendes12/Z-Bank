import { useEffect } from 'react';
import { Stack, useRouter } from 'expo-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuthStore, restoreToken } from '../src/store/authStore';

const queryClient = new QueryClient();

export default function RootLayout() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  useEffect(() => {
    restoreToken();
  }, []);

  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated) {
      router.replace('/home');
    } else {
      router.replace('/');
    }
  }, [isAuthenticated]);

  return (
    <QueryClientProvider client={queryClient}>
      <Stack>
        <Stack.Screen name="home" options={{ title: 'Home', headerShown: false }} />
        <Stack.Screen name="index" options={{ title: 'Bem-vindo', headerShown: false }} />
        <Stack.Screen name="login" options={{ title: 'Entrar', headerShown: true }} />
        <Stack.Screen name="register" options={{ title: 'Cadastro', headerShown: true }} />
      </Stack>
    </QueryClientProvider>
  );
}
