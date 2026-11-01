import { faceDesign } from './face-design.ts';

export const CardState = { closed: 'closed', revealed: 'revealed', matched: 'matched' } as const;

export function describeCard(index: number, size: number, face: number | null, matched: boolean) {
  const state = matched ? CardState.matched : face === null ? CardState.closed : CardState.revealed;
  const position = `Row ${Math.floor(index / size) + 1}, column ${(index % size) + 1}`;
  const label = `${position}, ${state}${face === null ? '' : `, ${faceDesign(face).label}`}`;
  return { state, label };
}
