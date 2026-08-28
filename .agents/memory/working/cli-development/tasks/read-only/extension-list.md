---
open-forge:
  description: Implement Extension catalogue and source listing without lifecycle inference
  tags: [Memory, Working, CLI, Task, Extension, List, ReadOnly, Contextual]
---

# Implement Extension List

## Task State

- State: Complete. Original accepted feature tip
  `95b1280de98c8e1928cafab2fc70f0f5c190a6ec` was rebased over Context as
  `5e6babf095ac319429c8b787e338a2e6192ef5d3` and squash-integrated into local
  `develop` at `db0d39a213d26f3ad0bab00de1da5922457d854f`. The final integrated
  correctness review found no material correctness, integration,
  Native-AOT/source-generated-JSON, CLI-contract, determinism, or C# Directive
  issue.
- Responsible role: bounded Extension Discovery Task Mastermind.
- Profile: Assured. The public command, package-source identity, deterministic
  embedded assets, and immutable lifecycle read schema are compatibility and
  safety boundaries. Contract/evidence ownership remains continuous in this
  Task rather than creating separate placeholder phases.
- Parent: [Read-Only Commands](_read-only.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/extension/list/interface.md)
  and [Behavior](../../../../crystallized/documents/cli/contracts/extension/list/behavior.md).
- Predecessors: CLI Foundation and Generic CLI Improvements are accepted; the
  current branch includes accepted Find and References facts plus the
  repository-root developer workflow. The exact predecessor's managed
  `1270/1270`, process, recursive no-write, warning-free Release, and supported
  local `linux-x64` Native AOT evidence is the beginning baseline.
- Last updated: 2026-08-26.

## Accepted Evidence

- Preflight is `ff5376a`; the original coherent production and evidence candidate
  is `371d67d`. The grouped `EL-R1-01` through `EL-R1-07` correctness repair is
  `5ac887d`, followed by the exact output-template style corrections `1ceeefa`
  and `7f5395d`. The grouped `EL-R2-01` through `EL-R2-03` repair and executable
  evidence packet is `56b0d07`.
- Fresh `EL-R2` rereview returned `ACCEPTED` with no material correctness,
  integration, Native-AOT/serialization, CLI-contract, determinism, or scoped C#
  Directive finding. After the Context-first rebase and squash integration, a
  second fresh read-only review of exact `develop` at `db0d39a` also returned
  `ACCEPTED` with no material finding. These verdicts close the candidate-only
  state; they are durably recorded here with the containing coordination commit.
- Locked restore passes. The Release solution builds with zero warnings and zero
  errors. Focused managed Unit is `51/51`; focused managed Integration is
  `32/32`; published managed Extension List EndToEnd is `16/16`, all with zero
  skips. Complete managed Unit is `931/931`, Integration is `340/340`, and
  EndToEnd is `98/98`, all with zero skips.
- The supported local `linux-x64` Native AOT root publishes and executes
  `extension list --available --json`. Native complete Integration is `340/340`
  and native complete EndToEnd is `98/98`, both with zero skips.
- Recursive public no-write snapshots cover entry identity, attributes,
  timestamps, lengths, and content hashes for workspace and explicit source.
  The reflection-free typed embedded provider has exact authored
  inventory/path/byte equality, every SHA-256 passes, and its deterministic gzip
  archive reproduces byte-for-byte. Source-generated manifest/lifecycle/result
  serialization, protected paths, forbidden writes/network/legacy input, all
  six project package-vulnerability checks, and changed-surface audits pass.
- `git diff --check` and .NET 10 `dotnet format` whitespace, style, and analyzer
  verification at warning severity pass with no delta. The exact touched-string
  audit finds no concatenation train, composite-format train, per-value
  `ToString` template, or StringBuilder template violation.
- `EL-R1-01` now rejects normalized lexical overlap before any missing/type
  classification and rejects physical overlap as soon as identity resolves;
  missing-child, file-child, and physical-alias scenarios prove the ordering.
- `EL-R1-02` physically resolves the lifecycle candidate against the workspace
  before reading and blocks linked-ancestor and linked-file escapes.
- `EL-R1-03` enforces the canonical portable path grammar/key, exact authored
  normalized workspace spelling, and pre-trust self/cycle dependency rejection,
  including Windows devices, superscript aliases, forbidden characters,
  trailing dot/space, Unicode/case aliases, and cycles on every platform.
- `EL-R1-04` observes cancellation at operation/source ingress and maps every
  source/lifecycle cancellation to interrupted for default, embedded-only, and
  explicit-source requests.
- `EL-R1-05` removes assembly/reflection resource lookup in favor of the exact
  typed archive provider and proves authored/provider/inventory set, byte, and
  hash equality under managed and Native AOT execution.
- `EL-R1-06` removes routine null guards from the trusted internal typed
  pipeline while retaining uncertain-ingress and real-invariant validation.
- `EL-R1-07` is satisfied by the complete exact-tip managed, audit, recursive
  no-write, and supported `linux-x64` Native AOT evidence above.
- `EL-R2-01` retains strict duplicate rejection for the lifecycle envelope and
  typed Extension coverage/package/path nodes while skipping only the exact
  opaque root `framework` value. Real reader, composed JSON, and published JSON
  scenarios preserve valid trusted Extension facts while rejecting typed
  duplicates.
- `EL-R2-02` executes the actual Extension List human renderer, output stage,
  and completion policy across all seven statuses with exact status, stream,
  and exit assertions. Narrow Integration journeys execute failed and
  interrupted typed terminal results through that production presentation
  boundary; published real-process journeys execute the five deterministically
  inducible human statuses on their contracted stdout/stderr streams.
- `EL-R2-03` expresses the touched root Discovery help as one coherent
  interpolated template while preserving the exact platform-newline bytes.
- The committed Unit suite now regenerates the reflection-free embedded gzip
  archive byte-for-byte from the complete authored asset set using its exact
  inventory-first ordering, in addition to the existing provider/inventory set,
  byte, and hash equality proof.
- The owned/touched renderer, help, diagnostic, status, archive, fixture, and
  test formatting audit applies coherent interpolated templates, exact culture
  providers, and raw multi-line blocks while preserving byte-exact output. A
  final delta audit for coherent strings outside `StringBuilder` remains clean
  after the grouped `56b0d07` repair.

## Outcome

`extension list` reports deterministic separate Installed and Available facts
from one exact workspace and one embedded or explicit local package universe. It
implements the complete binding, filters, trust/coverage, views, JSON,
diagnostics, help, status, stream, exit, no-write, real-filesystem, and supported
local Native AOT contracts. It never installs, infers lifecycle ownership,
executes package content, discovers a remote source, or reads a legacy receipt.

## Accepted Contracts And Task-Local Architecture

### Public And Pipeline Contract

- Register one real `extension` group and one `list` leaf by exact composed
  `System.CommandLine.Command` identity. Bare `extension` renders group help and
  performs no catalogue or lifecycle work.
- Accept no operands. Repeated `--installed` and `--available` are idempotent;
  repeated singleton `--source` is invalid. Neither section flag requests both;
  exactly one requests only that section; both request both in Installed then
  Available order.
- Form one immutable request, invoke one operation at most once, form one
  concrete result, and render compact, expanded, JSON, and bounded diagnostic
  output from that result. Use the existing schema-v1 envelope and exact seven
  status/stream/exit policy.
- The command-owned JSON result uses ordered, required fields for source,
  requested sections, lifecycle state/coverage, Installed rows, Available rows,
  findings, and counts. `--view` is a JSON no-op. The concrete graph is
  source-generated and reflection-free.

### Package And Catalogue Source Contract

- The current whole-file package format is one directory containing exact
  `extension.json` metadata and optional `payload/`. The only manifest properties
  are `id`, `name`, `description`, `version`, and `dependencies`; duplicate or
  unknown properties, malformed JSON/UTF-8, and invalid field types are rejected.
- Stable IDs contain lowercase ASCII alphanumeric segments separated by single
  hyphens. Dependencies are distinct exact stable IDs. Versions remain
  descriptive strings; no range, compatibility negotiation, registry, cache,
  download, provider, alias, or fallback semantics exist.
- A catalogue is one directory without a root `extension.json` whose direct
  package children have the exact package shape. One explicit source is
  classified structurally as one package or catalogue. Ambiguous or unsupported
  structure is not inferred from a folder name.
- Resolve dependency facts offline inside the selected source universe, reject
  duplicate IDs, unknown dependencies, cycles, and incomplete closure, and order
  identities ordinally and dependency closure deterministically.
- Prove explicit source/workspace lexical and component-wise physical
  disjointness in both containment directions before source reads. Never use a
  fake filesystem.
- The embedded source is the current first-party
  `src/extensions/development-toolkit` package. Core embeds an authored,
  deterministic resource inventory with SHA-256 for every manifest and payload
  asset and verifies bytes before exposing catalogue facts. This proves only
  distributed source identity.

### Lifecycle Read Contract

- The only input is exact `.agents/open-forge.lifecycle.json`. The replacement
  never reads, recognizes, aliases, migrates, or infers from
  `open-forge.extensions.json` or another legacy file.
- Add an immutable, strict, source-generated schema-v1 reader under
  `Framework/Lifecycle/`. The common envelope contains `schemaVersion: 1`,
  `fingerprintPolicy: "open-forge-markdown-v1"`, exact normalized
  `workspacePath`, an optional opaque `framework` JSON value, and an optional
  `extensions` section. Treat the unrelated Framework value as syntactically
  valid opaque JSON so its future semantic model cannot erase valid Extension
  facts.
- The Extension section contains `coverage: "complete"`, ordered package
  records, and ordered path records. A package record contains exact ID,
  descriptive version or null, exact source identity, distinct dependency IDs,
  and distinct owned target-relative paths. A path record contains a safe
  target-relative path, non-empty distinct owner IDs, baseline fingerprint, and
  fingerprint kind `semantic` or `exact-bytes`.
- Reject duplicate/unknown JSON properties, invalid UTF-8/JSON, unsupported
  values, duplicate IDs/paths/owners/dependencies, unsafe target paths,
  nonreciprocal ownership, unknown owners/dependencies, nondeterministic order,
  and workspace mismatch. Preserve safe partial identities only where no unsafe
  ambiguity is introduced.
- Missing lifecycle document or missing Extension section is `incomplete`, not
  a fabricated empty set. One valid complete empty Extension section proves
  `absent`; one valid complete and consistent non-empty section proves `trusted`.
  Malformed, unsupported, unverifiable, or inconsistent evidence remains
  `untrusted`/`incomplete`, or `blocked` where ambiguity is unsafe.
- This Task adds no writer, lock, plan, receipt publication, mutation application,
  recovery, or legacy lifecycle support.

## Behavior Matrix

| Boundary | Required result |
| --- | --- |
| Embedded + valid empty lifecycle | Both requested sections complete; Installed absent; Available deterministic |
| Installed only + unavailable explicit source | Trusted installed facts retained; complete safe source-unavailable observation is attention |
| Available requested + unavailable explicit source | Independently safe facts retained; missing required source coverage is incomplete |
| Missing lifecycle input | Lifecycle coverage incomplete; never an invented empty Installed section |
| Valid complete lifecycle section | Trusted or absent only after reciprocal identity/ownership/coverage validation |
| Malformed or unsupported lifecycle | Safe facts retained as untrusted/incomplete; unsafe ambiguity blocked |
| Explicit package source | Exactly its manifest/package facts; no embedded fallback |
| Explicit catalogue source | Direct packages only, exact IDs, complete offline dependency facts |
| Duplicate ID, manifest, dependency, or unsafe path | Invalid or blocked by the public safety boundary |
| Lexical/physical source overlap or alias | Blocked before available-source inspection |
| No filter / one filter / both | Both / exact requested section / stable two-section projection |
| JSON | One stdout document for every semantic status; diagnostics remain stderr |
| Human | Complete/attention/incomplete stdout; invalid/blocked/failed/interrupted stderr |
| Repeated reads | Byte-identical output and no workspace/source mutation |

## Ownership And Integration Neighborhood

### Expected Owned Paths

- `src/cli/core/OpenForge.Cli.Core/Commands/Extension/**`
- `src/cli/core/OpenForge.Cli.Core/Framework/Extensions/**`
- `src/cli/core/OpenForge.Cli.Core/Framework/Lifecycle/**`
- Mirrored Extension List Unit and Integration paths
- Extension List public EndToEnd fixture and scenarios
- This Task record

### Authorized Neighbor Collisions

- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`
- `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`
- Root/Core/test project files for deterministic embedded resources and public
  proof
- Existing root help, group ordering, direct-root, generated-serialization, and
  public EndToEnd tests when the exact Extension binding requires them

Every collision is reported for later integration. Expected paths are a
forecast, not authority to change unrelated meaning.

### Protected Paths And Meaning

- `Framework/Documents/Markdown/**`, `Framework/Sources/**`, Context, References,
  Route, generated-navigation application, mutation, recovery, lifecycle
  writers, plans, receipts, locks, and all legacy CLI behavior
- Shared Plan, Checkpoint, task indexes, parent Tasks, Architecture, Crystallized
  contracts, generated `Entries`, sealed Handoffs, and other worktrees
- No package download, registry/cache/network discovery, dynamic plug-in
  execution, reflection serialization, fake filesystem, or lifecycle inference

## Evidence Ladder And Budgets

1. Definitions, binder, request/result, finding/status, renderer/help, strict
   manifest/lifecycle models, source-generated serialization, and deterministic
   embedded-inventory Unit evidence.
2. Owned real-filesystem package/catalogue, physical overlap/alias, lifecycle,
   hash, repeated-read, and no-write Integration evidence.
3. Root composition, help/group order, affected Shell/Route/Find/References, and
   generated-serialization regressions.
4. Managed published `extension list` EndToEnd with recursive entry, metadata,
   and content-hash snapshots and zero skips.
5. Locked restore, warning-free Release build, format and diff checks, complete
   managed regressions, project/package/resource/protected-surface audits.
6. Supported local `linux-x64` Native AOT root, Integration, and EndToEnd
   execution with zero skips.

Review budget: one fresh bounded correctness review after a green candidate and
one local-improvement review only if material structure warrants it. Correction
budget: two distinct focused repair strategies before escalation. Stable review
IDs are `EL-R1` and `EL-I1`; no IDs are consumed at Preflight.

## Completion Contract

Accept only when the public command and direct operation satisfy every behavior
matrix row; all required managed/native evidence passes with zero skips; the
embedded bytes and hashes are deterministic; the lifecycle/package readers are
strict and source-generated; no write, legacy, protected-surface, dependency,
or generated-output drift exists; every intended change is committed in
coherent increments; and the worktree is clean.

After List acceptance, Inspect may consume its accepted package/source/lifecycle
facts. Promote only complete identical semantic units; do not promote List rows,
findings, statuses, rendering, or result meaning.

## Stop Conditions

Return a project change request before changing accepted public/cross-task
contracts, package or lifecycle authority, ownership/dependency direction,
protected semantic authorities, another lane, or the mutation-foundation split.
Stop before lifecycle inference, package download, dynamic plug-in loading,
registry/cache/network discovery, legacy receipt support, mutation/recovery
behavior, reflection serialization, fake filesystems, or speculative shared
frameworks.
