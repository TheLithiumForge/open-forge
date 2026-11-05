# ADR 041: Allow short names as typed

This historical record explains the retired one-to-thirty-two-character name limit and literal storage choice.

Status: Superseded by [Name length policy](name-length-policy.md)

## Original context

The early name-entry discussion favored a small amount of validation. Names were expected to be short labels, not account identities, and the team wanted to avoid transforming a person's text unexpectedly. The initial rule allowed names with 1 to 32 characters and stored them exactly as typed.

That choice belonged to an earlier interface design discussion. The game has no player-name entry field in its current control tree. Its text fields are a board code and snapshot JSON, with format-specific limits.

## Decision as originally made

The lower limit excluded an empty label. The upper limit was intended to keep an entry reasonably compact on a phone while allowing more than a very short nickname. Saving the original text was meant to preserve capitalization, spacing choices and the visible spelling a person had entered.

The team treated name validation as a presentation concern rather than an identity-verification system. No account lookup or remote uniqueness check was involved. That matched the broader local-game approach: a label helped readers recognize an entry, but did not establish a trusted person behind a result.

## Reasoning

Transforming text can surprise people when the displayed result differs from the field they submitted. Literal storage looked easy to explain and easy to debug: the saved value would match the original draft. A modest length range appeared sufficient for a prototype with only a few entries.

A larger range would have made wrapping and compact layouts harder to assess. A much smaller range risked excluding ordinary names without a clear gameplay reason. The original 32-character ceiling was a pragmatic visual estimate, not a result derived from the deck or scoring model.

## Alternatives considered then

Fixed abbreviations would have kept rows compact but shifted the burden to users to shorten their own labels. Automatically truncating would have silently changed identity cues. Rejecting a long draft was considered clearer than saving a shortened version that might become ambiguous.

The literal-storage choice also avoided deciding which edits were harmless cleanup and which changed the user's intended name. That restraint had an appeal, but it left several text questions outside the simple range check: what counted as a character, how visually identical strings compared, and how invisible spacing affected display.

## Why the record is superseded

The old decision's simplicity was incomplete. A numerical range and literal persistence did not by themselves define a predictable text-entry experience. The later policy handles the current name-length decision.

[Name length policy](name-length-policy.md) is the replacement authority. The top status link matters because a reader finding this record by its title could otherwise treat the old range as current. The historical value is the reasoning about preserving user intent and avoiding silent truncation, not continued authority over accepted input.

## Connection to existing controls

The current board-code field has a 32-character control limit for a completely different reason: its canonical code syntax is bounded and validated by a parser. The snapshot textarea has its own much larger limit plus a UTF-8 byte check. Those limits should not be generalized from this retired name decision.

[Text entry policy](text-entry-policy.md), [Form layout guide](form-layout-guide.md) and [ADR 042](adr-042-error-copy.md) are useful related pages. They concern the interaction around entering and correcting text, while this record preserves one specific historical validation choice.
