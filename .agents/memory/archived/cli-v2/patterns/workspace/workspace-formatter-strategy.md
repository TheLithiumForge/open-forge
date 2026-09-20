---
open-forge:
  description: "Historical CLI-v2 source: Select one workspace-aligned formatter strategy per affected file through explicit evidence, exact argv, trust-aware execution, and a manual fallback"
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Workspace Formatter Strategy

## Boundary

The accepted [Workspace Formatting contract](../../../../memory/crystallized/documents/cli/contracts/workspace-formatting.md) is authoritative for discovery precedence, supported tools, trust classification, authority, post-processing, validation, checksum refresh, and fallback behavior. This Pattern owns the reusable strategy and adapter arrangement.

## Shape

Keep discovery, selection, execution, and interpretation in one focused formatter slice:

```text
affected files
  + workspace formatter evidence
  + explicit formatter configuration
  -> applicable strategy candidates
  -> selected strategy or typed fallback
  -> completed primary mutation
  -> exact-file post-processing
  -> validation and checksum-refresh result
```

Each built-in adapter exposes one cohesive strategy:

```text
identifier and supported file kinds
  + configuration evidence reader
  + executable resolver
  + exact argv builder
  + result interpreter
```

Behavior is a direct function or class reference. Serializable tool and trust identifiers remain evidence interpreted through exhaustive typed branches, not keys in a service locator.

Represent custom execution as an argv vector and append already resolved exact file paths. Keep shell interpretation, discovery, trust decisions, and package acquisition outside an adapter's argv builder.

The selector returns either one complete selected strategy or one typed ambiguity, manual, or blocked outcome. The mutation coordinator consumes that result before primary application and invokes automatic formatting only through the accepted post-processing boundary.

## Source Locality

Keep each tool adapter focused and independently testable. Promote only demonstrated language, executable, configuration, or result helpers to the nearest common formatter scope. Do not build one monolithic tool switch or a universal process abstraction for hypothetical consumers.

## Review Checks

- One selected strategy owns one affected file.
- Discovery, selection, trust evidence, argv construction, execution, and interpretation remain visible boundaries.
- Commands are argv plus exact files rather than shell text.
- Serialized identifiers cannot discover executable behavior.
- Post-processing returns typed validation and checksum-refresh evidence to the coordinator.
- Tool adapters remain independently extensible.
- Exact trust and fallback semantics are linked to the Workspace Formatting contract instead of restated here.
