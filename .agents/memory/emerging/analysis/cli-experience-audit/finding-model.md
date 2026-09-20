---
open-forge:
  description: Why doctor warnings read as info and info reads as debug — four different kinds of statement are emitted through one finding channel, measured by code
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Doctor, Findings, Severity, Presentation]
---

# Finding Model

## Conclusion

The severity ladder is not mis-graded. **Four different kinds of statement are
being emitted through a single channel**, and severity is the only dimension
available to distinguish them — so it gets stretched to carry meaning it cannot.

Measured on this repository: of 11,455 findings, **594 are problems.** The other
10,861 are proposed fixes, gathered evidence, healthy state, and one capability
disclosure repeated 98 times.

That is why warnings read as information and information reads as debug output.
They _are_ information and debug output, wearing a severity label.

## Measured

### Warnings — 3,076

| Count   | Code                                     | What it actually is                 |
| ------- | ---------------------------------------- | ----------------------------------- |
| **594** | `reference.target-missing`               | **a problem**                       |
| 594     | `reference.candidates-several`           | a proposed fix for one of those 594 |
| 594     | `reference.candidate-route-neighborhood` | evidence behind a proposal          |
| 594     | `reference.candidate-literal-content`    | evidence behind a proposal          |
| 443     | `reference.candidate-filename`           | evidence behind a proposal          |
| 80      | `reference.candidate-title`              | evidence behind a proposal          |
| 66      | `workspace.frontmatter-malformed`        | **a problem**                       |
| 38      | `route.axioms-invalid`                   | arguable — see below                |
| 35      | `reference.same-target-path`             | an observation                      |
| 27      | `reference.fragment-missing`             | **a problem**                       |

**2,305 of 3,076 warnings — 75% — are not findings.** They are the _resolution
proposals and their supporting evidence_ for the 594 broken links, promoted to
the same severity as the problem itself. Each broken link emits roughly four
additional `WARNING` lines describing how it might be fixed.

A reader cannot distinguish "your link is broken" from "here is a filename that
looks similar", because both arrive as `WARNING`.

### Information — 8,378

| Count | Code                                   | What it actually is                     |
| ----- | -------------------------------------- | --------------------------------------- |
| 5,614 | `reference.target-valid`               | healthy state                           |
| 1,818 | `reference.cycle`                      | mandated structure, reported as notable |
| 924   | `reference.repeat`                     | work performed                          |
| 98    | `reference.external-unchecked`         | **a capability disclosure**             |
| 1     | `extension.lifecycle-document-missing` | arguably a real finding                 |

**8,377 of 8,378 are not findings.** They report what the tool did, not what the
workspace needs.

`reference.external-unchecked` is the clearest case and the one that prompted
this. Ninety-eight times, three lines each:

```
Link: .agents/directives/csharp/design.md:135:5
  Destination: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/null-safety/…
INFO  External link was not checked [reference.external-unchecked]; information only
```

That is not a property of the workspace. It is a property of the tool: _Open
Forge does not fetch external URLs._ Stating it per occurrence, ~300 lines'
worth, is the tool talking about itself in the middle of a diagnosis. It belongs
in the documentation, or at most one line per run.

## The four kinds

Naming them is the fix, because each wants different treatment:

| Kind           | Example                                                   | Where it belongs                         |
| -------------- | --------------------------------------------------------- | ---------------------------------------- |
| **Problem**    | broken link, malformed frontmatter, missing Loader        | a finding, always shown                  |
| **Proposal**   | "possible target: `guidance/testing.md`"                  | **inside** its problem, never standalone |
| **Evidence**   | "filename match", "route neighborhood", "literal content" | inside the proposal, at high detail only |
| **Disclosure** | "external URLs are not fetched"                           | once per run, or in `--help`             |
| **Coverage**   | "5,614 links resolved"                                    | a count, not 5,614 entries               |

Today all five arrive as findings, separated only by `INFO` / `WARNING`. That
single dimension cannot express "this is a problem" versus "this is how you might
fix it" versus "this is what I looked at".

## Proposed shape

One problem, its proposals nested, its evidence behind detail, and coverage as a
count:

```
2 problems in 21 links.

  .agents/loader.md:105          broken link -> patterns/_patterns.md
                                 the target does not exist
                                 possible: .agents/patterns/_patterns.md  (filename match)
                                 open-forge repair --relink .agents/loader.md:105

  .agents/maps/_maps.md:32       broken link -> nowhere/_nope.md
                                 no candidate found

  5,614 links resolved · 98 external links not checked
```

That is the whole of the current 8.8 MB output, at `standard`. The evidence
behind each proposal — filename match, route neighborhood, literal content,
title — appears at `verbose`, attached to the proposal it justifies.

## The rules that follow

- **A finding is a problem.** If nothing is wrong, it is not a finding. This
  deletes `target-valid`, `cycle`, `repeat`, and `external-unchecked` as
  findings — 8,454 of 11,455.
- **A proposal is a property of a problem, not a sibling of it.** Candidates
  nest inside the finding they resolve and inherit its severity. This removes
  2,305 warnings.
- **Evidence is a property of a proposal.** It exists to justify a suggestion and
  is only interesting when the suggestion is being evaluated. `verbose` only.
- **Coverage is a count.** "5,614 links resolved" replaces 5,614 entries.
- **A disclosure is said once, if at all.** "98 external links not checked" is
  one clause. That Open Forge does not fetch URLs is a documented property of the
  tool.

Applying only the first two rules takes this repository from 11,455 findings to
**696**, without deciding anything about presentation. The rest of the reduction
comes from the View-selection layer in G4.

## `route.axioms-invalid` — the borderline case

38 occurrences, and it illustrates why the `rules` config matters. Under the
proposed change that treats an absent or empty `## Axioms` as _inherited_
([structural-requirements-and-markers.md](structural-requirements-and-markers.md)),
most of these stop existing. What remains — a genuinely malformed or duplicated
section — is a real problem.

But whether a workspace wants to hear about it is a workspace decision. This is
exactly the class the `rules` map serves:

```json
{ "rules": { "route.axioms-invalid": "off" } }
```

The general principle: **structural preferences are configurable; correctness is
not.** A broken link is always a problem. A missing optional section is a
matter of house style.

## Relationship to the rest of the audit

This is upstream of the presentation work. G4's selection layer decides _how much
of a finding to show_; this decides _what is a finding at all_. Doing G4 without
this would produce a well-ordered, well-truncated rendering of 11,455 things that
mostly should not exist.

Order: fix the finding model, then apply the view model. The measured effect of
the first alone — 11,455 to 696 — is larger than anything presentation can
achieve on its own.
