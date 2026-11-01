import type { CheckOutcome } from "../../../../grading/grading.types.ts";
import { CheckStatus } from "../../../../history/statuses.ts";
import { CheckArea, CheckId } from "../check-ids.ts";
import { colorDistance, faceDistance, FaceThresholds, type FaceSignature } from "../face-signature.ts";

// Keep the L1 duplicate threshold. Only the later-layer capture path changes.
export const DuplicateFaces = { distance: 0.08, share: 0.05, cards: 256 } as const;
export function renderedFaces(faces: readonly FaceSignature[]): CheckOutcome {
  const complete = faces.length === DuplicateFaces.cards && faces.every((face) => face.shape.length > 0);
  const shared = faces.filter((face, index) => faces.filter((other, position) => position !== index
    && colorDistance(face, other) <= DuplicateFaces.distance * FaceThresholds.colors && faceDistance(face, other) <= DuplicateFaces.distance).length >= 2).length;
  return { id: CheckId.facesDistinct, area: CheckArea.faces, status: complete && shared <= faces.length * DuplicateFaces.share ? CheckStatus.pass : CheckStatus.fail,
    evidence: `Individual visible card screenshots, including scrolled cards: ${faces.length}/${DuplicateFaces.cards} captured, ${shared} share a render with more than one other card. L1 color and shape thresholds are unchanged.` };
}
