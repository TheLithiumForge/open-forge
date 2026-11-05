# Voice guide

This guide explains the voice already used by Neon Pairs and how that voice supports play, recovery and accessible feedback.

The game speaks briefly because the player's attention belongs on the cards. Controls name actions, status text reports facts, and help explains the next useful interaction. The small amount of personality sits around those jobs. This page is about that balance; exact number presentation is covered in [Numbers style](numbers-style.md), and failure messages have their own [error message guide](error-message-style.md).

## The voice in the current screen

`src/game/Game.tsx` opens with the eyebrow “A little focus. A perfect pair.” The title is simply “Neon Pairs”. The introduction then becomes practical: “Reveal two cards. Match their shape and marker to keep the pair.” Those lines do different work. The eyebrow sets a mood; the introduction tells a new player what to do. Reading only the introduction is enough to begin.

The board help is equally direct: “Use a pointer or Tab to enter the board. Arrow keys move between cards. Enter or Space reveals a card.” It names the supported interactions without requiring a player to know terms such as roving focus, keydown handlers or native activation. Those implementation terms are useful in engineering documents, but they would make the board instructions harder to use.

The tone is friendly and restrained. Interface text has no exclamation marks. The completion banner closes with “Well played.” after reporting the result, so encouragement follows something that actually happened. There is no running commentary on the player's skill or on the quality of a particular miss.

That division gives copy review a useful starting point. A phrase that explains a control belongs near the control. A phrase that describes the latest turn belongs in feedback. A phrase that merely adds atmosphere has to earn its space alongside the existing eyebrow and title.

## Labels name the action

The literal labels live in `src/game/controls/control-labels.ts`. They are short enough to recognize while scanning and specific enough to distinguish actions with different consequences.

| Current label | What the wording tells the player |
| --- | --- |
| Grid size | The select changes the board dimensions. |
| New Game | Start a fresh deal at the selected size. |
| Restart | Begin the current deal again. |
| Reproduce a game | Open the tools for repeating a deal or restoring progress. |
| Board code | The nearby field contains a deal identifier. |
| Load board | Use the entered code to start its deal. |
| Snapshot JSON | The larger field contains serialized progress. |
| Export snapshot | Put the current progress into that field. |
| Load snapshot | Restore the progress entered in that field. |
| Resume | Continue the unfinished game loaded from a snapshot. |

“New Game” and “Restart” are especially easy to blur in explanatory prose. The first changes the deal; the second repeats it. A sentence such as “restart for a different arrangement” would misdescribe the existing button. [New game and restart](new-game-and-restart.md) explains the behavior in detail. In a writing review, the important question is whether the reader can predict the button's result from the surrounding sentence.

The mix of capitalization is part of the current interface, rather than evidence of one universal casing scheme. “New Game” has two capitals, while “Load board” has one. Quoting the real control name makes instructions easier to follow than silently regularizing those labels in prose.

## Outcomes are facts, not judgments

`src/game/session/session.ts` constructs the turn feedback. A successful nonfinal turn begins “Pair found.” and follows with the matched-pair count. An unsuccessful turn says “No match. Cards close after a short pause.” Neither message blames the player. Both identify what happened, and the second explains the temporary state of the board.

The word “short” in that message describes the experience rather than asking the player to read a technical timer value. Engineering prose can name the `MISMATCH_MS` constant and the active-time behavior. The player needs to know why the cards remain visible and that they will close. That is a useful example of the same behavior needing different levels of detail in different places.

The final turn's feedback combines the pair result with completion: “Pair found. All pairs found in N moves and time.” The visible banner uses a related sentence: “Every pair found in N moves and time. Well played.” Here `N` and `time` describe substitutions, not literal interface text. The banner and turn feedback serve different reading situations; copying one into the other without checking the component would lose that distinction.

“Perfect” in the eyebrow is an atmospheric phrase about pairs. It is not a score classification. The implementation has moves, matched pairs, elapsed time and a local best move count, rather than grades such as excellent, average or poor. A release note or help paragraph that describes a “perfect rating” would create a feature the game does not have.

## Help uses the player's vocabulary

In player-facing explanations, “card”, “pair”, “board” and “move” carry most of the meaning. “Activate” is useful when discussing the shared action behind click, tap and keyboard input. In the board help, the implementation instead says “reveals a card”, which describes the visible effect. The [glossary](glossary.md) records both vocabularies and the places where they differ.

“Click” alone is too narrow for a description intended to cover every input route. The actual help names a pointer and the keyboard. Other prose can say “select Load board” without assuming a particular device. A reproduction report can be more specific when the distinction matters: for example, whether Enter submitted the board-code form or a pointer selected its submit button.

The phrase “shape and marker” in the introduction is deliberately economical. The complete face model also includes solid or outline fill. That detail matters when discussing the full face catalogue, especially the largest board. It does not mean the short introduction is a complete definition of identity. A technical explanation can link to [Card artwork](card-artwork.md) instead of expanding every piece of introductory copy into an implementation reference.

## Accessible names tell the same story

The visual card and its accessible name describe the same available information. `src/game/board/card-state.ts` builds a position first, then the state, then a face label when a face is available. The first hidden card is named “Row 1, column 1, closed”. With face 0 revealed at that position, the name becomes “Row 1, column 1, revealed, cyan solid circle, marker top”.

These are functional descriptions. Adding a playful nickname for a hidden card would make it harder to compare the name with the board position and could reveal information the player should not yet have. Adding decorative praise to a matched card would make repeated navigation more verbose. The existing state words already explain why that card's appearance and action differ.

Face labels come from `faceDesign` in `src/game/board/face-design.ts`. Their lowercase color, fill, shape and marker terms provide a vocabulary a player can remember. “Cyan solid circle, marker top” is reproducible; “bright round one” is an interpretation. The latter might be acceptable as a player's own description in research notes, but it is not a substitute for the generated name in an engineering example.

The result feedback is also available to assistive technology. Writing it as a complete thought matters because a reader may encounter it without visually scanning the cards. Its first clause establishes the event; any following clause supplies the result or the next state. [Screen reader notes](screen-reader-notes.md) covers the reading journey, and [Announcements policy](announcements-policy.md) owns delivery details.

## Recovery stays calm and concrete

The browser's two invalid-load messages are “Invalid board code.” and “Invalid snapshot.” They identify the rejected input without guessing how it became invalid. A code might have been mistyped, pasted incompletely or supplied with an unsupported size. A snapshot might be malformed JSON or a structurally inconsistent state. The short message does not pretend to distinguish causes the UI does not expose.

The success status for unfinished progress is more instructional: “Loaded snapshot. Select Resume to continue.” That second sentence matters because cards are not active until the player resumes. Completed progress uses “Loaded completed snapshot.” and has no continuation instruction. A generic “Success” would omit information that explains what the player sees next.

The game keeps failed drafts available for correction. Help can therefore describe correcting the field and selecting the load action again. Correction uses the existing field and load action. Detailed technical causes belong in [Error message style](error-message-style.md), where the browser notices and replay diagnostics are considered separately.
