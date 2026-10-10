import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';

interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  login: (token: string, refreshToken: string) => Promise<void>;
  logout: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  isAuthenticated: false,
  login: async (token: string, refreshToken: string) => {
    await SecureStore.setItemAsync('access_token', token);
    await SecureStore.setItemAsync('refresh_token', refreshToken);
    set({ token, isAuthenticated: true });
  },
  logout: async () => {
    await SecureStore.deleteItemAsync('access_token');
    await SecureStore.deleteItemAsync('refresh_token');
    set({ token: null, isAuthenticated: false });
  },
}));

export const restoreToken = async () => {
  const token = await SecureStore.getItemAsync('access_token');
  if (token) {
    useAuthStore.setState({ token, isAuthenticated: true });
  }
};
