import { colorTokens } from "../../src/design-system/tokens";

describe("design system - tokens de cor", () => {
  // Regra da seção 7.1 da spec do MVP: o lima elétrico nunca é cor de TEXTO sobre fundo claro.
  it("usa onPrimary escuro (#111111) sobre o primário no modo claro, nunca o próprio lima como texto", () => {
    expect(colorTokens.light.onPrimary).toBe("#111111");
    expect(colorTokens.light.textPrimary).not.toBe(colorTokens.light.primary);
  });

  it("define primário igual nos dois modos (é o mesmo lima elétrico da marca)", () => {
    expect(colorTokens.dark.primary).toBe("#C6F432");
    expect(colorTokens.light.primary).toBe("#C6F432");
  });

  it("define um token de sucesso/erro/alerta distinto do texto secundário em cada modo", () => {
    for (const mode of [colorTokens.dark, colorTokens.light]) {
      expect(mode.success).not.toBe(mode.textSecondary);
      expect(mode.error).not.toBe(mode.textSecondary);
      expect(mode.warning).not.toBe(mode.textSecondary);
    }
  });
});
