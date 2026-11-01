import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'vitest';
import { FACE_COUNT, faceDesign } from './face-design.ts';

test('face identities stay unique through shape, marker and fill', () => {
  const identities = Array.from({ length: FACE_COUNT }, (_, face) => {
    const design = faceDesign(face);
    return JSON.stringify([design.shape, design.center, design.outline]);
  });
  assert.equal(new Set(identities).size, FACE_COUNT);
  const diagonal = faceDesign(FACE_COUNT - 1).center;
  assert.ok(Math.abs(Math.hypot(diagonal[0] - 50, diagonal[1] - 50) - 40) < 0.000001);
  for (const face of [-1, FACE_COUNT, 0.5, NaN]) assert.throws(() => faceDesign(face));
});

test('face names and geometry match the reviewed identity snapshot', async () => {
  const expected: unknown = JSON.parse(
    await readFile(new URL('./face-design.fixture.json', import.meta.url), 'utf8'),
  );
  assert.deepEqual([faceDesign(0), faceDesign(64), faceDesign(9)], expected);
});

test('every face takes its shape, color, fill, marker and name from the ordered tables', () => {
  const shapes = ['circle', 'square', 'triangle', 'diamond', 'star', 'hexagon', 'plus', 'chevron'];
  const markers = [
    'top',
    'top-right',
    'right',
    'bottom-right',
    'bottom',
    'bottom-left',
    'left',
    'top-left',
  ];
  const colors = [
    '#63E7FF',
    '#FF71D6',
    '#8BEA7D',
    '#FFE082',
    '#FFB064',
    '#B49AFF',
    '#FF8296',
    '#D9F3FF',
  ];
  const names = ['cyan', 'magenta', 'green', 'yellow', 'orange', 'violet', 'rose', 'ice'];
  const centers = [
    [50, 10],
    [78.2842712474619, 21.7157287525381],
    [90, 50],
    [78.2842712474619, 78.2842712474619],
    [50, 90],
    [21.7157287525381, 78.2842712474619],
    [10, 50],
    [21.7157287525381, 21.7157287525381],
  ];
  for (let id = 0; id < 128; id++) {
    const marker = Math.floor(id / 8) % 8;
    const expected = centers[marker];
    assert.ok(expected);
    const actual = faceDesign(id);
    assert.equal(actual.shape, shapes[id % 8]);
    assert.equal(actual.color, colors[marker]);
    assert.equal(actual.outline, id >= 64);
    assert.equal(
      actual.label,
      `${names[marker]} ${id >= 64 ? 'outline' : 'solid'} ${shapes[id % 8]}, marker ${markers[marker]}`,
    );
    for (let axis = 0; axis < 2; axis++)
      assert.ok(Math.abs((actual.center[axis] ?? 0) - (expected[axis] ?? 0)) < 1e-10);
  }
});
