---
open-forge:
  description: Task 57 onboarding pages, the framework diagram, Highlights, and the expense splitter demos for new and existing codebases
  tags: [Memory, Working, Task, Documentation, Site, Demo, Contextual, Active]
---

# Task 57 — Onboarding and demos

## Outcome

Requested by the maintainer on 2026-09-25, after reviewing the documentation
site from [Task 52](task52-documentation-site.md). The site and the repository
documentation explain, from the first page, what the Core roles and Memory
states are and why they exist, and address new and existing codebases. Users
can try Open Forge on small, realistic demos whose inputs can later serve as
evaluations.

**Direction:** the maintainer named four gaps: greenfield versus brownfield
guidance, an early diagram of the categories and Memory, highlights such as
Decisions, and demos. Evaluations are recorded separately as
[Task 58](task58-demo-evals.md).

**In scope:**

- A responsive diagram component on the homepage, the introduction, and the
  Concepts overview.
- A new or existing project page, a Highlights page led by Decisions, and
  Demos pages, with sidebar, navbar, and footer links.
- The `demos/expense-splitter/` greenfield seeds and brownfield app, with seeds
  at four levels, reference records, and checklists.
- README, development guide, and source map pointers.
- Second round, requested the same day: the diagram near the top of the
  README as light and dark SVGs generated from the site's diagram data, a
  "Working with the CLI" page and an "Everyday flows" page with the use case
  and reason for each command (the existing guide stays as the reference), a
  "Why it exists" section on every Extension page and the overview, and README
  sections reorganized around the same reasoning.
- Third round, the same day: the maintainer corrected the brownfield model.
  Decisions record choices from the moment Open Forge is adopted, not rules
  the code already had. The homepage, the guide, Highlights, the README, and
  the brownfield demo now say so, and the demo's retroactive reference
  Decisions were removed. The diagram's "Read at startup" became "Loaded at
  startup" with a legend stating that child routes stay closed unless tagged.
  The mobile navigation, broken by a navbar `backdrop-filter`, and four
  admonitions that used pre-v4 title syntax were fixed, and a claim-by-claim
  audit corrected six inaccurate statements.
- Fourth round, the same day: the maintainer described the document flow
  that Project Documents and Planning should carry. Decisions record changes,
  the documents they affect are updated to say what's true now, and documents
  are kept top-down: top-level documents give an overview of the whole and
  summarize the narrower documents they link to, which link back up. The
  maintainer was explicit that this flow belongs to those Extensions, not to
  Core, and that Memory's own flow needs no change. Core's loader already had
  the general rule (detail in the narrowest source, accepted changes reach
  it), and the Decisions category already links to current results, so the
  only new rules are two Documents category Axioms: keep Documents top-down,
  and update the summaries above a changed Document. The site has a new
  Extensions page, "The document flow", the README a short subsection under
  Extensions, and the demo records now link both ways through `Current
Sources` and related-source sections. The `open-forge-cli` Skill from
  [Task 60](task60-cli-skill.md) shipped in the same change.
- Integration: the maintainer asked for one squashed commit on `develop`, with
  `main` fast-forwarded to it after `develop` takes the README edit made on
  `main` through GitHub.

**Preserve / out of scope:** shipped Framework and Extension content. No
evaluation harness, scoring, or published comparison.

**Done when:**

- [x] The site builds with broken links and anchors failing, and type-checks.
- [x] The diagram reads well on the wide homepage, in the docs column, and on
      a phone, in both themes.
- [x] The brownfield app's tests pass and it type-checks under strict settings.
- [x] The maintainer reviews the pages and the demos, and decides integration.

## Current State

**Now:** complete and integrated into local `develop` and `main`. Pushing is the maintainer's step.

**Design notes:**

- The diagram uses container queries, so it adapts to its own width: side by
  side above 1000 pixels, stacked with three-column Core cards in the docs
  column, and single-column on phones.
- Both demos use the same expense splitter so building fresh and changing an
  existing version can be compared. The brownfield rules (whole cents, payer
  absorbs rounding) live only in `NOTES.md`, and the existing tests don't catch
  their violation. That makes a lost reason observable.
- This repository dogfoods the Documents category, so the new top-down Axioms
  now apply to its own current documents too. They were not audited against
  it in this Task. A later pass should check that the top-level Vision,
  Architecture, and Principles summarize and link the scoped documents below
  them.
- Seed levels 1 and 2 deliberately omit the rounding rule. Levels 3 and 4 state
  it, as a table and as Open Forge records.
