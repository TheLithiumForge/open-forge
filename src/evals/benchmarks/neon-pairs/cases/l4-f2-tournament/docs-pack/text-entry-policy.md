# Text entry policy

This page records the treatment of person-entered names and distinguishes it from the game's existing code and JSON fields.

Neon Pairs currently asks for a board code and snapshot text, not a player name. There is no name field, group-name store or name rendering component in the source. The name policy below applies when such input is introduced; it does not describe a transformation already applied to the reproduction textarea.

## One name representation

A name can arrive with incidental spaces from typing, pasting or an input method. Keeping several representations of that same entered name would make later display and comparison harder to reason about. Every name a person types is stored and shown after trimming both ends, collapsing each run of inner whitespace to one space and normalizing to NFC. The stored value and visible value therefore describe the same processed name rather than exposing the original draft in one view and a different spelling in another.

The policy is about a person's name. It does not turn all user input into name text, and it does not change the board-code grammar or snapshot serialization. The current fields demonstrate why that distinction matters: significant punctuation and structural whitespace belong to their own formats.

For names, the other checks are documented separately in [Name length policy](name-length-policy.md) and [Duplicate names policy](duplicate-names-policy.md). This page explains representation and input boundaries without duplicating their conditions or error messages.

## Examples of representation

The following examples use quotes only to make surrounding whitespace visible. The quotes are not part of the entered name.

| Entered draft | Displayed and stored form | What changed |
| --- | --- | --- |
| `  Mira  ` | `Mira` | Space around the name is removed |
| `Ada   Lovelace` | `Ada Lovelace` | An interior run becomes one separating space |
| `Jean Luc` | `Jean Luc` | Ordinary spacing stays ordinary |
| `Cafe` followed by a combining acute on the e | `Café` | Canonically equivalent text gets a composed representation where available |
| `O'Neil` | `O'Neil` | The apostrophe is retained |

The last example matters because representation cleanup is not an instruction to remove meaningful punctuation or rewrite a person's preferred spelling. The policy does not transliterate scripts, strip accents or convert names to ASCII. Those would be additional transformations beyond what is stated.

NFC addresses canonical equivalence in Unicode text. It is not the compatibility normalization form. Text that merely looks similar is not necessarily canonically equivalent, and this policy does not define visual resemblance as identity. That boundary prevents a representation step from quietly becoming a broad name-matching heuristic.

A name can also contain multiple words. Collapsing a whitespace run does not merge words together; it leaves a separating space. Treating the policy as “delete all whitespace” would change `Ada Lovelace` into a different string and would not implement the stated behavior.

## Drafts and accepted values

A draft is what someone is still editing. A stored value is what an action has accepted. The policy determines the representation of accepted and displayed names, but it does not prescribe that the caret jump after every keystroke or that an input method's partial composition be rewritten while it is active.

That distinction leaves room to implement an editing experience without confusing it with persistence. A reviewer can inspect where a completed entry crosses from local draft state into the feature's model and where the model is rendered. If display uses an old raw draft while storage uses the processed value, the two surfaces no longer have the relationship described above.

The current Reproduction component provides a useful example of draft ownership, though its data is not a name. Its snapshot string lives in local React state and is sent to the decoder on Load snapshot. BoardCodeForm instead reads its native form field at submission. Either storage approach can host an input boundary; the choice of controlled or uncontrolled field does not by itself implement name representation.

No name persistence shape exists in `session/storage.ts`. Its preferences concern size and best move counts. Those fields contain no name preference key, snapshot property or import format.

## Board codes follow a different path

The Board code input is a native text input with `maxLength={32}`, spell checking disabled and autocomplete off. On submission, `use-game.ts` calls `code.trim()` before `parseBoardCode(code)`. That is the only cleanup in that loading path.

The grammar is a version prefix, supported side length and seed separated by hyphens. For example, surrounding spaces around `np1-6-1` are accepted by the control, but an internal space changes the grammar and is rejected. The loader does not collapse arbitrary interior text or repair the seed.

Successful submission replaces the game and refreshes the field from the current code because the input is keyed by generation. Unsuccessful submission preserves the draft. This gives a concrete reference for separating a draft from an accepted representation without applying name transformations to a deal identifier. See [Reproduction panel](reproduction-panel.md).

Board-code parsing is also available as a core function. Calling `parseBoardCode` directly is not the same as passing through the UI loader's trim step. When reading a failure report, the entry point matters. A format parser and an input adapter can have distinct responsibilities without the application guessing what malformed identifiers meant.

## Snapshot text remains structured data

Snapshot JSON has a native textarea length limit and a decoder byte limit. It can include whitespace for readability, and export deliberately produces multiple lines. Its structure is validated before the game is replaced.

Applying name cleanup to the entire JSON draft would conflate two kinds of input. Whitespace can occur between tokens or inside a JSON string, with different meanings. The existing code passes the complete draft to `decodeSnapshot`; it does not first treat it as display prose or a person's name.

The same is true for error preservation. On malformed JSON, the text remains editable and the current game remains present. That behavior supports correction of a structured draft. Accepted names use the normalized representation described above.

[Import safety policy](import-safety-policy.md) and [Error display guide](error-display-guide.md) explain the current parser and feedback boundaries. Keeping those links close to the input descriptions is more useful than inventing one universal cleanup function for every field on the page.

## Unicode review considerations

Canonical equivalence is easiest to miss when examples are written as visually identical letters. A composed accented character and a base character followed by a combining mark may render alike while having different underlying sequences. A useful representation check includes both forms and inspects the accepted value rather than only its screenshot.

The policy also leaves capitalization alone at this representation step. A comparison operation may have its own policy, but that does not require changing a person's displayed capitalization. Keeping display representation and comparison purpose distinct avoids presenting a comparison key as someone's chosen name.

Likewise, a font's treatment of a character does not change the text stored. A missing glyph, a ligature or a different script's shaping belongs to rendering and typography. The representation policy can be implemented correctly while a layout still needs work to display the name legibly. [Typography](typography.md) provides the visual context.

The current game has no name-character allowlist or prohibited-name list. Adding one while implementing spacing cleanup would be a separate product decision, not a consequence of Unicode normalization.

## Implementation evidence to look for

There are no name-specific tests in the repository because no name entry has been implemented. The useful evidence for a future implementation would connect its draft, accepted model and displayed text using the cases above. A check confined to an input's current value can miss a storage path that retains an older raw string.

Repeated processing should not progressively change the accepted spelling. The transformations described here have a stable result once applied. If editing, saving and displaying the same accepted name repeatedly removes more content each time, that points to an implementation doing more than the stated representation work.

The current code's strongest relevant lesson is explicit boundaries: BoardCodeForm submits a draft, the controller validates, replacement updates the model, and rendered controls follow that model. Names need their own representation boundary within the feature that introduces them. The present board-code and snapshot controls use their own input boundaries.
