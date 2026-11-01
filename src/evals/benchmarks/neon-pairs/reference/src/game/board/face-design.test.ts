import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
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
