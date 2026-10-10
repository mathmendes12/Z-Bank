import type { ConfigContext, ExpoConfig } from "expo/config";

// EXPO_PUBLIC_API_URL aponta para a URL do túnel em desenvolvimento e para o domínio da VPS nos
// ambientes publicados (preview/production) — ver mobile/.env.example e README "Rodando no
// celular Android".
export default ({ config }: ConfigContext): ExpoConfig => ({
  ...config,
  name: "Z Bank",
  slug: "zbank",
  version: "1.0.0",
  orientation: "portrait",
  icon: "./assets/icon.png",
  userInterfaceStyle: "automatic",
  scheme: "zbank",
  ios: {
    supportsTablet: true,
  },
  android: {
    adaptiveIcon: {
      backgroundColor: "#111111",
      foregroundImage: "./assets/android-icon-foreground.png",
      backgroundImage: "./assets/android-icon-background.png",
      monochromeImage: "./assets/android-icon-monochrome.png",
    },
    predictiveBackGestureEnabled: false,
  },
  web: {
    favicon: "./assets/favicon.png",
  },
  plugins: ["expo-router", "expo-secure-store", "expo-local-authentication"],
  // Política "fingerprint": o app nunca recebe uma atualização OTA incompatível com o build
  // nativo instalado (ver seção 8 da spec do MVP).
  runtimeVersion: {
    policy: "fingerprint",
  },
  updates: {
    url: "https://u.expo.dev/72341510-d05b-4302-858b-20a0244cc950"
  },
  extra: {
    eas: {
      projectId: "72341510-d05b-4302-858b-20a0244cc950",
    },
  },
});
