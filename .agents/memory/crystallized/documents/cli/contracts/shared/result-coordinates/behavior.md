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
genuine workspace presence or absence, one command-local `data` object, and one
next action or `null`.

Presentation projects those facts once into the schema-3 envelope member order
from the Interface Contract. It emits the shared envelope coordinates,
preserves typed `null` values, and does not move a shared fact into or duplicate
it under `data`. Text and JSON consume the same concrete result and do not rerun
the operation.

Counts form one object of named plain numeric scalars or `null`. A null count
records a value that is unavailable or does not apply; a limitation explains
why a measurement could not be established when the command has such a reason.

When a command exposes an authored occurrence or span, it forms the shared
`SourceLocation` only from exact typed evidence for the physical UTF-8 layer.
Line and column count Unicode scalar values from one. Byte offset and length
count UTF-8 bytes from zero and describe one half-open span. Unavailable or
not-applicable locations remain distinct typed states and become `null` only
under the Interface Contract.

## Open Maintainer Question

Counts currently include intentional nullable values for a finite set of count
names. The strict rule that every `null` has a limitation does not cover those
values. The maintainer must decide whether to omit those members or give each
one a limitation.

## Status And Output Conformance

The command-local contract selects one of the seven renamed semantic statuses
from its accepted conditions. The shared mapping then selects the fixed process
exit and human primary stream without command-local override. The statuses are
`completed`, `completed-with-warnings`, `incomplete`, `invalid-input`,
`blocked`, `failed`, and `cancelled`. JSON always selects one complete stdout
document for that same status. Diagnostics remain bounded on stderr. Human
colour capability follows that selected primary stream under the [automatic
colour contract](../global-flags/interface.md#automatic-colour); it does not
participate in result formation or structured serialization.

Terminal help and version stop before domain result formation and use their
shared text-only behavior. They do not manufacture a successful JSON envelope.

Unknown status, workspace-selection, or other shared finite values fail closed.
A renderer or output failure cannot reinterpret the command-local semantic
condition as a different ordinary status. Finding severity is always one of
`error`, `warning`, or `info`, and it is independent of semantic status.

## Compatibility Conformance

Every schema-3 result preserves the shared field order, names, types, presence,
nullability, finite values, source coordinates, status mapping, and stream rules.
A command-local additive field is compatible only when a consumer can observe
its absence without losing or changing prior meaning. A conforming
implementation does not use command-local evolution to change a shared
coordinate. A breaking change to a shared coordinate requires a future schema
version.

## Conformance Evidence

Evidence must prove:

- the exact schema-3 member order and presence for every status;
- `workspace` and `next` nullability and non-null command-owned `data`;
- absence of shared-member duplication under every command-local payload;
- every status-to-exit and status-to-human-stream mapping;
- one complete JSON stdout document plus bounded stderr diagnostics for every
  status;
- the `error`, `warning`, and `info` finding severity vocabulary;
- plain scalar counts and their limitation coordinates;
- text-only help and version bypass;
- Unicode-scalar line/column and UTF-8 half-open byte-coordinate vectors,
  including typed unavailable and not-applicable locations; and
- shared compatibility vectors plus command-local additive-field absence that
  preserves prior meaning.

## Related Current Sources

- [Shared CLI Result Coordinates Interface Contract](interface.md)
- [Shared CLI Operation Contract](../../../shared-operation-contract.md)
- [CLI Architecture](../../../architecture.md)
