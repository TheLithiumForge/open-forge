# Data exchange policy
This page explains which Neon Pairs data crosses a file or text boundary and how the existing formats relate to future exports.

The game has three exchange formats with deliberately different jobs. A board code identifies a starting deal. A snapshot describes a position on that deal. A replay describes activations on a logical timeline and produces a position when evaluated. Choosing the smallest useful format keeps a report understandable: a wrong shuffle needs a code, a wrong revealed state needs a snapshot, and a timeout ordering problem usually needs a replay.

The detailed contract is [Reproducibility](reproducibility.md). This page describes the boundary between those formats, browser session data and developer tooling. It is not an additional wire-format definition.

## Existing formats and new exports

The current snapshot and replay formats are JSON objects with numeric `version: 1`. The board code is a string whose prefix is `np1`. Their definitions predate the general policy for new JSON exports. Every new exported JSON object has a "format" of "neon-pairs/KIND", naming what it holds, and a whole-number "version", with its keys in alphabetical order at every level. Formats that already exist stay as they are. In particular, adding a discriminator to a version 1 snapshot would make it fail the existing exact-key check, and changing the board-code string into JSON would remove its usefulness in the Board code field.

There is an important distinction between naming a new exchange format and modifying a known one. `snapshot.ts` is still the authority for the fields of an exported snapshot. `replay.ts` is still the authority for a replay input. The general policy does not silently widen either accepted object. See [Format versioning](format-versioning-guide.md) for the compatibility decisions around those boundaries.

Current snapshot output is readable JSON produced by `JSON.stringify` with a two-space indentation argument. It writes fields in the construction order used by `encodeSnapshot`: `version`, `boardCode`, `matched`, `selection`, `moves`, `elapsedMs`, `started`. This order is not alphabetical. An importer accepts any object-key order when the required keys and values are present. The distinction matters when comparing documents: a change in whitespace or object-key order need not change the represented state.

## Choose the artifact by the question

| Question | Useful artifact | What it cannot establish |
| --- | --- | --- |
| Did two runtimes generate the same deal? | Board code | Which cards the player activated |
| Can another browser show this position? | Snapshot | The complete earlier action sequence |
| Does a click at the mismatch boundary get accepted? | Replay | The browser's real scheduling behavior |
| Does a loaded position show the right controls? | Snapshot plus browser steps | How that position was originally reached |
| Did a local best survive reload? | A storage-focused reproduction | An exported snapshot carries no best record |

A board code is short enough to copy into a report without a separate file. Its meaning is stable because the prefix fixes generation. A snapshot contains enough progress to avoid playing back a long game. A replay is the most explicit description of action order, but the browser does not currently record or load replay files through its reproduction panel. Developer tooling evaluates them through the core and CLI.

A screenshot may accompany any of these, but it does not replace their machine-readable information. A picture of two cards cannot tell an engineer the exact pending milliseconds. A board code plus an action sequence can distinguish a bad card layout from a bad state transition. The appropriate artifact depends on whether the disputed behavior is generation, state, time or presentation.

## What the exchange formats carry

A board code carries generation version, supported side length and seed. `generateBoard` derives the immutable deck. The result contains face identifiers in position order; the code does not carry artwork, card names or a language setting. Loading it creates a fresh hidden state with no moves and no elapsed time. The algorithm and literal example are documented in [Board code format](board-code-format.md).

A snapshot carries `boardCode`, matched position indices, a tagged selection, moves, elapsed milliseconds and the `started` flag, alongside its format version. It does not duplicate the deck. Pair count and completion are derived after loading. Omitting those derived values avoids accepting a record in which the supplied pair total disagrees with the matched positions. [Snapshot format](snapshot-format.md) gives the complete field and validation reference.

A replay carries `boardCode`, an ordered array of `{atMs, index}` actions and `endMs`, alongside its version. It starts from the hidden deal rather than from an imported snapshot. The action timeline advances game time before each activation. The final time is advanced even when no action occurs there. This is how a replay can describe a mismatch closing after the final click. [Replay format](replay-format.md) covers the timeline and diagnostic result.

None of these objects contains a browser storage key, a saved size preference, a per-size best record, a focus location or an animation flag. Those values have different lifetimes and authorities. A loaded board's size becomes the chosen size through session replacement; that is a browser consequence of loading, not a separate field in the artifact. A completed import is displayed without recording an import as a new played result.

## Browser text is an explicit boundary

The reproduction panel is a native `details` element with the summary “Reproduce a game”. It contains the Board code input and Snapshot JSON textarea. Editing either field only edits a draft. `Load board` or `Load snapshot` performs the actual load. This separation lets a player inspect and correct copied text without replacing a running game on each keystroke.

`Export snapshot` fills the textarea with a point-in-time representation. The player can select and copy that text manually. There is no dependency on the Clipboard API, no upload step and no account exchange. Export does not put the live session into the imported pause state. If the source game keeps running while the text is copied, the later source position may differ from the exported one.

The Board code control permits 32 characters and trims surrounding whitespace in `useGame.loadBoard`. The parser used inside JSON does not perform that trim. The Snapshot JSON control permits 65536 characters, while decoding separately enforces 65536 UTF-8 bytes. These are different measurements. A text field limit is an input affordance; the decoder remains the format boundary for programmatically supplied text too.

Successful load resets the board's remembered tab stop to its first position through a new session generation. Load and export buttons retain focus, with Resume handling its own disappearing-button focus transition. Focus is not serialized, so sharing a position cannot restore where the sender last tabbed. See [Reproduction panel](reproduction-panel.md) and [Focus policy](focus-policy.md) for interface details.

## Core functions and effectful adapters

The pure format functions operate on strings and game data. `parseBoardCode` returns size and seed or `null`; `decodeSnapshot` returns a reconstructed state or `null`; `readReplay` returns a final state or a classified problem. They do not operate browser controls, write storage or read files.

The browser adapter in `session/use-game.ts` decides when accepted data replaces a session. The CLI adapter in `scripts/replay-cli.ts` reads UTF-8 text and converts accepted replay results through `encodeSnapshot`. Pasting that output into the browser adds normal snapshot import behavior, including a pause for unfinished progress. The replay evaluator itself has no pause field.

## Trust and interpretation

All copied or loaded data is untrusted. “Valid snapshot” means that the object satisfies the documented state constraints, not that its claimed result has a verified play history. A hand-edited object can be accepted if its fields remain consistent. That is useful for debugging and does not provide evidence of a person's performance.

Replays give a more explicit history, but they still contain author-supplied actions and times. The evaluator applies normal rules, including ignored activations, and does not attest who supplied the file. There is no authentication or signed result in the current exchange model. Local best moves are local game records, not a remote ranking service.

A board code also reveals enough information for a developer to regenerate the whole deck. That is its purpose. The browser still keeps hidden face identity out of hidden-card DOM content. Those facts are compatible: the shareable deterministic deal is a debugging tool, while the ordinary board presents the information available during play. [Import safety](import-safety-policy.md) describes validation and replacement behavior in more detail.

## Following a reported disagreement

For a generation mismatch, begin in `src/game/deck.ts` and compare the literal vector. For a rejected position, follow `snapshot.ts` into `snapshot-selection.ts` and `saved-format.ts`. For an unexpected action result, follow `replay.ts` into `advanceGame` and `activateCard` in `game.ts`.

A useful report preserves the original artifact and says which boundary was exercised. “The CLI rejected this replay” and “the browser rejected the CLI's output” refer to different decoders. Keeping that distinction often removes an entire round of reproduction work.
