---
open-forge:
  description: Implement retained route mutation commands on the accepted mutation foundation
  tags: [Memory, Working, CLI, Task, Route, Mutation, Contextual]
---

# Route Mutation Commands

## Task State

- State: Active. Route Init is Complete in exact reviewed feature candidate
  `cb62b19f73afcace163371af9093d877821fa800`, pending protected integration.
  Route Create is next after that integration; later Route Mutation commands
  remain Planned in the accepted sequence.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Common prerequisites: Route discovery, `index`, and Mutation Foundation.
- Intended-membership formation prerequisite: accepted feature
  `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated at
  `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with closeout integrated at
  `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
  `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. It applies only to Route Init,
  Route Create, Route Move, and Route Remove. Route Update is independent of it.

## Shared Boundary

Each route command owns one concrete plan and result. Commands reuse accepted
route facts, validation, lock, lifecycle, atomic file, and external
recovery-bundle primitives. They do not share a universal route mutation request
or plan.

Init, Create, Move, and Remove consume the accepted
`Build(SourceCatalogue observedCatalogue, IReadOnlyList<SourceLogicalSource> intendedSources)`
overload to project their complete post-operation Generated Navigation graph from
real observed evidence. The overload extends existing formation only. It does not
create a prospective catalogue/source framework, virtual filesystem, temporary
checkout, or hidden Index. Existing `Build(SourceCatalogue)` remains unchanged
current-state behavior and delegates to the overload.

Implement in order because later commands consume identity, collision, overwrite,
and lifecycle facts established by earlier operations. Every command proves dry
run where contracted, unchanged bytes on blocked paths, revalidation after lock,
idempotence where promised, one verified external recovery bundle covering every
existing Replace/Delete prepared before the first target effect, retained partial state
without restoration, process streams, and Native AOT.

The maintainer accepted both generic exact-chain behavior and explicit
`route init --framework`. Framework mode is sequenced after root Install because
it reuses the neutral embedded payload/topology and requires trusted current base
lifecycle state. Route Move and Route Remove retain separate future
reference and lifecycle proportionality gates. Resolve those command-local gates
before their behavior lanes begin; the shared formation does not make either
command Active or Ready.

## Child Tasks

- [x] [Implement generic and Framework-aware sparse route initialization after root Install](route-init.md) — Complete at exact candidate `cb62b19`; pending protected integration — Implementer: Overseer-managed Task Mastermind
- [ ] [Implement one-file route creation below an existing routable parent](route-create.md) — Planned — Implementer: Not assigned
- [ ] [Implement bounded route content and metadata update without identity drift](route-update.md) — Planned — Implementer: Not assigned
- [ ] [Implement route move with reference, overwrite, generated-navigation, and recovery integrity](route-move.md) — Planned — Implementer: Not assigned
- [ ] [Implement route removal with dependency, reference, generated-navigation, and recovery integrity](route-remove.md) — Planned — Implementer: Not assigned

## Preparation Closeout

Initial read-only preparation completed for every M2 leaf on clean no-op branches
from exact base `33913dfe7f8f80598ca4765c516d308ed179c3ab`. It produced no
preparation commit, Gray, Red, or Green change. Two later shared prerequisites
are now integrated into local `develop`: canonical lifecycle creation at
`1d404c5cef3f5fd464ca771fc132a657f792f533` and the neutral Markdown link-label
projection at `89a35a7876f39123d9538bca24126ff7197b9459`.

| Command | Preparation branch | Readiness |
| --- | --- | --- |
| Route Init | `codex/route-init` | Complete at exact candidate `cb62b19`; pending protected integration |
| Route Create | `codex/route-create` | Next after Route Init integration; its accepted contract correction is integrated at `cd01b8a` |
| Route Update | `codex/route-update` | Waiting for Route Create and a closed callable/public-result boundary |
| Route Move | `codex/route-move` | Waiting for Route Update, its remaining proportionality/wire gate, and the accepted neutral-reference correction |
| Route Remove | `codex/route-remove` | Waiting for Route Move and its command-local result freeze; its parser and canonical lifecycle prerequisites are integrated |

The required behavior and integration order is root Install, Route Init, Route
Create, Route Update, Route Move, Route Remove, then root Update M3. Independent
preparation may overlap; dependent command behavior may not. Each leaf Task owns
its expected paths, protected integration neighborhood, evidence boundary, and
accepted preparation decisions or remaining maintainer-authority questions.
Preparation state alone is not authorization to implement an unresolved public
or shared surface. The integrated prerequisites add no Route Remove command,
public wire, or shared mutation behavior. Route Remove remains Planned after
Route Move.

Modern C# and the existing BCL-first foundations are sufficient for the prepared
work. No workaround, second Markdown parser, duplicate reference resolver, or
general route-mutation engine is justified. Route Create crossed its no-restore
verification boundary; the leaf Task retains that process/evidence limitation.

## Entries

<!-- open-forge:generated-index:start -->
- [Implement one-file route creation below an existing routable parent](route-create.md) - #Memory #Working #CLI #Task #Route #Create #Mutation #Contextual
- [Implement generic and Framework-aware sparse route initialization after root Install](route-init.md) - #Memory #Working #CLI #Task #Route #Init #Mutation #Contextual
- [Implement route move with reference, overwrite, generated-navigation, and recovery integrity](route-move.md) - #Memory #Working #CLI #Task #Route #Move #Mutation #Contextual
- [Implement route removal with dependency, reference, generated-navigation, and recovery integrity](route-remove.md) - #Memory #Working #CLI #Task #Route #Remove #Mutation #Contextual
- [Implement bounded route content and metadata update without identity drift](route-update.md) - #Memory #Working #CLI #Task #Route #Update #Mutation #Contextual
<!-- open-forge:generated-index:end -->
