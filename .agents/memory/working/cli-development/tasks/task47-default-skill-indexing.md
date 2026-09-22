---
open-forge:
  description: Task 47 follow-up to make default Index reach routed resources through native Skills
  tags: [Memory, Working, CLI, Task, Skill, Index, Contextual]
---

# Task 47 follow-up — default Skill indexing

## Outcome and authority

Recorded at the maintainer's request on 2026-09-22 under
[Task 47](task47-entrypoint-reachability.md), coordinated with
[Task 46](task46-routed-skill-resources.md). The desired outcome is for default
Index to reach a native `SKILL.md` and the routed resource catalogues beneath
that Skill without requiring the user to name the catalogue manually.

This selects a future capability for specification. The present instruction is
to record the task, not implement it. The exact traversal and write contract
must be settled before mutation or new scenario expectations are approved.
The current work is the separate [lossless wording proposal](../../src-wording-proposal.md).

## Current evidence

`use-workflow` is provided by the optional `workflows` Extension. Core provides
the Skills route, not the installed selector. The native Skill points to
`references/_references.md`; other Extensions add scoped recipe catalogues.

The current [Index selection resolver](../../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSelectionResolver.cs)
starts its default selection from Loader roots and recognized entrypoint
closures. The [published recovery journey](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs)
proves that adding a Planning recipe leaves its detached catalogue stale after
plain Index, while explicitly indexing that catalogue repairs it. The recent
Doctor correction now supplies the accurate targeted command.

That correction is complete. This task concerns automatic reachability, not a
return to a misleading bare Index recovery suggestion.

## Proposed user journey

A user installs Core and an Extension supplying a native Skill, then adds a
valid document beneath one of the Skill's routed reference catalogues. They
run `open-forge index` with no source argument. The relevant generated Entries
include the document; Doctor no longer reports that catalogue as stale.
The Skill instructions and authored document bytes stay unchanged. Repeating
Index makes no further changes.

This is the desired journey to specify, not a claim of existing behavior or
an approved new test implementation.

## Decisions to freeze

- Define how a discovered native Skill connects to its resource catalogues:
  recognized catalogue descendants, explicit contained Markdown links, or a
  bounded combination. Do not equate every link with a route or scan the whole
  workspace merely to make the example pass.
- Distinguish selecting `SKILL.md` as a traversal boundary from rewriting it.
  Native frontmatter and instructions must remain intact. Whether any authored
  Entries section in a Skill is supported needs an explicit contract; do not
  invent one or inject a generated section by default.
- Define discovery outside the standard Skills route, detached/unexposed Skills,
  multiple or absent catalogues, native metadata, compatibility entrypoint names,
  nested Skills, cycles, duplicate reachability, aliases and physical containment.
- Align Index selection/counts/dry-run output with Doctor, route inspection and
  context selection where they share topology facts. Index reachability does not
  automatically make reference bodies #LoadNow or authorize recipe execution.
- Preserve bounded filesystem effects and performance. Define how invalid,
  inaccessible or escaping resources are diagnosed without unauthorized writes.

## Specification and verification work

1. Trace Skill classification, discovery, generated-navigation topology and
   Index selection. Resolve Task 46's metadata questions once, with one shared
   model for the affected consumers.
2. Freeze before/after outcomes for default and explicit selection. Include the
   existing targeted-catalogue success case as preservation evidence.
3. Specify the journey above plus no-catalogue, multiple-catalogue, cycle/alias,
   malformed/unreadable and escaping-path cases only where supported by the
   accepted model. Agree on messages, status/exits, streams and JSON facts before
   authoring expectations.
4. Implement one bounded change after selection. Use pure topology/mapping tests
   where no real filesystem is needed and real-OS integration evidence where it
   is. No filesystem mock layer or broad filesystem abstraction is implied.
5. Verify dry-run makes no writes, apply only changes permitted generated
   interiors, authored bytes are preserved, repeated Index is idempotent, and
   Doctor agrees. Run the applicable managed/native gates and update contracts,
   help and current task state together.

## Current state

Recorded; not started. No code, source payload, snapshots or scenario tests are
changed by this task. Next action: settle the traversal model and its exact
expectations when this follow-up is selected for execution.
