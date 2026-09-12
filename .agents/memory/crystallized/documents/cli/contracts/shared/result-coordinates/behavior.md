---
open-forge:
  description: Technology-neutral formation and conformance for the public coordinates shared by CLI results
  responsibility: Define how one concrete command result forms one conforming shared envelope and process completion
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Behavior, Result, JSON, Status, Compatibility, CurrentTruth]
---

# Shared CLI Result Coordinates Behavior Contract

## Status And Authority

This is the accepted current Behavior Contract for forming the shared public
coordinates defined by the [Interface Contract](interface.md). It does not add a
command-local payload, finding, finite value, status condition, or `next` action.
Each command contract remains authoritative for those facts.

The behavior is technology-neutral. The [CLI
Architecture](../../../architecture.md) defines the concrete typed pipeline,
source-generated serialization, renderer selection, output writer, and process
completion that realize it.

## Formation

A conforming operation forms one complete concrete command result before
presentation. That result supplies one command identity, one semantic status,
genuine workspace presence or absence, one non-null command-local payload, and
one next action or `null`.

Structured presentation projects those facts once into the selected envelope
member order from the Interface Contract. It emits every shared member, preserves
typed `null` values, and does not move a shared fact into or duplicate it under
`result`. Human and structured presentation consume the same concrete result and
do not rerun the operation.

When a command exposes an authored occurrence or span, it forms the shared
`SourceLocation` only from exact typed evidence for the physical UTF-8 layer.
Line and column count Unicode scalar values from one. Byte offset and length
count UTF-8 bytes from zero and describe one half-open span. Unavailable or
not-applicable locations remain distinct typed states and become `null` only
under the Interface Contract.

## Status And Output Conformance

The command-local contract selects one of the seven semantic statuses from its
accepted conditions. The shared mapping then selects the fixed process exit and
human primary stream without command-local override. JSON always selects one
complete stdout document for that same status. Diagnostics remain bounded on
stderr.

Terminal help and version stop before domain result formation and use their
shared text-only behavior. They do not manufacture a successful JSON envelope.

Unknown status, workspace-selection, or other shared finite values fail closed.
A renderer or output failure cannot reinterpret the command-local semantic
condition as a different ordinary status.

## Compatibility Conformance

Every schema-v1 result preserves the shared field order, names, types, presence,
nullability, finite values, source coordinates, status mapping, and stream rules.
A command-local additive field is compatible only when an older consumer can
observe its absence without losing or changing prior meaning. A conforming
implementation does not use command-local evolution to change a shared
coordinate.

## Conformance Evidence

Evidence must prove:

- the exact six-member order and presence for every status;
- `workspace` and `next` nullability and non-null concrete `result`;
- absence of shared-member duplication under every command-local payload;
- every status-to-exit and status-to-human-stream mapping;
- one complete JSON stdout document plus bounded stderr diagnostics for every
  status;
- text-only help and version bypass;
- Unicode-scalar line/column and UTF-8 half-open byte-coordinate vectors,
  including typed unavailable and not-applicable locations; and
- shared compatibility vectors plus command-local additive-field absence that
  preserves prior meaning.

## Related Current Sources

- [Shared CLI Result Coordinates Interface Contract](interface.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Architecture](../../../architecture.md)
