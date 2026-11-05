# Storage quota notes

These notes explain the size of Neon Pairs' preference data, the browser limits relevant to it and the limits of the available test evidence.

The current preference payload is tiny. A full map containing all supported sizes at the maximum accepted move count, with selected size 16, serializes to 124 ASCII characters. That is 124 UTF-8 bytes for the value, excluding its storage key and any browser accounting overhead. Capacity failures are still possible because availability depends on the browser and the origin, not just this application's own string.

## Sizes measured from the current representation

These counts use the exact compact representation emitted by `writePreferences`, with `schemaVersion` first, then `size`, then `bestMoves`. They measure serialized text, not an estimate of on-disk browser database use.

| Preference contents | Value length in ASCII characters and UTF-8 bytes |
| --- | ---: |
| Selected size 4, empty best map | 43 |
| Selected size 16, empty best map | 44 |
| Selected size 4, all seven bests at 999999 | 123 |
| Selected size 16, all seven bests at 999999 | 124 |

The full example is:

```json
{"schemaVersion":1,"size":16,"bestMoves":{"4":999999,"6":999999,"8":999999,"10":999999,"12":999999,"14":999999,"16":999999}}
```

This is a largest ordinary value by text length under the validated preference shape. Selected sizes have at most two digits. There are seven canonical score keys, and accepted scores have at most six digits. Changing map insertion order changes the order of properties, not the total length. Extra fields in an externally edited string can make the input larger, but they are not retained in the reader's returned preference value.

These figures do not include a feature's collection of records. Nor do the snapshot and replay input byte limits describe the space occupied by preferences. Those formats are separately bounded interchange text; the browser preference writer does not save either one. [Persistence envelope](persistence-envelope.md) separates the representations.

## Browser capacity background

MDN's “Storage quotas and eviction criteria” gives Web Storage a nominal per-origin allowance of 5 MiB for local storage and a separate 5 MiB for session storage. Exhaustion can cause `setItem` to throw `QuotaExceededError`. This is background guidance, not a measured capacity result from the Neon Pairs suite or a promise of usable space in every browser context.

The much larger disk-percentage quotas discussed for other browser storage mechanisms are not a useful capacity target for this writer. The app calls `localStorage.setItem`; it does not store its preferences in IndexedDB, Cache Storage or a file-system API. WebKit's “Updates to Storage Policy” also describes storage pressure and removal of website data, reinforcing that a browser controls lifetime as well as capacity.

A nominal allowance is not a reservation for Neon Pairs. Browser policy can prevent writes from the beginning. Other values on the same origin can consume capacity. Private browsing can give data a shorter lifetime even if writing works. A small serialized value therefore supports the claim that the application contributes little storage, but does not prove a future write will succeed.

## Observations in the test suite

The source and tests provide several concrete observations, with different scopes:

| Case | Evidence | What it establishes |
| --- | --- | --- |
| Ordinary saved bests and sizes | HTTP-origin persistence browser journeys | Valid preference data survives reload in that context |
| Malformed stored text | Initialization with `{` | The app falls back to a playable default board |
| Denied storage property | Throwing `window.localStorage` getter | Matching still works when store acquisition fails |
| Throwing read and write methods | Unit store double | The value API returns defaults or suppresses the exception |
| Unrelated stored key | Map-backed unit store | A preference write leaves that key alone |

There is no test that fills a physical browser storage area to its exact capacity. There is no recorded capacity sweep for Firefox or Safari in the test suite. The README reports Chromium journeys on its named compatibility combination; that is functional evidence for the game, not a general quota benchmark. An engineer should not attach a browser-specific measured threshold to these results.

## How to investigate a suspected capacity failure

A focused reproduction can begin with a readable valid baseline and a store double that rejects writes. Finish a better game and observe the new in-memory best, then read the unchanged baseline from the store. This isolates the application's failed-write behavior without depending on how many megabytes a particular browser permits. [Storage testing guide](storage-testing-guide.md) gives examples of operation-specific doubles.

If the report specifically concerns real browser capacity, an isolated profile and stable HTTP origin make the experiment interpretable. Record the browser build, browser mode, origin, keys created by the experiment and the exception from the attempted update. Counting JavaScript string characters alone does not establish the browser's accounting method. The key, encoding and internal representation can all make that count differ from physical disk usage.

A test that fills a different port has not filled the game's origin. A test that navigates away and accidentally starts a different server port may appear to recover simply because it has entered an empty storage area. The artifact server chooses a private port; comparisons across independent runs need to account for that.

## Replacement versus accumulation

The writer replaces one aggregate under one key. It does not create a new key for every game or append an active-game snapshot on each timer tick. Repeated play can change numeric bests, but the set of supported size keys is bounded. Large boards do not cause correspondingly large preference objects because the deck and matched positions stay outside this save.

This explains why adding more storage machinery would not address the usual preference questions. A report of missing data is more likely to need origin, validation, browser policy and lifecycle evidence than a larger quota. That is a diagnostic priority, not a claim that actual quota errors are impossible.

The current code has no storage estimation call, persistence-permission request or cleanup pass to make room. Its response to a rejected write is to preserve the active session. The app also does not delete another origin user's keys as a recovery measure. [Storage failure policy](storage-failure-policy.md) follows those catches and their visible consequences.
