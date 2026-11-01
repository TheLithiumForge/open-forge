const BorderColor = /^(?:border-(?:top|right|bottom|left|block(?:-start|-end)?|inline(?:-start|-end)?)-color|border-color)$/;

export function hoverChanges(before: Readonly<Record<string, string>>, after: Readonly<Record<string, string>>): string[] {
  return [...new Set([...Object.keys(before), ...Object.keys(after)])].filter((property) => !property.startsWith("--") && !BorderColor.test(property) && before[property] !== after[property]);
}
