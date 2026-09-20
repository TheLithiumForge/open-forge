---
open-forge:
  description: "Historical CLI-v2 source: The CLI surrounds agent reasoning with five deterministic jobs for orientation, context, authoring support, health, and lifecycle management"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# CLI Core Job Model

## Context

The MVP command surface grew around individual mechanics. `load`, `find`, `chain`, `index`, `doctor`, `create`, `install`, and `extend` each proved useful behavior, but the resulting command list is not a stable product model.

An agent needs a small conceptual surface that follows its actual work. One magic framework command would hide intent and require semantic inference. A large collection of low-level utilities would preserve explicitness but increase discovery, tool calls, and decision cost.

## Decision

The CLI makes evidence and mechanical action cheap without owning the reasoning step. It supports the agent journey around reasoning:

```text
enter workspace
  -> orient
  -> retrieve and explain applicable context
  -> reason outside the CLI
  -> create, edit, or apply an explicit operation
  -> validate and repair
  -> recover or continue
```

Five user jobs define the CLI's product capability boundary:

1. **Orient.** Determine whether Open Forge is present, which workspace and scope apply, what state it is in, and which baseline or continuity context must be loaded.
2. **Retrieve and explain.** Resolve explicit routes, relationships, inheritance, dependencies, knowledge roles, syntax, provenance, and applicable context.
3. **Create and maintain.** Scaffold canonical artifacts and scopes, place them correctly, maintain derived navigation, and verify their structural contract. Agents and users continue to author semantic content directly in human-readable files.
4. **Validate and repair.** Diagnose the complete applicable workspace through read-only `doctor`, then repair mechanically safe findings only through the accepted explicit repair mode.
5. **Manage lifecycle.** Initialize, complete, upgrade, restore, and manage Extensions through distinct preservation-aware operations.

Planning, structured results, provenance, verification, transactions, rollback, and recovery are cross-cutting contracts. They support applicable jobs without becoming separate product jobs.

These jobs govern capability boundaries, not final command names, hierarchy, flags, or release slicing.

## Rationale

The five jobs are broad enough to cover the complete Framework journey while remaining small enough for an unfamiliar agent to understand cheaply.

The model keeps semantic reasoning in the agent and consequential direction with the user. The CLI supplies deterministic evidence, canonical structure, safe plans, and verified effects before and after that reasoning.

Treating direct file authoring as valid preserves the file-native contract. The CLI makes correct structure and placement cheaper without becoming a required gateway to user-owned meaning.

Separating lifecycle management from health prevents doctor from hiding installation, upgrade, restoration, or package intent. Treating plan and recovery as cross-cutting contracts prevents each command family from inventing incompatible safety.

## Alternatives And Tradeoffs

- One task-oriented magic command would reduce command discovery but require the CLI to infer semantic intent and obscure the operation being authorized.
- Many mechanical commands would keep behavior explicit but expose implementation details as product concepts and increase agent interaction cost.
- A read-only navigation tool would keep the CLI narrow but leave canonical authoring, repair, and lifecycle operations unnecessarily expensive.
- A required file-management gateway would make mutation more uniform but violate Open Forge's plain-file completeness and optional-tool boundary.

Broad job families require precise suboperations. Command design must keep those operations explicit without recreating the MVP's flat toolbox.

## Consequences

- Command and API design starts from the five jobs rather than preserving MVP vocabulary.
- Current mechanics such as index rebuilding may become effects or suboperations instead of top-level product concepts.
- Context retrieval and explanation share a product family without becoming semantic search.
- Scaffolding supports structure and placement without privately owning authored content.
- Health, repair, and lifecycle retain distinct authority.
- Every mutating operation uses the same planning, transaction, verification, and recovery contracts.
- Each replacement boundary implements one coherent accepted job slice rather than mechanically preserving an MVP command boundary or spanning unrelated jobs for migration coverage.

## Authoritative Sources

- [Open Forge CLI Architecture](../../documents/cli/architecture.md)
- [Open Forge CLI Interface](../../documents/cli/interface.md)
- [Open Forge Principles](../../documents/principles.md)

## Decision Relationships

- [Agent-first CLI product contract](cli-agent-first-product-contract.md)
- [CLI doctor and repair contract](cli-doctor-repair-contract.md)
- [Product direction](../product/product-direction.md)
