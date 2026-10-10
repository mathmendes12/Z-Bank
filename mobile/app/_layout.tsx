import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { useTokens } from "../src/design-system/useTokens";

export default function RootLayout() {
  const tokens = useTokens();

  return (
    <>
      <StatusBar style="auto" />
      <Stack
        screenOptions={{
          headerStyle: { backgroundColor: tokens.background },
          headerTintColor: tokens.textPrimary,
          contentStyle: { backgroundColor: tokens.background },
        }}
      >
        <Stack.Screen name="index" options={{ title: "Z Bank" }} />
      </Stack>
    </>
  );
}
