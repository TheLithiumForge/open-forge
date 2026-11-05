# ADR 023: Present rankings as ordered lists

This historical record explains the retired ordered-list presentation for rankings and results.

Status: Superseded by [Tables policy](tables-policy.md)

## Original context

The early results sketch emphasized a phone-sized page. A sequence of entries could be stacked vertically, with the name and result together and little need for horizontal movement. At that point, the team chose ordered lists for rankings and results because they read well on phones.

This was a presentation decision in the original design discussion; the current game has no ranking screen. The current playable implementation shows its own move count, pair progress, time and per-size best.

## Decision as made

Each entry would have been one list row, with ordering carrying the ranking meaning. Supporting text could sit under the primary name without creating a wide grid. The list's native numbering appeared to provide a useful relationship between visual position and reading order.

The design favored following one entry from top to bottom over scanning one metric down a column. That was a reasonable priority while the imagined result was a short linear reading task. It also made the narrow-screen prototype straightforward to build using familiar block layout.

## Reasons that supported the choice

An ordered list can express a sequence without an additional position field in every visible row. It adapts naturally to wrapping text, and native list semantics are familiar to accessibility tools. With few fields, a list gives each entry enough vertical space without forcing a compact row height.

The team also wanted to avoid a horizontal scrolling area outside the game board. A stacked entry appeared to solve that on small screens and kept the imagined results section visually close to ordinary prose. The old decision therefore came from a concrete reading concern, not from treating a list as a universally better data structure.

## Alternatives discussed

A tabular layout was more structured and better suited to comparisons across repeated fields. In the earliest sketch, that benefit seemed less urgent than fitting an entire entry into a phone width. Cards for individual results were another option, but they would have added visual framing while leaving the same question of how to compare one field across entries.

Plain paragraphs were rejected as too weak a representation of order. A manually typed rank inside every paragraph would have reproduced part of an ordered list without its structural meaning. The initial list choice was therefore internally coherent for the information the sketch expected to emphasize.

## Why this is historical

The weakness of a stacked list becomes apparent when readers want to compare the same metric across several entries. Repeated labels take space, and values no longer align in a way that supports scanning. The original rationale optimized the reading of an individual result and gave less weight to cross-entry relationships.

[Tables policy](tables-policy.md) is the replacement decision. The relevant historical lesson is that narrow-screen layout alone was too small a basis for choosing a representation of comparative data. The information's relationships also matter.

## Connection to the current code

`GameStatus.tsx` uses a definition list for the current game's labeled statistics and a paragraph for completion. That is a different information shape from a ranked collection of participants. It should not be interpreted as an implementation of this old ordered-list decision or as evidence against the replacement page.

[Status bar](status-bar.md), [Tables policy](tables-policy.md) and [Interface review checklist](interface-review-checklist.md) are useful companions when reviewing actual screens. The superseding link at the top is the authority boundary: this historical rationale can explain a prototype, but it does not direct a new results presentation.
