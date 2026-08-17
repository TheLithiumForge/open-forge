---
open-forge:
  description: Historical CLI-v2 source: Shape CLI commands so operation, subject, flags, help, and failure behavior remain visible and predictable
  tags: [Memory, Archived, Contextual, Historical, CLI, CLIv2, RawData]
---

# Predictable CLI Command Surface

## Boundary

The accepted [CLI Interface](../../../../memory/crystallized/documents/cli/interface.md) is authoritative for command names, operands, path grammars, flags, aliases, applicability, and public behavior. This Pattern owns the reusable command-tree and definition shape that makes that interface inspectable in source.

## Shape

Map one invocation to one visible leaf:

```text
open-forge <direct-command> [operands] [flags]
open-forge <group> <operation> [operands] [flags]
```

Choose the path by semantic locality:

| Relationship                                         | Shape          |
| ---------------------------------------------------- | -------------- |
| One complete job                                     | Direct command |
| Several operations share one domain or lifecycle     | Subject family |
| One operation genuinely spans several explicit kinds | Action family  |

Keep behavior out of group nodes. A leaf owns one operation contract and maps to one focused metadata object, named handler, and registration function. A mutating leaf reuses the shared preview and mutation boundary rather than registering a second planning command.

Represent shared syntax once at the composition scope:

```text
composition root
  -> shared command and flag definitions
  -> typed parser root
  -> directly imported group and leaf registration functions
```

Leaf actions consume inherited parser values and translate only applicable execution facts into complete request construction. They do not redeclare shared syntax or forward terminal and presentation state into domain behavior.

When a leaf supports guided subject selection, keep its selector and explicit operand path as two presentations over the same request resolver. The [request-construction contract](../../../../memory/crystallized/documents/cli/contracts/request-construction.md) owns the exact omitted, explicit, structured, and authority behavior.

Use purpose-specific operand metadata rather than a generic path or source definition reused across unrelated boundaries. Help, completion, parse failures, and typo suggestions derive from the same visible command metadata and accepted parser-result contract.

Use the [CLI Command Slice](command-slice.md) Pattern for source placement, the [Guided Operation](guided-operation.md) Pattern for request completion, the [Result And Display Boundary](result-display-boundary.md) Pattern for output, and the [Planned Mutation](../filesystem/planned-mutation.md) Pattern for persistent effects.

## Review Checks

- One public leaf maps to one explicit operation and one focused registration path.
- Grouping follows semantic locality, and group nodes contain no domain behavior.
- Shared syntax has one definition and one parser registration boundary.
- Leaf actions translate parser values instead of leaking parser or terminal state into handlers.
- Operands retain purpose-specific identities and cannot select unrelated input kinds through guessing.
- Help, completion, parsing, and registration consume the same metadata.
- Interactive and explicit inputs meet at one complete request boundary.
- Exact public behavior is linked to the CLI Interface instead of restated here.
