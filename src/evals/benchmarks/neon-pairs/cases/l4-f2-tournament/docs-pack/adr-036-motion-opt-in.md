# ADR 036: Permit brief decoration under reduced motion

This historical record explains the retired duration-based exception to reduced-motion handling.

Status: Superseded by [Motion policy](motion-policy.md)

## Original context

After the first reduced-motion work, the team discussed whether very short decoration should count as an exception. The prototype had small state feedback rather than large page movement, and it was tempting to classify a brief effect as too minor to need a separate static path.

The old decision allowed decorative animation under reduced motion when it lasted under 200 milliseconds. Duration was the proposed dividing line: short enough decoration could remain, while longer effects would be removed. This record describes that retired exception as it was understood at the time.

## Reasoning at the time

The team wanted to preserve a feeling of responsiveness around button and card state changes. A small interpolation could make an edge or highlight feel less abrupt, and the implementation already had a short border treatment. A numerical cutoff appeared to make the distinction easy to apply during review.

It also seemed to avoid a proliferation of motion branches. If a brief effect could be left alone, the author would not need an alternate declaration for every small transition. In a limited prototype, that looked like a reasonable compromise between visual polish and the broader reduced-motion intent.

## Alternatives in the original discussion

Removing the exception entirely would have made the reduced path more consistent but would have discarded the subtle transitions the team was trying to retain. A user-facing motion setting would have added another control and stored preference. Evaluating each effect only through subjective review would have been less easy to summarize than a duration cutoff.

The old choice preferred a simple numeric rule. Its appeal was predictability: an author could measure a duration without agreeing on descriptive terms such as gentle or minimal. The weakness was that the number summarized only one property of an effect.

## Why the exception did not hold up

A short effect can still move a large area, repeat, flash or distract. Conversely, a long change can be visually mild. Duration does not establish how an effect interacts with a player's preference or whether it carries necessary information. The old exception made implementation review look objective while leaving those more meaningful questions unanswered.

It also created a surprising result for users who had already expressed a reduced-motion preference in the browser or operating system. The application was interpreting “short” as sufficient reason to retain decoration, rather than examining the effect in the context of that preference.

## Current implementation boundary

The present `card.css` removes its border transition under `prefers-reduced-motion: reduce`, and `status.css` removes the completion animation under the same preference. The static card states and completion text remain. Those are current source observations; they do not follow from the retired under-200ms exception.

[Motion policy](motion-policy.md) supersedes this record. [ADR 025](adr-025-reduced-motion.md) explains the original accessibility commitment, while [ADR 050](adr-050-completion-banner.md) explains the completion presentation. This record is useful when an old note cites a short duration as justification. It explains where that reasoning came from and why it is historical, without reviving it as a current allowance.

## A current reduced-motion override

The existing card implementation does not rely on the historical short-duration exception. Its reduced-motion path removes the transition.

```css
@media (prefers-reduced-motion: reduce) {
  .card {
    transition: none;
  }
}
```
