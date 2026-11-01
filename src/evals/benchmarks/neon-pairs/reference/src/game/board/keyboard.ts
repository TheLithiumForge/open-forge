export const BoardKey = {
  left: 'ArrowLeft',
  right: 'ArrowRight',
  up: 'ArrowUp',
  down: 'ArrowDown',
  home: 'Home',
  end: 'End',
} as const;
export const ActivationKey = { enter: 'Enter', space: ' ' } as const;

export function navigationTarget(
  index: number,
  size: number,
  key: string,
  control: boolean,
): number | null {
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
    case BoardKey.home:
      return control ? 0 : rowStart;
    case BoardKey.end:
      return control ? size ** 2 - 1 : rowStart + size - 1;
    default:
      return null;
  }
}
