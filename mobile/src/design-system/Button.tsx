import { Pressable, Text, type GestureResponderEvent } from "react-native";
import { useTokens } from "./useTokens";
import { radii, spacing, typography } from "./tokens";

type ButtonProps = {
  label: string;
  onPress?: (event: GestureResponderEvent) => void;
  disabled?: boolean;
};

// Componente base do design system — prova de uso dos tokens (seção 7.1 da spec). Telas
// comportamentais (deslizar/segurar pra confirmar operação com dinheiro) chegam na Fase 3.
export function Button({ label, onPress, disabled }: ButtonProps) {
  const tokens = useTokens();

  return (
    <Pressable
      onPress={onPress}
      disabled={disabled}
      style={({ pressed }) => ({
        backgroundColor: tokens.primary,
        opacity: disabled ? 0.5 : pressed ? 0.85 : 1,
        paddingVertical: spacing.md,
        paddingHorizontal: spacing.lg,
        borderRadius: radii.md,
        alignItems: "center",
      })}
    >
      <Text style={{ color: tokens.onPrimary, ...typography.body, fontWeight: "700" }}>
        {label}
      </Text>
    </Pressable>
  );
}
