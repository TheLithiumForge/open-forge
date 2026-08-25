---
open-forge:
  description: Implement direct incoming and outgoing reference facts with shared source-universe filters
  tags: [Memory, Working, CLI, Task, References, ReadOnly, Links, Contextual]
---

# Implement References

## Task State

- State: Active in Preflight. The maintainer selected References before Context.
  Find, the neutral source and document facts, and the repository-root developer
  workflow are accepted. Production implementation is blocked until the exact
  command-local result object, finding codes, and supported Markdown-reference
  forms are accepted in the References contracts.
- Responsible role: Mastermind.
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

### Unresolved Contract Boundary

The Architecture requires each command contract set to define its exact JSON
`result` object, finite finding codes, command-local values, and compatibility
rules. The current References contracts describe the required facts but do not
define that exact object or code vocabulary. They also do not close whether the
fixed CommonMark reader reports only canonical inline navigation links or also
reference-style links, autolinks, and image/resource links, or how generated
`Entries` interiors are excluded from authored-reference evidence.

The recommendation is to close those meanings in the accepted References
Interface and Behavior contracts before production or tests freeze an accidental
public surface. Implementation must not infer them from Find, Doctor, Context, or
historical code.

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

| Scope kind                      | Paths or surfaces                                                                                                                                                           | Meaning                                                                                        |
| ------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| Expected                        | `src/cli/core/OpenForge.Cli.Core/Commands/References/**`; mirrored Unit, Integration, and EndToEnd References tests                                                         | New command-local behavior and evidence.                                                       |
| Expected                        | Narrow neutral additions under `Framework/Sources/**` and `Framework/Documents/Markdown/**`; affected Find source and tests                                                 | Shared source-reference/filter/location facts and behavior-preserving Find migration.          |
| Direct integration neighborhood | `Shell/Serialization/CliJsonContext.cs`, `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`, direct-root help and published-process tests                       | Explicit public registration and source-generated serialization only.                          |
| Protected                       | Command contracts until maintainer acceptance; projects, packages, central versions, build configuration, generated route navigation, Context, Index, mutations, frozen MVP | No accidental contract, dependency, topology, generated, later-command, or historical changes. |

## Execution Capsule

- Outcome: One accepted stateless `references` direct root reports exact one-hop
  incoming and outgoing facts and becomes the stable predecessor for Context.
- Profile: Assured because the current public result and parsed-reference boundary
  must be frozen before implementation.
- Current owner: Mastermind.
- Current boundary: Preflight and contract decision.
- Dependencies: Accepted Find/source/document facts and repository-root tooling.
- Focused evidence: References plus affected Source, Markdown, Find, Shell, Route,
  serialization, and published-process selections.
- Integration or full gate: Complete managed suite, public no-write evidence, and
  one supported local Native AOT publish and execution after the final executable
  change.
- Review budget: One independent integrated correctness and architecture review is
  available as `REF-R1`; none consumed.
- Council budget: Zero.
- Correction budget: One grouped correction cycle is available as `REF-C1`; none
  consumed.
- Stop conditions: An unstated public result or reference form, a second Markdown
  parser, changed shared-filter meaning, recursive graph behavior, package or
  project change, generated-route mutation, or a filesystem guarantee outside the
  accepted BCL boundary.
- Next action: Obtain maintainer acceptance of the recommended References contract
  closure, then freeze exact callables and implementation evidence before source
  mutation.

## Stop Conditions

Stop if reference extraction requires a competing Markdown parser, if a shared
filter changes meaning by direction, or if a recursive graph is introduced to
answer a direct-reference contract.

## Progress And Evidence

- Three bounded read-only repository explorations mapped the neutral source and
  document capabilities, direct-root command integration shape, and active test
  architecture. The Mastermind inspected the cited production sources and
  confirmed the missing shared resolver/filter/location boundaries and absent
  reference extractor.
- No production, test, contract, project, package, generated, Git, or external
  effect has occurred in this Preflight.
- Blocker: The unresolved contract boundary above.
