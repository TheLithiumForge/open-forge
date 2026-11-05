# Changelog style

This guide explains how Neon Pairs release notes describe changes in behavior, compatibility and evidence without becoming a second specification.

A release note helps a reader understand what a particular revision changes for play, reproduction or development. It can point to a detailed rule or decision, but its first job is to name a recognizable consequence. The examples below show how release notes describe changes in the game.

## Lead with the changed experience

“Restart repeats the current deal” is more informative than “improved game controls”. It identifies both an action and the result a player can expect. A note about a fix can be more specific still: “Restarting during a mismatch clears the old turn; its pending update cannot close a card in the restarted game.” That sentence gives the trigger, the unwanted interaction and the corrected behavior.

The corresponding implementation is split between game replacement and session timing. `replaceGame` increments `generation`, and `tickSession` ignores an update carrying another generation. A release note can mention that mechanism when engineers need it, but the behavior belongs first. [New game and restart](new-game-and-restart.md) supplies the full interaction contract.

An implementation-only note is appropriate when its consequence is a development workflow change. For example, a note about the browser command can say that `npm run test:e2e` builds current source before testing the resulting HTML. The point is which artifact was tested. Merely saying “updated test infrastructure” leaves the reader unable to tell what became more reliable.

## A change statement and its source

These examples show useful release-note granularity through changes in the game.

| Example note | Where its current result is visible |
| --- | --- |
| Matched cards remain available to keyboard focus, with their matched state exposed. | `board/Card.tsx` and board focus behavior. |
| Hidden cards omit their face artwork and identity from the rendered card. | `board/Card.tsx` and `board/card-state.ts`. |
| Invalid board and snapshot loads retain the entered text and active game. | `session/use-game.ts` and the reproduction controls. |
| Export produces readable snapshot JSON in the textarea for manual copying. | `controls/Reproduction.tsx` and `snapshot.ts`. |
| The replay command reports an invalid action's position in its diagnostic. | `scripts/replay-cli.ts`. |

The paths in that table are shorthand within the game source, except the explicit script path. A finished note can name the full path if the implementation detail matters. It does not need to enumerate every file touched by the change. A file list tells someone how a patch was organized; a release note tells them why the patch matters.

## Copy changes have behavioral context

A copy note can quote a short current string. “An unfinished snapshot load reports ‘Loaded snapshot. Select Resume to continue.’” explains why the status includes an instruction. The relevant change is not simply “friendlier text”; it helps a player understand the paused state.

Control names are also concrete references. “Load board” and “Load snapshot” identify different replacement operations. “New Game” and “Restart” identify different deals. A release note that uses “reset” for all four may sound shorter but loses the information a reader needs to follow the change.

## Versions name different artifacts

The specification is at 2.2. Board codes use the `np1` generation prefix. Snapshots and replays have format version 1. The game package identifies itself as `1.0.0`. These numbers are not interchangeable release counters, so a note about one names its subject explicitly.

A useful compatibility sentence for the current specification is: “Specification 2.2 retains version 1 board-code, snapshot and replay formats.” It tells someone with saved reproduction material what to expect. It does not imply that every implementation change is a format migration.

A deck-generation change would be especially consequential because the board-code version fixes the algorithm. A note discussing such a proposal belongs with [Board code format](board-code-format.md) and [Deck generation](deck-generation.md); it cannot claim unchanged reproducibility just because the code's visible spelling stayed familiar. Conversely, moving code between modules does not by itself alter a serialized version.

## Evidence is part of the release account

An implementation statement and a verification statement support different conclusions. “The build emits a self-contained `dist/index.html`” describes the distribution design. “The resulting HTML was opened from disk with network access blocked” describes an executed check. The second statement belongs in a release record only when that check was actually performed for the release being described.

The repository README records Node 26.12.1, npm 11.20.0, Playwright 1.64.1 and Chromium 156.0.8078.4 as the named tested combination. It also says a screen reader was not run. A release note can summarize that bounded evidence and link to [Browser support](browser-support.md) or [Accessibility audit](accessibility-audit.md). It cannot turn the recorded Chromium result into a claim about every browser or spoken output.

The source tests provide places to look for specific evidence: `tests/control-contract.e2e.test.ts` checks literal labels and statuses, `tests/announcements.e2e.test.ts` checks repeated feedback events, and `scripts/replay.test.ts` checks command output and failures. Mentioning a test file is useful traceability. It is not the same as reporting a passing run.

## A compact note, with commentary

The following corrective-release entry describes recovery behavior:

> Reproduction tools keep invalid drafts available for correction. Loading an unacceptable board code or snapshot reports the relevant notice and leaves the active game in place. The replay command gives file-specific diagnostics on stderr while leaving stdout available for successful snapshot JSON.

The first sentence names the reader benefit. The next two separate browser recovery from command-line output. The paragraph does not need a catalogue of parser branches because those belong in reference documentation. It also avoids implying that a browser notice now diagnoses every individual field.

For a smaller release, the first two sentences could stand alone. For a release centered on replay, the last sentence could lead and point to an example diagnostic. The right length follows the change's consequences. [Documentation style](documentation-style.md) covers source-grounded examples, and [Release checklist](release-checklist.md) is the place to find the operational checks that accompany publication.
