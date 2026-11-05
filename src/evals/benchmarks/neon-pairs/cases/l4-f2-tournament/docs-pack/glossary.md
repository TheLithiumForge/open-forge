# Glossary

This glossary explains the terms used in Neon Pairs, including the differences between player language and identifiers in the source.

The entries describe the current implementation and specification 2.2. Code names are shown literally so that a reader can search for them. A term can be useful in engineering discussion without belonging in the interface; the screen generally uses the shorter vocabulary described in [Voice guide](voice-guide.md).

## Play and the board

**Activation.** An attempt to use a card through the shared game action. A primary click, completed tap, Enter or Space can activate a card. `activateCard(state, index)` decides whether that attempt changes the state. Activation is broader than reveal: activating an already matched card, for example, leaves the game unchanged. Focus movement is a different operation.

**Board.** The square arrangement of card positions for one deal. In `src/game/deck.ts`, the `Board` interface contains `boardCode`, `size`, `seed` and `deck`. In interface discussion, “board” can also mean the visible grid and scroll area.

**Board code.** The text that identifies a starting deal, such as `np1-4-1`. It encodes a generation version, side length and seed. It does not contain moves or remembered progress. Loading a code starts that deal from its initial state. See [Board code format](board-code-format.md) for its accepted syntax and [Reproducibility](reproducibility.md) for its relationship to saved progress.

**Card.** One position in the deck and its corresponding native button. Two different cards can show the same face. A row and column identify a position; a face name identifies artwork shared by two cards.

**Closed.** A card whose face is not shown. It is also the literal selection kind `closed`, which means there is no currently revealed unmatched selection. Matched cards may remain face up when the selection is closed.

**Completion / win.** The state in which every position is matched. `isWon(state)` derives this by comparing `matched.length` with `deck.length`; there is no separately stored victory field in `GameState`. The interface displays the final moves and time. See [Win conditions](win-conditions.md).

**Deal.** The initial card arrangement determined by a board code. Restart repeats a deal, whereas New Game creates a fresh seed. The same deal can be played repeatedly with different results.

**Deck.** The row-order array of integer face IDs. `generateBoard` begins with two copies of each included face, shuffles them and returns the frozen board description. Positions remain stable as cards reveal and match.

**Eligible card.** A card that can supply the next reveal under the current game state. It excludes matched cards, repeating the first selection and attempts during blocked intervals. This is explanatory language, not a named TypeScript type.

**Face.** The identity depicted on a revealed card. The integer ID selects a shape, marker direction and fill. Color follows the marker distinction rather than adding another independent identity. Face IDs run from 0 through 127 across the complete artwork catalogue. A smaller board uses only the required initial range of faces. See [Face catalogue](face-catalogue.md).

**Face up.** Revealed to the player, either temporarily as part of a selection or permanently as a matched card. `faceUpCards` combines the matched positions with the currently selected positions. It is a presentation set derived from game state, not another persisted list.

**Fill.** The solid or outline treatment of the main shape. This distinguishes faces that otherwise share a shape and marker direction. In generated labels, the word is `solid` or `outline`. The card border and focus outline are separate features.

**Grid size / size.** The square board's side length. `GRID_SIZES` contains 4, 6, 8, 10, 12, 14 and 16. The select displays dimensions such as `6x6`; the state stores the number 6. Increasing size increases the card count.

**Index / position.** A zero-based location in the row-order deck. Player-facing row and column names begin at one. The first card is index 0 and row 1, column 1. [Numbers style](numbers-style.md) gives conversion examples.

**Marker.** The small circular mark positioned around the central shape. The directions are top, top-right, right, bottom-right, bottom, bottom-left, left and top-left. The matched-card check mark is a separate state indication.

**Match / matched pair.** Two distinct positions with equal face IDs that have been successfully revealed together. `matched` stores their positions, two entries per pair. Matched cards retain their faces and gain a border and check mark. Six matched positions mean three pairs.

**Mismatch / miss.** A turn whose second eligible card has a different face from the first. The selection becomes `mismatch`, with `first`, `second` and `remainingMs`. Both cards stay visible for the pending interval before closing together. “No match” is the interface wording; mismatch is the useful state-machine term. It is an ordinary result.

**Move.** One completed two-card choice, counted on the second eligible reveal. Both matches and misses count. A first reveal alone, repeated activation of that first card and ignored activation do not add moves. The counter saturates at `MOVES_MAX`, while play continues. [Scoring and moves](scoring-and-moves.md) explains the counting examples.

**Pair.** Two cards carrying one face identity. In the progress display, a matched pair contributes one toward the board's total. There are half as many pairs as cards. “Matched pair” specifies that the pair has been found.

**Selection.** The currently revealed unmatched portion of a turn. `Selection` is a union of `closed`, `one` and `mismatch`. The `one` branch contains `index`; the mismatch branch contains two indices and remaining active time. Already matched cards are tracked separately. [Selection state](selection-state.md) describes the transitions.

**Shape.** The central geometry in a face: circle, square, triangle, diamond, star, hexagon, plus or chevron. The neutral diamond on the back of a closed card is back artwork, not evidence that the hidden face has the diamond identity.

**Turn.** The two-card choice that starts with a first eligible reveal and resolves when a different eligible card is revealed. On a miss, the visible interval continues after that result. “Turn” is useful explanatory language; the state does not store a turn-number field separate from `moves` and `selection`.

## Session, time and records

**Active time.** Time that contributes to the running game while the page is shown and unfinished imported progress is not paused. The elapsed clock starts on the first valid reveal and stops at completion. The pending mismatch uses that same active-time basis. It is not simply the amount of wall time since opening the page.

**Best.** The lowest completed move count recorded locally for a particular board size. It is not a fastest-time record, a per-seed record or a remotely verified result. The status shows `none` before a size has a record. [Best score records](best-score-records.md) explains the record's scope.

**Celebration.** A session flag controlling the completion banner's pulse after play finishes the board. It is distinct from `isWon`: completion describes the game's data, while celebration describes a presentation event. That distinction lets an already completed position be displayed without implying that the player just made its final turn.

**Elapsed time.** The game's accumulated active milliseconds, stored in `elapsedMs`. The screen formats this value for reading. A browser rendering tick is not itself a unit of elapsed time. The measured duration can contain fractions in memory even though snapshot export writes whole milliseconds.

**Generation.** A session counter used to distinguish successive game replacements. `replaceGame` increments it. Timer updates carry a generation so that an update belonging to an earlier game can be ignored. It is unrelated to the generation-version prefix in a board code: one identifies a browser session replacement, the other identifies a deck algorithm.

**Pause / paused.** The session condition applied when unfinished progress is loaded from a snapshot. The Resume button clears it. Page visibility separately controls active time. `paused` belongs to `Session`, not the serialized core `GameState`.

**Preferences.** The last supported size and the local best-move map. The browser storage module reads and writes them under `neon-pairs.preferences.v1`. Reload uses the remembered size for a fresh deal.

**Remaining time.** The `remainingMs` field of a pending mismatch. It measures how much more active time the two unmatched faces stay visible. It does not mean time left to finish the game, and it is absent from the other selection branches.

**Session.** The browser-facing state that wraps `game` with pause state, generation, feedback, load notice, announcement counter, celebration and preferences. `src/game/session/session.ts` defines it. See [Session module](session-module.md) for the boundary.

**Started.** The `GameState` boolean indicating that the first valid reveal has happened. A started game may be paused, hidden or complete.

## Exchange and diagnostics

**Announcement.** Accessible result feedback supplied through the status region. It describes turn outcomes and load statuses. In source, `announcement` is also a session counter used to distinguish result events, including repeated identical text. The game itself does not play speech audio.

**Draft.** The text currently being edited in the Board code input or Snapshot JSON textarea. The corresponding load action validates it before replacement. Invalid drafts remain available for correction.

**Export.** Encoding current progress and placing the JSON text in the snapshot textarea. `encodeSnapshot` supplies the serialization. The interface lets the player select and copy it manually. “Export” here does not imply a downloaded file or a clipboard notification.

**Import / load.** Validating supplied text and, on success, replacing the active game with the described deal or progress. The UI labels are Load board and Load snapshot. Engineering prose also calls restoring external progress an import.

**Logical time.** The explicit timeline used by the game core and replay. `advanceGame` receives a duration rather than consulting a browser clock. Replay action timestamps describe this timeline; they do not encode a calendar or the period during which a browser page was hidden.

**Replay.** A versioned description of one board's timed activations. Its fields include `actions` and `endMs`. The CLI runs those actions through the same game transitions and prints the resulting snapshot. Replay has no control in the current game screen.

**Seed.** The positive integer component of a board code that initializes deterministic deck generation. Equal supported size, version and seed yield the same deck. The seed identifies the starting arrangement, not the player's later choices or result.

**Snapshot.** A versioned JSON description of progress, containing `boardCode`, matched positions, selection, moves, elapsed time and started state. It regenerates the deck from the code. It does not store every past action. [Snapshot format](snapshot-format.md) describes validation, and [Sharing FAQ](sharing-faq.md) helps choose between a code, snapshot and replay.

**Validation.** Checking unknown input against the relevant format and state constraints before using it. It checks acceptable state, not an authenticated play history.

**Version.** A label whose subject needs to be explicit. Specification 2.2, package version `1.0.0`, snapshot `version: 1`, replay `version: 1` and the `np1` board-code prefix refer to different artifacts. A change in one does not automatically change the others. [Format versioning guide](format-versioning-guide.md) discusses the data-format side.
