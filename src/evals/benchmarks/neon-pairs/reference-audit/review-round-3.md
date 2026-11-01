# Reference Review Three

Reference Review Three is the correction record for the load-status announcement regression found after the reference received 9.5 out of 10.

For example, loading a snapshot shows and announces "Loaded snapshot. Select Resume to continue." before you resume its paused game.

Status: corrected candidate. Independent judgment is pending. I17.6 still needs a person with NVDA.

## Correction

C09.4, C11.3 and C11.4 send the same load notice to the paragraph beside the reproduction controls and the existing keyed live region beside the board. Each event increments the announcement counter once. The game still has one polite atomic live region.

The browser status test expects "Invalid snapshot." in that live region. The accessibility journey submits each invalid board code, invalid snapshot and valid snapshot twice. It checks the exact status text, button focus, one live-region change per event and paused card states. Six new tree captures record these results. Speech is still a person review item.

## Verification

The [verification record](verification.json) gives install, type check, Vitest, fresh build, three consecutive browser runs and mutation results. The [trace map](../trace.json) links each affected clause to source, the browser journey and its tree captures. Product rules, requirement IDs and all 401 clauses stay unchanged.

Command logs from this round stay outside the game. There are 32 generated npm and browser log files in its ignored work folders. Automatic approval rejected their requested removal as "blocked by policy". The overseer needs to remove them.

## File changes

These files changed in this round. Generated output and ignored work folders are excluded:

| Change  | Path                                                  |
| ------- | ----------------------------------------------------- |
| Changed | `reference/src/game/session/use-game.ts`              |
| Changed | `reference/src/game/session/session.ts`               |
| Changed | `reference/src/game/session/session.test.ts`          |
| Changed | `reference/tests/status-controls.e2e.test.ts`         |
| Changed | `reference/tests/accessibility.e2e.test.ts`           |
| Changed | `trace.json`                                          |
| Changed | `reference-audit/AUDIT.md`                            |
| Changed | `reference-audit/evidence/accessibility-journey.json` |
| Changed | `reference-audit/mutation-results.json`               |
| Changed | `reference-audit/verification.json`                   |
| Changed | `reference-audit/seed-record.json`                    |
| Created | `reference-audit/review-round-3.md`                   |

Back to [Reference Audit](AUDIT.md).
