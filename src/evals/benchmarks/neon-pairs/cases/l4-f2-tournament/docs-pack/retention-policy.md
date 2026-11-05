# Retention policy

This page explains retention for saved feature lists and contrasts it with the compact best-score aggregate in the current game.

Retention describes how much accepted data remains after a save. It is separate from parsing a malformed value, clearing site data in browser settings or losing a private browsing session. The game currently remembers one best move count per supported size rather than preserving individual completed attempts. A browser dump of those bests cannot reconstruct the order or number of games played.

## Saved lists

A list can grow as a feature saves additional records, even when every individual record is small. Its retention boundary applies during saving, rather than acting as an expiration clock while the player is idle. A stored list keeps its newest 10 records and drops older ones when a new record is saved. This bounds the retained collection while leaving the active interaction independent of an ever-growing history.

This rule concerns a saved list. It does not turn the existing `bestMoves` object into a history or add a timestamp to it. Record types, sort fields and tie-breaking conventions are separate from the amount retained. The preference store implements the best-score aggregate rather than a feature list.

The list representation is discussed in [Persistence envelope](persistence-envelope.md). Parsing and validation failures are covered in [Corrupt data policy](corrupt-data-policy.md), because retention of accepted records and recovery from rejected input are different operations.

## The current aggregate retains outcomes, not attempts

`Preferences.bestMoves` is a `Readonly<Record<string, number>>`. The key is the canonical decimal side length, such as `4` or `12`, and the value is the best completed move count for that size. The selected board size lives separately in `Preferences.size`. Selecting another size does not discard the previous size's best.

The supported set has seven entries. Once read validation has succeeded, the score map can therefore contain at most seven different supported size keys. It can also contain fewer, including none. This bound follows from `GRID_SIZES` and canonical key validation, not from trimming an attempt history. Multiple completed games at the same size compete for the same map entry.

For example, suppose a browser has a 4x4 best of 10. Finishing later in 9 moves replaces that one number. Finishing in 11 moves leaves 9 in place. Finishing in 9 moves again also leaves it in place. Those outcomes do not append three records; there is still only one 4x4 entry. [Best score records](best-score-records.md) explains the completion rule in gameplay terms.

The selected size can change while all best entries remain identical. A visit that chooses 16x16 and then reloads may store `size: 16` alongside a score map containing only a 4x4 result. That is a valid aggregate. The remembered preference describes the next board size, while the map describes earlier eligible completions.

## Why no clock drives current retention

The preference format contains no creation date, last-played time or expiry value. `readPreferences` does not inspect the current time. It cannot tell whether a valid best was written a minute ago or on a much earlier visit. There is no scheduled cleanup effect, idle purge or age-based deletion in `use-saved-preferences.ts`.

This makes the data easy to explain: a valid best remains available across normal reloads until another relevant event changes the storage or the value is rejected. A better result can replace it. Browser policy or a person's site-data action can remove it. Another tab can overwrite the aggregate. None of those is an application age policy.

The distinction matters when comparing evidence from two visits. A gap between visits does not by itself identify the cause of a missing best. If the origin changed, the second visit may have read another storage area. If the value was edited into an invalid shape, the reader may have used its fallback. If the browser cleared data, the key may simply be absent. [Storage quota notes](storage-quota-notes.md) discusses environment-driven loss without treating it as list pruning.

## Save time is an application event

The preference writer runs from a React effect whose dependencies are the preferences object and store. This is not a page-exit flush. On initial mounting, the effect writes the current preference state. Later, a new preference object causes another effect run. There is no dependency on active elapsed milliseconds or every move in the game.

`replaceGame` always constructs a preference object with the new game's size. New Game and Restart consequently pass through a save-capable path even when the size value stays the same. Valid board-code and snapshot loads do so as well. Invalid loads update feedback without replacing preferences.

`rememberBest` returns the original object for equal or worse results. The session can still render a completed game, but the preference identity need not change. This keeps the storage effect aligned with meaningful preference updates in that path. It is not a universal deduplication layer: replacement actions can still write the same serialized preference contents again.

For list work, the save event named by the retention rule should be understood in the feature's own operation. The existing preference effect has no generic event log or timer that scans all browser keys. There is no central list manager in this source tree.

## Retained locally does not mean backed up

The app has no account or service that copies the aggregate elsewhere. A best kept in one profile remains local to that browser's storage context. Opening the same HTML in another browser does not fetch it. Exporting a snapshot copies game progress, not a backup of the browser's accumulated bests or selected-size history.

The UI has no general preference export, browser-data restore or clear-records control. A developer can inspect or change storage manually, but those are debugging actions rather than product controls.

## Retention and capacity are related, but different

A bounded collection makes its record count predictable. It does not guarantee every write succeeds: browser access can be blocked before capacity matters, and another application on the same origin may consume storage. A small value can fail to write even though its own size is well below a browser's nominal limit.

Conversely, a successful write does not prove indefinite lifetime. The browser can later clear data, a private context can end, or the person can remove site storage. Those environmental conditions are outside the saved-list retention operation. [Private browsing notes](private-browsing-notes.md) describes how a value can survive a reload yet disappear when the browsing session ends.

The current score aggregate illustrates the same separation at a smaller scale. It has a naturally small set of keys and bounded integer values, yet `setItem` remains inside a `try` block. Capacity savings reduce the amount being written; they do not change the need to continue play if the browser refuses it.

## Checking retention with feature records

The existing test suite establishes preference behavior rather than a list API. `storage.test.ts` checks independent minimums for two sizes and object identity for equal and worse results. Browser journeys check that completed bests survive reloads and game resets. None of those tests asserts a sequence of saved feature records.

For a feature-list review, concrete before-and-after examples can expose the retention boundary without adding fields to the product format. Records can be identified by the already defined domain representation in that feature's tests. The useful observation is which accepted records are present after a save, compared with the preceding accepted collection. Examples can use the feature's existing record fields.

It is also useful to keep failed writes separate from successful retention checks. A denied store can leave the feature's in-memory state ahead of the browser value. Inspecting the old browser string then proves only that the write failed, not that the retention calculation chose the wrong records. [Storage testing guide](storage-testing-guide.md) describes the store doubles that make this separation observable.

## What bests retain

The best map cannot answer how many attempts occurred, which deal produced a best, how long that attempt took, or whether two equal results happened on different days. None of those fields is retained. A new feature that needs a collection has a different persistence concern from calculating a size-specific minimum.

Keeping that distinction explicit avoids a misleading implementation shortcut: interpreting every best-map update as if it were an append to a historical list. The code's result is an aggregate with replacement semantics. The saved-list rule belongs to collections, while the current aggregate remains documented and tested according to its own source behavior.
