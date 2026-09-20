---
open-forge:
  description: Public schema-3 envelope, source-location, status, exit, stream, and compatibility coordinates shared by CLI results
  responsibility: Define the exact caller-visible result coordinates shared by replacement CLI commands
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Interface, Result, JSON, Status, Compatibility, CurrentTruth]
---

# Shared CLI Result Coordinates Interface Contract

## Status And Authority

This is the accepted current Interface Contract for the public result coordinates
shared by replacement CLI commands. It defines one schema-3 envelope, the
authored source-location primitive, semantic statuses, numeric process exits,
primary human streams, structured-output stream, terminal bypass, and shared
compatibility boundary.

Each command contract defines its exact non-null `data` object, finding codes,
command-local finite values, and `next` contents. Those local contracts cannot
reorder, omit, rename, duplicate, or reinterpret a shared coordinate.

## Schema-3 Envelope

For every domain operation using JSON output, the top-level object uses these
camel-case members in exactly this order:

```text
CliJsonEnvelopeV3 {
  schemaVersion: integer(3),
  command: exact command-owned machine identity,
  status: "completed" | "completed-with-warnings" | "incomplete" | "invalid-input" | "blocked" | "failed" | "cancelled",
  detail: "minimal" | "standard" | "full" | "debug",
  filter: ["error" | "warning" | "info", ...] | null,
  workspace: { path: string, selectedBy: "current-directory" | "explicit-workspace" } | null,
  summary: { headline: string, kind: string },
  findings: [
    {
      severity: "error" | "warning" | "info",
      code: string,
      title: string,
      message: string,
      subject: { kind, path, id, location },
      category: string | null,
      resolution: object | null,
      actions: array,
      candidates: array,
      evidence: array,
      provenance: object | null
    }
  ],
  effects: [
    { path, kind, action, outcome, reason, owner, before, after }
  ],
  counts: { "<name>": number | null, ... },
  limitations: [ { what, why, subject } ],
  data: command-owned object,
  recovery: { path, disposition } | null,
  next: { kind, command, reason } | null
}
```

`schemaVersion` is the integer `3`. `command` is the exact machine identity
owned by the selected command. `status` uses the seven shared semantic status
names. `detail` records the requested detail level. `filter` records the
selected severity values or is `null` when no filter was supplied; `all` selects
every severity. `workspace` is either `{ path, selectedBy }` or `null`, and
`selectedBy` is exactly `current-directory` or `explicit-workspace`.

`summary` carries the human headline and its typed kind. `findings`, `effects`,
`counts`, `limitations`, `data`, `recovery`, and `next` carry the complete
operation facts shared with text output. Command-local contracts define the
exact `data` object, finding codes, effect values, and next contents.

The serializer emits one minified JSON document. Detail-gated nested members
are omitted when the selected level does not include them; they are not replaced
with `null` or an empty collection. Finding severity is one of `error`,
`warning`, or `info`. Counts are plain numeric scalars or `null`, never objects
with an embedded value and reason. A limitation carries a reason when a
measurement could not be established. Requested authored content and mutation
receipts remain exact and complete.

Some command-defined counts are intentionally nullable when a value does not
apply. Whether those null values must each carry a limitation remains an open
maintainer question; the [Behavior Contract](behavior.md#open-maintainer-question)
records it without deciding it.

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

| Status                    | Process exit | Human primary stream |
| ------------------------- | -----------: | -------------------- |
| `completed`               |          `0` | stdout               |
| `completed-with-warnings` |          `2` | stdout               |
| `incomplete`              |          `3` | stdout               |
| `invalid-input`           |          `4` | stderr               |
| `blocked`                 |          `5` | stderr               |
| `failed`                  |          `1` | stderr               |
| `cancelled`               |        `130` | stderr               |

JSON is one complete stdout document for every semantic status. Bounded
diagnostics use stderr and do not invalidate that document. Terminal help and
version retain their shared text-only bypass and do not emit a domain result
envelope.

## Severity Vocabulary

Every finding has one of these shared severities. The JSON value is stable;
text uses the corresponding word.

| Value in JSON | Word in text | Meaning                                                               |
| ------------- | ------------ | --------------------------------------------------------------------- |
| `error`       | `Error`      | The command could not do part of its job, or the workspace is broken. |
| `warning`     | `Warning`    | The job was done, but something needs a look.                         |
| `info`        | `Info`       | Useful context. Nothing to do.                                        |

The resolution lanes used by Doctor and Repair are separate from severity:
`safe-exact`, `guided-choice`, `targeted-operation`, `manual-decision`,
`blocked-repair`, and `informational`.

## Compatibility

Schema-3 compatibility freezes the shared envelope field names and order, JSON
types, presence, required-versus-`null` rules, source-location shape and
coordinates, and shared finite values. A breaking change to any of those shared
coordinates requires a future schema version.

A command contract owns compatibility for its command-local `data`, finding
codes, finite values, and `next` contents. It may keep an additive optional
command-local field when absence preserves prior meaning.
Command-local evolution cannot weaken the shared presence, nullability, order,
status, exit, stream, or coordinate rules.

## Related Current Sources

- [Shared CLI Result Coordinates Behavior Contract](behavior.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Command Contract Set](../../../command-contract-set.md)
- [CLI Architecture](../../../architecture.md)
