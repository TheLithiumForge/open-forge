# Error display guide

This page explains how failed board-code and snapshot loads are reported without replacing the current game.

The current UI has two validation outcomes that are errors: `Invalid board code.` and `Invalid snapshot.`. They describe the attempted load, not a broken game. The player can correct the draft and retry while the current deal remains available.

## The visible location

Reproduction renders a paragraph with `id="load-notice"` and class `load-notice` inside `.load-status`. That container follows the native details element. It is adjacent to the reproduction tools but outside their collapsible body.

Both Load board and Load snapshot carry `aria-describedby="load-notice"`. Their visible names remain the action labels; the description associates the nearby outcome with those actions. The field labels stay Board code and Snapshot JSON rather than being replaced with an error sentence.

Because the paragraph is outside details, it remains in document flow when the panel is closed. A person does not have to reopen the JSON field merely to see the most recent load notice or Resume. The `.load-status` flex row wraps, has a `12px` gap and a `56px` minimum height; its paragraph can grow and shrink within the available width.

The same load outcome is also published through the game's feedback path. [Announcements policy](announcements-policy.md) describes that event handling. The local paragraph's role here is visible context beside the load tools, not a second independent validation system.

## Who decides that a load failed

BoardCodeForm reads its draft through FormData and calls the `loadBoard` prop. The controller in `use-game.ts` trims surrounding whitespace and checks `parseBoardCode`. Failure calls `notice(SessionNotice.invalidBoard)` and returns false before clock reset or replacement.

Reproduction passes the complete snapshot draft to `loadSnapshot`. The controller calls `decodeSnapshot`. Failure calls `notice(SessionNotice.invalidSnapshot)` and returns false, again before resetting or replacing the game. The textarea component does not try to validate JSON structure itself.

`SessionNotice` in `session.ts` owns the literal messages. The shared `notice` helper sets both `feedback` and `loadNotice` and advances announcement identity. The exact words therefore come from the session boundary that knows the operation failed, rather than being duplicated in each field's change handler.

This ordering is important. If a load reset the timer before parsing, even an invalid draft could alter play. If it replaced the game before reporting failure, the notice would not describe the preservation behavior promised by the controls specification. The current callbacks validate first.

## What a failure preserves

A failed board-code load leaves the input's DOM node intact because generation does not change. Its edited value remains available. A failed snapshot load leaves Reproduction's local string state intact. Neither failure clears the draft to make the panel look clean.

The game object, board size, matched positions, selection, moves and elapsed state are not replaced by the error path. The existing game can continue to advance active time. An invalid input is not an imported pause, and it does not freeze a pending mismatch merely because a notice appeared.

Focus memory also survives because Board is not remounted. A player who last visited a far card can return there after correcting a form. The current saved size is unchanged by a failed load; successful replacement is what updates that preference.

The load notice is a session field separate from the game. Adding it can produce a render without changing card state. This is why comparing the complete session object for identity is different from asserting that gameplay was replaced. [Game state model](game-state-model.md) and [Session module](session-module.md) explain those boundaries.

## Focus depends on how submission began

A click on Load board ordinarily leaves focus on the button. If the code is invalid, BoardCodeForm does not issue another focus call. A keyboard Enter submission from inside Board code leaves focus in the field on failure, so the user can continue editing.

On successful Enter submission, the input is about to be replaced because it is keyed by generation. BoardCodeForm explicitly focuses its Load board ref when the callback returns true. That is a success-path focus correction, not an error-summary jump.

Load snapshot is a `type="button"` action. Its handler passes local text to the controller and does not move focus elsewhere on failure or success. The snapshot textarea stays mounted. Export likewise keeps its action focus and fills the field rather than focusing or selecting it automatically.

These cases show why “errors focus the field” would be an inaccurate description of this interface. The source preserves the user's current interaction where it can. [Focus policy](focus-policy.md) covers the complete set of current focus transfers.

## Notices are outcomes, not sticky field annotations

`loadNotice` is one session string used for the latest published load outcome. It is not an error object keyed separately by field. The UI does not render two simultaneous field-specific error paragraphs or an error list above the panel.

After an invalid Board code attempt followed by an invalid snapshot attempt, the visible notice is the snapshot message. The Board code draft can still be invalid; the notice records the most recent action rather than continuously describing all field values.

Editing the snapshot string changes local state but does not itself clear the controller's notice. Editing Board code is native uncontrolled editing and likewise does not call a notice-clearing function. The outcome can remain visible while the user works on a correction.

Export replaces textarea text but does not explicitly clear `loadNotice`. Card turns update feedback without rewriting the load notice. New Game, Restart and a successful board-code replacement clear both through `replaceGame(..., false)`. Successful snapshot loading replaces the notice with its load outcome, and Resume clears it when continuing play.

This lifecycle explains a report that an old load message is still visible after editing. The message is action feedback, not a live validator attached to every keystroke. Adding an on-change clearing behavior would be a change to this lifecycle, not a fix to a missing CSS state.

## Format validity is more than native limits

Board code has a 32-character entry cap, but many shorter strings are invalid. An unsupported size, zero seed, leading zeros or malformed separators fail the parser even when the input is well below its native maximum. The loader's surrounding trim is a deliberate adapter step, not broad syntax repair.

Snapshot JSON has a 65536-character textarea limit and a UTF-8 byte bound in the decoder. Malformed JSON is only one failure kind. Unsupported versions, unknown properties, inconsistent matched positions, invalid selections and out-of-range counters can also produce the same UI error.

The UI intentionally gives the short outcome rather than exposing the decoder's internal reasoning. There is no implemented per-property error report or highlighted JSON line number. [Import safety policy](import-safety-policy.md) and [Snapshot format](snapshot-format.md) are the useful debugging references for engineers handling a rejected draft.

A completed valid snapshot is not an error merely because its result exceeds the local stored best. It loads as completed with a completion notice and static banner. An unfinished valid snapshot loads paused with an instruction to resume. The notice area carries both success and failure outcomes, so `.load-notice` is a more accurate name than a field-specific error class.

## Existing visual and semantic treatment

The notice paragraph uses the surrounding document styles and the load-status layout. The current fields do not receive `aria-invalid`, an error border class or an inline replacement label on failure. Those absences are part of the existing markup, not evidence that validation did not run.

The paragraph can wrap and has `min-width: 0` in its flex row. Resume can wrap alongside it on a constrained viewport. There is no absolute positioning that places a message over the field. [Form layout guide](form-layout-guide.md) explains how the notice row relates to the grid inside the disclosure.

Name-related errors do not exist in the current component tree. Their policies are in [Name length policy](name-length-policy.md) and [Duplicate names policy](duplicate-names-policy.md). The load forms use their own messages and have no shared name-error component.

## Evidence for preservation and retry

`tests/control-contract.e2e.test.ts` populates progress, submits both invalid drafts and compares exported state before and after. It also advances the browser clock to show that the preserved game is still running. That distinguishes failure preservation from an accidental reset or pause.

`tests/status-controls.e2e.test.ts` submits a valid code with Enter, then an invalid code, checking both focus and the unchanged card count. `tests/controls.e2e.test.ts` checks exact notices, retained drafts and action focus. Repeated invalid submissions are covered in the announcement and accessibility journeys.

A useful defect report includes the draft, action origin, notice text and a before/after state observation. For example, an invalid snapshot followed by an unchanged exported selection is evidence of preservation. A notice alone cannot prove it, and a preserved game alone cannot prove that the user was told what happened. Reviewing both gives the error path its full meaning.
