export const FACE_COLORS = [
  '#63E7FF',
  '#FF71D6',
  '#8BEA7D',
  '#FFE082',
  '#FFB064',
  '#B49AFF',
  '#FF8296',
  '#D9F3FF',
] as const;
const COLOR_NAMES = [
  'cyan',
  'magenta',
  'green',
  'yellow',
  'orange',
  'violet',
  'rose',
  'ice',
] as const;
const SHAPES = [
  'circle',
  'square',
  'triangle',
  'diamond',
  'star',
  'hexagon',
  'plus',
  'chevron',
] as const;
const MARKERS = [
  'top',
  'top-right',
  'right',
  'bottom-right',
  'bottom',
  'bottom-left',
  'left',
  'top-left',
] as const;
export const FACE_COUNT = 128;
const DIAGONAL_OFFSET = 40 / Math.SQRT2;
const MARKER_CENTERS = [
  [50, 10],
  [50 + DIAGONAL_OFFSET, 50 - DIAGONAL_OFFSET],
  [90, 50],
  [50 + DIAGONAL_OFFSET, 50 + DIAGONAL_OFFSET],
  [50, 90],
  [50 - DIAGONAL_OFFSET, 50 + DIAGONAL_OFFSET],
  [10, 50],
  [50 - DIAGONAL_OFFSET, 50 - DIAGONAL_OFFSET],
] as const;

export function faceDesign(face: number) {
  if (!Number.isInteger(face) || face < 0 || face >= FACE_COUNT) throw new Error('Invalid face.');
  const shape = SHAPES[face % SHAPES.length];
  const markerIndex = Math.floor(face / SHAPES.length) % MARKERS.length;
  const marker = MARKERS[markerIndex];
  const center = MARKER_CENTERS[markerIndex];
  const color = FACE_COLORS[markerIndex];
  const colorName = COLOR_NAMES[markerIndex];
  const outline = Math.floor(face / 64) === 1;
  if (!shape || !marker || !center || !color || !colorName) throw new Error('Invalid face.');
  return {
    shape,
    center,
    color,
    outline,
    label: `${colorName} ${outline ? 'outline' : 'solid'} ${shape}, marker ${marker}`,
  };
}

export type FaceShape = (typeof SHAPES)[number];
