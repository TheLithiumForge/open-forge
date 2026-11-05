# Name length policy

This page defines how the name-length check is interpreted and separates it from native text-field limits.

The current game has no person-name field or name validator. Its existing text limits belong to Board code and Snapshot JSON. They are useful examples of browser entry constraints, but neither is an implementation of the name policy on this page.

## The checked value

The length check concerns the accepted name representation, after the processing described in [Text entry policy](text-entry-policy.md). Counting the raw draft instead can produce a different result from counting the value that the feature will store and display. A name has 2 to 20 Unicode code points after normalization. Anything else blocks the action and shows "Use 2 to 20 characters." This check therefore belongs at the point where a draft would become an accepted name, rather than being inferred from the field's apparent width.

The message is the user-facing explanation of this failure. The numerical bounds are inclusive: the endpoints pass, and values outside them fail. The policy does not define an exception for a one-character name, a separate limit for a family name or an alternate bound for another script.

A failure blocks the action that is trying to accept the name. It is not a request to silently cut the entered value to fit. Truncating a longer string would accept a different name and would conceal which action failed. The policy specifies feedback and blocking rather than a repair that guesses the user's intended spelling.

## Code points, code units and visible text

JavaScript string `.length` counts UTF-16 code units. That is not always the number required here. Many characters outside the Basic Multilingual Plane occupy two code units but one Unicode code point. For example, `A😀` has three code units and two code points.

A user-perceived symbol can also contain several code points. An emoji sequence joined with a zero-width joiner may look like one symbol while contributing multiple points. A base letter plus a combining mark can be another example, depending on whether canonical composition produces a single point in the accepted form.

The policy chooses code points, not rendered glyph count or grapheme cluster count. A font can draw two code points as one ligature, and an input method can produce a composed-looking sequence. Neither observation changes which unit the check uses. Reviewing only a screenshot cannot establish that the correct unit was counted.

In JavaScript, iterating a string by code point is different from indexing its individual UTF-16 units. `Array.from(value).length` or a spread-based count illustrates that distinction for well-formed text. This is an explanatory example, not a requirement to allocate an array in a particular implementation. The source has no name helper.

## Boundary examples

The examples below assume that the value shown is already in its accepted representation. They illustrate the count, not an additional character allowlist.

| Value | Code-point count | Length result |
| --- | --- | --- |
| Empty string | 0 | Fails |
| `A` | 1 | Fails |
| `Al` | 2 | Passes |
| `A😀` | 2 | Passes |
| Twenty ASCII letters | 20 | Passes |
| Twenty-one ASCII letters | 21 | Fails |
| Twenty supplementary-plane symbols | 20 | Passes |

The supplementary-plane example can expose a validator that accidentally uses `.length`: it sees forty code units instead. The opposite mistake is counting a joined visual sequence as one merely because it occupies one displayed cell. A correct boundary check is about the string's code points after representation processing.

“Passes” here means that the length condition is satisfied. It does not imply that an action with other applicable checks is guaranteed to succeed. The [Duplicate names policy](duplicate-names-policy.md) addresses a separate group-level condition without changing the unit used by this check.

## The order is observable

A raw draft may have surrounding or repeated spacing that will not be retained in its accepted form. Counting before the representation step can reject a value that would fit after processing, or accept a value whose later transformations were not what the length helper expected. The policy's ordering avoids having the stored name disagree with the checked name.

A reviewer can follow one candidate from field submission to accepted model and then display. The decisive value is the same one used for the count. An implementation that checks one temporary string but saves a different raw draft has two representations and cannot establish the stated condition simply by having a correctly named length function.

There is no instruction here to run the full action after every individual keystroke. Composition and editing are part of the input experience. The check governs acceptance, while timing of draft feedback is not otherwise specified by the current code or this policy.

## Scope of current evidence

There is no name-length test in the repository. Keyboard and reproduction checks establish native field labels, draft preservation and load outcomes. They do not verify the name count merely because they type strings into controls.

When this policy is implemented, its evidence can name the processed candidate and observed outcome at the boundary. That is enough to distinguish a counting defect from a display problem or another group-level validation failure. The policy stays narrow: one accepted value, one counting unit, one inclusive interval and the stated blocking message.
