# Incident review: valid snapshots were rejected

This is an incident review about a snapshot reader that confused the exporter’s preferred representation with the set of valid imported states.

The current parser separates encoding choices from decoding constraints in `snapshot.ts`, `snapshot-selection.ts`, and `saved-format.ts`.

## The import was valid JSON and a valid position

The reporter had copied a snapshot into an editor, rearranged its fields for readability, and moved the matched-position list into the order in which the pairs had been found. Loading it produced `Invalid snapshot.` The same position exported by the game could be loaded successfully.

At first, the reviewer suspected a typo. Comparing the two texts showed equal values with different presentation. One object used compact JSON and one used indentation. One listed matched indices in ascending order and the other grouped them by discovery. Neither change violated the present format's semantic constraints.

The report was confusing because the parser correctly rejected many nearby invalid states. It rejected incomplete pairs, mismatches involving equal faces, and out-of-range positions. The problem was an additional assumption imposed on otherwise valid data. The [snapshot-format reference](snapshot-format.md) is the authority for those constraints; an encoder's output is one valid representation of them.

## The faulty shortcut

The first importer decoded a candidate, normalized it through the exporter, and compared the resulting text with the original. The shortcut was meant to catch unsupported data. It instead made whitespace, field ordering, and matched-list ordering part of acceptance.

Canonical output is useful when producing examples or comparing artifacts, but textual equality is much stronger than state validity. JSON object key order does not need to match the order used in a handwritten encoder. The matched list describes a set of solved positions, even though the runtime stores an array. The current exporter sorts that array numerically; the current decoder does not require that incoming order.

An intermediate patch removed the whitespace comparison but retained an ascending-index check. That accepted pretty printing and still rejected a valid whole-pair list in another order. The review finally traced the confusion to a missing distinction: encoding chooses a stable presentation, while decoding checks the stated data contract.

## A small example made the boundary clear

For `np1-4-1`, positions 0 and 11 contain one face, while 2 and 6 contain another. Both of the following matched lists describe the same two solved pairs:

```json
[0, 2, 6, 11]
```

```json
[11, 0, 6, 2]
```

A duplicate such as `[0, 11, 0, 11]` is invalid, and `[0, 2]` is invalid because it contains halves of different pairs. Order is not the property that separates these cases. Uniqueness, range, and complete face counts are.

Current `readMatched` checks each incoming index and counts occurrences of the associated face. Every represented face must occur twice. It preserves the incoming valid order in the returned list. `encodeSnapshot` later sorts a copy when producing readable text. No state mutation is needed merely to present the list consistently.

## Exact keys did not imply exact spelling of the whole text

The present parser requires the exact top-level fields and the exact fields for the selected selection case. It rejects unsupported format versions and unknown data fields rather than silently dropping them. This exactness concerns which properties exist, not the order in which those properties appear in JSON.

`hasExactKeys` compares the number of own keys and verifies presence of each expected key. It does not compare a serialized object string. `parseJson` treats the parsed value as unknown, after which object and numeric checks narrow it. Arrays and `null` are not accepted as top-level objects.

The review considered simply accepting any object with a board code and defaulting missing values. That would have removed the immediate rejection but made corruption harder to detect. A missing `started` flag or selection field changes the meaning of the state. The repair therefore kept the existing format constraints while removing accidental representation constraints.

## State consistency remained substantial

The current decoder rebuilds the board from its canonical code and validates the supplied progress against it. Selected positions are in range and unmatched. A pending mismatch uses distinct positions with different faces and a remaining integer interval from 1 through 800. Moves and elapsed time are nonnegative safe integers within their caps.

The minimum move count includes each matched pair and one additional attempt for a pending miss. An unstarted position has zero moves, zero elapsed time, no matches, and closed selection. A started zero-move position needs a single revealed card. These constraints catch contradictions without demanding a reconstructed history of every past action.

That final distinction prevented the incident response from overcorrecting in the opposite direction. A decoder cannot prove local gameplay history from a snapshot, and the format does not ask it to do so. The [import-safety page](import-safety-policy.md) explains this validation boundary separately from any claim of trustworthy scores.

## The byte limit was another independent layer

The parser checks a 65536 UTF-8-byte limit before parsing JSON. The UI also limits textarea characters, but those are different measures. Most native exports contain only ASCII, so the values often appear interchangeable in ordinary tests. Arbitrary pasted Unicode text can use multiple bytes per character.

In the incident, one reviewer proposed truncating oversized text so it could still be parsed. That would have changed the input and typically produced malformed JSON. The current path instead rejects input outside the stated limit and preserves the draft for correction. It does not silently repair a different document into a position the user did not supply.

The byte limit also applies regardless of whitespace. A valid semantic object padded beyond the limit is rejected because its transport text is too large. That is a stated input boundary, unlike rejecting harmless indentation within the limit. Keeping these explanations separate made the corrected parser easier to review.

## No replacement happened before validation

An import error was frustrating but did not need to destroy the active attempt. Current `useGame.loadSnapshot` decodes first. Only a valid returned state causes a clock reset and `replaceGame`. An invalid candidate publishes the error notice and keeps the game and draft.

That ordering protected players during the investigation. They could retry an edited snapshot without having their existing board cleared on each failure. It also kept diagnosis observable: the original state stayed available for export and comparison.

Successful unfinished imports load paused. Successful completed imports stay completed without Resume and without a new winning-action effect. Those session outcomes are downstream of parsing, not reasons for the decoder to accept or reject a particular presentation of the JSON. The [1.4 release notes](release-notes-1-4.md) describe the user journey.

## Resolution

The repair replaced text comparison with explicit shape and state validation, then kept canonical ordering solely in the exporter. Regression examples varied property order, indentation, and valid matched-index order while independently checking nearby invalid combinations.

The repository has focused snapshot, snapshot-selection, and saved-format tests, plus browser import journeys. The lasting lesson is specific: a stable export is helpful evidence, but the decoder's acceptance set comes from the format's actual constraints, not from byte-for-byte imitation of one producer.
