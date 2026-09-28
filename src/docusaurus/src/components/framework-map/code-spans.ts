// Splits diagram text on backticks, so both the site and the static SVG can show
// tags and commands as code.

export interface Span {
  readonly text: string;
  readonly code: boolean;
}

export function codeSpans(text: string): readonly Span[] {
  return text
    .split("`")
    .map((part, index) => ({ text: part, code: index % 2 === 1 }))
    .filter((span) => span.text.length > 0);
}
