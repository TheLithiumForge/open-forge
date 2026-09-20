---
open-forge:
  description: Add the interaction, Framework distribution, lifecycle provenance, and directory-create prerequisites for the next command wave
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Foundation, Shell, Framework, Lifecycle]
---

# Next-Wave Shared Foundations

## Task State

- State: Complete. D0 and all four foundations are integrated in local
  `develop`; the combined acceptance gate passes as recorded below.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Profile: Assured because these four foundations cross public interaction,
  Native AOT distribution, persisted lifecycle, and filesystem mutation
  boundaries.
- Baseline: `33913dfe7f8f80598ca4765c516d308ed179c3ab`, exact tree
  `a56f3c201013b5999841414e1df469713f08cdfe`.

## Integrated Acceptance

The accepted candidates are squash-integrated in dependency order with exact
tree equality:

- D0 contract freeze: commit `38e1498de19a40f82d830a43dd0c419fd651bbc8`,
  tree `5dc4abc3deb658dba7d0dc5236db362fe79446dd`.
- SF1 interaction: commit `e782090cfd58d8bd7b9de2c92aaa43c977eb989f`,
  tree `001ac3c00772ed30086e5481aecb9d26346a822b`.
- SF2 embedded payload: commit `680915ab36fdcfd0eabe631cde47f83fc6cc9951`,
  tree `88b742a27f6a5630ae73c5e9c34afae99d61df19`.
- SF3 lifecycle provenance: commit
  `098935695abbee62bd960937e228eb53ad25a2c8`, tree
  `72b44bc13013125e5bb5f9f2751f19ed2ef9a054`.
- SF4 directory mutation: commit
  `33913dfe7f8f80598ca4765c516d308ed179c3ab`, tree
  `a56f3c201013b5999841414e1df469713f08cdfe`.

The reviewed combined baseline has a Release build with `0` warnings and `0`
errors; managed Unit `1284/1284`, Integration `500/500`, and EndToEnd `125/125`;
Native AOT Integration `500/500` and EndToEnd `125/125`; and zero skips in every
stated run.

The later accepted lock-location correction removes lock bootstrap state from
`WorkspaceLockResult`. The lock is a persistent external zero-byte ordinary file
under `LocalApplicationData/OpenForge/locks/v1`; missing `.agents` is an ordinary
lease-bound directory-create effect owned by the command plan.

## Outcome And Dependency Graph

Four non-overlapping foundations may execute in parallel:

```text
SF1 Native Interaction ─┬─ Extension Create
                       ├─ Route Inspect interaction correction
                       └─ Root Install
SF2 Framework Payload ───── Root Install
SF3 Lifecycle Provenance ── Root Install
SF4 Directory Create ────── Root Install ── Route Init
```

Extension Create and the Route Inspect correction depend only on SF1. Root
Install depends on SF1-SF4 plus the accepted intended-membership formation. Its
non-wire command-local/module work may proceed independently alongside C1 and
C3. The exact public Install JSON result schema is accepted and frozen,
including typed residual values `none`, `retained`, and `unknown`. Root
composition, shared JSON
registration, public help/process evidence, and program ledgers remain
sequential integration-owned surfaces; this does not imply a C1/C3 behavior
dependency.

## Shared Boundaries

- Use ordinary modern C#, pinned runtime behavior, BCL APIs, source-generated
  serialization, and real owned streams/filesystems. Exceptional machinery:
  `none`.
- `Shell/Interaction` owns terminal transport only. Command questions, choices,
  defaults, validation, confirmation, and results remain local.
- `Framework/Distribution` owns exact embedded asset and inventory facts only.
  Commands own installation and route policy.
- `Framework/Lifecycle` owns transparent persisted provenance only. It does not
  gain an instance registry, migration engine, or command policy.
- The shared directory capability owns only lease-bound missing-directory
  revalidation, ordinary BCL creation, verification, and neutral residual facts.
  Commands plan `.agents` and its descendants in parent-first order after the
  external lease is acquired. Directory selection and command results remain
  local.
- Do not change product behavior, add or remove features, or choose an
  architectural alternative without maintainer acceptance. Agents may surface
  alternatives and evidence upward.

## Protected Integration Surfaces

- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`
- `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`
- root command tree/help and broad process tests
- command folders outside the selected child Task
- program-level Plan, Task, and Checkpoint files

## Evidence And Integration

Each child runs the focused evidence available within its owned boundary. SF1
proves the neutral transport in Unit evidence; its protected root/command and
published-process evidence runs with the first prompt-capable consumers. SF2-SF4
additionally prove their affected published `linux-x64` Native AOT boundary.
After all four candidates integrate sequentially, run
format, warning-free Release build, complete managed regression, supported
Native AOT publication/execution, and exact changed-path/dependency audits.

## Child Tasks

- [x] [Add one native Shell question-and-answer transport](interactive-session.md) — Complete at integrated `e782090`.
- [x] [Embed and read the canonical Framework payload](framework-payload.md) — Complete at integrated `680915a`.
- [x] [Add per-target Framework source-asset provenance](lifecycle-provenance.md) — Complete at integrated `0989356`.
- [x] [Add one lease-bound ordinary-BCL directory-create effect](directory-create.md) — Complete at integrated `33913df`.

## Entries

- [Add one lease-bound ordinary-BCL directory-create mutation effect](directory-create.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Foundation #Mutation #Directory
- [Embed and read the canonical Framework payload through ordinary BCL resources](framework-payload.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Foundation #Framework #Distribution #EmbeddedResource #NativeAOT
- [Add one native Shell question-and-answer transport without a prompt framework](interactive-session.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Foundation #Shell #Interaction
- [Add exact per-target canonical source-asset provenance to Framework lifecycle schema v1](lifecycle-provenance.md) - #Memory #Archived #Contextual #Historical #CLI #Task #Foundation #Framework #Lifecycle #Provenance
