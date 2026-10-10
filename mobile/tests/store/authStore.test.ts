import { useAuthStore } from '../../src/store/authStore';
import * as SecureStore from 'expo-secure-store';

jest.mock('expo-secure-store');

describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.setState({ token: null, isAuthenticated: false });
    jest.clearAllMocks();
  });

  it('deve fazer login e salvar os tokens', async () => {
    const { login } = useAuthStore.getState();
    await login('fake-access', 'fake-refresh');

    expect(SecureStore.setItemAsync).toHaveBeenCalledWith('access_token', 'fake-access');
    expect(SecureStore.setItemAsync).toHaveBeenCalledWith('refresh_token', 'fake-refresh');
    
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.token).toBe('fake-access');
  });

  it('deve fazer logout e remover os tokens', async () => {
    useAuthStore.setState({ token: 'fake', isAuthenticated: true });
    
    const { logout } = useAuthStore.getState();
    await logout();

    expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('access_token');
    expect(SecureStore.deleteItemAsync).toHaveBeenCalledWith('refresh_token');

    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.token).toBeNull();
  });
});
