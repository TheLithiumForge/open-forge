---
open-forge:
  description: Historical analysis that prepared the accepted loader alignment, maintenance migration, reference cleanup, and verification pass
  tags: [Memory, Analysis, Archived, Historical, Framework, Loader, Routing, Maintenance, Migration]
---

# Loader Migration Readiness

Status: implemented and archived on 2026-07-26. The current contract lives in [loader maintenance](../../crystallized/documents/maintenance/payload/agents/loader.md), while the installed loaders remain authoritative for their runtime wording.

## Question

What should the next loader migration preserve, clarify, remove, document, and verify now that the Vision, Principles, top Architecture, and Framework Architecture provide an accepted design baseline?

## Scope

This analysis covers:

- The dogfood and installable loaders
- The canonical `AGENTS.md` entry and `CLAUDE.md` bridge
- Current and unmigrated maintenance documents
- Accepted architecture, decisions, evaluation evidence, tests, implementation behavior, and relevant history
- A migration-ready candidate for loader content, tone, maintenance, and verification

This analysis originally proposed changes without applying them. The accepted pass subsequently aligned the loader, root harness maintenance, supporting decisions, governance references, and verification.

CLI overhaul internals, Extension architecture, category-specific behavior, Markdown syntax help, and knowledge-role help remain outside this migration except where the loader must preserve a boundary with them.

## Evidence

### Accepted Current Design

- [Open Forge Principles](../../crystallized/documents/principles.md)
- [Top Open Forge Architecture](../../crystallized/documents/architecture.md)
- [Open Forge Framework Architecture](../../crystallized/documents/framework/architecture.md)
- [Typed authoritative-source terminology](../../crystallized/decisions/authoritative-source-terminology.md)
- [Routing model rationale](../../crystallized/decisions/routing-model.md)
- [Routing surface rationale](../../crystallized/decisions/routing-surfaces.md)
- [Tag rationale](../../crystallized/decisions/tags.md)
- [Loading reliability rationale](../../crystallized/decisions/loading-reliability.md)
- [Scope and slug rationale](../../crystallized/decisions/scope-and-slugs.md)
- [Source and packaging rationale](../../crystallized/decisions/source-and-packaging.md)
- [User-facing writing contract](../../crystallized/decisions/user-facing-writing.md)

### Current Runtime Sources

- [Dogfood loader](../../../loader.md)
- [Installable loader](../../../../src/open-forge/.agents/loader.md)
- [Dogfood AGENTS.md](../../../../AGENTS.md)
- [Installable AGENTS.md](../../../../src/open-forge/AGENTS.md)
- [Dogfood CLAUDE.md](../../../../CLAUDE.md)
- [Installable CLAUDE.md](../../../../src/open-forge/CLAUDE.md)

The source and dogfood versions of all three surfaces are currently byte-identical. Each loader has 49 non-empty authored lines before its generated entries, inside the existing 35-to-70-line maintenance budget.

### Current Maintenance Sources

- [AGENTS entry maintenance contract](../../crystallized/documents/maintenance/payload/AGENTS.md)
- [Claude bridge maintenance contract](../../crystallized/documents/maintenance/payload/CLAUDE.md)
- [Agents runtime maintenance route](../../crystallized/documents/maintenance/payload/agents/_agents.md)

The review found no migrated loader contract. The accepted migration added [loader maintenance](../../crystallized/documents/maintenance/payload/agents/loader.md), which now replaces the legacy descriptor under the [source and packaging decision](../../crystallized/decisions/source-and-packaging.md).

### Migration References

- The superseded loader descriptor was removed after its useful contract was migrated. Git history preserves it as historical input.
- [Current Open Forge Routing scope](../../crystallized/documents/framework/routing/_routing.md)
- [Current Open Forge Markdown scope](../../crystallized/documents/framework/markdown/_markdown.md)
- [Accepted state and synchronization](../../crystallized/documents/framework/truth.md)
- [Current Memory Architecture](../../crystallized/documents/framework/memory/_memory.md)
- [Overwrite descriptor](../../../../docs/framework/concepts/overwrites.md)
- [Payload boundary descriptor](../../../../docs/framework/concepts/payload-boundary.md)
- [Layer descriptor](../../../../docs/framework/concepts/layers.md)
- [Current Core Primitives scope](../../crystallized/documents/framework/primitives/_primitives.md)

These references preserve useful constraints and historical wording, but they are not evidence that can override the accepted current architectures.

### Behavioral Evidence

- [Loading evidence and recommendations](../../crystallized/documents/evaluations/recommendations-synthesis.md)
- [v7 synthesis](../../crystallized/documents/evaluations/v7-synthesis.md)
- [v8 synthesis](../../crystallized/documents/evaluations/v8-synthesis.md)
- [v9 synthesis](../../crystallized/documents/evaluations/v9-synthesis.md)
- [v10 synthesis](../../crystallized/documents/evaluations/v10-synthesis.md)
- [Completed link and loader review](../../archived/sessions/2026-07-20_link-contract-and-loader-review.md)
- [CLI closure tests](../../../../src/cli/cli.closure.test.ts)
- [CLI unit tests](../../../../src/cli/cli.unit.test.ts)
- [Current CLI implementation](../../../../src/cli/cli.ts)

The evidence supports these conclusions:

- Agents self-enforce loading instructions; tags do not mechanically load or enforce content
- Reliability-critical context should be unconditional and early
- Direct, colocated routes outperform prose indirection
- Tool output is a compliance surface when it makes a required traversal cheap
- The closeout printout raised measured #KeepInMind recheck compliance from roughly half of runs to three of three
- Route-carried context, conflict reporting, narrowed scope, and ordinary relevance routing have worked under dogfood evaluation
- The CLI faithfully emits the loader, visible transitive #LoadNow closure, complete routed #KeepInMind set, and adjacent overwrites

## Assumptions

- The accepted Framework Architecture remains the authoritative structural design
- `AGENTS.md` remains the canonical cross-runtime entry, with provider files acting only as bridges
- The loader remains the one universal Framework bootstrap after `AGENTS.md`
- Plain Markdown remains complete without the CLI
- The current two load-policy tags, three layer tags, and three truth or synchronization tags remain sufficient
- Source and dogfood authored loader content should remain aligned even when generated route entries may eventually differ
- The loader should optimize reliable agent behavior rather than market Open Forge or explain its full product model

## Analysis

### Overall Assessment

The loader is already architecturally strong. Its current section order, compact terms, grouped Axioms, defined tags, conditional CLI help, and final generated root registry should be preserved.

Its history explains why. Earlier versions mixed workflow selection, Memory lifecycle, continuity records, extensions, route examples, CLI catalogues, and rejected mechanics into one large bootstrap. The current loader removed those category concerns, grouped the universal contract, adopted relative Markdown links, and stayed within a small authored budget.

The migration should therefore be a surgical alignment pass, not another redesign.

### AGENTS.md Assessment

The current `AGENTS.md` managed block is excellent as written:

- It identifies Open Forge as the workspace operating contract
- It requires the loader before every task
- It requires applicable Open Forge behavior throughout the task
- It does not duplicate authority, routing, tags, conflict behavior, or category mechanics
- It remains natural, provider-neutral, and difficult to misread
- Its source and dogfood blocks are identical

No wording change is recommended.

The managed-block installation model is also sound. Workspace instructions outside the markers remain user-owned, while Open Forge can update only its bounded entry block.

### CLAUDE.md Assessment

The current Claude bridge is also correct as written:

- It contains only the exact provider-native `@AGENTS.md` import inside the managed markers
- It introduces no parallel policy
- It keeps `AGENTS.md` canonical
- It preserves Claude-specific workspace content outside the managed block
- Its source and dogfood blocks are identical

No wording change is recommended.

The linked external Claude Code contract should be rechecked only when the bridge syntax or provider behavior changes.

### What The Loader Must Contain

The loader should remain authoritative for only the universal bootstrap contract:

1. The minimum route terms required before navigation
2. Type-aware authority, accepted direction, conflict, and inheritance behavior
3. Top-down entrypoint routing and recursive scope mechanics
4. The distinction between generated navigation metadata and authoritative routed meaning
5. Overwrite adjacency and file-local precedence
6. The exact meanings of reserved loading, layer, truth, and synchronization tags
7. The principle that loading and tags change visibility or timing rather than creating authority
8. Compact deterministic help for commands with a universal trigger
9. The generated registry of direct active root routes

Everything required for correct bootstrap behavior must remain understandable from the installed loader alone.

### What The Loader Should Not Contain

The loader should not absorb:

- ACE or product marketing
- The complete Memory lifecycle
- Core primitive definitions beyond the route tags needed for initial navigation
- Workflow selection or execution details
- Skill activation details
- Template selection and instantiation behavior
- Extension package, trust, update, or removal behavior
- CLI architecture or command catalogues
- Unimplemented `help syntax` or `help roles` commands
- Installer, upgrade, restoration, transaction, or receipt mechanics
- Markdown authoring examples beyond the route syntax agents must interpret
- Historical alternatives, rejected tags, or migration compatibility explanations beyond active compatibility names
- A generalized methodology or instructions that restate ordinary agent capability

Those concerns belong to routed authoritative sources and should enter context only when selected.

### Necessary Alignment Changes

The next migration should consider these precise changes:

1. Define `framework route` as a standard Core or Memory route shipped by Open Forge, matching the Framework Architecture
2. Define `scope route` through narrowed authority or meaning rather than ambiguous ownership
3. Replace semantic `owning route`, `routed owner`, and `owner's authority` wording with typed authority or source-role language
4. Replace `local active truth` with clearer accepted workspace-specific state
5. Express authority by role and scope instead of presenting every source as one undifferentiated priority list
6. State explicitly that loading and tags do not create authority
7. Make the `load --bodies` hint apply at every required #KeepInMind refresh, not only task start or resume
8. Consider clearer #Memory and #Extension summaries now that Templates are a distinct Core route

Item 7 is the only behavioral clarity issue found. The #KeepInMind definition correctly requires rechecks before handoff and closeout, but the current CLI line describes `load --bodies` only as a task-start or resume action. That weakens the direct tool cue at the exact boundary where dogfood evidence found the printout most valuable.

### Tone And Delivery

The current delivery is appropriate:

- Imperative where agent behavior is mandatory
- Descriptive where terms and tags are defined
- Compact without using compressed headline fragments
- Technical without becoming an implementation manual
- Honest that the CLI automates rather than owns the Markdown contract

The migration should retain short headings and line-based rules. It should prefer semicolons only when they keep one closely related contract on one line. It should avoid slogans, marketing language, jokes, em dashes, and abstract governance vocabulary.

`User` is preferable to introducing `operator` in this bootstrap. The loader directly addresses the agent's interaction with task instructions, and `user` is immediately understood without adding another term. `Operator` remains valuable in product and architecture language when active decision authority is itself the subject.

### Candidate Authored Loader

The following candidate shows the recommended direction. It intentionally excludes the generated `Entries` body, which remains derived from direct root entrypoints.

```md
# Open Forge Loader

This is the main Open Forge `entrypoint`. Read it after `AGENTS.md` to enter this workspace.

## Terms

- `entrypoint` - Markdown file that makes a folder routable; Open Forge uses `_{folder-name}.md`, while `index.md`, `_index.md`, `references.md`, and `_references.md` are compatibility names
- `entry` - Generated route line under `Entries`
- `root route` - Route exposed directly by this loader
- `framework route` - Standard Core or Memory route shipped by Open Forge
- `scope route` - Local routed subtree that narrows authority or meaning
- `scoped framework route` - Framework route initialized inside a `scope route`
- `slug` - Concrete folder name used in a route path
- `axiom` - Mandatory instruction under an `Axioms` heading in a loaded file

## Axioms

### Authority And Inheritance

- Platform constraints and runtime safety bound every action
- Clear user direction governs goals, priorities, consequential tradeoffs, and accepted changes within its stated scope; follow it when safe and allowed, and do not ask for the same confirmation again
- A declared external source of truth is authoritative for the facts delegated to it
- Axioms of loaded ancestor `entrypoints` apply below them; a child adds only what is specific to its scope
- Follow loaded `axioms` within their scope; accepted workspace-specific state overrides Open Forge defaults, and unresolved conflicts must be reported
- A request to act also accepts any decision required to perform that action; if the direction is ambiguous, keep it #Contextual and clarify before work depends on it
- Investigate an apparent conflict with #CurrentTruth before changing either side; report conflicts that remain unresolved
- When accepted direction changes #CurrentTruth, update its authoritative route or system and preserve useful superseded context
- Prefer material in a narrower selected non-directive scope over broader material of the same type when safe and allowed; loaded directives add to ancestors, and conflicts are reported

### Routing

- Open Forge routes through small Markdown `entrypoints` whose generated `Entries` expose direct routes
- Load an `entrypoint` before opening its routed files, then use its `Entries` to select what the request needs
- A folder is routable only when it contains one recognized `entrypoint`; every folder in a nested route path needs its own `entrypoint`
- A `scope route` uses the same mechanism with a concrete `slug`; initialize a framework `entrypoint` inside it only when that scoped framework route is needed
- Generated `Entries` are navigation metadata; entries without a load-policy tag are on demand, and their routed destinations remain authoritative for detailed meaning
- When `{name}.overwrite.md` exists, read it immediately after `{name}.md`; it inherits the base route and has final precedence within that file's scope

### Tags And Loading

Defined tags have the meanings below when they appear in loaded content or generated `Entries`; undefined tags remain routing and search signals. Loading and tags change visibility, timing, or classification; they do not create authority by themselves.

#### Defined Tags

- #LoadNow - When this `entry` appears in an already-loaded parent's `Entries`, read it immediately in listed order. When it points to an `entrypoint`, apply the same rule to that file's `Entries`.
- #KeepInMind - Read the complete routed set at task start or resume, after detected context restoration, and before a handoff or closeout. Recheck it during work only when its follow-ups may have changed, and follow each result according to its source role and scope.
- #Core - Base routing, workspace orientation, and agent primitive routes
- #Memory - Self-growing Markdown memory for live work, continuity, accepted records, history, and candidate learning
- #Extension - Optional packaged routes, capabilities, integrations, and support material
- #Contextual - Supporting context, not accepted current truth unless restored, validated, accepted, or promoted
- #CurrentTruth - Accepted current state within its stated scope, below user instructions, runtime safety, platform constraints, and declared external sources of truth
- #Evergreen - Material that must stay aligned with accepted current state. It creates no authority or load policy.
  - When accepted state changes, update only affected #Evergreen material you may edit before work depends on it, and no later than closeout; batch related updates when safe
  - Keep #Evergreen material coherent with what it represents now; preserve useful superseded context in the matching decision or archive, and report affected material you cannot update

### CLI

When the Open Forge CLI is available, use each applicable command below. Every command automates the same plain-file contract, which remains complete without the CLI.

#### Applicable Commands

- `open-forge load --bodies` - Read or refresh effective baseline and continuity context at every required #KeepInMind boundary
- `open-forge chain <route> --heading Axioms` - Read inherited rules after selecting a route
- `open-forge index` - Rebuild generated `Entries` after adding, moving, or removing a routed file or changing its route metadata
- `open-forge doctor` - Validate routing after structural framework changes and before closing them out

## Entries

{Generated direct root routes}
```

This draft remains within the existing authored size budget. Its structure is unchanged, while authority, terminology, and the continuity tool cue become more explicit.

### Maintenance Migration

The accepted migration created:

```text
.agents/memory/crystallized/documents/maintenance/payload/agents/loader.md
```

That maintenance contract should follow the current maintenance pattern:

#### Source

- Link the installable loader as the canonical runtime source
- Link the dogfood loader as the repository counterpart
- Link the Framework Architecture for the current structural role
- Link the accepted routing, tags, loading, scope, terminology, and packaging decisions for rationale
- Link the old loader descriptor only as a migration input until it is archived

#### Contract

- Preserve the loader's universal-only responsibility
- Preserve the section order and final generated registry
- Preserve plain-file completeness and conditional CLI assistance
- Keep source and dogfood authored content aligned
- Keep generated root entries local and derived
- Preserve the accepted authority, inheritance, route, scope, tag, overwrite, and continuity meanings
- Keep category-specific behavior out
- Keep the authored body within a reviewable size budget

#### Verification

- Compare source and dogfood authored loader content with generated regions excluded
- Verify the loader is the first item emitted by `load`
- Verify transitive visible #LoadNow order
- Verify the complete routed #KeepInMind set and overwrite adjacency
- Verify only direct active root routes appear in generated loader entries
- Verify aliases remain compatibility input and multiple entrypoints fail before mutation
- Verify `chain` begins with the loader and preserves ancestor and overwrite order
- Verify `doctor` catches malformed, stale, unsafe, and escaping route structures
- Run root and installable `index` and `doctor`
- Run affected unit, closure, packaging, and build checks

After acceptance and replacement, the old loader descriptor was removed. The new maintenance contract and installed loaders now carry its current value without preserving a competing document.

The existing AGENTS and CLAUDE maintenance contracts should receive only terminology and relationship alignment during that migration. Their source files and runtime wording do not need redesign.

### Verification Coverage And Gaps

Current tests already cover:

- Managed AGENTS and CLAUDE block installation, preservation, equality, marker validation, and idempotence
- Direct loader registry generation
- Canonical and compatibility entrypoint names
- Multiple-entrypoint rejection before writes
- Fresh install wording for selected critical contracts
- Loader-first #LoadNow traversal
- Complete #KeepInMind discovery
- Overwrite adjacency
- Loader-to-target chain order
- Link containment and symlink safety
- Stale generated-region detection
- Direct directive activation and document shape

The analysis found that no test directly compared the authored source and dogfood loader bodies. The accepted pass added that direct parity check.

A focused comparison should ignore the generated `Entries` body because dogfood and installable root routes may validly differ. It should compare everything before the generated start marker and the stable closing shape after the generated end marker.

Tests should avoid freezing every sentence. Exact wording assertions are justified only for critical user-facing boundaries whose accidental loss would change behavior. Structural and behavioral verification should carry most of the contract.

## Alternatives Or Interpretations

### Leave The Loader Unchanged

This is defensible because the current loader is already compact, coherent, and functional. It would leave minor architecture and terminology drift plus the weaker closeout CLI cue.

### Move Terms Or Scope Mechanics Into A Routed File

This would reduce baseline lines but make correct top-down routing depend on concepts an agent has not yet learned how to select. The current terms are small and universal enough to remain in the loader.

### Expand The Loader Into A Framework Overview

This would make the bootstrap more self-explanatory at the cost of repeating routed category contracts and increasing baseline context. The historical loader already demonstrated this failure mode. The README and architectures are the correct explanatory surfaces.

### Add CLI Help Commands Now

`help syntax` and `help roles` are good overhaul candidates, but mentioning unimplemented commands in the runtime loader would convert intent into a false current contract.

## Current Conclusion

Confidence: high.

The root harness migration is complete and should remain unchanged. The loader needs only a constrained alignment pass followed by a new maintenance contract and stronger source-to-dogfood verification.

The strongest candidate changes are:

1. Typed authority terminology
2. Framework-route and scope definitions aligned with the Framework Architecture
3. Explicit separation of loading from authority
4. A `load --bodies` cue that covers every required #KeepInMind boundary
5. Clearer Memory and Extension tag summaries

No architectural split, new primitive, new tag, new command, or broader loader responsibility is justified.

## Limits And Open Questions

- The candidate wording has not yet been reviewed or accepted by the maintainer
- The next pass should verify that “according to its source role and scope” remains immediately understandable without the temporary knowledge-role helper
- The exact authored-body comparison helper should be chosen during implementation rather than designed inside this document
- Broader descriptor migration may still reveal stale terminology or references that should be migrated with their corresponding current contracts

None of these questions blocks review of the recommended migration direction.

## Next Evidence Or Promotion

After maintainer review:

1. Revise or accept the candidate loader wording
2. Update installable and dogfood loaders together
3. Create the loader maintenance contract
4. Align the AGENTS and CLAUDE maintenance terminology without changing their runtime blocks
5. Add authored loader parity verification
6. Update affected exact-string tests
7. Reindex and validate root and installable payloads
8. Run affected unit, closure, packaging, and build checks
9. Archive the superseded loader descriptor
10. Promote accepted conclusions into the loader, maintenance documents, and any affected decisions, then archive this analysis

## Related Sources

- [Temporary terminology helper](../../working/terminology-helper.md)
- [Temporary knowledge-role helper](../../working/knowledge-role-helper.md)
- [Open Forge design baseline](../../archived/sessions/2026-07-26_open-forge-design-baseline.md)
- [Open Forge design baseline part 2](../../archived/sessions/2026-07-26_open-forge-design-baseline-part-2.md)
