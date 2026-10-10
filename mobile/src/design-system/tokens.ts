// Tokens de cor, tipografia, espaçamento e raio do Z Bank — "Lima elétrico".
// Definidos uma vez aqui, consumidos por todo o app. Ver seção 7.1 da spec do MVP.

export type ColorTokens = {
  background: string;
  surface: string;
  primary: string;
  onPrimary: string;
  textPrimary: string;
  textSecondary: string;
  success: string;
  onSuccess: string;
  error: string;
  onError: string;
  warning: string;
  onWarning: string;
};

export const colorTokens: { dark: ColorTokens; light: ColorTokens } = {
  dark: {
    background: "#111111",
    surface: "#1C1C1C",
    primary: "#C6F432",
    onPrimary: "#111111",
    textPrimary: "#F4F4F0",
    textSecondary: "#9A9A9A",
    // Sucesso/erro/alerta com contraste AA sobre o fundo escuro (#111111).
    success: "#4ADE80",
    onSuccess: "#111111",
    error: "#FF6B6B",
    onError: "#111111",
    warning: "#FFC861",
    onWarning: "#111111",
  },
  light: {
    background: "#F4F4F0",
    surface: "#FFFFFF",
    // O lima elétrico nunca é cor de TEXTO sobre fundo claro (contraste insuficiente) — só vive
    // em superfície/botão, sempre com onPrimary (#111111) por cima.
    primary: "#C6F432",
    onPrimary: "#111111",
    textPrimary: "#111111",
    textSecondary: "#5C5C58",
    // Sucesso/erro/alerta com contraste AA sobre o fundo claro (#F4F4F0).
    success: "#1A7F37",
    onSuccess: "#FFFFFF",
    error: "#C8352B",
    onError: "#FFFFFF",
    warning: "#8A5A00",
    onWarning: "#FFFFFF",
  },
} as const;

export type ColorScheme = keyof typeof colorTokens;

export const spacing = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
} as const;

export const radii = {
  sm: 8,
  md: 16,
  lg: 24,
  full: 9999,
} as const;

export const typography = {
  display: { fontSize: 32, fontWeight: "700" as const },
  title: { fontSize: 20, fontWeight: "700" as const },
  body: { fontSize: 16, fontWeight: "400" as const },
  caption: { fontSize: 13, fontWeight: "400" as const },
} as const;
