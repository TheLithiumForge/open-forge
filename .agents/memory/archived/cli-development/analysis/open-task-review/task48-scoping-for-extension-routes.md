---
open-forge:
  description: "Historical record: Review of Task 48 Scoping for Extension Routes, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 48 Scoping For Extension Routes Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task48-scoping-for-extension-routes.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 48 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 48](../../../../working/cli-development/tasks/task48-scoping-for-extension-routes.md)

## Current Conclusion

**Recommendation:** Do after 1.0.

**Size:** Medium, driven by whether a scoped copy of an Extension entrypoint stays managed by its Extension. If it does, Extension update and remove need new alignment logic and the task becomes Large.

The gap is real and the direction is right: scoping belongs to the routing model, not to whatever Core ships. It is additive, though. Nothing breaks for existing workspaces, a manual workaround exists, and no other task waits on it. The record also misses the largest piece of work, which is how Extension update and remove would treat scoped copies.

## What It Implies

A user with Planning installed could run `open-forge route init --framework memory/release-notes/crystallized/decisions` and get a scoped Decisions entrypoint with Planning's rules instead of a placeholder. The CLI would align targets against installed Extension routes as well as the embedded Framework, and the lock would record who owns each copied segment. The route init contract, and possibly the Extension update and remove contracts, would change.

## State Today

Verified at `815324f9`:

- `RouteInitPlanningInspector` projects only `EmbeddedFrameworkSourceProjector` output, so Framework mode sees only Core routes.
- The route-init alignment tests use one-segment shapes such as `memory/release-notes/working`. The three dropped cases the record lists are absent.
- Core ships only the four Memory states below `memory`, as the record says.
- **Stale:** the record's table assigns `memory/emerging/observations` and `memory/working/handoffs` to `orchestration`. Since `a887e730` (2026-09-24) they belong to the `observations-and-handoffs` package, which `orchestration` depends on.
- The record has no Acceptance section and no Axioms marker.

Work the record does not mention:

- **Update alignment.** Framework update maps a scoped copy back to its canonical payload file through `FrameworkSourceAlignment.ReadAsset`. Extension update matches manifest paths exactly. An Extension-owned scoped copy would be a claim the package manifest does not list, so Extension update and remove would need the same alignment.
- **Exclusions.** `route init` does not consult `.agents/open-forge.json`. Copying Extension content should respect `removedExtensions` and `removedDirectories`, as the [remove contract](../../../../crystallized/documents/cli/contracts/remove/interface.md) requires of other managers.
- **Source availability.** The [Extension remove contract](../../../../crystallized/documents/cli/contracts/extension/remove/behavior.md) notes that package source may be gone. Copying needs the source bytes, which only the embedded catalogue guarantees.

One open question is already answered. Extension remove rejects removal "when a route host cannot be safely removed while retained routed descendants depend on it," so uninstalling an Extension with user scopes beneath its route is blocked.

Today's workaround: generic `route init` creates draft entrypoints with placeholder metadata, and the user copies Planning's `_decisions.md` rules by hand.

## Dependencies

- **Blocked by:** nothing now. The removal model it must respect is settled by [Task 50](../../tasks/task50-unified-remove.md).
- **Blocks:** nothing.
- **Overlaps with:** [Task 55](../../tasks/task55-alternative-root.md), which could move the root, and [Task 31](../../tasks/task31-implementation-duplication.md), whose remaining slice touches route command structure.

## Remaining Work

1. Decide ownership of a scoped copy:
   - **User-owned copy.** Copy bytes once with no claim. No update or remove changes, but the copy never receives package updates.
   - **Extension-owned copy.** Record it under the Extension and teach Extension update and remove to align it, matching Framework mode.
2. Decide whether `--framework` keeps its name or becomes one mode that reports each segment's owner.
3. Build alignment against the embedded Framework plus installed Extensions read from the lock and the embedded catalogue. Block when a package's source is unavailable.
4. Honor removal exclusions during alignment and copying.
5. Restore the three dropped cases against Planning routes.
6. Update the route init contract, the Extension contracts if needed, `docs/cli.md`, and the record's table.

## Pros And Cons

| Pros                                                                      | Cons                                                                        |
| ------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| Makes scoping work the same for Core and Extension routes                 | Additive convenience for a manual task users can already do                 |
| Restores test coverage for scopes between managed segments                | Extension-owned copies spread alignment logic into two more commands        |
| Scoped Decisions or Handoffs get their real rules instead of placeholders | Depends on package source bytes that only the embedded catalogue guarantees |
| Follows the Principles' "Context By Relevance" and recursive scoping      | Competes with 1.0 polish for attention                                      |

## Risks And Open Questions

- User-owned copies drift from the package over time. Extension-owned copies are consistent but cost more. The maintainer should choose.
- If Task 55 moves the root, the alignment code should use the root abstraction rather than `.agents` literals.
- Should `route init` honor exclusions at all, given that users may create excluded files by hand?

## Next Check

**Action:** Record the ownership choice for scoped copies in Task 48 and add an Acceptance section.

**Would change the conclusion:** User reports that scoped Extension routes are a common first-week need, which would argue for doing it before 1.0.

**Acceptance needed:** The maintainer for the ownership model. The implementer owns the flag naming if the contract keeps one mode.
