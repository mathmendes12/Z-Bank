import { useEffect } from 'react';
import { Stack } from 'expo-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuthStore, restoreToken } from '../src/store/authStore';

const queryClient = new QueryClient();

export default function RootLayout() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  useEffect(() => {
    restoreToken();
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      <Stack>
        {isAuthenticated ? (
          <Stack.Screen name="home" options={{ title: 'Home', headerShown: false }} />
        ) : (
          <>
            <Stack.Screen name="index" options={{ title: 'Bem-vindo', headerShown: false }} />
            <Stack.Screen name="login" options={{ title: 'Entrar', headerShown: true }} />
            <Stack.Screen name="register" options={{ title: 'Cadastro', headerShown: true }} />
          </>
        )}
      </Stack>
    </QueryClientProvider>
  );
}
