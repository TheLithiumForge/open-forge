const Ansi = /\u001b\[[\d;]*m/g;

export function passedTests(output: string): boolean {
  const text = output.replace(Ansi, "");
  return /(?:^|\n)\s*(?:#|ℹ)?\s*pass\s+[1-9]\d*\b/m.test(text)
    || /\bTests\s+[1-9]\d*\s+passed\b/i.test(text)
    || /(?:^|\n)\s*[1-9]\d*\s+passed\b/m.test(text);
}
