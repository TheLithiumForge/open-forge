# Sharing FAQ
This page answers practical questions about copying deals, saving progress and reproducing game behavior in Neon Pairs.

The browser tools are inside “Reproduce a game”, below the main game controls. They exchange text locally. You can copy a board code or snapshot into whatever channel you already use; the game does not send it to another player for you.

## What should I send someone?

Send the board code when you want them to begin with the same cards. Send a snapshot when you want them to see your current matches and revealed cards. For an engineer investigating action timing, a replay is often more useful because it includes the order and logical times of activations.

A code is not a saved game. Loading it starts the hidden deal with zero moves and elapsed time. A snapshot is not a recording: it does not say which earlier misses led to the position. The three roles are compared in [Data exchange policy](data-exchange-policy.md).

## How do I copy my current position?

1. Open Reproduce a game.
2. Select Export snapshot.
3. Select the text in Snapshot JSON and copy it manually.

The exported text is a point-in-time copy. Your running game can continue to advance afterward. If another person loads the copy later, they receive that saved position rather than a live connection to your browser. Export uses the current active-clock reading, so it does not depend on waiting for the next visible clock update.

Export fills the textarea; it does not automatically copy to the clipboard or download a file. You can save the copied text outside the game if you want to keep it across reloads. The game remembers chosen size and local bests, but does not automatically save active progress on reload.

## How does the recipient load it?

Paste the full JSON text into Snapshot JSON and select Load snapshot. Unfinished progress loads paused. Select Resume when ready to continue. Matches, the first revealed card or a pending mismatch are restored from the text, along with moves and active elapsed time.

A pending mismatch continues with its saved remaining interval after Resume. It does not start a new 800ms interval just because it crossed browsers. A completed snapshot loads completed without a Resume button. [Snapshot format](snapshot-format.md) explains both the saved fields and the browser import lifecycle.

## Why does a fresh-looking snapshot still show Resume?

The browser pauses every unfinished imported snapshot, including an untouched initial state. This is independent of whether the elapsed clock has started. Resume removes the import pause; the clock still waits for the first valid reveal if `started` was false in the snapshot.

By comparison, loading a board code starts its fresh deal without an imported-progress pause. Use a board code when a shared starting arrangement is all you need.

## Can I keep editing a code while playing?

Yes. The Board code input is a draft until Load board is submitted. Revealing cards does not overwrite that draft. A new deal, Restart or successful load replaces the displayed code with the current board's code.

The Snapshot JSON textarea is also a draft. Typing there does not import on every keystroke. Export snapshot replaces its text with a new export, so copy any draft you want to retain before using that button. See [Reproduction panel](reproduction-panel.md) for control details.

## Why does a code work in the input but fail inside JSON?

The Board code control trims surrounding whitespace before parsing. A `boardCode` string inside a snapshot or replay goes straight to the strict parser. Spaces inside that JSON string are part of the code and make it invalid.

Other common mistakes are a zero seed, leading zeros, unsupported side lengths and using an application version as the prefix. The current form is `np1-N-seed`. For example, `np1-4-1` is valid. [Board code format](board-code-format.md) includes accepted and rejected examples.

## What happens when copied text is invalid?

The browser keeps your current game and the draft you entered. It shows `Invalid board code.` or `Invalid snapshot.` beside the reproduction controls. The active game can continue while you correct the draft; a failed load does not pause it.

Check that you copied the entire JSON object, including closing punctuation, and that the board code has not been altered. A valid JSON object can still describe an invalid game position, such as a single matched card without its partner. The current snapshot notice does not identify the failing field. [Import safety](import-safety-policy.md) describes the validation boundary and diagnostic policy.

## Why can a short-looking snapshot exceed the limit?

The textarea has a 65536-character limit, and the decoder separately limits the full text to 65536 UTF-8 bytes. Characters outside ASCII can take more than one byte. Indentation and trailing whitespace count too.

Normal game exports contain a small, fixed set of fields and bounded position arrays. If manually edited text is rejected near the boundary, character count alone does not establish that it fits. The byte measurement and field constraints are documented in [Snapshot format](snapshot-format.md).

## Does sharing change either person's best record?

Sending text has no effect on the recipient's browser until they load it. A snapshot contains no per-size best records. Loading a completed position displays its result without treating that load as a played winning turn.

If someone resumes an unfinished import and later completes it through play, normal local completion handling applies. The record is associated with that browser's size category. Sharing does not merge the sender's preferences or score history into the receiver's storage. See [Best score records](best-score-records.md).

## Can I resume on another device?

Yes, by explicitly transferring a snapshot. Local browser storage does not move active progress between devices. A board code also works across devices, but it only restores the starting arrangement.

Focus location is not part of the snapshot. After a successful load the board's remembered tab stop starts at its first card; you can move focus while the imported position is paused. Resume returns focus to the board's remembered position before its button disappears. See [Focus policy](focus-policy.md).

## Why is the exported time a little different from internal time?

Live state can track fractional milliseconds. Snapshot text uses whole milliseconds: elapsed time is rounded down, and a pending mismatch's remaining time is rounded up. The latter keeps a positive interval pending even when less than one millisecond remains.

This does not add or remove a full turn. It is the representation boundary between a precise live clock and the accepted integer snapshot fields. Repeated text comparisons should also account for numerically sorted matched positions; match discovery order is not preserved by export.

## Where are the replay controls?

There are no replay controls in the version 1 game UI. Replays are evaluated by the game core and developer CLI. Run `npm run --silent replay -- PATH` in a configured checkout to print the final snapshot for a replay file.

A successful command exits 0 and prints snapshot JSON to stdout. Failure exits nonzero, writes a reason to stderr and leaves stdout empty. Paste successful output into Snapshot JSON to inspect the final position in the browser. That import has the same unfinished-progress pause as any other snapshot. [Replay format](replay-format.md) gives complete examples.

## Can a snapshot prove that someone achieved a result?

No. These formats support reproduction, and consistent snapshots can be written by hand. Validation checks board positions, pairs, counters and selection constraints without reconstructing every earlier turn. Replays supply explicit actions and times, but those are also supplied by their author.

The game has no account or trusted result service attached to these artifacts. For a bug report, reproducibility is the useful property: the same code identifies the same deck, and the same valid replay gives the same core transitions.

## What makes a useful sharing bug report?

Include the original code, snapshot or replay text and say how you used it. State whether the issue happened on load, after Resume, after a card activation or during the CLI conversion. An exact remaining interval is more useful than saying the cards closed “too soon”.

For a visual issue, a screenshot can accompany the artifact. For a timing issue, preserve the original action order rather than rearranging equal-time actions. Also distinguish the product specification version from the format version: the current specification is 2.2 while these exchange formats remain version 1. [Format versioning](format-versioning-guide.md) explains that distinction.
