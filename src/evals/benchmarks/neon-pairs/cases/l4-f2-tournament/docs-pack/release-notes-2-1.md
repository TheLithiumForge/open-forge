# Neon Pairs 2.1: reduced motion

These release notes describe the motion review that kept Neon Pairs state feedback available when optional visual effects were removed.

The release's useful outcome was narrow: a reduced-motion preference leaves the same readable cards and result, without making the memory task a different game.

## The two effects under review

| Surface | Ordinary presentation | Reduced-motion presentation |
| --- | --- | --- |
| Card border | A 120ms `border-color` transition | No transition |
| Newly completed result | One 600ms banner pulse | Static banner immediately |

These are the effects authored in `card.css` and `status.css`. Cards do not rotate between identities or rearrange the board. A match still adds its green border and corner check; a reveal still shows the face. The final summary remains ordinary text with moves and elapsed time.

The review asked what information would disappear if each effect stopped at its first frame. None of the outcome needed to disappear. This was more useful than deciding whether an effect felt “subtle” to the people watching a demonstration. A small effect can still be uncomfortable or distracting to someone who has requested less motion.

## Why the game clock did not change

The 800-active-millisecond mismatch interval is an observation opportunity, not a decorative transition. Removing it would make an incorrect pair close immediately and could make reduced-motion play harder. The current code keeps it in `game.ts`, independent of CSS.

`advanceGame` changes the logical remainder; the visual media query changes presentation. An animation ending does not authorize another reveal, and a rendering callback does not define how much active time passed. The [mismatch interval page](mismatch-interval.md) and [clock design](clock-design.md) explain those existing boundaries.

The review considered a global duration utility and set it aside. With only two authored effects, local media-query rules expressed the actual behavior clearly. A general mechanism could also obscure the distinction between a presentation duration and a game interval.

## Imports were not new wins

A completed snapshot shows a static result even without a reduced-motion preference. Loading supplied finished progress is different from performing the final match. Current `replaceGame` clears `celebration`, while the winning activation path sets it. `GameStatus` derives whether the board is complete and uses the session flag for the optional class.

An early sketch attached the pulse to every mount of a completed banner. That was convenient but tied the event to rendering. A supplied completed position would then look like a newly earned result. The current session distinction keeps that meaning explicit. [Snapshot restoration](snapshot-format.md) and the [completion guide](win-conditions.md) provide the current detail.

## Evidence, rather than a final screenshot

`tests/motion.e2e.test.ts` inspects computed transition property and duration, then the completion pulse's duration and single iteration. Its reduced-motion case emulates the media preference and checks the banner animation and card transition values.

A final screenshot alone could miss an unwanted preceding animation. A media-query block in the source could be overridden later in the cascade. Computed styles check the behavior that the built page actually applies.

These checks verify how the authored effects respond to the preference. The [motion policy](motion-policy.md) and [reduced-motion decision](adr-025-reduced-motion.md) explain the present treatment.

No animation framework, cosmetic settings panel, or celebration sound was added in this release. The release kept the geometric cards and silent play, then made the optional effects easier to reason about. [2.2](release-notes-2-2.md) follows the next edge-case work around timing, zoom, and complete browser journeys.
