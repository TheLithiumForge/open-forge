import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { PNG } from "pngjs";
import { faceDistance, signatureOf, type FaceSignature } from "../face-signature.ts";
import { DuplicateFaces, renderedFaces } from "./rendered-faces.ts";
import { CheckStatus } from "../../../../history/statuses.ts";

const Sheet = { size: 44, first: { x: 65, y: 105 }, rightMarker: { x: 65, y: 413 } } as const;
test("the old signature loses marker distinction in the reviewed grayscale capture", async () => {
  const image = PNG.sync.read(await readFile(new URL("../../reference-audit/face-sheet-grayscale.png", import.meta.url)));
  const captures = [Sheet.first, Sheet.rightMarker].map(({ x, y }) => {
    const card = new PNG({ width: Sheet.size, height: Sheet.size });
    PNG.bitblt(image, card, x, y, Sheet.size, Sheet.size, 0, 0);
    return PNG.sync.write(card);
  });
  const [first, right] = captures; assert.ok(first && right);
  assert.notDeepEqual(first, right);
  assert.ok(faceDistance(signatureOf(first), signatureOf(right)) <= DuplicateFaces.distance);
});

test("individual visible captures keep the duplicate policy and catch repeated render sets", () => {
  const identities: FaceSignature[] = Array.from({ length: DuplicateFaces.cards / 2 }, (_, index) => ({
    colors: Array.from({ length: 64 }, (_, bin) => bin === index % 64 ? 1 : 0),
    shape: Array.from({ length: 24 * 24 }, (_, cell) => Math.floor(cell / 24) < 12 === (index < 64) ? 255 : 0),
  }));
  assert.equal(renderedFaces(identities.flatMap((face) => [face, face])).status, CheckStatus.pass);
  const fallback = identities[0]; assert.ok(fallback);
  assert.equal(renderedFaces(Array.from({ length: DuplicateFaces.cards }, (_, index) => identities[index % 32] ?? fallback)).status, CheckStatus.fail);
  assert.equal(renderedFaces(identities).status, CheckStatus.fail);
});
