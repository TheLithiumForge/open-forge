---
open-forge:
  description: Historical lessons and salvage inventory from the removed C# CLI and route-list implementation
  responsibility: Preserve useful implementation evidence without making the removed source or program current authority
  tags: [Memory, Archived, Contextual, Historical, CLI, Architecture, Implementation, Testing, Reset]
---

# CLI Implementation Reset

## Status And Source

The maintainer stopped the first C# replacement implementation on 2026-08-21 and
authorized a greenfield architecture under `src/cli/`. Git commit `4b873de`
preserves the final route-list WIP. Commit `aa7d178` preserves the working program
records and the governance added immediately before cleanup. Neither commit is
current implementation authority.

The old production project, repository-root C# configuration, Native AOT
workflow, and active CLI release records were removed. Existing test source was
moved to `src/cli/tests/` as candidate evidence for later Tasks.

## Ideas Worth Preserving

The new architecture should reevaluate and retain these ideas when they still fit
the complete system:

- One explicit command tree built from typed parser symbols rather than string
  dispatch, reflection, scanning, or service location.
- Parser-owned typed values, with any lexical guard limited to syntax the parser
  cannot expose.
- Immutable invocation, request, result, presentation, rendered-output, output,
  and process-completion messages.
- Directly callable stages that validate before effects, invoke an operation at
  most once, select one renderer, and write through explicit output boundaries.
- Concrete command results under one fixed semantic status and process-exit
  policy, without a universal domain result.
- Source-generated serialization with reflection disabled and actual Native AOT
  execution evidence.
- Real `System.IO` integration tests, owned temporary workspaces, unchanged-byte
  assertions, deterministic projections, and built-process end-to-end evidence.
- Separate cheap unit, real-boundary integration, and complete-process evidence.
- Managed BCL-first physical filesystem work with an Architecture stop when the
  required guarantee cannot be proved portably.

## Boundaries Not To Restore

The new implementation must not restore these shapes merely because they existed
or passed earlier evidence:

- Replacement-specific C# control files at the repository root.
- Command behavior implemented before the global architecture and actual
  foundation are complete.
- One oversized Task that mixes architecture, several unresolved capabilities,
  behavior, structure, and acceptance.
- Route-specific branching and policy in the process-wide application shell.
- Large definition, containment, selection, Loader, or rendering files that mix
  independently testable responsibilities.
- Help text that post-processes rendered library output or repeats symbol
  spellings outside their definitions.
- Physical-containment checks that validate only a final target and allow an
  intermediate path to leave and later re-enter the workspace.
- Passing test counts as a substitute for top-down architecture review, complete
  contract coverage, or Native AOT process evidence.

## Preserved Test Evidence

The files under `src/cli/tests/` preserve useful contract examples, fixtures, and
failure cases. They are intentionally non-authoritative until a new Task maps each
case to a current contract and target production boundary. A later Task may keep,
rewrite, split, or remove a test. It must preserve the underlying accepted
behavior whenever the command contract still requires it.

The returning-link-chain case is especially important. It proves that physical
containment must reject the first transition outside a selected workspace even
when a later link points back inside.

## Replacement

The current [Replacement CLI Architecture](../../crystallized/documents/cli/architecture.md)
defines the greenfield boundary. The new top-down Plan and its child Tasks will
replace the removed active program before implementation resumes.
