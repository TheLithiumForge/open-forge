---
open-forge:
  description: Implement generic and Framework-aware sparse route initialization after root Install
  tags: [Memory, Working, CLI, Task, Route, Init, Mutation, Contextual]
---

# Implement Route Init

## Task State

- State: Planned after root Install integration and the shared directory-create
  foundation; all route-mode and directory-effect decisions are closed.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/init/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/init/behavior.md).

## Accepted Mode Boundary

Generic mode retains exact-chain initialization and the fixed draft scaffold.
`--framework` accepts one desired concrete route, aligns exact canonical
non-root Framework segments uniquely against the embedded topology, treats
inserted segments as scope labels, and creates only the requested sparse chain.
It is not `install --route`, a blueprint/Template engine, or a `--scope`
placeholder language.

## Expected Outcome

For one exact concrete route target, inspect the current chain, plan every missing
entrypoint, preserve existing routable parents and authored content, apply the
fixed accepted scaffold under lock/revalidation, refresh generated navigation,
verify the resulting chain, and form one concrete result.

## Architecture

- Keep definitions, binding, request, plan, operation, result, and renderers at
  `Commands/Route/Init/`.
- Use route shared identity/topology facts and mutation primitives.
- Keep scaffold selection and route-init policy local.
- Reuse the neutral embedded Framework distribution and trusted root Install
  lifecycle. ID-form scope conversion remains one Route Init value policy; exact
  `.agents/...` targets are never slugged.
- Keep inserted scope entrypoints user-owned. Lifecycle-manage only copied
  canonical Framework assets and derived generated regions, with exact
  `sourceAssetPath` provenance.
- Reuse the shared parent-first ordinary-BCL directory capability while holding
  the workspace lease for descendants below `.agents`. Generic mode reports a
  missing `.agents` as the one bootstrap directory created and verified by
  `WorkspaceLockManager` immediately before lease acquisition; Framework mode
  requires an existing trusted Install. Keep directory effects separate from
  file effects and retain verified created or bootstrapped directories as
  reported residuals after contention, later failure, or interruption.
- Model inspect, plan, apply, verify, and lifecycle effects separately.

## Evidence

Cover full existing, partially missing, completely missing, invalid segments,
collisions, physical aliases, dry run, lock race, generated navigation,
idempotence, external bundle preparation/retention for any existing
Replace/Delete, human/JSON/help/diagnostics, unchanged unrelated bytes, process
exits, and AOT. Cover Framework alignment with zero/multiple/consecutive scopes,
all scope positions, slug rules and exact-path non-slugging, ambiguity,
reordering, root recreation, trusted/outdated Install state, exact embedded
bytes, sparse creation, and lifecycle ownership exclusion for scope files.
Prove the shared directory capability through missing-parent races,
post-verification, retained residuals, and file/recovery non-regression.
Cover generic missing-`.agents` bootstrap planning, cancellation, contention,
residual reporting, nullable lock bootstrap outcomes across
acquired/failed/cancelled results, and exclusion from the descendant applier;
plus Framework-mode refusal when trusted Install state is absent.

## Stop Conditions

Stop on ambiguous route alignment, managed-segment reordering, root recreation,
broad subtree ownership, general slug/template machinery, missing trusted root
Install state, lifecycle meaning not defined by accepted contracts, a
command-local directory mechanism, directory rollback or recovery, or behavior
that weakens the accepted bootstrap, lease, revalidation, and retained-residual
boundary. Do not move or duplicate the workspace lock or use
`LocalApplicationData` for it.
