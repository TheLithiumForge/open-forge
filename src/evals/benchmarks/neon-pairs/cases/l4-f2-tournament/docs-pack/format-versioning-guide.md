# Format versioning guide
This page explains which versions Neon Pairs exposes and how to assess compatibility when its exchange behavior changes.

The current product specification is version 2.2. Board generation, snapshots and replays still use their original format versions. A product release and an exchange format answer different questions: the release says which application behavior was delivered; the format says how an artifact is interpreted. The package's `1.0.0` version is another independent value and does not select a decoder.

## The version map today

| Surface | Version marker | Source authority |
| --- | --- | --- |
| Starting deal | `np1` prefix inside the board code | `BOARD_VERSION` in `deck.ts` |
| Progress position | Numeric root `version: 1` | `SNAPSHOT_VERSION` in `snapshot.ts` |
| Action timeline | Numeric root `version: 1` | `REPLAY_VERSION` in `replay.ts` |
| Product behavior | Specification status 2.2 | Existing game specification |

The JSON version does not absorb the board generation version. A snapshot is version 1 and contains a separately parsed board code. A replay has the same layering. Its action semantics and its starting arrangement both matter, but they are represented by different fields.

This is why replacing a JSON `version` with a product release string fails. The current readers compare the parsed value directly with their numeric constant. The string `"1"` is also different from number `1`. There is no negotiation, version range or “closest supported version” branch.

## Compatibility follows the reader

An existing snapshot reader accepts exactly seven root fields and three exact selection shapes. An existing replay reader accepts exactly four root fields and two action fields. Adding an optional annotation may look harmless to its writer, but an unchanged reader rejects it. In these formats, an additive field is not automatically a compatible change.

Property order and whitespace are different. Both readers parse JSON before checking key sets, so a reordered object with the same values is accepted. Formatting can still cross the byte limit. An export style change can therefore be semantically neutral for small files and affect acceptance near a size boundary.

Accepted array order depends on the field. A snapshot's matched list is validated by membership and whole-face counts; export sorts it numerically. A replay's action list is chronological data with stable ordering at equal times. Sorting a replay action list to make output look tidy can change its result even if no timestamp changes.

The general policies for future exchange objects and field diagnostics are in [Data exchange policy](data-exchange-policy.md) and [Import safety](import-safety-policy.md). They are separate from the accepted version 1 wire definitions. Reading a policy page does not make the current decoder accept fields absent from its exact-key lists.

## A board prefix fixes more than a seed

The reproducibility specification explicitly ties `np1` to the generation algorithm and says that a generation change requires another version. That includes initial paired-face ordering, Park-Miller constants, rejection sampling and backward shuffle order. A different unbiased shuffle is still a different meaning for the same seed.

An implementation refactor that preserves every generated deck leaves that meaning intact. A change that silently produces different cards for an existing code breaks the artifact even if the code still parses. This is the most direct compatibility promise in the exchange model because a short board code appears inside both JSON formats too.

The current `parseBoardCode` supports one prefix. No older-prefix dispatch table, migration helper or multi-version generation registry exists. Future support cannot be assumed from the fact that the prefix contains a digit. [Board code format](board-code-format.md) documents the actual parser and vectors.

## Semantic changes can matter without a new key

A replay's meaning includes advancing time before activation, preserving equal-time array order and resolving a mismatch at its exact closing boundary. Reversing one of those operations would change a version 1 result without changing its object shape. Comparing field lists alone would miss the compatibility effect.

Snapshot meaning includes the lower move bound, the range of pending mismatch time, and the distinction between unstarted and started-zero-move states. Loosening a check can accept objects an older reader rejects. Tightening one can reject previously accepted files. Either observation is more precise than calling the change “validation cleanup”.

Presentation changes have a different scope. Redrawing a face while preserving its numeric matching identity does not alter the generated deck. Changing a visible load notice does not alter accepted JSON. Those changes may need interface or acceptance evidence, but they do not automatically change the exchange format's interpretation.

## What this code does with unsupported data

Snapshot decode returns `null` for an unsupported version; replay read returns the `format` problem. The browser reports its existing generic snapshot failure, while the CLI explains that the file is not a version 1 replay with the exact root fields. Neither tries a best-effort fallback to another version.

There is also no automatic snapshot upgrade on page load. Active progress is not read from local storage. The user explicitly supplies a snapshot through the reproduction controls, and the current decoder accepts or rejects it. Preferences and per-size bests belong to a separate storage contract described in [Persistence overview](persistence-overview.md).

The CLI's successful output is version 1 snapshot JSON even though its input is a replay. It does not preserve the replay's actions, and it does not stamp the output with the package release. This conversion can be used as evidence that a replay's interpreted final position is stable across an implementation change.

## Reviewing a proposed change

The useful comparison starts with concrete artifacts. A generation change can be checked against the published seed states and deck. A snapshot change can be checked against an initial position, a one-card position, a pending mismatch and a completed state. A replay change can be checked with equal timestamps, an exact timeout action, ignored activations and an endpoint beyond the elapsed cap.

Those are existing semantic boundaries. They reveal different failure modes: parser acceptance, reconstructed state, action order and output serialization. A test that only checks that JSON parses cannot establish compatibility at the latter three levels.

For an old artifact, ask whether the new reader accepts it and whether it represents the same position or actions. For a new artifact, ask whether the old reader accepts it at all. Exact-key formats often give different answers in those two directions. Recording the direction avoids a vague claim that a change is simply “backward compatible”.

The source currently supplies only single-version readers, not a general migration framework. Any future version handling will need concrete design work at the affected format. The existing boundaries remain the starting evidence: [Snapshot format](snapshot-format.md), [Replay format](replay-format.md) and [Reproducibility](reproducibility.md).
