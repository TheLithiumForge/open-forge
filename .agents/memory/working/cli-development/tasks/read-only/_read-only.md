---
open-forge:
  description: Implement retained read-only source, context, extension, and generated-navigation commands
  tags: [Memory, Working, CLI, Task, ReadOnly, Source, Extension, Index, Contextual]
---

# Read-Only Commands

## Task State

- State: Active. References is Active in Preflight after maintainer selection and
  is blocked on its exact result, finding-code, and Markdown-reference contract
  closure. Find is Complete and squash-integrated into local `develop` at
  `1f03d16`. The repository-root developer workflow is Complete and
  squash-integrated into local `develop` at `d9e0686`. Find Child 1 is Complete and
  accepted at exact commit `96fe413`
  (`Accept Find source catalogue`). Modern C# Improvements is Complete and
  accepted at exact `a1cbf09`. Its Preflight was accepted at `55eb82e`, Framework
  at `a90af59`, Shell/root at `fe10525` (`Modernize Shell nullable flow`), Route
  Inspect/family at `62a1dd9` (`Modernize Route Inspect nullable flow`), Route List
  at `273eb45` (`Modernize Route List nullable flow`), and Tests/support at exact
  clean source commit `6af5fb1` (`Modernize test support nullable flow`). All five
  ordered mutation batches and the final gate are accepted, with no task-local
  correction pass consumed. Find Child 2 is Complete and accepted at exact
  `ff7ce3f` (`Accept Find query operation`). Child 3 focused Preflight is accepted
  at exact `28d316a`, Gray at exact `a76a217`, original Red at exact `22d3bff`,
  the Integration metadata correction at exact `6a9a0de`, and the mirrored
  EndToEnd metadata correction at exact `eea3d59` (`Complete Find presentation
metadata evidence`). Its post-commit Red reproduction succeeded as intentional
  Red: managed non-AOT `win-x64` publish passed, and published Find EndToEnd was
  `13` total with `13` intentional failures and zero skips, all terminating at
  absent Green root registration. Historical pre-correction Green review returned
  narrowly to Red for the frozen-contract human `\t`/lowercase `\uXXXX` mismatch
  and diagnostic escaped-code-unit slicing defect. Supplemental escaping Red
  correction is accepted at exact `a865fd1` (`Correct Find escaping evidence`),
  with corrected Red focused Unit `206` total, `92` pass, `114` intentional
  Gray-boundary failures, and zero skips. Child 3 Green is accepted at exact commit
  `cb7874c` (`Implement Find presentation`) over corrected Red `a865fd1`. It is
  production/root-composition-only, with no test, support, contract, project,
  package, configuration, generated-routing, or Route behavior change. Its exact
  scope is the 15 production paths for Find binding/request/result builder/
  validation, compact/expanded/JSON/diagnostic/help/shared text escaping, Shell
  direct-root command tree/root factory, and root composition. Fresh Green evidence
  passes all recorded focused, affected, publish, and `13/13` published Find
  EndToEnd gates with zero skips; the final bounded correctness review is `PASS`
  with no material findings. Blue is accepted at exact `3f81e76`
  (`Simplify Find JSON projection`) after changing production structure only in
  `src/cli/core/OpenForge.Cli.Core/Commands/Find/Shared/Rendering/FindJsonProjection.cs`:
  the duplicate local finite status and finding-code switches were replaced by
  `CliStatusDefinitions.Read(...).MachineName` and
  `FindDefinitions.ReadFindingCode(...)`, and the two duplicate private mapping
  methods were removed. No behavior, public output/order/schema, test/support,
  contract, package/project/configuration, generated routing, Shell/root, Route,
  workspace-write, or Native AOT change occurred. The warning-free Release build,
  format verification, diff check, focused Unit `206/206`, and focused Integration
  `43/43` pass with zero skips; no managed republish or Native AOT claim is needed.
  Fresh bounded Blue correctness review is `PASS`: all 7 status and 17 finding-code
  mappings and undefined-value exception behavior are exact; JSON model/property
  order/context is unchanged; static canonical readers remain source-generation/
  AOT-safe. Fresh local improvement review is
  `APPROVED — NO_MATERIAL_IMPROVEMENTS`. Fresh Purple assessment ran read-only
  from exact clean Blue `3f81e76` against the exact ten Find Child 3 test/support
  surfaces. Verdict: `NO_MATERIAL_IMPROVEMENTS`; the no-op Purple acceptance is
  recorded at exact `426d4f5`. Its focused Unit `206/206`,
  focused Integration/serialization `43/43`, and published Find EndToEnd `13/13`
  evidence pass with zero skips, and source diff/check against `3f81e76` is
  clean/empty. No test/support, production, contract, project, package,
  configuration, generated, Route, Shell, or root file changed, and no Purple
  code/test commit is manufactured. No Native AOT claim is made, and the
  record-only commit is not a test change. Final acceptance is recorded in the
  commit containing this record update.
- Responsible role: Mastermind.
- Last updated: 2026-08-25.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisites: Foundation, Route Discovery, Generic CLI Improvements, and Find
  Child 1 are accepted. The exact production/source baseline before Find remains
  `063c59d`.

## Outcome And Boundaries

Read-only commands answer source, reference, context, extension, and generated
navigation questions without creating locks, lifecycle files, caches, indexes
outside explicit `index`, recovery artifacts, or workspace mutations.

Find proceeds through its ordered source-catalogue, query-operation, and
presentation/acceptance children. No shared source capability may be mutated in
parallel. References may begin only after the source and document facts it needs
are accepted. Context waits for accepted Find and References facts. Extension
discovery may proceed independently on its accepted source-catalogue boundary.
Index waits for routing, document, source, and reference facts.

Every child owns its request, result, findings, presentation, help, and public
scenario. Shared facts move only through a Mastermind integration increment.

## Child Tasks

- [x] [Implement deterministic CommonMark-aware source discovery and accepted Find projections](find.md) — Complete and integrated at `1f03d16`; Child 1 is Complete at exact `96fe413`, Child 2 at exact `ff7ce3f`, and Child 3's no-op Purple at exact `426d4f5`. The final managed/native gate and package, artifact, static, public no-write, and protected-surface audits passed — Implementer: Mastermind
- [ ] [Implement direct incoming and outgoing reference facts with shared source-universe filters](references-command.md) — Active in Preflight; production blocked on contract closure — Implementer: Mastermind
- [ ] [Implement ordered context selection, loading reasons, overlap, size, and token projections](context.md) — Planned — Implementer: Not assigned
- [ ] [Implement Extension catalogue and source listing without lifecycle inference](extension-list.md) — Planned — Implementer: Not assigned
- [ ] [Implement exact Extension package inspection without mutation or installation behavior](extension-inspect.md) — Planned — Implementer: Not assigned
- [ ] [Implement deterministic generated Entries projection and idempotent index application](index-command.md) — Planned — Implementer: Not assigned

## Historical Find Progress And Current Routing Condition

The Find planning packet is accepted at `b2e3106` on clean branch
`feature/cli-find`; `develop` remains `e77902a`, and the production/source
baseline before Find remains exact `063c59d`. Child 1 is accepted at exact
`96fe413`, and Child 2's query implementation and typed result are accepted at
exact `ff7ce3f`. Child 3 Gray is accepted at exact `a76a217`, and original Red is
accepted at exact `22d3bff`, with metadata corrections at exact `6a9a0de` and
`eea3d59`, and supplemental escaping Red correction at exact `a865fd1`. The
current corrected Red Unit boundary is `206` total with `92` pass and `114`
intentional Gray-boundary failures, zero skips. Child 3 Green is accepted at
exact `cb7874c` (`Implement Find presentation`) over corrected Red `a865fd1`;
fresh focused and affected evidence passes, published Find EndToEnd is `13/13`,
and the final bounded correctness review is `PASS` with no material findings.
Blue is accepted at exact `3f81e76` (`Simplify Find JSON projection`) after the
one-file `FindJsonProjection.cs` canonical mapping simplification. Its focused
Unit `206/206` and generated-serialization Integration `43/43`, warning-free
build, format verification, and diff check pass with zero skips; its bounded
correctness review is `PASS`, and its local improvement review is
`APPROVED — NO_MATERIAL_IMPROVEMENTS`. The no-op Purple acceptance is recorded
at exact `426d4f5` with verdict `NO_MATERIAL_IMPROVEMENTS`;
focused Unit `206/206`, focused Integration/serialization `43/43`, and published
Find EndToEnd `13/13` pass with zero skips, and source diff/check is clean/empty.
No test/support, production, contract, project, package, configuration,
generated, Route, Shell, or root file changed, and no Purple code/test commit is
manufactured. No Native AOT claim was made, and the record-only commit was not a
test change. Final acceptance is recorded in the commit containing this record
update. The [Child 1 acceptance record](find-source-catalogue.md)
contains the neutral authority, Route-local policy boundary, final evidence, and
recorded cancellation limitation.

Find Child 3 and the Find parent are Complete. Their accepted feature tree was
squash-integrated into local `develop` at `1f03d16`. The final managed/native gate
and package, artifact, static, public no-write, and protected-surface audits remain
the accepted Find evidence. The repository-root developer workflow is Complete at
`d9e0686`. The maintainer selected References before Context; its Preflight is
active and production remains blocked on the contract boundary recorded in its
Task.

The temporary compatibility-name correction renames the Working Index and
References Task files and stages their command contracts under candidate route
names until the replacement Index command proves final physical-identity
handling. The repository index manager refreshed generated Entries after the
renames. These explicit parent-before-children links retain phase state and
history that generated navigation does not define:

1. [Implement Find](find.md)
2. [Establish The Neutral Find Source Catalogue](find-source-catalogue.md) — Complete
3. [Modern C# Improvements](../modern-csharp-improvements.md) — Complete; all five ordered modernization batches and the final managed, Native AOT, package, audit, and public no-write gate are accepted
4. [Implement The Find Query Operation](find-query-operation.md) — Complete; final acceptance at exact `ff7ce3f`
5. [Present And Accept Find](find-presentation-acceptance.md) — Complete and
   included in the Find tree integrated at `1f03d16`; Preflight `28d316a`, Gray
   `a76a217`, original Red `22d3bff`, metadata corrections `6a9a0de` and
   `eea3d59`, supplemental escaping Red `a865fd1`, Green `cb7874c`, Blue
   `3f81e76`, and no-op Purple `426d4f5` remain recorded.

## Entries

<!-- open-forge:generated-index:start -->

- [Implement ordered context selection, loading reasons, overlap, size, and token projections](context.md) - #Memory #Working #CLI #Task #Context #ReadOnly #Loading #Contextual
- [Implement exact Extension package inspection without mutation or installation behavior](extension-inspect.md) - #Memory #Working #CLI #Task #Extension #Inspect #ReadOnly #Contextual
- [Implement Extension catalogue and source listing without lifecycle inference](extension-list.md) - #Memory #Working #CLI #Task #Extension #List #ReadOnly #Contextual
- [Author and accept deterministic CommonMark-aware source discovery and Find projections](find.md) - #Memory #Working #CLI #Task #Find #ReadOnly #Markdown #Contextual
- [Bind, present, and accept the non-shipping Find command through its public views, JSON, diagnostics, and evidence](find-presentation-acceptance.md) - #Memory #Working #CLI #Task #Find #ReadOnly #Presentation #Acceptance #NativeAOT #Contextual
- [Implement the exact Find query operation over the accepted source catalogue and fixed Markdown facts](find-query-operation.md) - #Memory #Working #CLI #Task #Find #ReadOnly #Query #Markdown #Matching #Projection #Contextual
- [Establish the neutral Framework source catalogue and migrate Route List and Inspect without public behavior change](find-source-catalogue.md) - #Memory #Working #CLI #Task #Find #ReadOnly #Sources #Route #Framework #Architecture #Contextual
- [Implement deterministic generated Entries projection and idempotent index application](index-command.md) - #Memory #Working #CLI #Task #Index #Generated #Mutation #Contextual
- [Implement direct incoming and outgoing reference facts with shared source-universe filters](references-command.md) - #Memory #Working #CLI #Task #References #ReadOnly #Links #Contextual

<!-- open-forge:generated-index:end -->
