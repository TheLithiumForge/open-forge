---
open-forge:
  description: Review the candidate Extension content layout and exact consumer permissions for copied files and Library links
  tags: [Memory, Working, CLI, Extension, Library, Proposal, Contextual, Candidate]
---

# Extension And Library Destination Proposal

## Recommendation And Status

Use `content/` for Extension package files and one consumer-owned permissions
file for exact destinations outside `.agents/`. Keep `library`, `attach`,
`sync`, and `detach`. Extension files are copied and managed through Extension
lifecycle state. Library files remain live relative symlinks managed through
the separate Library record.

This is a proposal for [Task 24](extensions-evolution.md) and
[Task 25](workspace-library-destination-projections.md), prepared while the
remaining commands are implemented. The user must review the final functional
draft before contracts or implementation change. No phase or milestone horizon
is assigned. [Task 26](extension-internal-consolidation.md) remains the later
pure six-command refactor.

## Names And Package Layout

`content/` describes the files an Extension contributes in familiar language.
`contents/` is a reasonable alternative, but adds no distinct meaning.
Keeping `payload/` avoids a rename but retains transport-oriented wording.
Recommend one atomic pre-release switch to `content/` across package creation,
reading, embedded assets, examples, and evidence. Do not add aliases or dual
readers. Existing installed ownership remains identified by destination paths;
a package directory rename alone does not reinstall files.

A proposed package has this shape:

```text
catalogue/team-review/
  extension.json
  README.md
  content/
    .agents/directives/review.md
    .apm/agents/reviewer.md
```

Keep existing manifest identity, descriptive version, and exact dependency
semantics. Files below `content/` retain their workspace-relative paths. The
manifest requests no authority. A package-contained permissions file is never
read as consumer approval. Extension source remains separate from the selected
consumer workspace under its current source-disjointness contract.

## Consumer Permissions

Recommend `.agents/open-forge.permissions.json`, edited explicitly in the
consumer workspace. This proposed shape grants only exact external file paths:

```json
{
  "schemaVersion": 1,
  "extensions": [
    {
      "id": "team-review",
      "paths": [".apm/agents/reviewer.md"]
    }
  ],
  "libraries": [
    {
      "id": "team-knowledge",
      "sourceRoot": "shared/team-knowledge",
      "paths": [".apm/agents/team-advisor.md"]
    }
  ]
}
```

These are proposed fields, not current CLI syntax or accepted schemas. Each
root property is required. Unknown, duplicate, null, or incorrectly typed
properties are rejected. IDs use their existing grammars. Entries and paths
are unique and sorted. Paths are canonical workspace-relative `/` paths with
no empty, dot, parent, backslash, absolute, or glob spelling. Each path names
one file, never a directory subtree. A Library grant also binds its recorded
source root. Extension grants apply to the selected stable ID; the existing
explicit source selection remains necessary and is shown in the plan.

An absent permissions file grants no destination outside `.agents/`. A
malformed file blocks a mutating request that needs external permission.
The current `.agents/` rules remain in force, including their exclusions.
Every dependency must have its own grant for each external path it needs.
A copied file grant never authorizes a Library link, or vice versa.

The first version has no permission prompt or approval-writing command. Dry
run reports missing exact grants with the source, package or Library, target,
and effect. The consumer edits the file, reviews the change, and reruns.
`--automatic`, `--force`, and `--prune` never grant a destination. Read and
revalidate permission bytes with the immutable plan under the workspace lease;
source metadata and lifecycle records cannot widen them.

Reject repository metadata, the source tree, Open Forge control and recovery
files, overwrite companions, and existing Framework-owned or independently
managed controls even if listed. Do not grant the workspace root or whole
manager roots. An ordinary unowned `.apm/agents/reviewer.md` can be admitted;
that grant does not authorize `.apm/` configuration or another agent file.
Keep other managers' formats opaque and preserve their own runtime meaning.

## Library Projection Example

Task 23's accepted first release remains `.agents/**`-only. Task 25 would add
exact approved outside paths at the same source-relative and consumer-relative
location, with no remapping or broader source permission:

```text
workspace/
  shared/team-knowledge/
    .agents/directives/review.md
    .apm/agents/team-advisor.md
  .agents/
    open-forge.permissions.json
    open-forge.libraries.json
  .apm/agents/
    team-advisor.md -> ../../shared/team-knowledge/.apm/agents/team-advisor.md
```

The source root remains contained and read-only. The Library record retains
`id`, `sourceRoot`, and ordered managed `paths`; the raw relative link is
derived from those facts, as in the accepted Task 23 design. Extending admitted
paths needs an explicit contract freeze and does not silently change Task 23's
strict reader. Source additions outside `.agents/` are eligible only when their
exact path has a consumer grant. Required observations cover the complete
`.agents/` inventory plus every approved external source path. An unavailable
observation never becomes evidence of retirement.

Attach creates only missing links and declared real parents. Any occupied or
independently managed destination blocks the complete request. Parent creation
does not grant ownership of sibling files. Link capability remains required,
with no copy fallback or Git operation.

## Update, Removal, And Recovery

Extension Update retains its current baseline/current/intended comparison,
shared-owner rules, force/prune boundaries, and final lifecycle publication.
A new external target needs an exact grant before effects. An unapproved path
in the selected package closure blocks the complete mutation. Extension Remove
remains source-independent and retains its existing preserve-or-delete rules.
Permission is an additional admission check, never ownership evidence.

Library Sync requires complete inventory before changing any projection. It
creates approved additions and removes retired paths only when the destination
is still the exact registered link. An unavailable source blocks Sync effects.
Detach remains whole-library and source-independent, removes only exact
registered links, and publishes or removes the Library record last. Neither
operation follows a link to mutate its source. Modified files and retargeted
links remain protected by their existing command rules.

Recommend that revoking a grant blocks later effects at that external path,
including Update, Remove, Sync, Detach, and any explicit recovery application.
Revocation itself never deletes files or releases ownership. To retire content,
remove or detach while the grant exists, then revoke it. To recover after early
revocation, deliberately restore the exact grant and rerun the operation.
Read-only inspection remains available. This simple rule avoids creating an
implicit deletion permission that survives revocation.

Use the accepted lock, expected-state revalidation, no-follow checks, bounded
file/link effects, verification, and external recovery evidence. External
manager parents must be ordinary contained directories; reject linked ancestry
and collisions. Recovery retains copy bytes or exact link identity as
appropriate, never source-file bytes for a projection. Interrupted application
retains truthful partial effects and recovery evidence, without automatic
rollback. No native bridge or stronger malicious-same-user guarantee is proposed.

## User Decision And Implementation Gate

The user needs to accept or change the proposed `content/` spelling and the
exact-file permission model, including the revocation rule. The strongest
alternative is an explicit directory allowlist: it reduces permission edits
for growing packages but gives later source additions access to more paths.
Exact files are the recommended first version for deliberate local tooling.

After that review, freeze the complete changed contracts, reserved-path list,
permission result/error representation, strict package and record readers,
and lifecycle/recovery effects before implementation. Revisit the earlier
source-reader fail-closed and Update parent-catalogue candidates in Task 24;
they are not silently accepted by this proposal or the Task 26 refactor.

Use focused pure evidence for permission grammar and admission, real-filesystem
evidence for copies, links, retirement, revocation, collisions, and recovery,
and exactly three simple public journeys per command. Required full managed
and supported Native AOT gates follow the accepted task applicability rules.

## Preparation Capsule And Provenance

Profile: Direct prose reconciliation and candidate design. One Task Mastermind
owns this slice. Independent review and council budgets are zero; no dedicated
writing review is selected. The owner inspects the complete final changed pack
and corrects it locally. This capsule assigns no implementation authority or
new Task 24–26 horizon.

The queue baseline is `2c62f59aff8d0992b81621c298c4aabc9ab72c9a`, tree
`fa607717e461f4e6092d38b69e566ff82a6b963a`. Eleven inherited dirty drafts
matched inventory SHA-256
`57e1b65d138a7b77122ad3be82a407f655ca466ce299b14f5dbd254f1b8d38c7`.
Their full pre-edit bytes and binary diff were preserved outside tracked
content. Main's older idea and checkpoint overlap was read as input, preserving
its package vocabulary, consumer permission, Library separation, and user
review meaning. The invalid old Task 24 milestone 0/8 was superseded.

The Repair Task record is adopted verbatim from immutable
`eae8eb366e8ad50d8c18cca6a4e08e9d3b6d22bb`, blob
`9b96d8b37e3978682694bae547c4a3774b103035`. It records accepted upstream
refreeze and Green authorization; agent names there are transition provenance,
not current runtime handles. Its parent state is aligned separately. No Repair
source or test is included. Library design was read at accepted contract tip
`c3f01acb76c572ee486fdc24c6a2379b27459391`, not copied into this lane.

Expected changes are the eleven inherited Markdown drafts, the directly
required operations parent and accepted Repair Task record, and this proposal.
All production, tests, accepted product contracts, sealed handoffs, other
worktrees, model observations, and publication surfaces are protected.
Validation covers authored prose, relative links, exact queue and progress,
host-path absence, immutable Repair blob identity, diff checks, and targeted
Entries generated by this worktree's CLI after formatting.

## Preparation Evidence

The same-worktree command
`dotnet build src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj -c Release --no-restore`
completed with exit 0, zero warnings, and zero errors. It refreshed the local
managed development publication from unchanged production and configuration.
The published `OpenForge.Cli.dll` SHA-256 is
`73639d0719b15518ddb340180614b36370aec7fc333dad00ffcb4aaba0e21fb9`.

All thirteen authored Markdown files passed local Prettier formatting before
Index. The fourteenth path is the verbatim accepted Repair Task record and is
verified by blob identity instead of reformatted. Relative file links resolve
inside this lane, and no host path was introduced into tracked prose. The
owner inspected the final authored changes, queue order, proposal examples,
acceptance boundaries, and immutable Repair provenance. No production test or
full managed/Native AOT acceptance claim follows from this prose-only change.

Targeted Index selects the Task 24, Task 25, Task 26, proposal, and Repair
leaves through this worktree's freshly published development CLI. Its complete
preview has no findings and names only the Tasks region (26 to 27 entries) and
Operations region (four entries, canonical whitespace). Index runs after the
formatter; generated bytes are retained exactly. Final apply and unchanged
repeat receipts are returned with the candidate.
