import { expect, test } from 'vitest';
import { faceDesign } from '../face-design.ts';

const FACE_COUNT = 128;

test('gives every face an identity independent from color', () => {
  const labels = Array.from({ length: FACE_COUNT }, (_, face) => faceDesign(face).label);
  expect(new Set(labels).size).toBe(FACE_COUNT);
  expect(faceDesign(0)).toMatchInlineSnapshot(`
    {
      "center": [
        50,
        10,
      ],
      "color": "#63E7FF",
      "label": "cyan solid circle, marker top",
      "outline": false,
      "shape": "circle",
    }
  `);
  expect(faceDesign(64)).toMatchInlineSnapshot(`
    {
      "center": [
        50,
        10,
      ],
      "color": "#63E7FF",
      "label": "cyan outline circle, marker top",
      "outline": true,
      "shape": "circle",
    }
  `);
  const diagonal = faceDesign(FACE_COUNT - 1).center;
  expect(Math.hypot(diagonal[0] - 50, diagonal[1] - 50)).toBeCloseTo(40, 3);
});
