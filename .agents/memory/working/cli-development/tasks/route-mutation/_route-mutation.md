---
open-forge:
  description: Implement retained route mutation commands on the accepted mutation foundation
  tags: [Memory, Working, CLI, Task, Route, Mutation, Contextual]
---

# Route Mutation Commands

## Task State

- State: Active. Route Init is Complete and squash-integrated at
  `cc5085ce51ca624d07c347b014e036b8c3b7e1b4`. Route Create is Complete and
  squash-integrated at `19412d2a562ae66d1b4642256d854438df75366f`, exact tree
  `2bbba7e216e75809e99213e0ccd155bffe720d1f`, from reviewed candidate
  `392114a03a3c1329eb3ce9410795dcd36419815c`. The separate [CLI Quality
  Remediation](../cli-quality-remediation.md) root Task is Complete and
  squash-integrated at `862cbf2a`, exact tree `571f104f`, from accepted
  implementation candidate `a4ccf19a`, tree `97254e65`. Task 3 “Route Update”
  is Complete at accepted closeout `27df8325`, tree `b68d4349`. Task 4 “Route
  Move” is Complete at accepted closeout
  `631983ea1ec7d7ad5f5fc3999f938ba5f445ed81`, tree
  `d6f6fdf7d1caf62a9ac68582609c7282921f557c`, and is squash-integrated by the
  commit containing this record. Route Remove remains Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Common prerequisites: Route discovery, `index`, and Mutation Foundation.
- Intended-membership formation prerequisite: accepted feature
  `f82c2b168657baf2fac50c76e2ff2cc0ed3776d7`, squash-integrated at
  `cc35c853fd7b55d31e3fb2c9a454d9ab61c1884e`, with closeout integrated at
  `18f2acff31cfd6600a16430ac5d689d05482e297`, exact tree
  `39f0a8c4e6d3695cfbe7407dfd6043dc5ec9680a`. Route Init, Route Create,
  Route Update, Route Move, and Route Remove consume its accepted
  generated-navigation formation or projection capabilities.

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
current-state behavior and delegates to the overload. Update consumes the same
accepted generated-navigation formation and projection behavior through its
command-local destination/navigation planning boundary.

Implement in order because later commands consume identity, collision, overwrite,
and lifecycle facts established by earlier operations. Every command proves dry
run where contracted, unchanged bytes on blocked paths, revalidation after lock,
idempotence where promised, one verified external recovery bundle covering every
existing Replace/Delete prepared before the first target effect, retained partial state
without restoration, process streams, and Native AOT.

The maintainer accepted both generic exact-chain behavior and explicit
`route init --framework`. Framework mode is sequenced after root Install because
it reuses the neutral embedded payload/topology and requires trusted current base
lifecycle state. Route Move completed its separate reference and lifecycle
proportionality gates. Route Remove retains its own future command-local gate;
the shared formation does not make it Active or Ready.

## Child Tasks

- [x] [Implement generic and Framework-aware sparse route initialization after root Install](route-init.md) — Complete and squash-integrated at `cc5085ce` — Implementer: Overseer-managed Task Mastermind
- [x] [Implement one-file route creation below an existing routable parent](route-create.md) — Complete and squash-integrated at `19412d2` — Implementer: Overseer-managed Route Create Task Mastermind
- [x] [Implement bounded route content and metadata update without identity drift](route-update.md) — Complete at accepted closeout `27df8325`, tree `b68d4349` — Implementer: Overseer-managed Route Update Task Mastermind
- [x] [Implement route move with reference, overwrite, generated-navigation, and recovery integrity](route-move.md) — Complete at closeout `631983ea`, tree `d6f6fdf7`, and squash-integrated by the commit containing this record — Implementer: Route Move Task Mastermind with explicit Gray/Red owners and one continuous Brilliant Implementer
- [ ] [Implement route removal with dependency, reference, generated-navigation, and recovery integrity](route-remove.md) — Planned — Implementer: Not assigned

CLI Quality Remediation is a separate root Task, not a child of Route Mutation.
Route Create's protected integration owns its two predecessor slices; the
Complete remediation Task consumed/revalidated them and closed its residual
serializer/help work and batches 3–9. Local remediation integration remains
before Route Update activation; that boundary is now complete.

## Preparation Closeout

Initial read-only preparation completed for every M2 leaf on clean no-op branches
from exact base `33913dfe7f8f80598ca4765c516d308ed179c3ab`. It produced no
preparation commit, Gray, Red, or Green change. Two later shared prerequisites
are now integrated into local `develop`: canonical lifecycle creation at
`1d404c5cef3f5fd464ca771fc132a657f792f533` and the neutral Markdown link-label
projection at `89a35a7876f39123d9538bca24126ff7197b9459`.

| Command      | Preparation branch   | Readiness                                                                                                                                                                                                                            |
| ------------ | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Route Init   | `codex/route-init`   | Complete and squash-integrated at `cc5085ce`; reviewed closeout `c580149`, tree `a1810c4`                                                                                                                                            |
| Route Create | `codex/route-create` | Complete and squash-integrated at `19412d2`; reviewed candidate `392114a`, tree `2bbba7e`; it supplies the `RouteCreateJsonContext` and Route-help predecessor slices plus the maintainer-approved required effect-change correction |
| Route Update | `codex/route-update` | Complete at accepted closeout `27df8325`, tree `b68d4349`, from base `5aad04a`, tree `0f56b2c`; final executable behavior is `8a398f2e`, tree `3bb4a224`, with holistic review PASS                                                  |
| Route Move   | `codex/route-move`   | Complete at phase 6/6, milestone 12/12; accepted closeout `631983ea`, tree `d6f6fdf7`, is squash-integrated by the commit containing this record                                                                                      |
| Route Remove | `codex/route-remove` | Planned behind Task 12 and the accepted adoption slice; Route Move, parser, and canonical lifecycle prerequisites are integrated                                                                                                   |

The Route Mutation dependency order remains root Install, Route Init, Route
Create, Route Create protected integration with its two predecessor slices,
separate CLI Quality Remediation integration, Route Update, Route Move, Route
Remove, then root Update M3. The project-control ledger now places Task 12 and
the accepted adoption slice before Route Remove without changing that internal
dependency. Independent preparation may overlap; dependent command behavior may
not. Each leaf Task owns
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
