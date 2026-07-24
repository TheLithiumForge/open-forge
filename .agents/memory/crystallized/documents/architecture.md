---
open-forge:
  description: Current Open Forge system boundary, execution path, layer ownership, truth model, and packaging
  tags: [Memory, Document, CurrentTruth, Evergreen, Architecture, Framework]
---

# Open Forge Architecture

## System Boundary

Open Forge is a plain-file operating contract for agent work. `AGENTS.md` is the canonical workspace entry, minimal harness bridges such as `CLAUDE.md` import that contract, and `.agents/loader.md` defines the universal routing and authority model. The framework remains usable by reading and editing Markdown without the CLI.

## Execution Path

```text
workspace instruction entry
  -> loader authority, tags, and direct root routes
  -> baseline-loaded directives and route entrypoints
  -> task-relevant routed bodies
  -> native agent work, optionally shaped by installed primitives
```

Each routed folder has one small `entrypoint`. Generated `Entries` expose only direct children, so agents select context top-down from paths, descriptions, and tags. Load-policy tags change when material is read; they do not create authority.

## Layers

- Core owns the loader, routing, workspace orientation, and the directive, pattern, guidance, skill, and workflow primitives.
- Memory records working, emerging, crystallized, and archived state, including descriptions of behavior that are not active merely because Memory contains them.
- Extensions add optional routed files and support material through the existing Core and Memory structures.

Layer tags describe installable responsibility, not authority rank.

## Truth Lifecycle

```text
unsettled material
  -> contextual candidate
  -> accepted direction
  -> promotion at the owning route or system
  -> synchronization of affected Evergreen material
  -> archival or pruning of superseded context
```

The [defined truth tags](../../../loader.md#defined-tags) keep authority and synchronization independent: #CurrentTruth does not imply #Evergreen, and #Evergreen does not create authority.

Clear user direction becomes accepted without redundant confirmation; ambiguous direction remains #Contextual until work depends on it. Memory entrypoints own capture, movement, consolidation, archival, and restoration of recorded state. Accepted behavior that should guide future work belongs in the matching Core primitive. Current views present coherent state, while decisions preserve useful rationale and consequences instead of duplicating the selected behavior.

## Sources And Packaging

- `src/open-forge/` is the installable Core and Memory payload.
- Root `.agents/` dogfoods that payload and may add repository-specific material.
- `docs/framework/` governs maintainers and mirrors every installed payload file with a descriptor.
- `src/extensions/` contains optional first-party packages.
- `src/cli/cli.ts` implements deterministic installation, routing, validation, extension lifecycle, and packaging assistance.
- `dist/` is generated release output and is never edited by hand.

Shared contract changes update dogfood, installable source, governance, and tests together. Generated route indexes are rebuilt after structural or metadata changes.

## Related Current Views

- [Current product vision](vision.md)
