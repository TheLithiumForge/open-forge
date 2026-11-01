import type { CDPSession, Page } from '@playwright/test';

const AX_STATES = new Set([
  'disabled',
  'focusable',
  'focused',
  'expanded',
  'selected',
  'checked',
  'pressed',
  'live',
  'atomic',
  'relevant',
  'busy',
  'readonly',
  'required',
  'invalid',
]);

function scalar(value: unknown): string | number | boolean | null {
  return typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean'
    ? value
    : null;
}

export async function accessibilityTree(session: CDPSession) {
  const { nodes } = await session.send('Accessibility.getFullAXTree');
  const byId = new Map(nodes.map((node) => [node.nodeId, node]));
  function text(id: string): string {
    const node = byId.get(id);
    if (!node) return '';
    if (node.role?.value === 'StaticText') return String(node.name?.value ?? '');
    return (node.childIds ?? []).map(text).join('');
  }
  return nodes
    .filter((node) => !node.ignored)
    .map((node) => ({
      role: String(node.role?.value ?? ''),
      name: String(node.name?.value ?? ''),
      value: scalar(node.value?.value),
      text: text(node.nodeId),
      properties: Object.fromEntries(
        (node.properties ?? [])
          .filter((property) => AX_STATES.has(property.name))
          .map((property) => [property.name, scalar(property.value.value)]),
      ),
    }));
}

export function accessibilityNode(
  tree: Awaited<ReturnType<typeof accessibilityTree>>,
  role: string,
  name?: string,
) {
  const node = tree.find(
    (node) => node.role === role && (name === undefined || node.name === name),
  );
  if (!node) throw new Error(`Missing accessibility node: ${role}.`);
  return node;
}

// Arrow keys reach a card from the focused one, one row or column per press.
export async function moveKeyboardTo(page: Page, index: number, size: number): Promise<void> {
  const from = await page
    .locator('.card')
    .evaluateAll((cards) => cards.findIndex((card) => card === document.activeElement));
  if (from < 0) throw new Error('Focus a card before moving by keyboard.');
  const rows = Math.floor(index / size) - Math.floor(from / size);
  const columns = (index % size) - (from % size);
  for (let step = 0; step < Math.abs(rows); step += 1)
    await page.keyboard.press(rows < 0 ? 'ArrowUp' : 'ArrowDown');
  for (let step = 0; step < Math.abs(columns); step += 1)
    await page.keyboard.press(columns < 0 ? 'ArrowLeft' : 'ArrowRight');
}
