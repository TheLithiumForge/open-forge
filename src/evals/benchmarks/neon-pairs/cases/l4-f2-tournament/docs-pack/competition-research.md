# Shared-device playtest notes

This page is a qualitative account of how people played Neon Pairs together and what those sessions suggest for competition design.

The sessions examined local play with the current game. The observations concern attention, learning and handoff.

## Session setting

The working setup was deliberately modest: one browser game, people seated close enough to see the screen and paper notes beside the device. Event sketches were discussed on paper because the game has no participant entry or bracket controller. The browser supplied the actual card play, its ordinary status area and the existing reproduction tools.

Three settings were considered: a laptop on a table, a tablet passed between adjacent seats and a phone held by the current player. These are contrasting situations, not a representative sample of device usage. Their value was in making different problems observable. A laptop makes spectating easy; a held phone makes physical transfer and changing viewing angles much more prominent.

The discussion concentrated on what people did without prompting. Could someone enter the board, distinguish a face, recover after a miss and understand that their board was complete? Could a person who had looked away rejoin the conversation? Those questions were more productive than asking participants whether a finished competition feature sounded appealing in the abstract.

## Laptop: learning the faces aloud

In the laptop session, Mara began with the pointer and described each revealed face aloud. Her first descriptions used only shape: circle, star, triangle. After encountering similar shapes with different markers in a larger practice board, she started adding direction. The vocabulary changed from “the star near the corner” to “the star with the marker on the right.”

Jon, sitting beside her, remembered locations in row terms instead. He pointed to a horizontal band and then counted columns. They were attending to the same artwork but encoding different information. Neither strategy suggested a problem with the deck; the difference was in how people organized memory.

This observation agrees with the face model in `src/game/board/face-design.ts`: shape and marker direction both contribute to identity, and the game also includes a fill distinction. It also explains why the introduction in `Game.tsx` mentions shape and marker. A report that two faces “looked the same” can mean that a participant initially remembered only part of the available design.

The useful follow-up question was which feature they had noticed, rather than whether they had memorized the official face name. [Face catalogue](face-catalogue.md) and [pair identity](pair-identity.md) provide the source context for interpreting such reports.

## Keyboard: movement looked like hesitation

Leah used Tab to enter the board and then moved with the arrows. An observer initially wrote down several pauses as indecision about the puzzle. On closer discussion, some of those pauses were time spent checking the focus outline and moving to a remembered position. The participant knew which card she wanted before she reached it.

This was a useful correction to the observation method. Card navigation and memory retrieval are separate activities, even when they occur between the same two reveals. Counting only the delay between clicks would have concealed the difference entirely, since this participant was not primarily clicking.

Leah also revisited an already matched card while moving across the board. She treated it as a landmark rather than as a card she intended to play. The current cards remain focusable in that state. That behavior made sense in the session, but an observer looking only at focus position might have misclassified it as a failed selection.

The input trace records navigation behavior. The repository README records accessibility-tree checks; screen-reader testing has not been performed. [Board keyboard navigation](board-keyboard-navigation.md), [focus policy](focus-policy.md) and [screen reader notes](screen-reader-notes.md) distinguish those subjects.

## Tablet: the handoff was an activity of its own

In the tablet session, moving the device between people interrupted the group's visual reference. The outgoing player tilted it toward the next person, while another participant tried to read the result from an angle. The result was still present, but not everyone saw it at the same time.

The next player first adjusted the tablet on the table, then looked for the board. That sequence would have been easy to describe as slow play if the observer had measured from physical handoff. It was mostly positioning the device. The observation helped separate the social transition from actual interaction with the game.

People also used the visible completed board as a conversation piece. They pointed at remembered mistakes and explained how they finally located a pair. The exposed cards carried a short history of the attempt that a bare result number would not capture. In a group setting, that retrospective conversation was part of the appeal.

The presentation of a competition handoff remained undecided. The prototype used ordinary play and a paper event sketch. It showed what people looked at and discussed. [Competition overview](competition-overview.md) explains the intended social context.

## Phone: scrolling and tapping had different purposes

On the phone, Niko sometimes touched the board to move its visible area rather than to reveal a card. His comments made that intention clear: he was searching for a remembered row near the edge. A raw touch trace would have contained contact with several cards, but contact alone would not have described the intended action.

The research note therefore distinguishes an attempted tap, a completed activation and a scroll gesture. These are useful observational categories, not additional game states. A participant saying “I touched that one” can refer to any of them, and the distinction can explain an apparent missing move.

The observer's view was narrower on the phone too. People beside the player often saw the device's back or an oblique angle, so they talked more about the eventual result than about individual reveals. That was a different social experience from the laptop, even though the underlying card actions were unchanged. [Pointer and touch](pointer-and-touch.md) and [large board rendering](large-board-rendering.md) cover the relevant existing behavior.

## The mismatch pause helped memory and confused tapping

Several exchanges centered on a miss. One participant used the visible interval to repeat both face descriptions aloud. Another tapped quickly again and interpreted the lack of a new reveal as a lost input. The two reactions came from different expectations about whether the turn had finished.

It was useful in the discussion to replay the participant's explanation in ordinary language: “I was still looking at those two” versus “I thought I could choose the next one.” Both describe a reading of the same moment. Neither requires a different deck or a new scoring mechanism to explain it.

## Spectators remembered more than the current player expected

People who watched closely often remembered a location that the player had forgotten. Sometimes that produced a laugh; sometimes it produced an unsolicited hint. When the device moved out of view, conversation changed from specific positions to general encouragement. The physical setting strongly influenced how much information was shared.

This is relevant to the feel of local competition. The current application has no way to know who watched the screen or who spoke during a turn. No screen arrangement can make that social history part of the board code. The notes identify a design tension.

A future research session could explore how different groups feel about spectating, but that is a research question rather than a committed feature. The existing observations support only the narrower conclusion that sharing a device also shares attention in ways that vary with posture, distance and familiarity.

## Evidence retained from the discussion

These sessions identified misunderstandings during local play. Shape-only memory, keyboard travel, device adjustment and the visible mismatch interval each explain a different kind of report. The observations are useful starting points for further sessions.

A board code can anchor an arrangement, a snapshot can anchor progress and a replay can anchor actions. The observer's account adds the human context those artifacts omit. [Competition deals](competition-deals.md), [browser test guide](browser-test-guide.md) and [acceptance](acceptance.md) explain the technical evidence.
