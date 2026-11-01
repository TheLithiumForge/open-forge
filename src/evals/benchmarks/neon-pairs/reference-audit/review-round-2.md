# Reference Review Two

Reference Review Two is the correction record for the eight remaining findings after the reference received 9 out of 10.

For example, Enter in Board code now focuses Load board before the deal replaces the input. The action keeps its button focus.

Status: corrections implemented. Full verification results are in verification.json. Independent judgment of these corrections is pending.

## Corrections

The numbered rows follow the review packet:

| Item | Change                                                                                                                                                                                                                         | Evidence                                                                    |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------- |
| 1    | Form submission focuses the stable Load board button before loading. Valid and invalid Enter submissions keep button focus.                                                                                                    | `reference/tests/status-controls.e2e.test.ts`                               |
| 2    | Game composes the single keyed live region beside the board. Load notices use separate session data and stay beside reproduction controls with Resume. Invalid load input keeps the turn result unchanged.                     | Game and Reproduction components, status controls and announcement journeys |
| 3    | A 360x800 journey checks the result line inside the browser viewport during a 16x16 mismatch and after keyboard movement to the last card. Help text has a 12px bottom gap.                                                    | Status controls journey and `game.css`                                      |
| 4    | StatusLabel defines "Best:" for both empty and recorded scores. The value is none or the move count.                                                                                                                           | Status controls journey                                                     |
| 5    | The session harness has its own HTML entry, source and build output. The face sheet has no session branch or Game import.                                                                                                      | Art journey and timer lifecycle journey                                     |
| 6    | Combined duplicate game imports in snapshot.ts and game.test.ts. Removed the blank import gap in game.ts.                                                                                                                      | Type check and source review                                                |
| 7    | Zoom readability covers the game result, load notice and Resume text. A missing zoom tab rejects its promise. Restart is checked alone with a new mismatch across the earlier deal's deadline, then at its own exact deadline. | Zoom journey, standalone Restart selection and full browser command         |
| 8    | The NVDA example and table both use one-based rows and columns.                                                                                                                                                                | `screen-reader-journey.md`                                                  |

## Accepted review

The round-2 judge passed V12.7. All 128 grayscale faces are distinguishable at 44px. The accepted PNG stays unchanged. The automatic mask check stays separate from that visual verdict.

I17.6 is still the one open person item. A person runs the NVDA journey and records the actual spoken result and versions before it is closed.

## Verification

The [verification record](verification.json) holds the locked install, type checks, Vitest, fresh build, three consecutive browser runs and all mutation results. The [audit](AUDIT.md) gives current line counts and clause review items. Specs, requirement IDs, clause counts and protected checks are unchanged.

## File changes

The following files changed from the first correction packet. Generated output and ignored work folders are excluded:

| Change  | Path                                                  |
| ------- | ----------------------------------------------------- |
| Changed | `reference-audit/evidence/accessibility-journey.json` |
| Changed | `reference-audit/mutation-results.json`               |
| Changed | `reference-audit/mutations.ts`                        |
| Created | `reference-audit/review-round-2.md`                   |
| Changed | `reference-audit/screen-reader-journey.md`            |
| Changed | `reference/.gitignore`                                |
| Changed | `reference/.prettierignore`                           |
| Changed | `reference/README.md`                                 |
| Changed | `reference/art/face-sheet.tsx`                        |
| Deleted | `reference/art/session-harness.tsx`                   |
| Created | `reference/art/session/index.html`                    |
| Created | `reference/art/session/session-harness.tsx`           |
| Changed | `reference/package.json`                              |
| Changed | `reference/src/game/Game.tsx`                         |
| Changed | `reference/src/game/controls/Reproduction.tsx`        |
| Changed | `reference/src/game/game.css`                         |
| Changed | `reference/src/game/game.test.ts`                     |
| Changed | `reference/src/game/game.ts`                          |
| Changed | `reference/src/game/session/session.ts`               |
| Changed | `reference/src/game/session/session.types.ts`         |
| Changed | `reference/src/game/session/use-game.ts`              |
| Changed | `reference/src/game/snapshot.ts`                      |
| Changed | `reference/src/game/status/GameStatus.tsx`            |
| Changed | `reference/tests/art.e2e.test.ts`                     |
| Changed | `reference/tests/control-contract.e2e.test.ts`        |
| Changed | `reference/tests/control-labels.ts`                   |
| Changed | `reference/tests/controls.e2e.test.ts`                |
| Changed | `reference/tests/game-fixture.ts`                     |
| Changed | `reference/tests/lifecycle.e2e.test.ts`               |
| Changed | `reference/tests/readability.ts`                      |
| Created | `reference/tests/status-controls.e2e.test.ts`         |
| Changed | `reference/tests/timer-lifecycle.e2e.test.ts`         |
| Changed | `reference/tests/zoom-extension.ts`                   |
| Changed | `reference/tests/zoom.e2e.test.ts`                    |
| Changed | `reference/tsconfig.node.json`                        |
| Created | `reference/vite.session.config.ts`                    |
| Changed | `trace.json`                                          |
| Changed | `reference-audit/AUDIT.md`                            |
| Changed | `reference-audit/verification.json`                   |
| Changed | `reference-audit/seed-record.json`                    |

Back to [Reference Audit](AUDIT.md).
