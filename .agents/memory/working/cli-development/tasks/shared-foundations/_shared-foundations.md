---
open-forge:
  description: Add the interaction, Framework distribution, lifecycle provenance, and directory-create prerequisites for the next command wave
  tags: [Memory, Working, CLI, Task, Foundation, Shell, Framework, Lifecycle, Contextual]
---

# Next-Wave Shared Foundations

## Task State

- State: Ready after this contract freeze is integrated.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Profile: Assured because these four foundations cross public interaction,
  Native AOT distribution, persisted lifecycle, and filesystem mutation
  boundaries.
- Baseline: the exact integration commit containing this contract freeze.

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
Install depends on SF1-SF4 plus the accepted intended-membership formation. Root
composition, shared JSON registration, public help/process evidence, and program
ledgers remain sequential integration-owned surfaces.

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
  revalidation, ordinary BCL creation, verification, and neutral residual facts
  for descendants below `.agents`. The existing lock manager owns and reports
  the one visible missing-`.agents` bootstrap immediately before lease
  acquisition. Directory selection and command results remain local.
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

- [ ] [Add one native Shell question-and-answer transport](interactive-session.md) — Ready after contract-freeze integration.
- [ ] [Embed and read the canonical Framework payload](framework-payload.md) — Ready after contract-freeze integration.
- [ ] [Add per-target Framework source-asset provenance](lifecycle-provenance.md) — Ready after contract-freeze integration.
- [ ] [Add one lease-bound ordinary-BCL directory-create effect](directory-create.md) — Ready after contract-freeze integration.

## Entries

<!-- open-forge:generated-index:start -->
- [Add one lease-bound ordinary-BCL directory-create mutation effect](directory-create.md) - #Memory #Working #CLI #Task #Foundation #Mutation #Directory #Contextual
- [Embed and read the canonical Framework payload through ordinary BCL resources](framework-payload.md) - #Memory #Working #CLI #Task #Foundation #Framework #Distribution #EmbeddedResource #NativeAOT #Contextual
- [Add one native Shell question-and-answer transport without a prompt framework](interactive-session.md) - #Memory #Working #CLI #Task #Foundation #Shell #Interaction #Contextual
- [Add exact per-target canonical source-asset provenance to Framework lifecycle schema v1](lifecycle-provenance.md) - #Memory #Working #CLI #Task #Foundation #Framework #Lifecycle #Provenance #Contextual
<!-- open-forge:generated-index:end -->
