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

export async function moveKeyboardTo(page: Page, index: number, size: number): Promise<void> {
  await page.keyboard.press('Control+Home');
  for (let row = 0; row < Math.floor(index / size); row += 1)
    await page.keyboard.press('ArrowDown');
  for (let column = 0; column < index % size; column += 1) await page.keyboard.press('ArrowRight');
}
