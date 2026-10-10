import { useColorScheme } from "react-native";
import { colorTokens, type ColorTokens } from "./tokens";

// Resolve os tokens de cor pro modo atual (segue o sistema, como pedido na seção 7.1 da spec).
export function useTokens(): ColorTokens {
  const scheme = useColorScheme();
  return scheme === "light" ? colorTokens.light : colorTokens.dark;
}
