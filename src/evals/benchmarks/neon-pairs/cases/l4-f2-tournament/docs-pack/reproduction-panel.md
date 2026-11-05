# Reproduction panel

This page describes the board-code and snapshot controls implemented by `Reproduction.tsx` and `BoardCodeForm.tsx`.

The panel lets a player reproduce a deal or transfer progress without a server or clipboard integration. A board code describes the initial deal; a snapshot describes a position reached on that deal. The UI preserves that distinction with separate fields and actions. The formats themselves live in [Board code format](board-code-format.md) and [Snapshot format](snapshot-format.md).

## The disclosure and its surroundings

`Game.tsx` renders Reproduction after the Board. Its native `details` element has class `reproduction` and no `open` attribute, so it starts collapsed. The summary reads `Reproduce a game`, taken from `ReproductionControl.summary` in `control-labels.ts`. Opening and closing the disclosure is browser behavior; the component has no separate expanded-state variable.

Inside `.reproduction-body`, the order is the BoardCodeForm, the Snapshot JSON label, the textarea and the snapshot action row. After the details element, a `.load-status` container holds the notice paragraph and, while the imported game is paused, Resume. The notice is therefore outside the collapsible content. Closing the disclosure does not remove the load outcome or the way to resume from the document.

That location is deliberate in the current layout. A paused game can still be understood and continued after a user closes a large JSON field. It also keeps the visible load notice beside its controls rather than mixing it into the statistics definition list. The announcement path is discussed in [Announcements policy](announcements-policy.md); the ordinary paragraph with `id="load-notice"` is useful visible feedback in its own right.

## Board-code drafts

BoardCodeForm receives `code`, `generation` and a `loadBoard(code): boolean` callback. Its input uses `defaultValue={code}`, not a React `value` tied to every render. The input is keyed by generation. This combination gives the form two behaviors that are easy to lose when simplifying its state:

- Card actions and clock renders keep a user's edited draft.
- Replacing the game mounts an input initialized with the replacement's code.

A person can type a draft, play a card and return to the draft without the code reverting. Conversely, New Game, Restart and successful imports update the displayed current code through the replacement generation. A failed load does not replace the generation, so it also leaves the entered text available for correction.

The input's native name is `boardCode`, its id is `board-code`, and its visible label is `Board code`. `maxLength` is 32, spell checking is disabled and autocomplete is off. The form reads the draft with `new FormData(event.currentTarget).get('boardCode')`. It does not find the field through a query for its class.

Submitting prevents browser navigation. If the form value is a string, it passes it to `loadBoard`; only a true result triggers `loadButton.current?.focus()`. The ref belongs to the `Load board` submit button. This focus step matters when Enter submits from the input: a successful load replaces that input node, while a failed load leaves it intact. On failure from the keyboard, the user remains in the draft. Clicking the button naturally leaves focus on that button in either case.

## Validation and replacement

The callback in `use-game.ts` trims surrounding whitespace before calling `parseBoardCode`. Internal changes to the code are not guessed. `np1-4-1` is valid; an unsupported side length such as `np1-5-1` is not. The parser's grammar and seed bounds belong to the format page, rather than to the input's character limit.

An invalid code publishes `Invalid board code.` and returns false. It does not reset the clock or replace the game. A valid code resets the clock, creates the deal's starting state and commits it through `replaceGame`. A successful board-code load does not show the paused-snapshot notice; it begins a fresh hidden deal and clears the old load text.

For a concrete distinction, type `np1-6-1` while playing a four-column game. Nothing changes until the form is submitted. After success, the board has 36 cards and Grid size displays 6x6. The new board's remembered tab stop starts at its first card, but the Load board action retains focus. [Control bar](control-bar.md) explains why the select follows this replacement automatically.

## Snapshot editing

The textarea uses component state: `const [snapshot, setSnapshot] = useState('')`. It has id `snapshot-json`, the label `Snapshot JSON`, five rows, `maxLength` 65536 and disabled spell checking. Its `onChange` copies the current textarea value into local state. Unlike the board-code input, it is not keyed by session generation.

This means that starting a new deal does not silently replace the textarea with a serialization of that deal. The field remains an editing surface until the user changes it or exports a snapshot. Loading a snapshot also does not rewrite its draft into a canonical form. Preserving the text makes it possible to correct malformed input and retry without reconstructing it.

The textarea is full width and vertically resizable. It provides ordinary text selection and copy behavior. Export places readable JSON into this field; the user can select it manually. There is no permission prompt for clipboard access and no dependency on an available Clipboard API.

The character limit on the textarea is not the entire format limit. `decodeSnapshot` also enforces a UTF-8 byte bound. A string's browser length and encoded byte length can differ, so validation remains necessary even when entry passed through this field. Programmatic callers also need the parser rather than relying on a UI attribute.

## Export is a read of the current position

`Export snapshot` calls `setSnapshot(exportSnapshot())`. The callback in `use-game.ts` first calls `clock.synchronize()` because the rendered state may trail the active clock by a tick, then encodes `current.current.game`. The UI gets a current position without pausing, resetting or replacing play.

The serialization uses whole milliseconds for elapsed time and a pending mismatch remainder. An exported pending mismatch retains a positive remainder. Those details allow a resumed position to continue the same waiting interval rather than immediately close due to a rounded zero. [Snapshot selection notes](snapshot-selection-notes.md) discusses the representation; the UI itself neither computes the remainder nor constructs the JSON fields.

Export is an ordinary `type="button"` action. It leaves focus on the export button and fills the textarea. It does not automatically select all text, copy it or move focus into the field. Native selection is available when the user chooses to copy. The browser test explicitly selects the field and checks that its complete contents can be selected.

## Loading progress and continuing it

`Load snapshot` passes the entire local draft to `loadSnapshot`. The callback decodes the complete text before any replacement. Invalid input publishes `Invalid snapshot.` and preserves both game and draft. A valid unfinished snapshot loads paused with the notice `Loaded snapshot. Select Resume to continue.` A valid completed snapshot stays completed and reports `Loaded completed snapshot.`

The paused flag belongs to the session, not to the textarea. Card actions are blocked at that boundary while board focus can still move. The visible Resume button is a sibling of the notice in `.load-status`. It is rendered only when `paused` is true. An unfinished snapshot with no elapsed time is still an unfinished import and follows the pause path; the UI does not infer readiness from its counters.

`Game.tsx` wraps resume in a function that first calls `board.current?.focusRemembered()` and then `controller.resume()`. The order gives focus a durable target before the Resume button disappears. The controller resets its clock adapter, clears `paused`, `feedback` and `loadNotice`, and leaves the loaded game state in place. If a pending mismatch had 300ms left when saved, it continues for that remaining active interval after Resume.

A completed import does not offer Resume. GameStatus shows its completion text, and the celebration flag is false because `replaceGame` clears it. Importing an impressive completed result does not award a local play record. Finishing later after resuming unfinished progress uses the ordinary completion path. The difference comes from session transitions rather than a display-only exception.

## CSS structure

`reproduction.css` gives the details panel a `24px` top margin, panel background, neutral border and `12px` radius. Its summary has a minimum height of `44px` and `12px 16px` padding. The body is a grid with a `12px` gap and `4px 16px 16px` padding. BoardCodeForm is another grid, this time with a `6px` gap between its label and control row.

The shared `.control-row` is a wrapping flex row aligned at the bottom. The board input gets `flex: 1` and `width: 16ch`; its `min-width: 0` comes from the shared control stylesheet. This lets it shrink or wrap around the action without forcing the whole page wider. At widths up to 599px, buttons directly inside `.control-row` get `flex: 1`.

The load-status row also wraps, with a `12px` gap and a `56px` minimum height. Its paragraph can grow and has `min-width: 0`. These are layout properties, not promises that every possible notice occupies one line. Long or wrapped text participates in document layout instead of being painted over nearby controls. [Form layout guide](form-layout-guide.md) covers how these nested rows differ from the deal bar.

## Behaviors worth checking together

`tests/controls.e2e.test.ts` covers edited drafts surviving a card action, native limits, exact errors, readable exports and action focus. `tests/status-controls.e2e.test.ts` tests successful and unsuccessful Enter submission from the board-code field. `tests/focus-entry.e2e.test.ts` exercises Resume returning to a far remembered card on a phone board. Snapshot and session tests cover acceptance and replacement independently of the rendered controls.

A useful manual sequence begins by editing Board code to an invalid value, revealing a card and submitting the draft. The current game remains and the draft survives. Correct it to a valid code, submit with Enter, and inspect the focused Load board button. Export that fresh game, import it, then close the disclosure. The notice and Resume remain available. Navigate on the paused board, resume and observe that focus follows the remembered location.

Replay does not add another row to this panel. The current specification leaves it in the developer CLI and game core. A reader looking for timed action playback should go to [Replay format](replay-format.md), not assume a hidden UI control exists in the disclosure.
