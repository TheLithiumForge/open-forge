---
open-forge:
  description: Correct three bounded read-only CLI dogfooding defects before Mutation Foundation resumes
  tags: [Memory, Working, CLI, Task, ReadOnly, Context, Find, RouteInspect, Dogfooding, Contextual, Complete]
---

# Correct Read-Only CLI Dogfooding Defects

## Task State

- State: Complete on local feature candidate `deb3f14` from exact clean
  integrated baseline `b6ce31f`; local squash integration remains a separate
  Overseer gate.
- Responsible role: Overseer acting directly and sequentially.
- Parent: [Complete The Replacement CLI](00-cli-development.md).
- Task source: Maintainer direction and accepted findings `CLI-EDGE-011`,
  `CLI-EDGE-012`, and `CLI-EDGE-015`.
- Last updated: 2026-08-27.

## Problem And Expected Outcome

Three bounded defects reduce trust in the otherwise accepted read-only CLI:
Context interprets native Skills through the Open Forge-only metadata parser,
compact Context and Find findings omit known source coordinates, and Route Inspect
help still calls implemented Context unavailable. Correct those defects without
reopening command grammar, selection, statuses, JSON, diagnostics, or unrelated
repository metadata.

The completed result consumes the accepted form-aware
`SourceAuthoredMetadataFacts`, names one bounded escaped affected coordinate in
each compact finding when typed evidence knows one, and describes Context as an
available related command. Valid and malformed Skill behavior, distinct compact
finding paths, published-process composition, and supported local Native AOT
execution have focused regressions.

- Execution profile: Standard, direct, sequential correction. The defects cross
  three accepted read-only commands but require no architectural decision, new
  dependency, mutation, or parallel lane.

## References And Authority

| Source | Question it answers | Authority | Change boundary |
| --- | --- | --- | --- |
| [CLI edge-case ledger](../edge-cases.md) | Which defects and closure conditions are accepted? | Active working evidence | Close only `CLI-EDGE-011`, `012`, and `015` |
| [Routed Authored Metadata](read-only/routed-authored-metadata-foundation.md) | Which parser and source-form semantics own Skill metadata? | Accepted and integrated | Consume; do not replace or reclassify |
| [Context Interface](../../../crystallized/documents/cli/contracts/context/interface.md) | What must Context expose? | Accepted contract | Preserve all meaning except accepted correction |
| [Find Interface](../../../crystallized/documents/cli/contracts/find/interface.md) | What must Find expose? | Accepted contract | Preserve all meaning except accepted correction |
| [Route Inspect Interface](../../../crystallized/documents/cli/contracts/route/inspect/interface.md) | What must Route Inspect help expose? | Accepted contract | Correct related-command availability only |
| [C# Callable Design](../../../../directives/csharp/design.md) | How should facts and call surfaces be shaped? | Binding Directive | Follow throughout |
| [CLI Implementation](../../../../directives/open-forge/cli/implementation.md) | Which CLI implementation and evidence rules apply? | Binding Directive | Follow throughout |

## Accepted Architecture And Decisions

- `SourceDocumentForm` remains the sole source classifier. Context passes the
  already classified form and parsed Markdown document to the accepted
  `SourceAuthoredMetadataParser`; it adds no Skill special case or second parser.
- `ContextGraphSource` carries the cohesive form-aware authored metadata fact.
  Native Skills have a description and zero authored Open Forge tags. Malformed
  required Skill metadata remains unavailable and fails closed.
- Compact rendering chooses an existing typed coordinate in this order:
  explicit finding subject, source path, then finding path. It escapes and bounds
  the coordinate locally without changing finding formation or structured output.
- Route Inspect help changes only its Context related-command sentence. Doctor
  remains truthfully unavailable until its planned Task.
- `CLI-EDGE-013` and `CLI-EDGE-014` remain preserved evidence for Doctor and the
  later explicit repository-content migration. No temporary Doctor, fallback,
  authored-metadata repair, or dogfooding content cleanup is admitted here.

## Scope And Paths

### Included

- Context graph metadata formation, graph metadata model, loading-closure state
  checks, and external Markdown link formation only where the cohesive metadata
  type requires it.
- Context and Find compact human renderer coordinate selection and escaping.
- Route Inspect related-command help text.
- Focused Unit, real-filesystem Integration, and published EndToEnd evidence for
  the three corrections.
- Plan, Task, Checkpoint, task index, and edge-ledger closeout records.

### Excluded

- Doctor, Repair, Cleanup, Index, Mutation Foundation, lifecycle schema, generated
  navigation behavior, repository metadata migration, or any write operation.
- Command grammar, binding, selection, status, exit, stream, next-action, JSON,
  diagnostics, or unrelated help changes.
- New packages, reflection, mocks, fake filesystems, snapshots, remote actions,
  publication beyond local test artifacts, or pushes.

| Scope kind | Paths or surfaces | Meaning |
| --- | --- | --- |
| Expected production | `Commands/Context/**`, `Commands/Find/**`, and exact `Commands/Route/Inspect/**/RouteInspectHelpSections.cs` | Smallest command-local corrections |
| Expected evidence | Mirrored Context, Find, and Route Inspect Unit/Integration/EndToEnd paths | Focused behavior and process proof |
| Expected records | This Task, parent Plan/Task, Checkpoint, task index, and edge ledger | Durable state and later order |
| Protected | Mutation, Lifecycle, Recovery, Git, Index application, packages, projects, configuration, and authored `.agents` content outside these records | No change without a new decision |

## Behavior And Acceptance Matrix

| ID | Behavior or condition | Evidence tier | Expected observation |
| --- | --- | --- | --- |
| DGC-001 | Routed valid native Skill metadata enters Context closure | Real-filesystem Integration and published process | Complete selection with no false closure finding for the Skill |
| DGC-002 | Malformed required Skill metadata remains unavailable | Real-filesystem Integration and published process | Incomplete status with exact Skill path evidence |
| DGC-003 | Ordinary Open Forge Markdown metadata behavior is unchanged | Existing Unit, Integration, EndToEnd | Complete/missing/malformed behavior remains accepted |
| DGC-004 | Compact Context findings name distinct known coordinates | Unit and published process | Same-code rows remain distinguishable, escaped, bounded, and ordered |
| DGC-005 | Compact Find frontmatter findings name distinct known coordinates | Unit and published process | No `subject=none` when source/path evidence exists |
| DGC-006 | Route Inspect help names available Context and registered grammar | Unit and published process | No planned/unavailable Context wording; Doctor wording unchanged |
| DGC-007 | Structured meaning and filesystem state do not change | Existing JSON plus no-write process evidence | Typed findings/statuses match and workspace snapshots are unchanged |
| DGC-008 | Supported local Native AOT hosts execute focused journeys | Native AOT Integration and EndToEnd | No managed-only behavior or skips |

## Execution Capsule

- Current owner: Overseer; no delegated lane.
- Current boundary: Complete local candidate and acceptance.
- Dependencies: exact tree-equivalent audit integration `b6ce31f`, accepted
  `SourceAuthoredMetadataParser`, and accepted Context/Find/Route Inspect results.
- Beginning evidence: the exact predecessor tree passed warning-free Release,
  managed Unit `1024/1024`, Integration `409/409`, EndToEnd `116/116`, and
  supported `linux-x64` Native AOT Integration `409/409`, all with zero skips.
- Focused evidence: affected Unit and Integration filters plus published Context,
  Find, and Route Inspect process tests.
- Full gate: locked restore when required, warning-free Release build, format and
  diff checks, full managed Unit/Integration/EndToEnd, supported local
  `linux-x64` Native AOT Integration and EndToEnd execution, path/dependency audit,
  and no-write evidence.
- Review budget: direct fresh-diff correctness and architecture review `DGC-R1`
  consumed after Green; no material finding remained.
- Correction budget: no grouped correction cycle was required.
- Stop conditions: new public-contract choice, new parser/classifier, dependency
  or project change, mutation/lifecycle change, repository-content migration,
  network requirement, remote action, or push.
- Next action: squash-integrate the accepted local candidate when authorized,
  then resume Mutation Foundation at its persisted lifecycle-schema decision.

## Progress And Evidence

- Current result: Complete. Context now consumes the accepted form-aware authored
  metadata fact and preserves `SourceDocumentForm` as the sole classifier. Valid
  native Skills no longer create false closure findings; malformed Skills remain
  incomplete with their exact path. Compact Context and Find findings choose an
  existing typed subject/source/path coordinate, escape it, and bound it to 240
  characters. Route Inspect help names available
  `open-forge context [source-reference...]` while Doctor remains unavailable.
- Red evidence: focused Unit `0/4`, real-filesystem Integration `1/2`, and
  published EndToEnd `0/5` demonstrated the three preexisting defects with zero
  skips. The first raw EndToEnd invocation exposed only the local `DOTNET_ROOT`
  selection and was rerun against the installed .NET 10 runtime before behavior
  was assessed.
- Green evidence: focused Unit `4/4`, real-filesystem Integration `2/2`, and
  published EndToEnd `5/5` pass with zero failures/skips. The shared pure
  `OpenForgeDocumentSeed.SkillEntrypoint` removes duplicated authored Skill-route
  construction while Integration and EndToEnd retain separate real-workspace
  ownership.
- Full evidence: offline restore from the prepared local NuGet package source,
  warning-free Release build, format verification, and `git diff --check` pass.
  Managed Unit `1031/1031`, Integration `411/411`, and EndToEnd `120/120` pass;
  freshly published portable `linux-x64` Native AOT Integration `411/411` and
  EndToEnd `120/120` pass. Every suite has zero failures and skips.
- Dogfooding evidence: repository Context remains correctly incomplete for the
  preserved 65 malformed/noncanonical sources, but all 65 compact findings name
  a coordinate and none names the valid `experience-design/SKILL.md`. Find retains
  66 expected frontmatter findings, all source-specific and none
  `subject=none`. Route Inspect help reports available Context exactly once,
  reports no unavailable Context, and retains unavailable Doctor.
- Review result: `DGC-R1` found no material correctness, architecture, AOT,
  evidence-tier, source-locality, or protected-surface defect. Exactly eight
  production files and fourteen test/support files changed; projects, packages,
  configuration, JSON, diagnostics, mutation, lifecycle, repository authored
  content, and remotes did not change.
- Blockers: None.
- Residual risk: local squash integration is intentionally not claimed. The
  preserved `CLI-EDGE-013` Doctor dependency and `CLI-EDGE-014` repository
  metadata migration evidence remain open for their recorded owners.

## Completion And Closeout

All eight behaviors pass on clean local implementation candidate `deb3f14`.
`CLI-EDGE-011`, `012`, and `015` carry exact closure evidence. The later order
remains local candidate integration → Mutation Foundation → public Index → route
mutation → lifecycle mutation → operations → delivery → final acceptance. No
remote action or push occurred.
