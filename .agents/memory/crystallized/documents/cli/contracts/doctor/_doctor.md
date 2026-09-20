---
open-forge:
  description: Route the accepted current read-only Doctor Interface and Behavior contracts
  responsibility: Route the Doctor contracts without adding command meaning or implementation detail
  tags: [Memory, Crystallized, CLI, Release, Command, Contract, Doctor, CurrentTruth]
---

# Doctor Command Contract Set

## Status And Authority

This routed set is the accepted current Crystallized authority for the non-shipping
`doctor` command. Its sibling Interface and Behavior files are the detailed
authorities for the public surface and technology-neutral operation. This
entrypoint provides navigation only.

Doctor is always read-only and stateless. It does not create a plan, backup,
temporary file, Git change, lifecycle effect, or repair effect. It does not
acquire the workspace mutation lock.

The accepted [CLI Architecture](../../architecture.md) defines the shared
implementation boundary. The [Shared Result
Coordinates](../shared/result-coordinates/interface.md) define the exact JSON
result schema and exit mapping. Gate 5 must prove source-generated
YamlDotNet and STJ serialization, fixed Markdig where used, real `System.IO`,
Native AOT, OS locking, isolated tests, and package journeys. This contract set
does not claim that implementation or proof.

## Contract Roles

- [`interface.md`](interface.md) defines the complete public syntax, diagnostic
  catalogue, observable result, errors, examples, and public verification.
- [`behavior.md`](behavior.md) defines deterministic six-domain diagnosis,
  coverage, finding formation, result formation, read-only safety, and
  conformance.
- No Technical Design file exists for this command. Its concrete implementation
  follows the accepted CLI Architecture and is not repeated here.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

- [Current technology-neutral six-domain diagnosis, findings, coverage, and conformance for `doctor`](behavior.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Doctor #Behavior #Diagnosis #Determinism #Safety #CurrentTruth
- [Current accepted read-only interface for complete workspace diagnosis and repair next actions](interface.md) - #Memory #Crystallized #CLI #Release #Command #Contract #Doctor #Interface #Diagnosis #Findings #Safety #CurrentTruth
