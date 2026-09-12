---
open-forge:
  description: Public expanded and compact envelopes, source-location, status, exit, stream, and compatibility coordinates shared by CLI results
  responsibility: Define the exact caller-visible result coordinates shared by replacement CLI commands
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Interface, Result, JSON, Status, Compatibility, CurrentTruth]
---

# Shared CLI Result Coordinates Interface Contract

## Status And Authority

This is the accepted current Interface Contract for the public result coordinates
shared by replacement CLI commands. It defines the expanded and compact envelopes, the
authored source-location primitive, semantic statuses, numeric process exits,
primary human streams, structured-output stream, terminal bypass, and shared
compatibility boundary.

Each command contract defines its exact non-null `result` object, finding codes,
command-local finite values, and `next` contents. Those local contracts cannot
reorder, omit, rename, duplicate, or reinterpret a shared coordinate.

## Schema-V1 Envelope

For a domain operation using expanded JSON presentation, the top-level object uses
camel-case members in exactly this order. Every member is present, including a
member whose value is `null`:

```text
CliJsonEnvelopeV1 {
  schemaVersion: integer(1),
  command: exact command-owned machine identity,
  status: "complete" | "attention" | "incomplete" | "invalid" | "blocked" | "failed" | "interrupted",
  workspace: { path: string, selectedBy: "current-directory" | "explicit-workspace" } | null,
  result: command-owned object,
  next: { command: string, reason: string } | null
}
```

`schemaVersion` is the integer `1`. `command` is the exact machine identity
owned by the selected command. `workspace` is either `{ path, selectedBy }` or
`null`; `selectedBy` is exactly `current-directory` or `explicit-workspace`.
`result` is the command-owned concrete object and is never `null`. `next` is
either `{ command, reason }` or `null`.

The shared `command`, `status`, `workspace`, and `next` members are not duplicated
inside the command-local `result` object.

## Compact JSON Envelope

Explicit --json --view=compact uses this exact ordered envelope. Every envelope
member is present, including null workspace or next:

```text
CliCompactJsonEnvelope {
  schemaVersion: integer(2),
  view: "compact",
  command: exact command-owned machine identity,
  status: existing semantic status,
  workspace: { path: string, selectedBy: "current-directory" | "explicit-workspace" } | null,
  result: command-owned compact object,
  next: { command: string, reason: string } | null
}
```

The serializer emits minified JSON. Shared identity, status, workspace, next,
exit and stream semantics are identical to expanded output. Command Interfaces
define exact compact membership; omission of supporting evidence is neither null
nor an empty or unavailable observation. Counts and coverage describe the whole
operation. No arbitrary truncation or filtering is implied.

A command may retain its complete result as its compact core when its facts are
all needed, especially for mutation receipts that cannot safely be recovered by
rerunning the operation. Requested authored content remains exact. Any compact
location shape that omits byte coordinates is explicitly defined by its command
contract; all retained coordinates preserve the meanings below.

## Source Location

A command result that exposes an authored occurrence or span uses this shared
primitive:

```text
SourceLocation {
  line: integer >= 1,
  column: integer >= 1,
  byteOffset: integer >= 0,
  byteLength: integer >= 0
}
```

`line` and `column` are 1-based Unicode-scalar positions. `byteOffset` is
zero-based from the start of the exact UTF-8 physical layer. `byteLength` is a
nonnegative UTF-8 byte count. The byte span is half-open:
`[byteOffset, byteOffset + byteLength)`.

When a location applies to a public fact, it is `null` only when it is
unavailable and a typed finding explains that unavailability. A location field
may also be `null` when location does not apply to that fact. Parser-native spans
are not exposed.

## Status, Exit, And Stream Coordinates

Human primary output follows this exhaustive table. Bounded diagnostics remain
on stderr:

| Status        | Process exit | Human primary stream |
| ------------- | -----------: | -------------------- |
| `complete`    |          `0` | stdout               |
| `attention`   |          `2` | stdout               |
| `incomplete`  |          `3` | stdout               |
| `invalid`     |          `4` | stderr               |
| `blocked`     |          `5` | stderr               |
| `failed`      |          `1` | stderr               |
| `interrupted` |        `130` | stderr               |

JSON is one complete stdout document for every semantic status. Bounded
diagnostics use stderr and do not invalidate that document. Terminal help and
version retain their shared text-only bypass and do not emit a domain result
envelope.

## Compatibility

Schema-v1 compatibility freezes the shared envelope field names and order, JSON
types, presence, required-versus-`null` rules, source-location shape and
coordinates, and shared finite values. A breaking change to any of those shared
coordinates increments `schemaVersion`.

A command contract owns compatibility for its command-local `result`, finding
codes, finite values, and `next` contents. It may keep an additive optional
command-local field in schema v1 only when absence preserves prior meaning.
Command-local evolution cannot weaken the shared presence, nullability, order,
status, exit, stream, or coordinate rules.

## Related Current Sources

- [Shared CLI Result Coordinates Behavior Contract](behavior.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Command Contract Set](../../../command-contract-set.md)
- [CLI Architecture](../../../architecture.md)
