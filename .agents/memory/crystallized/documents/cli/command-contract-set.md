---
open-forge:
  description: Accepted current replacement CLI command-contract roles, topology, and authority boundaries
  responsibility: Define the command-local Interface, Behavior, and optional Technical Design roles and link them to detailed contracts without duplicating command detail
  tags: [Memory, Crystallized, CLI, Release, Document, Evergreen, CurrentTruth, Contract, Set, Interface, Behavior, TechnicalDesign, Routing, Locality]
---

# CLI Command Contract Set

This accepted Crystallized Document is the single concise `#Evergreen` overview
of the replacement CLI command-contract set. It defines the contract roles,
topology, and authority boundaries, and links to the detailed command-local
contracts. It is a Document, not a Pattern, and it does not replace the detailed
contracts it links. The replacement does not ship yet. Accepted implementation
and evidence exist for several foundations and commands, including public
`index`, while the broader retained command set and delivery remain incomplete.
The active [CLI Development](../../../working/cli-development/_cli-development.md)
route records exact execution state.

## Interface Contract

An Interface Contract defines what a caller may enter and what the caller may
observe. It covers the command purpose, exact syntax, operands, flags, defaults,
repetition, ordering, composition, human and structured output, semantic
results, errors, scenarios, and process-status mapping when that mapping is
accepted. It does not choose libraries, modules, algorithms, or storage
structures.

Shared [Global Flags](contracts/shared/global-flags/interface.md), [Source
References](contracts/shared/source-references/interface.md), and other shared
contract files define reusable input once. A command-local Interface states how
that shared input applies to its own operation without copying the shared
meaning.

## Behavior Contract

A Behavior Contract defines how a conforming implementation resolves, executes,
verifies, and reports an operation without depending on one technology. It covers
deterministic input resolution, current facts and completeness, selection and
ordering, result formation, read-only or mutation effects, safety, recovery, and
conformance evidence. It does not add public syntax or choose a runtime, library,
parser, module, or private schema.

Mutation contracts use the [Typed Operation Flow](shared-operation-contract.md#typed-operation-flow).
The command-local Behavior defines its exact facts, effects, safety, recovery,
and result conditions within that shared flow.

## Technical Design

A Technical Design is optional. It records concrete implementation context only
when a command or real shared capability needs a separate, reviewable source. It
may record accepted, experimental, recommended, or open implementation choices
and trace them to exact Interface or Behavior facts. It cannot add public syntax,
weaken a safety guarantee, or change a semantic result.

The accepted [CLI Architecture](architecture.md) controls high-level C# structure,
dependency direction, cross-cutting boundaries, physical source and test
organization, libraries, serialization, filesystem, recovery, testing, Native
AOT, and distribution. Local Technical Designs remain subordinate to that
Architecture and may not reopen or override it. A local design may identify
required evidence and may record accepted implementation evidence when that
context belongs with the design. The active CLI Development route remains the
source for program-wide execution state.

### Current local Technical Designs

The direct-command topology below lists local Technical Designs for `find`,
`index`, and `context`. The remaining direct commands, `route` operations,
`extension` leaves, and shared contract scopes have no local Technical Design
now. That absence is intentional. Their implementation choices remain
controlled by the Architecture and their command-local contracts until a real
local boundary earns a separate design.

## Authority boundaries

Each detailed command-local Interface and Behavior remains authoritative for its
exact question. An optional local Technical Design records implementation context
without changing either contract. Detailed Behaviors link to their Interfaces
for public meaning and to shared Framework sources for the meaning they consume.

The [Shared CLI Operation Contract](shared-operation-contract.md) remains
authoritative for cross-command conventions. It does not replace command-local
contracts. Entrypoints route their local files and generated `Entries`; they do
not create another Interface, Behavior, or Technical Design authority. The
complete detailed source set remains under [`contracts/`](contracts/_contracts.md).

## Current topology

Direct commands keep their detailed files in one local scope:

| Command      | Interface                                                            | Behavior                                                           | Technical Design                                                            |
| ------------ | -------------------------------------------------------------------- | ------------------------------------------------------------------ | --------------------------------------------------------------------------- |
| `find`       | [`find Interface`](contracts/find/interface.md)                      | [`find Behavior`](contracts/find/behavior.md)                      | [`find Technical Design`](contracts/find/technical-design.md)               |
| `index`      | [`index Interface`](contracts/index-candidate/interface.md)           | [`index Behavior`](contracts/index-candidate/behavior.md)           | [`index Technical Design`](contracts/index-candidate/technical-design.md)   |
| `status`     | [`status Interface`](contracts/status/interface.md)                  | [`status Behavior`](contracts/status/behavior.md)                  | None; no Technical Design exists.                                           |
| `context`    | [`context Interface`](contracts/context/interface.md)                | [`context Behavior`](contracts/context/behavior.md)                | [`context Technical Design`](contracts/context/technical-design.md)         |
| `references` | [`references Interface`](contracts/references-candidate/interface.md) | [`references Behavior`](contracts/references-candidate/behavior.md) | None; no Technical Design exists.                                           |
| `doctor`     | [`doctor Interface`](contracts/doctor/interface.md)                  | [`doctor Behavior`](contracts/doctor/behavior.md)                  | None; no Technical Design exists.                                           |
| `repair`     | [`repair Interface`](contracts/repair/interface.md)                  | [`repair Behavior`](contracts/repair/behavior.md)                  | None; no Technical Design exists.                                           |
| `install`    | [`install Interface`](contracts/install/interface.md)                | [`install Behavior`](contracts/install/behavior.md)                | None; no Technical Design exists.                                           |
| `update`     | [`update Interface`](contracts/update/interface.md)                  | [`update Behavior`](contracts/update/behavior.md)                  | None; no Technical Design exists.                                           |
| `cleanup`    | [`cleanup Interface`](contracts/cleanup/interface.md)                | [`cleanup Behavior`](contracts/cleanup/behavior.md)                | None; no Technical Design exists.                                           |

The public `index` command files are currently staged under
[`contracts/index-candidate/`](contracts/index-candidate/_index-candidate.md).
The public `references` command files are currently staged under
[`contracts/references-candidate/`](contracts/references-candidate/_references-candidate.md).
The replacement `index` command has proved that recognized compatibility
entrypoints are identified by physical identity and processed once. Candidate
staging remains in place until a separate accepted route migration moves these
contracts to `contracts/index/_index.md` and
`contracts/references/_references.md`. That migration is not implied by the
proved identity behavior. Frozen `open-forge-old` compatibility does not change
either command's public identity or the replacement's logical contract topology.

The grouped `route` command is routing-only. Each operation has its own local
Interface and Behavior files:

| Operation       | Interface                                                         | Behavior                                                        |
| --------------- | ----------------------------------------------------------------- | --------------------------------------------------------------- |
| `route inspect` | [`route inspect Interface`](contracts/route/inspect/interface.md) | [`route inspect Behavior`](contracts/route/inspect/behavior.md) |
| `route list`    | [`route list Interface`](contracts/route/list/interface.md)       | [`route list Behavior`](contracts/route/list/behavior.md)       |
| `route init`    | [`route init Interface`](contracts/route/init/interface.md)       | [`route init Behavior`](contracts/route/init/behavior.md)       |
| `route create`  | [`route create Interface`](contracts/route/create/interface.md)   | [`route create Behavior`](contracts/route/create/behavior.md)   |
| `route update`  | [`route update Interface`](contracts/route/update/interface.md)   | [`route update Behavior`](contracts/route/update/behavior.md)   |
| `route move`    | [`route move Interface`](contracts/route/move/interface.md)       | [`route move Behavior`](contracts/route/move/behavior.md)       |
| `route remove`  | [`route remove Interface`](contracts/route/remove/interface.md)   | [`route remove Behavior`](contracts/route/remove/behavior.md)   |

The genuinely shared contracts live in the permanent [`contracts/shared/`](contracts/shared/_shared.md)
scope:

- [Global Flags Interface](contracts/shared/global-flags/interface.md) and
  [Global Flags Behavior](contracts/shared/global-flags/behavior.md)
- [Source References Interface](contracts/shared/source-references/interface.md)
  and [Source References Behavior](contracts/shared/source-references/behavior.md)
- [Source Universe Filters Interface](contracts/shared/source-universe-filters/interface.md)
  and [Source Universe Filters Behavior](contracts/shared/source-universe-filters/behavior.md)

Consumers link to these shared files instead of copying their meaning into each
command. Source Universe Filters are reusable operation-specific flags, not
global flags. No shared Technical Design file is needed now.

The grouped [`extension`](contracts/extension/_extension.md) route has six actual
leaf operations. Each leaf keeps its Interface and Behavior in its own local
scope:

| Operation           | Interface                                                       | Behavior                                                      | Technical Design                   |
| ------------------- | --------------------------------------------------------------- | ------------------------------------------------------------- | ---------------------------------- |
| `extension list`    | [`list Interface`](contracts/extension/list/interface.md)       | [`list Behavior`](contracts/extension/list/behavior.md)       | None; no Technical Design exists. |
| `extension inspect` | [`inspect Interface`](contracts/extension/inspect/interface.md) | [`inspect Behavior`](contracts/extension/inspect/behavior.md) | None; no Technical Design exists. |
| `extension create`  | [`create Interface`](contracts/extension/create/interface.md)   | [`create Behavior`](contracts/extension/create/behavior.md)   | None; no Technical Design exists. |
| `extension install` | [`install Interface`](contracts/extension/install/interface.md) | [`install Behavior`](contracts/extension/install/behavior.md) | None; no Technical Design exists. |
| `extension update`  | [`update Interface`](contracts/extension/update/interface.md)   | [`update Behavior`](contracts/extension/update/behavior.md)   | None; no Technical Design exists. |
| `extension remove`  | [`remove Interface`](contracts/extension/remove/interface.md)   | [`remove Behavior`](contracts/extension/remove/behavior.md)   | None; no Technical Design exists. |

The `extension` group performs no operation or wizard. Its six leaves share the
accepted lifecycle facts and safety shape while keeping Framework and Extension
ownership, source selection, and removal boundaries separate. These contract
routes do not replace the accepted Architecture.

The [CLI contract document Templates](../../../../templates/cli/documents/_documents.md)
are optional copy-ready starters for a new command-local set. They are not
current command contracts and do not update existing files.
