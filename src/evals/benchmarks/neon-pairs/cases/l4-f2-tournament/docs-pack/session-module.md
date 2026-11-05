# Session module

This page explains how `src/game/session` connects the pure game model to React, browser lifecycle and saved preferences.

The directory contains both pure transformations and browser adapters. `session.ts` can be called without React; `use-game.ts` provides a controller for components; the clock and storage hooks perform effects. Keeping those roles distinct makes it possible to explain a click as a sequence of ordinary data decisions followed by rendering and persistence.

## The Session value

`Session` contains eight fields. `game` is the complete `GameState`. `paused` gates an imported unfinished game. `generation` identifies replacement lifetimes. `feedback` is the current published result or notice. `loadNotice` retains load-related text for the reproduction controls. `announcement` distinguishes feedback events. `celebration` records whether this session just completed through play. `preferences` contains selected size and best moves.

These fields are not redundant copies of the core. `isWon(game)` derives victory from matched positions; there is no second `won` boolean in the session. `celebration` answers a different question from victory: a completed import is won but has no new-completion treatment. `paused` likewise describes how the browser entered the game, not whether the snapshot describes a legal state.

The game formats serialize game progress, not the whole session. A generation, browser pause, draft form text or feedback event does not appear in an exported snapshot. This lets the same snapshot be reconstructed in another session without carrying browser bookkeeping into the core model.

## Initialization

`initialSession(code, preferences)` creates a game with `createGame`. It initializes `paused` and `celebration` to false, `generation` and `announcement` to zero and both message fields to empty strings. It retains the supplied preferences.

The React controller obtains its browser store once through a lazy `useState(browserStore)`. A second lazy state initializer reads preferences, requests a fresh board for the valid saved size and calls `initialSession`. The first board therefore depends on the saved size but not on an automatically restored progress record. Active progress remains in memory until explicitly exchanged through a snapshot.

`browserStore` catches failure while obtaining `window.localStorage` and returns `null` if access is blocked. `readPreferences` can use that null store and return the default. Initialization is consequently resilient to the storage access failure that browsers can raise before `getItem` is even called.

## The current ref and React state

`useGame` maintains both React's `session` value and a `current` ref. Its `commit(next)` first compares object identity. If nothing changed it returns. Otherwise it updates the ref and then calls `setSession(next)`.

This order gives event handlers and timer callbacks immediate access to the latest committed session even before React has rendered it. Several operations can happen in a single browser task. Reading only the session captured by the last render could make an export or subsequent activation use an older game.

The ref is not an independent state machine. Every change still passes through the same commit operation, and React renders the corresponding state. The clock hook receives the ref and commit function so it can apply deltas to current data rather than a stale closure. [React boundaries](react-boundaries.md) follows the component consequences.

## Card activation

The exposed `activate(index)` synchronizes the clock, then commits `activateSession(current.current, index)`. Synchronization can close an overdue mismatch before the core decides whether the new activation is allowed. It also accounts for the interval before a final winning match.

`activateSession` rejects input while paused. It calls `activateCard` and returns the same session if the game is unchanged. For a first reveal, whose resulting selection is `one`, it returns a session with only the new game. There is no completed-turn message yet.

For a completed attempt, it derives victory and chooses feedback. A mismatch reports that the cards will close after a short pause. A match reports the matched pair count with singular/plural wording. A final match combines the pair result with the total moves and formatted elapsed time. The function increments `announcement` once for that turn.

A winning activation calls `rememberBest`, and `celebration` becomes true. The source implements direct game-derived transitions rather than a separate event bus.

## Why feedback has an event counter

Two consecutive mismatch attempts can produce identical text. A string comparison alone would make them look like the same event. The session counter gives `Game.tsx` a changing key for the span containing the text, so repeated outcomes remain distinct render events.

A first reveal does not increment the counter. A timer closing a mismatch does not increment it either: `tickSession` changes the game, while the turn result remains the same feedback. A new deal clears the text silently without incrementing the counter. Imports increment it to publish the load notice.

`loadNotice` serves the reproduction area separately from turn feedback. It is populated by imports and invalid load attempts and cleared on replacement without import or on Resume. A later turn result can change `feedback` without treating ordinary play as a new load message.

## Replacing a game

`replaceGame(session, game, imported)` preserves the surrounding session, installs the supplied game and increments generation. It sets the saved size to the new game's size, clears celebration and computes import pause and notices.

If `imported` is false, the replacement is unpaused, its load notice is empty and the last turn feedback is cleared. If true and unfinished, the session is paused with the loaded-snapshot notice. If true and already won, it is unpaused with the completed-snapshot notice. The latter is a static completed state rather than a completion event through activation.

A replacement does not itself call `rememberBest`. This matters for completed imports, whose supplied moves are not treated as a newly played local result. Finishing an unfinished imported game through activations does reach the ordinary completion path and can record its resulting best moves.

The generation increment has two consumers. Timer logic can reject work associated with an earlier game, and `Game.tsx` uses it to remount the board. `BoardCodeForm` also keys the input by generation so a replacement supplies a fresh current-code value. A restart of the same code still gets a new lifetime.

## Controller actions

| Controller action | Validation or preparation | Session effect |
| --- | --- | --- |
| `newGame(size?)` | Validate size, choose a fresh code excluding the current seed, reset clock | Replace with a new initial game |
| `restart()` | Recreate the current code, reset clock | Replace with a new initial game of that deal |
| `loadBoard(code)` | Trim and parse code; reset clock only on success | Replace, or publish invalid-code notice |
| `loadSnapshot(text)` | Decode complete state; reset clock only on success | Import replacement, or publish invalid-snapshot notice |
| `exportSnapshot()` | Synchronize active clock | Encode latest game and return text |
| `activate(index)` | Synchronize active clock | Apply session activation |
| `resume()` | Reset observation baseline | Clear pause and load feedback |

The exposed load methods return a boolean. `BoardCodeForm` uses that result to move focus only after successful loading. Snapshot loading is a button action on the current textarea string, so it does not replace the draft with a normalized serialization automatically.

The private `notice(feedback)` helper changes both message fields and increments `announcement` while preserving the game. It does not bump generation or reset the clock. An invalid submission therefore leaves progress and board focus lifetime intact.

## Logical ticks

`tickSession(session, generation, milliseconds)` returns unchanged data when the captured generation differs from the current one or when the session is paused. It then delegates to `advanceGame`. If that function returns the existing game, the existing session is returned too.

A logical tick can update elapsed time and close a mismatch. It does not publish a new turn result, save a score or create a fresh board. Those decisions remain with activation and replacement. This small contract makes it possible to test stale-callback rejection without starting a browser timer.

The hook applies an additional generation check before obtaining an observation. Its effect has an active flag and clears scheduled work at cleanup. Session rejection is the final data-level protection, not the only lifecycle defense. [Clock design](clock-design.md) details scheduling, visibility and fractional steps.

## Preferences and effects

`Preferences` contains `size` and `bestMoves`. The key is `neon-pairs.preferences.v1`, and the stored object includes `schemaVersion: 1`. A valid size comes from `GRID_SIZES`; each best score uses a canonical size key and a finite safe integer count between that board's pair count and `MOVES_MAX`.

`rememberBest` returns the original preferences when an existing score is less than or equal to the new count. Otherwise it returns a new preferences object and best map. This reference stability means an ordinary clock tick or an equal result need not trigger another preference effect.

`useSavedPreferences` runs `writePreferences` after preference or store changes. Writing is intentionally outside the transition functions. A session activation can decide the new best without synchronously depending on browser storage. The writer catches failures, so the in-memory game remains playable even when persistence fails.

The reader returns a complete fallback on invalid saved content, including an invalid entry within the best map. It does not salvage a subset of scores from a partly invalid preference object. The writer touches only its own key, and the store interface has no removal or enumeration method.

## Focus is a neighboring concern

The board hook owns its remembered index; `Game` coordinates Resume through the board's limited focus ref before calling the controller. Replacement remounts the board through generation, while Resume retains it. Form drafts likewise remain local to controls. [React boundaries](react-boundaries.md) explains those lifetimes without adding presentation state to the session.

## Evidence and investigation

`session.test.ts` covers stale generations, paused activations and ticks, completion through play, completed imports and repeated feedback events. It checks that a final imported unfinished game can record a best, and it checks capped move counts. Its feedback snapshot test uses a JSON fixture.

`storage.test.ts` supplies stores that work and stores whose methods throw. Browser environment tests additionally make acquisition of localStorage fail. This separates the browser integration failure from validation and operation failures in the narrow store interface.

For an incorrect result, inspecting `activateCard` establishes the game transition first. If the game is right but the notice, celebration or preference is wrong, `activateSession` and `replaceGame` are the next places to inspect. If the data is right but an interaction sees old values, the controller's synchronization and commit path become relevant. The directory's separation makes those questions answerable without debugging the entire component tree at once.
