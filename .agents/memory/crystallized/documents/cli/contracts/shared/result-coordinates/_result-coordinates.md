---
open-forge:
  description: Accepted shared CLI result envelope, source-location, status, exit, stream, and compatibility contracts
  responsibility: Route the public result-coordinate Interface and technology-neutral Behavior contracts
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Shared, Result, JSON, Status, CurrentTruth]
---

# Shared CLI Result Coordinates

## Status And Authority

This contract set defines the public coordinates shared by every replacement CLI
command result. The [Interface Contract](interface.md) defines the caller-visible
JSON envelope, source-location primitive, semantic statuses, process exits,
primary streams, and compatibility boundary. The [Behavior Contract](behavior.md)
defines technology-neutral formation and conformance from one concrete command
result.

Command-local contracts continue to define their exact `result` object, finding
codes, command-local finite values, and `next` contents. The [Shared CLI
Operation Contract](../../../shared-operation-contract.md) defines when the
cross-command status and stream rule applies. The [CLI
Architecture](../../../architecture.md) defines concrete result types, pipeline
relationships, source-generated serialization, and process completion without
redefining these public coordinates.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- [Technology-neutral formation and conformance for the public coordinates shared by CLI results](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #Behavior #Result #JSON #Status #Compatibility #CurrentTruth
- [Public schema-v1 envelope, source-location, status, exit, stream, and compatibility coordinates shared by CLI results](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Shared #Interface #Result #JSON #Status #Compatibility #CurrentTruth
<!-- open-forge:generated-index:end -->
