---
open-forge:
  description: Source-aligned documentation repair packets after Core reduction and workflow Skill extraction
  tags: [Memory, Working, Plan, Documentation, Contextual]
---

# Tasks 42/43: documentation after extraction

## Outcome

Explain what the software already ships. Core provides the Memory states and
base capabilities; optional Extensions provide the deeper roles. Workflow
selection is supplied by the native `use-workflow` Skill. This work changes
documentation, not installed files, routing, loading rules or upgrade behavior.

[Task 42](../task42-minimal-core.md) records the accepted reduction and its
remaining contracts. [Task 43](../task43-workflows-as-skill.md) records the accepted
workflow direction. Their former `not started` status did not mean these moves
need to be performed again.

## Frozen source mapping

The seven Memory source files below exist. Their canonical frontmatter uses
`Extension`, not the old automatic loading tags. Preserve all other tags and
role semantics from the actual source. Repository dogfood loading overrides do
not redefine installed defaults.

Contract paths in this table are relative to
`.agents/memory/crystallized/documents/maintenance/payload/agents/`.
Source paths are repository-relative.

| Contract                           | Canonical installed source                                                                                      |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `memory/crystallized/decisions.md` | `src/extensions/planning/content/.agents/memory/crystallized/decisions/_decisions.md`                           |
| `memory/crystallized/documents.md` | `src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md`                  |
| `memory/emerging/analysis.md`      | `src/extensions/planning/content/.agents/memory/emerging/analysis/_analysis.md`                                 |
| `memory/emerging/ideas.md`         | `src/extensions/planning/content/.agents/memory/emerging/ideas/_ideas.md`                                       |
| `memory/emerging/observations.md`  | `src/extensions/orchestration/content/.agents/memory/emerging/observations/_observations.md`                    |
| `memory/working/checkpoints.md`    | `src/extensions/planning/content/.agents/memory/working/checkpoints/_checkpoints.md`                            |
| `memory/working/handoffs.md`       | `src/extensions/orchestration/content/.agents/memory/working/handoffs/_handoffs.md`                             |
| `workflows.md`                     | `src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md` and sibling `references/_references.md` |

The old `src/open-forge/.agents/workflows/_workflows.md` does not exist. Do not
recreate it, redirect a dead link to an unrelated local orchestration recipe,
or claim that an ordinary recipe is an overwrite companion.

## D1: Planning-owned Memory contracts

One Luna/xhigh worker owns exactly these four files under the contract prefix:

- `memory/crystallized/decisions.md`
- `memory/emerging/analysis.md`
- `memory/emerging/ideas.md`
- `memory/working/checkpoints.md`

For each file, replace the obsolete Core source link with its exact source from
the table; identify Planning as the installing Extension; align the classification
bullet with the complete source tag list, replacing the automatic-load claim
with optional Extension classification. Keep the role, state and authority
semantics unchanged. Verification must distinguish installing Core alone from
installing Planning. Preserve valid repository counterparts and actual overwrite
links; return any missing counterpart rather than inventing one.

Do not edit ancestors, indexes, source payload or task records. Receipt:
`.temp/beta-follow-ups/receipts/D1.md`.

## D2: other Extension-owned Memory contracts

One Luna/xhigh worker owns exactly these three files under the contract prefix:

- `memory/crystallized/documents.md`
- `memory/emerging/observations.md`
- `memory/working/handoffs.md`

Apply D1's exact transformations using Project Documents and Orchestration as
the declared installers. Observations must not retain its former mandatory
`KeepInMind` classification; the canonical Extension source is on demand.
Do not change Emerging's own `KeepInMind` behavior. Preserve each role's existing
meaning and genuine repository-only customizations.

Exclusions are the same as D1. Receipt:
`.temp/beta-follow-ups/receipts/D2.md`.

The Handoffs contract's repository counterpart currently points into archived CLI
history. Replace that counterpart link with the real current
`.agents/memory/working/handoffs/_handoffs.md`. Preserve archived records unchanged.

## D3: workflow maintenance contract

One Luna/max worker owns only the existing `workflows.md` contract under the
prefix. Keep the filename stable in this correction to preserve inbound links.
Retitle and describe it as maintenance for the optional Workflow Support Skill,
not a Core category. Its Source section must link to the native `SKILL.md` and
the reference catalogue separately, and to the actual repository counterparts
`.agents/skills/use-workflow/SKILL.md` and `references/_references.md`.

Replace the obsolete claims that a Workflows root ships, is baseline-loaded,
or supplies primitive authority. Native metadata is `name` and `description`;
the recipe catalogue carries its own Extension/Workflow metadata and explicit
selection rules. Retain the exact Goal/Steps/Completion recipe contract and
optional use, composition and stopping rules supported by the current Skill.
Delete the false description of a local recipe as an adjacent overwrite.
Also link the optional catalogue template at
`src/extensions/workflows/content/.agents/templates/workflows/_workflows.md`;
its instructions explicitly avoid introducing a new root primitive.

State that the Skill and its catalogue define workflow selection; the skill
mechanism does not make its resources loader-reachable ordinary routes. Do not
resolve Tasks 46/47 by rewriting this distinction. Verification should inspect
the installed Workflow Support package and its dependency composition, not
claim that Core installs a workflow route.

Receipt: `.temp/beta-follow-ups/receipts/D3.md`.

## D4: ancestor and conceptual consistency

One Luna/max worker, after the coordinator accepts the D1–D3 meaning. It may
draft against the frozen mapping concurrently but must not assume unfinished
sibling text is accepted.

Own these exact files:

- `.agents/memory/crystallized/documents/maintenance/payload/_payload.md`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/_agents.md`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/_memory.md`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/crystallized/_crystallized.md`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/emerging/_emerging.md`
- `.agents/memory/crystallized/documents/maintenance/payload/agents/memory/working/_working.md`

Broaden the maintenance route's source description to reviewed Core and Extension
installation files while retaining its repository-only role. Correct the
Memory loading table: Core still loads Memory, Working and Crystallized as
declared, and Emerging retains its own continuity behavior; the seven deeper
roles are optional Extension-provided routes. Remove statements that Working
ships Checkpoints/Handoffs as automatic children. Keep their role definitions
and navigation to the maintenance contracts. Align generated entry descriptions
only with the accepted changed child frontmatter.

No change to loader authority or loading semantics is allowed. Read all selected
ancestor axioms before editing. This is documentation-only source alignment;
it does not require running broad `index` or Doctor against the repository.
Receipt: `.temp/beta-follow-ups/receipts/D4.md`.

## D5: conceptual source alignment

One Luna/max worker owns exactly these eight files, with paths relative to
`.agents/memory/crystallized/documents/`:

- `framework/primitives/workflows.md`
- `framework/primitives/model.md`
- `framework/primitives/_primitives.md`
- `framework/primitives/skills.md`
- `framework/routing/scope.md`
- `framework/truth.md`
- `maintenance/helpers/dictionary.md`
- `maintenance/helpers/knowledge-roles.md`

These sources still describe a separate Workflows Core primitive or root. Align
them with accepted Task 43 and the canonical Skill: six Core primitives, with
Workflows expressed through Skills. Keep the Workflow recipe definition and
composition rules; remove the separate shipped Workflows root and its automatic
authority. Keep existing document locations to avoid an unrelated route migration.
In `_primitives.md`, describe the retained workflow document as the Skill-based
recipe relationship, not a seventh primitive.

In `routing/scope.md`, replace the example based on a
Workflows root with an equivalent example under an existing Core root, preserving
the rule that a nested familiar name does not recreate another root route.
In the dictionary, keep Skill and Workflow as distinct useful terms while
removing Workflows from the list of Core primitives. Do not change native Skill
resource ownership, automatic indexing scope, or the six remaining primitive
roles.

In `skills.md`, preserve the distinction between a capability and a repeatable
recipe, while explaining that the latter is selected through the Skill mechanism.
In `truth.md` and `knowledge-roles.md`, remove the seventh-Core-primitive claim
without deleting Workflow as a useful recipe term. Keep selection distinct from
execution and all existing authority rules intact.

Receipt: `.temp/beta-follow-ups/receipts/D5.md`. D5 uses the frozen source mapping
and native Skill as inputs, not unfinished D3 prose. A contradiction outside these
eight files is reported to the coordinator for a bounded follow-up; it does not
expand D5's authority.

## D6: Memory model and installed layout

One Luna/max worker owns exactly these six files relative to
`.agents/memory/crystallized/documents/`:

- `framework/architecture.md`
- `framework/memory/_memory.md`
- `framework/memory/model.md`
- `framework/memory/crystallized.md`
- `framework/memory/emerging.md`
- `framework/memory/working.md`

In the architecture's shipped-tree example, show Core alone as it actually ships.
Describe the seven deeper Memory routes through their named optional Extensions,
not as default directories. Remove the distinct Workflows root and seventh Core
primitive, following the same frozen D5 meaning. The Memory model and state
documents retain their four states, authority, transitions, recursive scopes and
role meanings. Correct only their default availability, source links and loading
claims. Emerging retains `KeepInMind`; optional Observations has no automatic
continuity tag. Correct the Working document's Handoffs link to the current
Working route instead of the archived CLI route. Update immediate entry labels
to match changed child descriptions.

Receipt: `.temp/beta-follow-ups/receipts/D6.md`. The coherent documentation review
must cover D1–D6 together. This partition totals 28 existing documentation files;
none is shared between two workers.

## Coordinator reconciliation

The coordinator owns the Task 42/43 completion reconciliation and current task
navigation. Record that the extraction shipped while documentation closeout
remained. Do not invent upgrade guarantees or claim every historical acceptance
checkbox passed without evidence. An unproven upgrade requirement remains
explicitly unqualified.

The inventory also found old topology in the retained Decisions
`.agents/memory/crystallized/decisions/framework/routing-surfaces.md` and
`workflow-shape.md`. The coordinator checks whether their current-status sections
need a narrow supersession note linking the accepted Task 43 outcome; preserve
the historical rationale rather than silently rewriting it as if it always
described the new layout. They are excluded from worker edits.

Specific unfinished evidence: fresh installs do not establish an upgrade from
formerly Core-owned or user-edited files into Extension ownership. Do not claim
automatic transfer, pruning, or a guaranteed user-owned recipe catalogue. Those
remain separate task questions; the documentation correction records the limit.

## Verification and acceptance

Workers compare their exact changed claims with canonical source frontmatter and
body, check each changed relative link and heading target, inspect their diff,
and report remaining contradictory references. They do not run shared formatting,
builds, package tests or repository-wide navigation generation.

After the wave drains, the coordinator checks all changed links and claims,
reviews the coherent prose pack once against source, and verifies that no
runtime source, package manifest, test or snapshot changed. Use focused Markdown
format checking on the owned files only. No new tests are needed for this
documentation-only correction. If verification reveals that an installed source
must change, stop: that is a separate behavioral scope.

## Divergences observed

The repair is larger than the eight leaf contracts: their Memory ancestors also
claim automatic loading, and conceptual Workflow documents still describe the
retired Core root. Those are consistency dependencies, not permission to rewrite
unrelated Framework documentation.
