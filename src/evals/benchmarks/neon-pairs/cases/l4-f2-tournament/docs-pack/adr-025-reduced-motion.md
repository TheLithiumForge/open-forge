# ADR 025: Provide a static reduced-motion presentation

This record explains the original reduced-motion commitment for card feedback and game completion.

## Context

The game has no need to communicate identity through a spinning card, a sweeping camera or a moving board. The important observations are whether a card is closed, revealed or matched, and whether the board is complete. Motion can decorate those states, but the states remain understandable without it.

The first reduced-motion decision was to honor the browser's `prefers-reduced-motion: reduce` setting for the game's visual feedback. The product retains static state changes while removing its card border transition and completion pulse under that preference. This record explains the accessibility reason; [Motion policy](motion-policy.md) holds the current motion guidance.

## The two current effects

`card.css` gives card borders a 120ms color transition. Hover changes a border, and matched state has its own green border. Under reduced motion the transition becomes `none`, so these states remain available without an interpolated change.

`status.css` gives a played completion one 600ms pulse through the `.celebrate` class. Its reduced-motion media query sets animation to `none`. The completion paragraph still appears and remains visible. A player does not have to wait for a moving effect to learn the final moves or time.

The mismatch interval is different. Showing two faces for 800 active milliseconds is a rule about observation time, not a decorative animation. The cards still close according to the same game transition. Removing the transition on a border does not change the mismatch countdown or the number of moves.

## Reasoning

The browser preference is already a user-controlled signal and works for the offline file without storing a second application setting. A game-level toggle would have duplicated that preference and added another persistence question. Treating motion as required feedback would also have made the reduced presentation less informative.

The chosen effects are modest even in ordinary mode. The banner pulses once, and card hover changes only a border color. That restraint reduces distraction on large boards, where many animated objects would compete with the faces the player is trying to remember.

## Alternatives considered

Disabling all visual changes under reduced motion was never appropriate: revealing a face and marking a match are the game. Keeping every short effect because it is brief was also too weak a basis; short duration alone does not describe how an effect feels or whether it is necessary. The later historical exception in [ADR 036](adr-036-motion-opt-in.md) is superseded and does not govern the current implementation.

A separate simplified theme could have mixed motion choices with color and geometry differences, making accessibility evidence harder to compare. The current media queries change the effects directly while preserving the same card shapes, layout and labels.

## Evidence and limits

`motion.e2e.test.ts` checks the ordinary durations and iteration count, then emulates reduced motion and verifies no card transition or victory animation. That is computed-style evidence from the built page, not an assumption that a media query exists somewhere in authored CSS.

The tests do not establish comfort for every player. They establish that the documented presentation paths are selected by the browser preference and that the static game remains complete. [Visual review checklist](visual-review-checklist.md), [ADR 050](adr-050-completion-banner.md) and [Feedback without audio](feedback-without-audio.md) explain why the feedback still carries meaning in that path.

## The current completion styles

The summary has a persistent visual container; the optional celebration class adds a bounded effect that the media query can remove.

```css
.victory {
  padding: 12px 16px;
  margin-bottom: 12px;
  color: var(--text);
  border: 1px solid var(--border);
  border-radius: 8px;
}
.celebrate {
  animation: victory-pulse 600ms ease-out 1;
}
@keyframes victory-pulse {
  50% {
    background: var(--panel);
    opacity: 0.75;
  }
}
@media (prefers-reduced-motion: reduce) {
  .celebrate {
    animation: none;
  }
}
```
