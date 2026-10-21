---
open-forge:
  description: What the lossless and readable kernel council decided, how it ranked twelve proposals, and what v2 and v3 changed as a result
  responsibility: Record council 3's inputs, method, rankings, chair decisions, measurements and open choices for the maintainer
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loader, Council, Adoption]
---

# Lossless And Readable Kernel Council

This is council 3. It follows the [loader council](../loader-council/record.md) and the [opt-in council](../opt-in-council/record.md), whose v2 the maintainer reviewed. Nothing here is accepted direction until the maintainer accepts it.

## What the maintainer asked

The maintainer mostly liked v2 and asked for six things:

1. Keep v2 and make it lossless, with better wording. They also asked how v2 could remove so many rules.
2. A simplified experience with a scaling degree of complexity.
3. Progressive adoption for brownfield projects, because the framework felt aimed at new projects willing to take its whole structure.
4. A very good introduction, with README examples and the accent on customizability. A brownfield project should be able to start with as little as Crystallized Memory and Directives, or less.
5. An ultra-lightweight install mode.
6. A v3 whose sentences read well to a person on a first pass, because people deciding whether to use a tool have little patience.

During the review they added that linking paths outside `.agents/` was right, because brownfield projects keep instructions in many places.

## Method

Twelve read-only members with different perspectives worked from a frozen snapshot of v2 rebased on `develop`: an adoption strategist, a Unix minimalist, an essence guardian, a lossless auditor, a model-behavior realist, a CLI engineer, a layering architect, a brownfield team lead, an impatient evaluator, an onboarding designer, a plain-language writer and a contrarian. Each received a [statement ledger](ledger.md) of every dropped and partly kept rule and a strict definition of lossless: every behavior keeps a home at its old strength where the people or agents who need it will meet it, and a rule is redundant only when a surviving rule that implies it can be quoted.

Stage 1 produced twelve independent proposals, kept in [proposals](proposals.md). Stage 2 had each member rank the eleven others four times without knowing their authors, kept in [reviews](reviews.md). The planned Stage 3 ratification vote did not run, because the maintainer paused the worker runtime. The chair reviewed the implemented branches instead.

## Rankings

Average position across eleven reviewers, lower is better.

| Deliverable | First | Second | Third |
| --- | --- | --- | --- |
| Lossless ledger | Lossless Auditor 1.00 | Layering Architect 2.55 | Contrarian 2.82 |
| Ladder, lightweight install and brownfield | Layering Architect 1.27 | Onboarding Designer 2.36 | CLI Engineer 2.55 |
| Introduction | Layering Architect and Onboarding Designer 1.82 | | Impatient Evaluator 2.91 |
| v3 text | Layering Architect 1.55 | Lossless Auditor 1.82 | Model-Behavior Realist 2.91 |

Every reviewer ranked the auditor's ledger first. The architect's proposal ranked first or tied first on every deliverable, so it became the base text.

## How v2 removed so many rules

The council found five kinds of removal:

- **Compression that kept behavior.** Inheritance, conflict and overwrite rules survived as single sentences.
- **Moves to where a rule is used.** CLI command lists moved into the CLI Skill, and saving moved into its own policy. These were sound when readers still meet the destination before they act.
- **Lost definitions and examples.** Routing vocabulary and the C# scope example lost their home.
- **Authoring rules treated as assumed competence.** The Pattern acceptance threshold and the link and tag conventions disappeared.
- **Safeguards turned into implications.** "Investigate before editing conflicting accepted sources" and the #Evergreen deadline were not implied by anything that survived.

The first two kinds were sound. The last three lost behavior, and a word count is not evidence that meaning survived.

## Chair decisions

The reviews converged on four open conflicts, and the chair followed the convergence:

1. **Where restored rules live.** Duties that apply everywhere stay in the loader at full strength, including the communication defaults, the acceptance and conflict rules and #Evergreen. Authoring mechanics move to the `## Instructions` of `.agents/README.md`, which the loader requires before any authoring. Vocabulary and tag labels sit in its `## Terms you may meet`.
2. **The ultra-lightweight install.** A managed `--preset minimal`, not an unmanaged two-file seed. The wizard keeps Essentials as its recommendation, and Minimal is the brownfield recommendation in the README and guides.
3. **When categories are read.** Guidance, Maps, Patterns, Skills and Templates stay on demand with explicit "check these Entries when" triggers. Emerging gets its #KeepInMind refresh back, because candidate capture happens at refresh points.
4. **Other file formats.** Loader links accept any text file in the workspace, read as an opaque leaf. Open Forge does not interpret another tool's frontmatter or activation settings, and folders or globs are not expanded.

The reviews also surfaced defects that v2 shipped with, all fixed:

- Update, Configure and Extension commands brought back loader lines the user had removed, and Update deleted links to documents outside `.agents/`.
- Index refilled an empty loader list.
- Under Essentials, the recommended default, `working/saving.md` was Git-ignored with the rest of Working, so the saving policy silently never loaded. It now lives at `memory/saving.md` in the shared Memory catalogue, and Install leaves it out whenever Working Memory is left out.
- Loader lines needed a tag, so an on-demand link without one made the loader malformed.

## Measurements

Counted with the CLI's estimate method, characters divided by four.

| Payload | Startup files | Startup words | Loader words |
| --- | --- | --- | --- |
| `develop` before v2 | 12 | about 4,560 | 2,052 |
| v2 | 7 | about 1,430 | 823 |
| v2 lossless | 8 | 2,451 | 1,185 |
| v3 | 8 | 2,705 | 1,413 |

Lossless costs about 1,000 startup words over v2, and is still 46% below the shipped startup. v3 adds about 250 more for fuller sentences.

## v3

v3 keeps every v2 obligation and rewrites the loader, the managed `AGENTS.md` block and `.agents/README.md` for a person reading them for the first time, on a separate branch stacked on v2:

- The loader opens with what the file does and that the `Entries` list is the whole setup.
- Each step and rule starts with a short bold label, such as **List your working files** or **What counts as accepted**, so a reader can scan it.
- Conditions come before required reads, so the procedure reads in the order an agent acts.
- `.agents/README.md` says who it is for first, then lists each folder with one line on what it holds.
- Each catalogue opens by saying whether it is optional and what it needs, then what it holds.

A checker verified that all 46 dropped or partly kept statements still have a home in both v2 and v3.

## Risks and the proof that would settle them

- Restored rules may bloat what an agent holds in mind. The startup figure above is the cost.
- On-demand categories depend on agents following "check these Entries when" triggers. A task with a mixed 200-file change, run with and without the CLI, would show whether they do.
- The introduction is written by the maintainer's own tools. The cheapest real proof is three unfamiliar developers, twenty minutes each, asked to link a document, explain what loads, add a capability and turn it off, without coaching.

## Open choices for the maintainer

- Whether the communication defaults should become an optional policy file in a later release, with existing installations keeping them.
- Whether a `link` command is worth its cost. The engineer estimated three to five days and recommended shipping copyable entry lines first.

## Maintainer decisions after review

On 2026-10-21 the maintainer reviewed the PR and decided:

- Minimal is the loader, Directives and Crystallized Memory, because a loader with nothing listed does nothing for the user. Essentials and Full Core build on it, so each built-in preset includes the one before it.
- The interactive wizard offers Minimal first and recommends it. Unattended Install without a preset still uses Full Core.
