---
open-forge:
  description: Measured comparison of a three-band and a four-layer grouping for the CLI, what the tree already satisfies, and why cross-layer purity matters more than the intra-layer cycles
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Architecture, Layers, Dependencies, Testing]
---

# Layer Adherence

Asked whether the CLI should be grouped as **Shell → Processing → Presentation**
with sub-bands, instead of the four layers recorded in
[CLI Layers](../../../crystallized/documents/cli/layers/_layers.md), and whether
the intra-layer cycles then stop mattering.

Measured against `OpenForge.Cli.Core`, 2,270 files, by counting every `using`
that crosses a band.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## The short answer

**Both models are right, and they are not competing.** Three bands is the
_reading order_ — it answers "what happens to a request, in sequence". Four
layers is the _dependency rule_ — it answers "what may reference what". A
codebase needs both, and they describe the same tree at different resolutions.

**The instinct that intra-layer cycles matter less is correct**, and it is worth
acting on: every cycle currently in the tree is inside one band under either
model, so none of them breaks layer purity. They cost testing convenience, not
architectural integrity, and they should be ranked accordingly.

**The top boundary is already almost pure**, which is the result that decides
how much this is worth.

## What the tree already satisfies

| Invariant                           | State                                                                                         |
| ----------------------------------- | --------------------------------------------------------------------------------------------- |
| Framework never depends on Commands | **Holds.** Zero files.                                                                        |
| Framework never depends on Shell    | **Holds.** Zero files.                                                                        |
| Shell never depends on Commands     | **Holds**, after one regression was reverted — see below.                                     |
| Commands depend inward on Framework | 785 files. Correct direction.                                                                 |
| Commands depend on Shell contracts  | 491 files. Correct direction.                                                                 |
| Shell depends on Framework          | 9 files, all in composition, for `CliWorkspace`. Not prohibited by the accepted Architecture. |

That first row is the valuable one. **Framework has no path back into a command
at all**, which is what makes it independently testable and what a merged
"Processing" band would stop stating.

### One regression, found by this scan and reverted

Phase 1 moved `CliCompactJsonProjection` onto the shared
`WorkspaceSelectionWireVocabulary`, which lived under `Commands/Shared/`. That
created the **only** `Shell → Commands` dependency in the tree and violated the
accepted rule _"Shell types do not depend on concrete commands"_.

The dedup was right; the destination was wrong. The vocabulary maps
`CliWorkspaceSelectionMethod`, a Shell-and-Framework concern consumed by both
Shell and Commands, so it belongs beside `CliHumanText` in
`Shell/Presentation/Shared/Rendering/`. Moved there; the invariant holds again.

The lesson generalises: **when deduplicating, the owner goes at the narrowest
scope that covers every consumer — and "narrowest" is measured against the layer
graph, not the folder tree.** A shared owner placed one layer too low inverts a
dependency silently, and nothing but a boundary scan catches it.

## The two groupings, side by side

Files were assigned to bands by what they do, then edges counted between bands.

| Grouping                                                   | Violating edge kinds | Files involved |
| ---------------------------------------------------------- | -------------------- | -------------- |
| Four layers: Shell → Framework → Operations → Presentation | 2                    | ~70            |
| Three bands: Shell → Processing → Presentation             | 2                    | ~70            |

**Identical.** Inspection shows most of the ~70 are a classification artifact —
files such as `Commands/Find/Models/Presentation/FindPresentationModels.cs` are
_models_ that the scan banded as presentation because of the path segment — and
the genuine remainder are composition edges, where an operation factory wires up
its own renderer. Composition legitimately reaches across layers; that is what
composition is.

So neither model is further from the code than the other. The choice is about
what each one lets you state.

### What four layers buys

`Operations → Framework` is 2,541 file-edges in one direction and **zero** back.
Naming Framework as its own layer is what turns that into a checkable rule. A
merged Processing band still has the property but no longer asserts it, and an
unasserted property is one refactor away from being lost — as the regression
above demonstrates within a single phase.

### What three bands buys

It matches how a request actually moves, and it is the shape a person holds in
their head. The four-layer record states the order but a reader has to assemble
it; three bands state it directly.

### The reconciliation

Keep four layers as the dependency rule, and state the three-band reading order
on top of it. That is what the layers record already does, and the fix is
presentational rather than structural: lead with the three bands, then give the
four layers as the enforcement.

```text
Shell            arguments -> a request
  Processing     Framework facts -> command meaning -> one result
Presentation     a result -> text -> an exit code
```

with `Processing` being exactly `Framework` then `Operations`, in that order,
never the reverse.

## Where the cycles land

All six Framework cycles — `Sources`↔`Workspace`,
`Sources`↔`OperationalContributors`, `Sources`↔`GeneratedNavigation`,
`Extensions`↔`Lifecycle`, `Mutation`↔`Recovery`, `Libraries`↔`Permissions` — are
**inside Framework**, so they are intra-band under both models.

The instinct is right: they do not break layer purity, and they are not the
thing to fix first. They do have a cost, and it is the one the grouping was
meant to buy:

- A cycle means the two capabilities cannot be tested independently. `Sources`
  cannot be exercised without `Workspace`, and the reverse.
- A cycle usually means the two are one capability that has not been named, or
  that one reached for a fact it should have been handed.

Both are real, and both are cheaper to fix than a cross-layer violation. Ranked
below boundary purity, not dismissed.

## Is it worth doing, and what it implies

**Worth doing: yes, but as a rule to enforce rather than a refactor to perform.**
The expensive version — restructuring folders to match the bands — buys almost
nothing, because the dependency facts already hold. The cheap version buys the
whole benefit:

- **A boundary test.** One test that reads the `using` graph and asserts the
  four invariants above. It is perhaps thirty lines, runs in the unit suite, and
  would have caught the phase 1 regression on the commit that introduced it.
  This is the single highest-value item in this document.
- **A directive clause** placing a shared owner at the narrowest scope that
  covers its consumers _in the layer graph_, so the next dedup does not repeat
  the mistake.
- **The three-band reading order** added to the layers record, which is writing,
  not refactoring.
- **The six cycles**, inverted one minority edge at a time, opportunistically,
  when the surrounding code is already being changed.

**What it does not imply:** no folder moves, no namespace churn, no change to
`Commands/`, `Framework/` or `Shell/` as top-level areas. The tree is already
the shape both models describe.

## Method

One script, `using`-graph based: every `using OpenForge.Cli.Core.*` in every
file, mapped from the declaring file's band to the referenced namespace's band,
counted per band pair, then evaluated against each grouping's allowed
directions. Band assignment is by path, which is why presentation-named model
folders skew the raw violation count and why the residue was inspected by hand
rather than trusted.
