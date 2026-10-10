import { useCallback, useEffect, useState } from "react";
import { ActivityIndicator, SafeAreaView, Text, View } from "react-native";
import { Button } from "../src/design-system/Button";
import { useTokens } from "../src/design-system/useTokens";
import { spacing, typography } from "../src/design-system/tokens";
import { fetchHealth, type HealthResponse } from "../src/services/api";

type LoadState =
  | { status: "loading" }
  | { status: "success"; data: HealthResponse }
  | { status: "error"; message: string };

// Tela única da Fase 0: prova o critério de pronto — o app, instalado via EAS, chama a API
// local pelo túnel e mostra a resposta de /health.
export default function HealthScreen() {
  const tokens = useTokens();
  const [state, setState] = useState<LoadState>({ status: "loading" });

  const check = useCallback(() => {
    setState({ status: "loading" });
    fetchHealth()
      .then((data) => setState({ status: "success", data }))
      .catch((error: unknown) =>
        setState({
          status: "error",
          message: error instanceof Error ? error.message : "Erro desconhecido",
        })
      );
  }, []);

  useEffect(() => {
    check();
  }, [check]);

  return (
    <SafeAreaView style={{ flex: 1, backgroundColor: tokens.background }}>
      <View style={{ flex: 1, justifyContent: "center", padding: spacing.lg, gap: spacing.md }}>
        <Text style={{ color: tokens.textPrimary, ...typography.display }}>Z Bank</Text>
        <Text style={{ color: tokens.textSecondary, ...typography.body }}>
          Fase 0 — conexão com a API local pelo túnel.
        </Text>

        {state.status === "loading" && <ActivityIndicator color={tokens.primary} />}

        {state.status === "success" && (
          <Text style={{ color: tokens.success, ...typography.body }}>
            {state.data.service}: {state.data.status}
          </Text>
        )}

        {state.status === "error" && (
          <Text style={{ color: tokens.error, ...typography.body }}>{state.message}</Text>
        )}

        <Button label="Tentar de novo" onPress={check} />
      </View>
    </SafeAreaView>
  );
}
