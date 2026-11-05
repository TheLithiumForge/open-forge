# Duplicate names policy

This page explains the group-level name comparison policy and the distinctions a reviewer needs when checking it.

The current source has no name list or group editor. A board code identifies a deal, a card label identifies a position and face state, and a local best is indexed by board size. None of those is a collection of person-entered names. The policy below belongs to a feature that introduces such a collection.

## Comparison boundary

The group being edited supplies the comparison scope; the draft alone cannot establish whether it repeats an existing entry. The candidate's accepted representation comes from [Text entry policy](text-entry-policy.md). Names in one group compare without regard to case, after normalization. A repeat blocks the action and shows "Each name must be different." This lets the same visible spelling with different capitalization be recognized as a repeat without requiring that all displayed names be rewritten in one case.

The scope is one group. Neon Pairs has no global name registry across devices, browser origins or unrelated groups. A local comparison can answer whether an entry repeats within the group being acted on without implying that the person owns a unique account name.

The check concerns names, rather than board positions or face identities. Two cards intentionally share a face, and a replay can intentionally activate the same position more than once. Those repetitions follow their own game rules. A generic “remove duplicates everywhere” helper would not express this policy's subject or scope.

## Display value and comparison value

Case-insensitive comparison does not by itself prescribe changing a name's displayed capitalization. `Mira` and `mira` can compare as the same candidate while the accepted display spelling remains the one chosen by the person who entered it. Showing a lowercased comparison key as the name would conflate two purposes.

A useful implementation trace therefore distinguishes the value displayed, the value accepted into the group and whatever representation is used to compare it. The comparison representation is an internal tool. It is not a new user-visible identifier, a format field or a replacement for the accepted name.

The policy does not specify locale selection or a particular Unicode case-folding API. That matters for names whose case behavior differs across languages or scripts. Name normalization retains diacritics and does not transliterate names. Examples with ordinary Latin letters illustrate the unambiguous case while case behavior across Unicode varies.

The representation step is also not a license to treat all visually similar strings as equal. Latin and Cyrillic lookalikes, differing accents and compatibility characters can resemble one another without being the same processed string. Visual similarity would be a broader identity rule than the policy states.

## Small examples

| Existing accepted name | Candidate | What the example demonstrates |
| --- | --- | --- |
| `Mira` | `Mira` | A direct repeat |
| `Mira` | `mira` | Capitalization does not distinguish the names |
| `Mira` | `MIRA` | The comparison is not limited to the first letter |
| `Ada Lovelace` | The same name after representation processing | The comparison uses accepted name text |
| `Mira` in another group | `Mira` in this group | The stated scope is the group being edited |

These cases are illustrations of the policy rather than a complete Unicode test corpus. They also do not decide length validity; [Name length policy](name-length-policy.md) covers that independent check. A candidate can satisfy length and still repeat an existing name.

A group with several entries can expose a bug that a two-entry example misses. If only the immediately preceding name is checked, a repeat of the first entry can slip through after a different middle entry. The policy speaks about the group, so the relevant evidence is membership across that group rather than adjacency in the rendered form.

## Blocking has a concrete meaning

A repeated candidate does not become an accepted additional name as part of the failed action. The exact feedback above explains why acceptance stopped. Automatically adding a suffix, changing capitalization or deleting one of the existing names would implement a different outcome from the one stated.

This policy does not decide which error wins when several checks fail at once. It also does not add live validation on every keystroke, a suggestion picker or a search across previous games. Those are separate interaction choices, and none is present in the source.

The current reproduction handlers demonstrate an explicit acceptance boundary: parse first, replace only on success, otherwise publish a notice. A name feature can be reviewed with the same question—where does a candidate become accepted?—without reusing the board parser or claiming that load errors are name errors. [Error display guide](error-display-guide.md) provides the current feedback mechanics.

## Evidence a reviewer can assess

Because no name feature exists, the existing tests do not establish this policy. A screenshot containing distinct visible names would be weak evidence even for a future feature: the comparison bug might only appear on submission, on an edit or after a change in capitalization.

Useful examples include an exact repeat, a case-only repeat, a repeat that is not adjacent in the displayed list and a different accepted name. An edit scenario reveals whether the current entry is being confused with another entry. A separate-group example reveals whether an implementation accidentally enforces a broader scope than the policy states.

For a defect report, the accepted group values, candidate value, action and observed message make the comparison reproducible. Display order can be recorded as context, but it does not define identity. The policy stays about equality within the group, and its representation and length dependencies remain in their own linked pages.
