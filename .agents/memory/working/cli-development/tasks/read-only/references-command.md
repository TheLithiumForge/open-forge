---
open-forge:
  description: Implement direct incoming and outgoing reference facts with shared source-universe filters
  tags: [Memory, Working, CLI, Task, References, ReadOnly, Links, Contextual]
---

# Implement References

## Task State

- State: Complete. The neutral foundation is accepted at `e7516f0`, the
  command-local operation at `f7bb9f9`, the bounded structure improvement at
  `6922ca6`, final Red evidence at `43b75f3`, and public Green at `23d5e2e`.
  Managed `1270/1270`, published-process, no-write, architecture,
  protected-surface, and final supported local `linux-x64` Native AOT evidence
  pass. Final acceptance is recorded in the commit containing this record.
- Responsible role: Overseer; one Task Mastermind owns each active sequential
  increment.
- Task source: This file.
- Last updated: 2026-08-25.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/references-candidate/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/references-candidate/behavior.md).

## Expected Outcome

`references` reports accepted direct incoming or outgoing references for one
source with exact source identity, deterministic provenance, and shared
source-universe filters.

## Architecture

- Keep command models and projections at `Commands/References/`.
- Create `Commands/References/Shared/Extraction/` for command-local reference
  extraction until another consumer proves identical link semantics.
- Consume the neutral source catalogue, physical-path resolution, typed document
  reads, fixed CommonMark pipeline, and Markdown document facts. Route facts are
  evidence only where a References contract explicitly needs them. Generated
  `Entries` and route edges are not authored Markdown references.
- Promote source-reference resolution, source-universe set formation, and the
  Architecture's authored `SourceLocation` primitive only at the neutral
  Framework boundary required by References and accepted later consumers. Migrate
  Find to those promoted facts without changing Find behavior or output.
- Keep direction, incoming default-universe policy, extraction semantics,
  occurrence rows, coverage, findings, status, rendering, diagnostics, and JSON
  command-local.

### Preflight Capability Map

| Need                                                           | Accepted current source                                                        | Task boundary                                                                                                                |
| -------------------------------------------------------------- | ------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------- |
| Logical source identity, collisions, and base/overwrite layers | `Framework/Sources/Inventory/**` and `Framework/Sources/Identity/**`           | Reuse directly.                                                                                                              |
| Physical containment and verified UTF-8 reads                  | `Framework/Filesystem/**` and `Framework/Sources/Reading/**`                   | Reuse directly.                                                                                                              |
| Include/exclude source-universe rules                          | Find-local `FindUniverseResolver` over neutral catalogue facts                 | Promote only shared resolution, expansion, and set facts. Keep Find and References policy local.                             |
| Authored locations                                             | Find-local `Utf8SourceMap` and `FindSourceLocation`                            | Establish the Architecture's neutral `SourceLocation` and migrate Find without wire changes.                                 |
| Markdown parsing                                               | `Framework/Documents/Markdown/**` and the fixed Markdig pipeline               | Extend neutral document facts only when the accepted link grammar requires a shared parsed fact. Do not add another parser.  |
| Reference extraction and target resolution                     | No accepted production capability                                              | Implement locally below `Commands/References/Shared/Extraction/` until another accepted consumer proves identical semantics. |
| Public command integration                                     | Find's direct-root binding, rendering, JSON, Shell, and root-composition shape | Follow the proven shape without sharing command meaning.                                                                     |

### Closed Preflight Decisions

- The accepted References Interface owns the complete concrete JSON `result`, all
  nullable-member rules, the finite finding vocabulary, exact status aggregation,
  next actions, views, diagnostics, and help. No implementation-local public value
  may be added.
- The fixed Markdig 1.3.2 pipeline admits non-image inline links,
  reference-style links, and explicit autolinks only. Inline links use the complete
  `LinkInline.Span` as their use location and `UrlSpan` as their destination
  location. Reference-style links use the link-use span and the resolved
  `LinkReferenceDefinition.UrlSpan`. Explicit autolinks preserve their exact use
  span and have a null destination location because the pinned parser exposes no
  independent destination span. Raw HTML links, code, image/resource embeds, and
  bare URL text remain excluded.
- The one fixed Markdown pipeline retains precise source locations and enables
  Markdig's GitHub auto-identifier extension. Neutral heading facts expose the
  produced identifier. This implements the authoritative Framework rule for
  GitHub-compatible visible-ATX fragments, Unicode, and deterministic duplicate
  suffixes without a custom slugger or second parser. A heading whose supported
  visible text or identifier cannot be established leaves fragment verification
  unavailable.
- A neutral Markdown generated-region fact establishes only the exact final
  `## Entries` section and its single ordered marker pair. It does not parse route
  entries or assign command findings. References excludes a region only from that
  fact. Find retains its current command-local tag-scan exclusion because its
  accepted behavior is not identical to the stricter Framework region contract.
- References declares its source argument with zero-or-one parser arity so the
  binder can return the accepted concrete invalid result when it is missing; the
  binder rejects a second operand and every recoverable direction/filter/source
  semantic error before domain work. Shell-owned unknown-option, delimiter,
  global-validation, help, and version flows retain shared Shell behavior and do
  not manufacture a command result.

### Sequential Implementation Increments

1. **Neutral foundation and Find migration.** Add source-reference resolution at
   `Framework/Sources/Identity/`, ordered source-universe filter resolution at
   `Framework/Sources/Selection/`, and `SourceLocation` plus `Utf8SourceMap` at
   `Framework/Sources/Locations/`. Extend the existing fixed Markdown pipeline and
   facts with link, heading-fragment, and generated-region facts. Migrate only
   behavior-identical Find resolution, filter, and location mechanics; preserve
   Find status, findings, ordering, rendering, JSON, and public process behavior.
2. **References domain and operation.** Add command-local request, selection,
   extraction, resolution, occurrence, coverage, result, status, and operation
   behavior below `Commands/References/`. Implement direct one-hop outgoing and
   incoming inspection without public binding or rendering.
3. **Public binding, presentation, and acceptance.** Add symbols, definitions,
   binder, invalid/workspace result formation, human/JSON/diagnostic/help
   projection, source-generated JSON registration, direct-root composition, and
   EndToEnd/AOT evidence. Register exactly one direct `references` root.

Each increment is integrated and verified before the next begins. No parallel
agent or worktree may mutate shared source, document, Find, Shell, serialization,
or root-composition surfaces during this task.

## Requirements

Implement direct directions, exact and ID subjects, collisions, supported local
reference forms, unchecked external URL facts, include/exclude behavior, filter
validity by direction, unreadable and malformed source handling, deterministic
ordering, views, JSON, diagnostics, help, and no-write behavior.

Do not recurse reference graphs, compute semantic impact, fetch external URLs, or
diagnose broken links beyond the command contract.

## Evidence

Unit covers extraction and normalization rules, direction/filter matrix, ordering,
status, and renderers. Integration uses real Markdown paths, anchors, spaces,
Unicode, external URLs, missing targets, ambiguous IDs, compatibility sources,
and unchanged snapshots. EndToEnd and AOT prove public streams and exits.

### Behavior And Acceptance Matrix

| ID     | Behavior or condition                                                   | Evidence tier               | Required observation                                                                                                                  |
| ------ | ----------------------------------------------------------------------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| REF-B1 | Exact source operand, direction, repetition, and filter applicability   | Unit, Integration, EndToEnd | Native parser facts form one complete request; invalid forms do no domain work.                                                       |
| REF-B2 | Shared source identity and filter expansion                             | Unit, Integration           | IDs, exact paths, collisions, base/overwrite pairs, unions, and exclusion precedence retain exact evidence.                           |
| REF-B3 | Direct outgoing extraction and target resolution                        | Unit, Integration           | Base precedes overwrite, authored order and duplicates remain, locations are exact, and no target becomes a traversal seed.           |
| REF-B4 | Complete incoming scan and deterministic order                          | Integration                 | Every effective physical layer is inspected once; complete empty results require complete coverage.                                   |
| REF-B5 | Local, external, malformed, missing, ambiguous, and unsafe destinations | Unit, Integration           | Safe facts remain visible with typed resolution and status; HTTP(S) is never fetched.                                                 |
| REF-B6 | Separate section and aggregate status                                   | Unit, Integration           | Unevaluated directions are absent and one section cannot hide another section's coverage or status.                                   |
| REF-B7 | Compact, expanded, JSON, diagnostics, and help                          | Unit, Integration, EndToEnd | One typed result drives every projection; JSON uses source generation and `--view` is a no-op under JSON.                             |
| REF-B8 | Public process, no-write, and cancellation                              | EndToEnd                    | Arguments, streams, exits, bounded diagnostics, cancellation, and unchanged bytes match the contracts.                                |
| REF-B9 | Managed and local Native AOT regression                                 | Complete gate               | References and affected Find, Route, Shell, source, document, and serialization evidence pass without reflection or dependency drift. |

### Scope And Paths

| Scope kind                      | Paths or surfaces                                                                                                                                          | Meaning                                                                                        |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Expected                        | `src/cli/core/OpenForge.Cli.Core/Commands/References/**`; mirrored Unit, Integration, and EndToEnd References tests                                        | New command-local behavior and evidence.                                                       |
| Expected                        | Narrow neutral additions under `Framework/Sources/{Identity,Selection,Locations}/**` and `Framework/Documents/Markdown/**`; affected Find source and tests | Shared source-reference/filter/location/document facts and behavior-preserving Find migration. |
| Direct integration neighborhood | `Shell/Serialization/CliJsonContext.cs`, `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`, direct-root help and published-process tests      | Explicit public registration and source-generated serialization only.                          |
| Protected                       | Accepted command contracts; projects, packages, central versions, build configuration, generated route navigation, Context, Index, mutations, frozen MVP   | No accidental contract, dependency, topology, generated, later-command, or historical changes. |

## Execution Capsule

- Outcome: One accepted stateless `references` direct root reports exact one-hop
  incoming and outgoing facts and becomes the stable predecessor for Context.
- Profile: Assured because the current public result and parsed-reference boundary
  must be frozen before implementation.
- Current owner: Overseer.
- Current boundary: Complete final local `linux-x64` Native AOT publication and
  execution after public Green `23d5e2e`.
- Dependencies: Accepted Find/source/document facts and repository-root tooling.
- Focused evidence: References plus affected Source, Markdown, Find, Shell, Route,
  serialization, and published-process selections.
- Integration or full gate: Complete managed suite, public no-write evidence, and
  one supported local Native AOT publish and execution after the final executable
  change.
- Review budget: `REF-R1` is consumed. Its fresh reviewer verdict is `PASS` with
  no material findings against `149d2ac..e7516f0`.
- Council budget: Zero.
- Correction budget: `REF-C1` is consumed by the grouped Task Mastermind
  correction of increment 2 path identity, coverage, event propagation, ordering,
  invariants, and decisive evidence.
- Stop conditions: An unstated public result or reference form, a second Markdown
  parser or custom heading slugger, changed shared-filter or Find meaning,
  recursive graph behavior, package or project change, generated-route mutation,
  or a filesystem guarantee outside the accepted BCL boundary.
- Next action: Project-level local integration may squash the accepted
  `codex/cli-references` tree into `develop`, prove tree equality, and then make
  Context eligible to start. Do not push.

## Stop Conditions

Stop if reference extraction requires a competing Markdown parser, if a shared
filter changes meaning by direction, or if a recursive graph is introduced to
answer a direct-reference contract.

## Progress And Evidence

- Three bounded read-only repository explorations mapped the neutral source and
  document capabilities, direct-root command integration shape, and active test
  architecture. The Overseer inspected the cited production sources and confirmed
  the missing shared resolver/filter/location boundaries and absent reference
  extractor.
- A final read-only proof against pinned Markdig 1.3.2, Framework Markdown facts,
  generated-region readers, and Shell/Find binding established the exact span and
  invalid-result boundaries recorded above. The authoritative Framework routing
  path contract closes fragment identity through GitHub-compatible heading IDs.
- The accepted predecessor at `d3beb3b` had no C# source, test, project, or
  package drift from the recorded managed `1190/1190` baseline. At that boundary,
  local Native AOT remained a final-gate limitation because the installed SDK had
  no portable `linux-x64` runtime pack and no download was authorized.
- Preflight is closed. No production or test mutation occurred during it, no
  remote action or push occurred, and the task is Ready for its first sequential
  implementation increment.
- Neutral increment 1 is accepted at exact commit `e7516f0` (`Add neutral
references foundations`). It adds Framework source-reference, ordered universe,
  UTF-8 location, fixed-pipeline link/fragment/generated-region facts and migrates
  behavior-identical Find mechanics. Release build passes with zero warnings and
  errors; focused Unit is `359/359`, focused Integration `65/65`, and the complete
  managed suite is `1199/1199` with zero skips.
- Fresh independent review `REF-R1` returns `PASS` with no material findings. Its
  independently observed complete split is Unit `844/844`, Integration `285/285`,
  and EndToEnd `70/70`, with focused foundation `16/16`, clean diff, and no
  protected-path drift. No remote action or push occurred.
- Command-local increment 2 is accepted at `f7bb9f9` (`Implement References
operation`) after grouped correction `REF-C1`. Focused References evidence is
  Unit `8/8` and real-filesystem Integration `12/12`; affected neutral regressions
  are Unit `487/487` and Integration `67/67`; the exact managed suite is
  `1219/1219` with zero skips and a zero-warning Release build. Seventeen changed
  files stay entirely within command-local References production and mirrored
  Unit/Integration paths. No public command, Shell, root, serializer, contract,
  configuration, Framework, Find, remote, or push change occurred.
- The mandatory architecture/locality review for materially large behavior
  classes completed through a `gpt-5.6-luna`/`max` Blue Structure Improver. Commit
  `6922ca6` (`Refine References operation structure`) extracts source/catalogue/
  filter finding translation into a focused command-local mapper, reduces the
  primary operation from 724 to 572 lines, and retains the destination resolver
  and result builder as cohesive owners. The post-change Release build is clean;
  References Unit `8/8`, Integration `12/12`, affected Unit `487/487`, affected
  Integration `65/65`, and the full Unit `852/852`, Integration `297/297`, and
  EndToEnd `70/70` split pass with zero skips. No tests or protected paths changed.
- Final public Red is accepted at `43b75f3` (`Freeze References public evidence`)
  after test-only evidence-integrity corrections. Clean detached reproduction
  stops Unit at exactly five missing public References types, Integration at one
  missing source-generated JSON registration, and published References at twelve
  intentional absent-root failures. Red changes nine test/support paths only.
- Public Green is accepted at `23d5e2e` (`Present References command`). It adds one
  direct-root symbol/binding/result flow, compact/expanded/JSON/diagnostic/help
  presentation, explicit source-generated serialization, and one root
  registration. Focused Unit is `36/36`, Integration `23/23`, and published
  References EndToEnd `12/12`; affected Unit is `541/541`, affected Integration
  `252/252`; full Unit `880/880`, Integration `308/308`, and EndToEnd `82/82`
  produce `1270/1270` with zero skips and a zero-warning Release rebuild.
- Public smoke proves help, one root registration, compact and JSON success,
  typed invalid results and exit `4` for missing source and repeated direction,
  empty stderr, and unchanged workspace hashes. No network, fetch, write, cache,
  index, reflection, dynamic serialization, raw process-argument rescan, or push
  occurred. The binder, JSON projection, and expanded renderer were split below
  the 200-line review trigger. Larger References test fixtures/classes and the
  operation/destination owners were reviewed as cohesive evidence/algorithms; no
  further split protects behavior or locality.
- The first final Native AOT attempt used the supported `linux-x64` RID with
  `--no-restore` and no fetch. It stopped before compilation at
  `Microsoft.NETCore.Native.Publish.targets(70,5)` because
  `PrivateSdkAssemblies` was absent. The Ubuntu SDK installation exposed only its
  distro-specific `ubuntu.24.04-x64` local Native AOT pack. That did not make the
  portable product RID unsupported; it meant the matching portable runtime pack
  had not been restored.
- After exact restore authority was granted, the standard SDK command
  `dotnet publish src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj --configuration Release --runtime linux-x64 --output artifacts/publish/linux-x64/open-forge -p:OpenForgeSkipDevelopmentPublish=true`
  restored `Microsoft.NETCore.App.Runtime.NativeAOT.linux-x64` `10.0.11` from the
  configured NuGet source and published successfully without a project, package,
  version, RID-matrix, or source workaround. A subsequent publication with the
  repository CI command's `--no-restore` boundary also passed. The final root
  artifact at
  `artifacts/publish/linux-x64/open-forge/OpenForge.Cli` is an ELF64 PIE x86-64
  executable with SHA-256
  `d16b359512bec10324669193d8c56a78bd5d56cce3fb23ff84cd6b9201fd8c84`.
- The explicit `linux-x64` solution build passed with zero warnings and errors.
  Build-selected published References EndToEnd passed `12/12`; its representative
  journeys retain the accepted unchanged-workspace proof. Native AOT Integration
  passed `308/308`, and the Native AOT EndToEnd executable passed `82/82`. Every
  run had zero skips. Their ELF64 PIE x86-64 executables have SHA-256
  `a6b22aca2b9e16b7bd168c47e188d9afd217249fc4c58368ce2df3c7e576c469`
  and `3e0a52ba2f4e6d55ff2bcc2abc04a77212a7eacfb6dfcdd37c0359f93b6b3343`,
  respectively. REF-B1 through REF-B9 pass. No production, test, contract,
  project, package, configuration, or generated route source changed for this
  final gate, and no remote publication or push occurred.
