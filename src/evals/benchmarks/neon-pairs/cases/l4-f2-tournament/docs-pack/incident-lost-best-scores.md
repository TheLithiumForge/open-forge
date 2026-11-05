# Incident review: a best score disappeared

This is an incident review about a preference-write bug that replaced saved best moves while starting another deal.

The current code separates deal replacement from updates to saved best moves.

## What the player saw

The report was short: finish a four-column board, choose a larger size, return to the original size, and the Best display says `none`. Reloading did not bring the result back. The active board was still playable, so the problem looked like a minor label issue until the stored JSON was inspected.

In the reproduction, the first completion wrote a valid score for size 4. A later size change saved a preference object with the new size but an empty best map. Returning to size 4 therefore showed the value that was actually in storage: no score. The UI had not merely failed to render a record. A later write had replaced the record's container.

The loss was limited to the local preference value on that browser origin. There was no server copy to restore and no account history to compare. A copied snapshot could describe a completed position, but it was not an automatic backup of the preference map. The [persistence overview](persistence-overview.md) explains these distinct data lifetimes.

## The investigation timeline

During the first triage pass, the reviewer suspected a size-key mismatch. Numeric values from the select become strings when used as object keys, and a display could accidentally look under a different spelling. That hypothesis was attractive because changing size appeared to trigger the symptom. Reading storage showed the old key was absent rather than misspelled.

The next pass compared preference writes before and after New Game. The faulty branch created a new default preference object while creating the fresh board, then saved that object as if it were the existing preference state. It preserved the selected size and forgot the accumulated records.

The reproduction became clearer when two sizes had records. A new record for one size could erase the other, which disproved the narrower idea that only returning to the default size was broken. The final check compared the complete serialized object across a new deal, a restart, and a size change. Those operations replaced game progress but had no reason to recreate the unrelated best map.

The number of affected users is unknown because the game has no telemetry or server data. The overwritten local value had no recovery source in the product.

## Root cause: one reset had two meanings

The faulty design treated “new game state” as permission to replace every nearby value. Board progress and preferences happened to live in one controller, so a reset helper made them both look disposable. The product's meaning was different: matched positions, selection, moves, and elapsed time belong to an attempt; preferred size and accumulated bests belong across attempts.

The current source exposes this boundary in `replaceGame`. It replaces the game, clears imported-pause and celebration-related state as appropriate, advances the session generation, and constructs preferences with the existing object spread plus `size: game.size`. The existing `bestMoves` reference is retained unless another operation actually changes it.

The win path is separate. `activateSession` calls `rememberBest` only after an activation produces a won game. `rememberBest` looks up the previous value for that size. It returns the original preference object for an equal or worse result and copies the map only for an improvement. There is no reset of all sizes as a side effect of improving one.

This is more than an optimization based on reference equality. The identity boundary makes it visible which operations change persistent meaning. `useSavedPreferences` writes when its preference dependency changes. Ordinary timer ticks and focus movement have no reason to construct a new best map.

## Why the first tests missed it

The test suite initially played one board to completion and reloaded. That path proved a completion could be saved and read. It did not prove a later unrelated action preserved earlier entries. A storage test that round-tripped a single newly created object also passed because serialization itself was correct.

The missing scenario involved a sequence of valid actions: record size 4, use size 6, then revisit size 4. Another useful sequence was recording a better size-6 score while checking the size-4 value. Both test persistence of relationships rather than merely success of an individual write.

Current `storage.test.ts` checks separate minimum moves by size, equal and worse results, and a better replacement. It also checks blocked storage, malformed values, the move cap, and unrelated storage keys. Browser saved-best and persistence-contract journeys cover the integration with rendered status and session actions.

## A second issue discovered during analysis

The incident discussion surfaced a tempting but incorrect recovery idea: load a completed snapshot and let the usual completion effect recreate the best. Current behavior intentionally treats that as viewing supplied progress, not performing the winning action. A completed import therefore does not establish a new local record.

That distinction prevents an importer from accidentally changing preference history just by inspecting an old or edited completed state. An unfinished import that is resumed and later completed through play can take the normal win path. The [snapshot release notes](release-notes-1-4.md) explain the user-facing result, while [best-score scope](adr-032-best-score-scope.md) covers the decision.

The incident response did not solve lost preferences by changing this import boundary. It corrected the destructive write. That kept the cause and the repair aligned instead of adding a new side effect in a different feature.

## Reading current storage failures accurately

The current `readPreferences` validates the whole stored envelope. The schema version must be 1, size must be supported, and best keys must use canonical supported size spellings. Scores are nonnegative safe integers bounded by the move cap and no lower than the number of pairs for that size. An invalid entry causes fallback to the default size and an empty best map.

That fallback can produce a visually similar `none` result, but it is a different failure from the overwrite. A malformed saved value is rejected at read time; a valid empty map can result from an earlier replacement. Storage access can also throw or be unavailable, in which case the game remains playable without reliable saving.

These distinctions matter during diagnosis. A screenshot of Best alone cannot tell whether a record was never written, overwritten later, rejected on reload, or read from a different origin. The project recommends a stable local HTTP origin for preference checks because file-URL storage varies between browsers. The [storage testing guide](storage-testing-guide.md) covers that environment boundary.

## Outcome and remaining limits

In this incident, the repair preserved the existing preference map across deal replacement and kept record changes in the completion path. Review expanded from “does a best survive reload?” to “does it survive the valid session actions around reload?” That expanded review was the lasting lesson.

The product still does not reconcile several open tabs, synchronize devices, or keep an independent record archive. The specification explicitly leaves multiple-tab overwrite behavior outside its tested scope. Those limits do not excuse losing data during a normal single-session size change, which is why this incident belongs to persistence correctness rather than a request for online features.
