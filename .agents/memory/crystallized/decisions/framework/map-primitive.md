---
open-forge:
  description: Map replaces Workspace as the Core primitive for locating important local and external truth
  tags: [Memory, Decision, CurrentTruth, Framework, Core, Map, Migration]
---

# Map Primitive

## Context

The Core primitive named Workspace answered one question: where does relevant local or external truth live, and when should it be used? Its content was already a map, while `workspace` also named the complete environment in Framework and CLI contracts. The overloaded name made the primitive look like a container for workspace knowledge instead of concise navigation.

## Decision

The maintainer accepted contained folder links in Task 83 on 2026-10-16.
Maps deliberately point to useful modules and folders as well as individual
files. The CLI therefore validates an existing folder by existence under the
same containment rules as a file. A folder has no headings, and a missing folder
needs an authored edit rather than a guessed file replacement. The
[shared destination contract](../../documents/cli/shared-operation-contract.md#authored-local-destinations)
defines that behavior.

Map is the Core primitive for durable navigation to important local and external destinations. Its canonical root is `.agents/maps/`, its entrypoint is `_maps.md`, and its type tag is #Map.

The ordinary term `workspace` continues to mean the environment Open Forge operates in. A Map points to destinations and explains their relevance. It does not replace Memory or the destination's current information.

The former `.agents/workspace/` root and #Workspace tag are not Map aliases. The frozen MVP installer stops before writing when it finds the former root and directs the user through a Git-backed manual migration.

## Rationale

Map states the primitive's purpose directly and preserves a clear boundary between navigation and knowledge. Reserving `workspace` for the environment removes an overloaded Framework term without changing routing mechanics.

A fail-closed manual migration avoids inferring ownership of user-authored files in a frozen installer that has no directory-move transaction or complete migration evidence.

## Consequences

- Fresh installations expose Maps and do not install a Workspace primitive
- Existing Map routes remain coarse and recursively scopable
- Extensions use Map paths and tags for this primitive; files outside `.agents/` still belong to the ordinary workspace environment
- Historical sources may retain Workspace when describing the former primitive
- Existing installations must review and move former Workspace routes before Core reinstall can continue

## Authoritative Sources

- [Core primitive model](../../documents/framework/primitives/model.md)
- [Current Map document](../../documents/framework/primitives/map.md)
- [Installed Maps entrypoint](../../../../maps/_maps.md)
- [Maps maintenance contract](../../documents/maintenance/payload/agents/map.md)

## Decision Relationships

- [Distinct Core primitive roles](core-primitives.md)
- [Routing model](routing-model.md)
- [Source and packaging](source-and-packaging.md)
