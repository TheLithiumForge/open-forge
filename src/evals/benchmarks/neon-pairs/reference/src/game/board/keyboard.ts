export const BoardKey = {
  left: 'ArrowLeft',
  right: 'ArrowRight',
  up: 'ArrowUp',
  down: 'ArrowDown',
} as const;

interface KeyModifiers {
  readonly ctrlKey: boolean;
  readonly altKey: boolean;
  readonly shiftKey: boolean;
  readonly metaKey: boolean;
}

// I06 lists only the plain arrow keys. Every other key and modifier combination, such as
// Alt+ArrowLeft for Back or Ctrl+End, stays with the browser.
export function navigationTarget(
  index: number,
  size: number,
  key: string,
  modifiers: KeyModifiers,
): number | null {
  if (modifiers.ctrlKey || modifiers.altKey || modifiers.shiftKey || modifiers.metaKey) return null;
  const rowStart = Math.floor(index / size) * size;
  switch (key) {
    case BoardKey.left:
      return Math.max(rowStart, index - 1);
    case BoardKey.right:
      return Math.min(rowStart + size - 1, index + 1);
    case BoardKey.up:
      return Math.max(index % size, index - size);
    case BoardKey.down:
      return Math.min(size * (size - 1) + (index % size), index + size);
    default:
      return null;
  }
}
