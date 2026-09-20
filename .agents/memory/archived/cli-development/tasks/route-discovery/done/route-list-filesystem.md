---
open-forge:
  description: Implement route-list inventory over shared physical safety with typed reads, aliases, findings, and cancellation retention
  tags: [Memory, Archived, Contextual, Historical, CLI, Task, Route, List, Filesystem, Containment, Complete]
---

# Implement Route-List Filesystem Inventory

## Task State

- State: Complete.
- Implementer: Mastermind.
- Parent: [Route Discovery](../_route-discovery.md).

## Expected Outcome

Route list inventories only proven-contained sources with strict reads and
deterministic known findings. Cancellation retains confirmed safe evidence and
never claims complete coverage.

## Source Placement And Components

Create `List/Shared/Filesystem/`:

- `RouteListInventoryRequest`: selected workspace, logical roots, and cancellation.
- `RouteListInventoryReader`: coordinator over shared physical resolver, directory
  reader, and strict file reader.
- `RouteListDirectoryEnumerator`: deterministic parent-before-child enumeration of
  one already-proven directory.
- `RouteListInventoryFacts`: immutable contained sources, aliases, and known
  filesystem outcomes.
- `RouteListFilesystemFindingPolicy`: translates shared typed facts into
  command-local finding codes and paths.

Keep shared physical path resolution in `Core/Framework/Filesystem`; do not wrap
it with another final-target resolver.

## Required Behavior

- Prove lexical and physical containment before reading or enumerating each
  candidate.
- Treat contained aliases, duplicate physical identities, cycles, external,
  dangling, inaccessible, missing, and unsupported states distinctly.
- Do not inspect descendants of an external or unproved path.
- Strict UTF-8 and metadata reads preserve invalid-encoding, access, and I/O direct
  causes.
- Append already-known relevant findings before cancellable child enumeration.
- On cancellation retain deterministic known findings and safe partial inventory,
  mark coverage incomplete, and stop further filesystem access.
- Never write, touch timestamps, create caches, or follow a path outside the
  workspace.

## Evidence

Integration ports and expands the real-OS alias matrix: workspace-root and
ancestor links, contained file/directory aliases, external aliases,
external-then-reentering chain, dangling, self-cycle, multi-cycle, duplicate
physical alias, inaccessible/read errors, invalid UTF-8, and unchanged snapshots.

Unit evidence covers typed translation policy, ordering, known-evidence retention,
and cancellation result formation without fake filesystem behavior.

### Accepted Implementation Evidence

- The inventory walks deterministic canonical logical roots breadth first and
  proves component-wise physical containment before every enumeration or source
  read. It translates Foundation physical, entry, directory, and strict UTF-8
  outcomes without using `Directory.Exists` or `File.Exists` as a typed result.
- Immutable facts retain contained logical sources, exact authored metadata,
  overwrite relations, physical aliases, known findings, and a catalogue for the
  accepted selection stage. Finite contained aliases remain distinct from active
  ancestor cycles.
- Real-workspace Integration covers ordinary and empty inventory, canonical and
  compatibility entrypoints, routed Markdown and `SKILL.md`, overwrite and orphan
  forms, generated-navigation non-authority, invalid UTF-8, locked reads,
  workspace-root and contained aliases, an escaping `.agents` ancestor, external
  and reentering aliases, dangling links, self and multi-node cycles, duplicate
  physical identities, cancellation retention, deterministic repetition, and
  unchanged snapshots.
- The completed increment passes `dotnet format` verification, the full Release
  solution build with zero warnings and errors, 214 Unit cases, and 69
  Integration cases.
- Public process and Native AOT evidence remain deliberately deferred until
  complete route-list Acceptance because the root remains command-free. Topology
  consumes these immutable inventory facts without further filesystem access.

### Review Record

- A bounded exploratory audit reached its step limit after identifying typed
  final-entry collapse and missing real-OS boundary evidence. Mastermind inspection
  completed the audit, replaced Boolean existence checks with Foundation component
  facts, and added the missing focused matrix.
- The first correctness review found that cancellation could omit an already-known
  entry finding and that inventory file facts normalized rather than rejected an
  unproved physical-path shape. Both received direct regression evidence and
  correction. The focused rereview passed with no remaining finding.
- The local improvement review recommended constant-time alias deduplication,
  centralized source-form predicates, independently selectable typed test cases,
  narrower candidate resolution, and mirrored fixture locality. Each correction
  remains leaf-local. Final improvement review found no material remaining change.
- The strongest alternative was a fake filesystem or test-only production seam for
  deterministic cancellation. The accepted real-OS case instead observes one
  pending owned-file read, cancels without a delay, and fails explicitly when the
  platform cannot establish that boundary. Its bounded 32 MiB fixture cost avoids
  widening production or weakening safety evidence.
- Reconsider this conclusion if the real-OS cancellation boundary becomes
  unreliable or materially expensive, or if a later accepted shared capability
  provides deterministic cancellation coordination without a fake filesystem.

## Stop Conditions

Stop on any need to inspect an outside path, collapse a typed state, guess a
workspace fact, skip target-platform safety, or add native code. Return to the
shared filesystem Task if its contract is insufficient.
