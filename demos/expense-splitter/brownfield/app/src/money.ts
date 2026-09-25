// Money is always a whole number of cents. Floating point never touches an amount.

export const CENTS_PER_UNIT = 100;
const FRACTION_DIGITS = 2;
const AMOUNT_PATTERN = /^(\d+)(?:\.(\d{1,2}))?$/;

export function parseAmount(text: string): number {
  const match = AMOUNT_PATTERN.exec(text.trim());
  if (!match) {
    throw new Error(`"${text}" is not an amount. Use a form like 12 or 12.50.`);
  }
  const [, whole = "0", fraction = ""] = match;
  return Number(whole) * CENTS_PER_UNIT + Number(fraction.padEnd(FRACTION_DIGITS, "0"));
}

export function formatCents(cents: number): string {
  const sign = cents < 0 ? "-" : "";
  const absolute = Math.abs(cents);
  const whole = Math.floor(absolute / CENTS_PER_UNIT);
  const fraction = String(absolute % CENTS_PER_UNIT).padStart(FRACTION_DIGITS, "0");
  return `${sign}${whole}.${fraction}`;
}
