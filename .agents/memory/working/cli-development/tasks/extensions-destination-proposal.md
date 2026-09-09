---
open-forge:
  description: Review the candidate Extension content layout and exact consumer permissions for copied files and Library links
  tags: [Memory, Working, CLI, Extension, Library, Proposal, Contextual, Candidate]
---

# Extension And Library Destination Proposal

## Accepted Direction And Remaining Design

Task 24 is complete. The user subsequently accepted the revised Library flow:
the attached folder is its content root, optional `--to` maps it to a consumer
directory, and Library directory grants include future descendants. Only leaf
files are symlinked. Extensions retain `content/` and exact-file grants.
The [Task 25 accepted direction](workspace-library-destination-projections.md#accepted-functional-direction)
supersedes this earlier joint draft wherever its Library selection, mapping or
permission proposal differs. The remainder preserves the original reasoning.

On 2026-09-08 the user accepted the `content/` rename and required an allowlist
with a CLI question when a destination has not already been added. This replaces
the earlier manual-edit-only recommendation. Use one consumer-owned permissions
file for exact destinations outside `.agents/`; Extension files are copied and
Library files remain relative symlinks under their separate ownership records.
Keep `library`, `attach`, `sync`, and `detach`.

The concrete interaction below is the revised functional draft for Tasks 24 and 25. Exact file grants are the recommended implementation of the accepted
allowlist requirement; there are no wildcard or directory grants. The user did
not separately choose every schema or revocation detail. Freeze the complete
contracts and implementation boundaries after presenting this revised flow.
Task 26 remains the later behavior-preserving six-command refactor.

## Names And Package Layout

`content/` describes the files an Extension contributes in familiar language.
`contents/` is a reasonable alternative, but adds no distinct meaning.
Keeping `payload/` avoids a rename but retains transport-oriented wording.
The accepted direction is one atomic pre-release switch to `content/` across package creation,
reading, embedded assets, examples, and evidence. Do not add aliases or dual
readers. The user explicitly declined legacy handling; update the first-party packages
and current reader only. Existing installed ownership remains identified by destination paths;
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
are unique; readers accept authored order and writers sort canonically. Paths are canonical workspace-relative `/` paths with
no empty, dot, parent, backslash, absolute, or glob spelling. Each path names
one file, never a directory subtree. A Library grant also binds its recorded
source root. Extension grants apply to the selected stable ID; the existing
explicit source selection remains necessary and is shown in the plan.

An absent permissions file grants no destination outside `.agents/`. A
malformed file blocks a mutating request that needs external permission.
The current `.agents/` rules remain in force, including their exclusions.
Every dependency must have its own grant for each external path it needs.
A copied file grant never authorizes a Library link, or vice versa.

### Approval In The CLI

A prompt-capable human mutation collects all missing exact grants for the
selected Extension closure or Library before any permission or content effect.
It shows the package/Library identity, source, destination, copy/link effect,
and that approval will be remembered in this consumer workspace. Ask once for
the complete displayed set, with No as the default:

```text
This Extension needs access to files outside .agents:
  team-review: .apm/agents/reviewer.md (copy)
Add these files to this workspace's allowlist and continue? [y/N]
```

Yes authorizes exactly that displayed set. Existing grants are reused without
another question. No, an empty answer, end-of-input or cancellation grants
nothing and produces no permission or content effects. Do not widen the grant
to a folder, future source additions or a different identity. Each dependency
has its own exact grants; a Library grant also binds its source root.

Dry-run lists missing grants and planned effects without a permission question
or writing. Existing command-selection questions keep their own contracts.
JSON, redirected/noninteractive input and `--automatic` never prompt or grant
permission. They return the missing grants and a concrete next action: rerun
interactively or edit the exact consumer permission entries. `--force` and
`--prune` do not bypass permission. Reuse the existing typed interactive session
and human-mode admission rules; introduce no second prompt system.

Before persisting approval, finish the complete command preflight. Acquire the
normal workspace lease and revalidate the displayed source, requested paths,
permission bytes, ownership and expected destinations. A changed snapshot stops
the request; approval is not transferred to changed files or a broader plan.
Write the strict permission file atomically, preserving all unrelated grants,
and report this declared control-file effect before applying approved content.
A failed permission write prevents content effects. If a later effect fails,
truthfully report that the approved permission remains; no automatic rollback
or hidden grant removal occurs. Permission admission and content ownership
remain distinct. Preserve prior control-file bytes or proven absence in the
existing operation recovery bundle. Restoring permission is an explicit manual
edit; this does not add an automatic Repair operation. Exact result coordinates
are defined in the pending Task 24 contract pack.

Malformed or unsafe permission storage is diagnosed and never overwritten by
an approval prompt. Package-contained permission files do not grant authority.

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
strict reader. Source additions outside `.agents/` require exact consumer permission before
projection. How the CLI discovers never-approved external source files remains
an open Task 25 choice: explicit path selection or a broader source inventory.
The existing approved-path proposal alone cannot discover new missing grants.
Required observations cover the complete
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

Revoking a grant prevents subsequent changes at that external path, including
Update, Remove, Sync, Detach and explicit recovery application. Revocation does
not itself delete content or release ownership. A prompt-capable mutation may
ask to restore the exact missing grant through the same explicit approval flow;
noninteractive requests stop with a concrete missing-permission result. To retire
content without another question, remove or detach before revoking the grant.
Read-only inspection remains available. This proposed rule is an admission
boundary, not a malicious-same-user security guarantee.

Use the accepted lock, expected-state revalidation, no-follow checks, bounded
file/link effects, verification, and external recovery evidence. External
manager parents must be ordinary contained directories; reject linked ancestry
and collisions. Recovery retains copy bytes or exact link identity as
appropriate, never source-file bytes for a projection. Interrupted application
retains truthful partial effects and recovery evidence, without automatic
rollback. No native bridge or stronger malicious-same-user guarantee is proposed.

## Contract And Implementation Gate

The user accepted `content/`, an allowlist and an interactive question for
missing entries. Present this revised exact-file flow, including prompt-free
dry-run/automation and retained approvals after later failure, before freezing
the implementation contracts. The earlier no-prompt proposal is superseded.

Freeze the complete changed contracts, protected destination list, permission
result/error representation, prompt admission, strict readers, explicit grant
publication and lifecycle/recovery effects. Revisit source-reader fail-closed
and Update parent-catalogue candidates within Task 24; neither is silently
accepted by this proposal or the Task 26 refactor.

Use focused pure evidence for grammar and admission; real-filesystem evidence
for remembered approval, copies, links, retirement, revocation, collisions and
recovery; and exactly three simple public journeys per command. Reuse existing
interactive test boundaries for prompt decisions. Full managed and supported
Native AOT gates follow each accepted Task's applicability check.

## Historical Preparation Capsule And Provenance

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
