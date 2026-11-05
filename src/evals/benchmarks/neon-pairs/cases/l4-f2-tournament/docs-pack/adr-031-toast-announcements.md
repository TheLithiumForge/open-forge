# ADR 031: Let each feature publish a toast

This historical record explains the retired feature-owned live-region and toast announcement design.

Status: Superseded by [Announcements policy](announcements-policy.md)

## Original context

As the prototype gained turn results and reproduction messages, it was tempting to let each piece of UI own its feedback completely. The board could report matches, the import form could report loads and a later feature could publish its own result. This appeared to fit the principle of keeping behavior near its component.

The old decision gave each feature its own `aria-live` region for messages, shown as a toast. A component would update its region when its operation completed and display the same message in a transient floating presentation. [Announcements policy](announcements-policy.md) is the replacement authority.

## Decision as made

Feature ownership meant the code performing an operation also decided when to publish its text. The design avoided a central message dispatcher and allowed each component to be developed with a local feedback area. It seemed particularly convenient for reproduction forms, where validation already belonged near the submitted draft.

The toast treatment kept the main board layout from moving when a message appeared. In the early visual sketch, a brief message near a screen edge looked less disruptive than reserving vertical space for status. The team expected each result to be short and infrequent.

## Why it seemed appropriate

The initial examples were isolated. Loading a bad code produced one error. Finishing a pair produced one result. In those examples, local ownership made both the render code and the interaction easy to understand. A shared announcement mechanism looked like premature infrastructure for a small page.

There was also a real separation between visible location and semantic delivery. A load error needed to be close enough to its control to explain what failed, while a turn result needed to be available during board use. Feature-local regions seemed to make those associations natural.

## Problems hidden by isolated examples

Events do not always remain isolated. A final pair is both a match and a completion. An import can replace the state while a previous message is still visible. Two consecutive misses can have identical text even though they are separate events. Feature-local ownership by itself does not determine how those combinations should be presented or spoken.

Transient visual presentation adds another distinction: a message can disappear visually while still being relevant to a user correcting a form. A toast position can also compete with controls at a small viewport or equivalent zoom layout. These concerns were less visible in the initial desktop sketch than in a full keyboard journey.

The historical decision was therefore not merely a styling preference. It distributed responsibility for event identity and lifetime across components. That made the whole-page behavior harder to reason about than any individual toast component suggested.

## Relationship to current source

The current session has separate feedback text, load notice text and an announcement sequence. `Game.tsx` renders feedback in the game result area, and `Reproduction` keeps a visible load notice beside its controls. That arrangement should be read from the current policy and source, not reconstructed from the old per-feature toast idea.

The useful history is why local message ownership looked attractive and why whole-journey interactions exposed its limits. [ADR 050](adr-050-completion-banner.md), [Error display guide](error-display-guide.md) and [Screen reader notes](screen-reader-notes.md) explain the related present-day concerns.
