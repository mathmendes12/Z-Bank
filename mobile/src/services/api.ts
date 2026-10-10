// Cliente HTTP mínimo da Fase 0 — só prova que o app alcança a API pelo túnel. Vira um client
// mais completo (interceptor de token, retry) a partir da Fase 1.

const apiUrl = process.env.EXPO_PUBLIC_API_URL;

export type HealthResponse = {
  status: string;
  service: string;
};

export async function fetchHealth(): Promise<HealthResponse> {
  if (!apiUrl) {
    throw new Error(
      "EXPO_PUBLIC_API_URL não está definida — configure mobile/.env a partir do .env.example."
    );
  }

  const response = await fetch(`${apiUrl}/health`);
  if (!response.ok) {
    throw new Error(`GET /health respondeu ${response.status}`);
  }

  return (await response.json()) as HealthResponse;
}
