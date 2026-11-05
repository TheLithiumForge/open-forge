# ADR 053: Keep specifications near the implementation

This record explains repository documentation as a reviewable companion to code, tests and reproducible examples.

## Decision

Keep the product specification with the project so a behavior change can be understood alongside its implementation and evidence. The product specification belongs in `docs/` alongside the source.

Repository documentation was chosen because the game has precise rules that are not obvious from a screenshot. The mismatch interval is active time, Restart repeats a code, unfinished imports pause, and a closed card must not expose identity. These details deserve readable explanations rather than only test assertions or comments scattered through components.

## Different documents do different work

The rules page defines game behavior. Technology describes compiler settings, package pins, commands and delivery. Architecture explains pure functions and browser adapters. Acceptance provides journeys that exercise those definitions. Decision records explain why a choice was made and preserve retired reasoning when it is superseded.

That separation lets an engineer find the right level of answer. A parser field list belongs in the format description; the reason for choosing JSON belongs in [ADR 010](adr-010-snapshot-json.md). A browser test can show a focus edge passing; [ADR 024](adr-024-roving-focus.md) explains why focus is remembered independently of selection.

## Why version control helps

A documentation change can be reviewed in the same context as a changed rule or component. An exact board code or replay example can remain attached to the version of the behavior it describes. Superseded records can link to their replacements without erasing the rationale that shaped older code or prototypes.

An external wiki might be convenient to edit, but it can drift from the source revision a reader has checked out. Generated API reference would identify function signatures without explaining the session boundaries or product intent. Comments alone would make cross-cutting journeys difficult to assemble. Repository prose complements those mechanisms rather than replacing all of them.

## Code remains evidence

A page that says a behavior exists is weaker than the source and tests when they disagree. The current game can be inspected through `Game.tsx`, the session adapter and pure core. Documentation names those real boundaries so a reader can verify an account instead of relying on a vague architectural diagram.

The same discipline applies to scope choices. The game has no locale framework or dedicated print stylesheet. Decision records explain those choices and distinguish historical reasoning from current implementation.

## Examples and links

The documentation set links related pages by filename. A reader can move from board-code syntax to seeded generation, from snapshot validation to import pause, or from large-board layout to focus scrolling. Links are most useful when they lead to a different level of explanation rather than repeat the same paragraph on several pages.

Examples use documented codes and public formats rather than hidden debugging endpoints. That keeps the explanation useful for a developer, a coding agent or a person investigating a report. The same examples can inform focused tests and browser fixtures, though expected test values still need independent justification.

## Costs

Prose needs maintenance. Moving behavior can leave stale source paths, and copying a rule into many pages can create conflicting versions. The repository location makes that drift reviewable; it does not prevent it automatically. [Documentation style](documentation-style.md), [ADR 054](adr-054-acceptance-journeys.md) and [Code review guide](code-review-guide.md) explain the supporting habits without requiring a separate documentation service.

The outcome is a project whose behavior, implementation and reasons can be read together.
