---
open-forge:
  description: Post-beta-2 readiness analysis for finishing existing CLI journeys and defining evidence for a stable 1.0 release
  tags: [Memory, Analysis, Release, CLI, Contextual, Candidate]
---

# 1.0 release readiness after beta 2

## Question and status

What should Open Forge finish or decide before stable 1.0, given the published
beta 2 and remaining CLI work?

This analysis was refreshed after beta 2 publication and public installation
checks on 2026-09-29. Its recommendations create no implementation authority or
Task IDs, and do not make every listed Task a release blocker.

## Current baseline

Task 62's optional `applyTo` loading, authoring, generated Entries, selected CLI
filters, documentation, independent review, and local qualification are complete.
Its receipts report 9,331 passing executions across six Windows managed/native
modes, 34 expected platform exclusions, native smoke success, generated npm
package installation, and successful site checks. See [Task 62](../../working/cli-development/tasks/task62-glob-scoped-loading.md)
and its [execution receipts](../../working/cli-development/tasks/task62/execution.md).

Beta 2 uses commit `00e3ba0294679163d95b42a81295091254debfb7`, pushed to
`develop` and `main`. Both branch Builds and Release 36515807876 passed the
six-platform qualification. All seven npm packages, six portable archives and
checksums were verified. Public Windows installs and a beta 1-to-beta 2 upgrade
passed, including preservation of user files and the previous Loader in recovery.
The documentation site and the versioned npm README are verified. Exact receipts
and the npm default-page cache limitation belong in
[Task 59](../../working/cli-development/tasks/task59-beta-2-release.md).

The accepted six-host model is Linux glibc, macOS, and Windows on x64 and ARM64.
Beta 1 and beta 2 each have qualification evidence. Do not call these platforms
missing or transfer that proof to a future stable candidate. See the
[distribution document](../../crystallized/documents/cli/distribution.md).

[Task 55](../../working/cli-development/tasks/task55-alternative-root.md) is a
separate open investigation of alternative roots and APM interoperability.

## Recommendation

Prioritize completing and documenting existing user journeys: finding sources,
explaining loading, updating managed content while preserving recovery, and
navigating shipped Skills and templates. Decide any new stable customization or
compatibility promise before coding against it. Treat the accepted Task order as
a work order, not an automatic blocker list. Require current contract evidence
and a current reproduction before calling an issue a blocker.

### Fix Find metadata projection and reconcile state documentation

From the repository root using the public beta 2 npm package, this query returns
`incomplete` (exit 3), one valid match, and an unavailable metadata projection:

```text
open-forge find --include .agents/memory/crystallized/documents/cli/contracts/find/interface.md --content metadata --format json --detail full
```

The Find contract supports metadata content. [FindProjectionContentBuilder](../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Find/Shared/Projection/FindProjectionContentBuilder.cs)
returns unavailable when `source.Route` is null. Populate the valid source result,
while retaining `incomplete` when required projection facts are unavailable. [Task 64](../../working/cli-development/tasks/task64-cli-defects-and-contract-drift.md)
and the [Find contract](../../crystallized/documents/cli/contracts/find/interface.md)
own expected behavior and evidence.

Task 64 also finds that the [Workspace State Files decision](../../crystallized/decisions/framework/workspace-state-files.md)
lists only `allowInstallPaths` and `removedCategories`, while current behavior
also supports `removedFiles`, `removedDirectories`, `removedExtensions`, and
`removedLibraries`. Reconcile the decision, documentation, and validation with
the supported state fields.

The public beta 1-to-beta 2 update also reproduced a result-status mismatch:
eight Framework files updated with `completed` and exit 0, no findings, and a
retained recovery bundle. The [Update interface](../../crystallized/documents/cli/contracts/update/interface.md)
has a status-table row listing retained recovery as `completed-with-warnings`
with exit 2, although its successful-retention prose and current process test
support `completed`. The prior
Loader bytes were preserved, and Doctor recognized the bundle with verified
integrity and one informational finding. This is contract drift,
not evidence of lost content. Task 64 should align the table with successful
retention and the existing process evidence. A partial-write failure must retain
its primary failed result even when its recovery bundle is kept.

[Task 61](../../working/cli-development/tasks/task61-documentation-accuracy-and-voice.md)
supplies provenance for older findings. The Loader explicitly allows an ordinary
entrypoint to omit local `Axioms`. Recheck any remaining `Next:` wording under
[Task 32](../../working/cli-development/tasks/task32-minimal-output-sweep.md)
before treating an older observation as open. The public upgrade's Doctor
recommendation led to a valid recovery inspection.

### Decide the supported customization promise

[Task 63](../../working/cli-development/tasks/task63-keeping-edits-through-updates.md)
needs a maintainer decision before stable behavior is documented. An ordinary
update replaces an edited managed file and saves its old bytes in the recovery
bundle. The supported way to retain a local change is `{name}.overwrite.md`.
`removedFiles` excludes an exact path from update planning and leaves a
still-present file untouched. That exclusion behavior is specified and tested.
Whether users may rely on it to retain direct edits remains undecided in Task
63. See the [update contract](../../crystallized/documents/cli/contracts/update/behavior.md).

Recommend a concise 1.0 statement for overwrite companions, update preview,
recovery, and restoration. Explain that ordinary managed files follow shipped
content and recovery preserves replaced bytes. Do not claim unrecoverable data
loss. Include the maintainer's decision on whether `removedFiles` may be used to
retain a direct edit, without presupposing the choice.

### Specify any default Skill indexing expansion first

[Task 47](../../working/cli-development/tasks/task47-entrypoint-reachability.md)
is already in the accepted before-1.0 order. Its remaining work is to define
default Index reachability into resources under native Skills. The current
bare `index` contract remains scoped to Loader roots and recognized entrypoint
closure. The targeted Doctor repair already gives a real command for a detached
stale catalogue, so it does not require broadening default Index. See the
[Skill indexing follow-up](../../working/cli-development/tasks/task47-default-skill-indexing.md).

Freeze discovery, traversal, containment,
generated-write limits, and native `SKILL.md` preservation before implementation.
Acceptance should show that generated Entries reach valid resources, authored
Skill and resource bytes stay intact, Index is idempotent, and Doctor agrees.
Do not expand this to whole-workspace traversal or implicit recipe execution.

## Shipped content and CLI journeys

Use the dependency-aware chain in the [current Task selection](../../working/cli-development/tasks/_tasks.md):

1. [Task 53](../../working/cli-development/tasks/task53-loading-and-scoping-audit.md): audit every current `LoadNow` and `KeepInMind` entry and default scope. Recount after Task 62, then recommend keep, scope, or remove for each entry, including Checkpoints refresh.
2. [Task 54](../../working/cli-development/tasks/task54-tag-trimming.md): apply accepted tag trimming to shipped files and generated Entries, preserving loading and useful discovery meaning. Keep archived tags historical.
3. [Task 44](../../working/cli-development/tasks/task44-template-content.md): make a bounded final pass over shipped templates and Core content once the retained Core inventory is known.

Run [Task 32](../../working/cli-development/tasks/task32-minimal-output-sweep.md),
[Task 39](../../working/cli-development/tasks/task39-output-audit.md), and
[Task 34](../../working/cli-development/tasks/task34-interpolated-value-markup.md)
alongside where file ownership permits. Task 32 should correct `Next:` actions,
especially for retained recovery. Task 39 should record per-command output
verdicts, and Task 34 should apply consistent argument markup.
Then [Task 37](../../working/cli-development/tasks/task37-wording-review-against-proposals.md)
should make a bounded pass over help, shared messages, and minimal headlines.
Do not reopen every sealed historical proposal.

## Compatibility and stable delivery evidence

Version and release-channel machinery exists, but it does not define the
compatibility promise. Recommend a short 1.0 decision for stable command names
and options, exits and statuses, structured-output schemas, supported frontmatter,
workspace-state fields, customization boundaries, and how breaking changes or
migrations will be handled. Match the promise to behavior intended for support.
Do not infer it from the version script.

The accepted platform horizon is Linux glibc, Windows, and macOS on x64 and
ARM64. Both the root [`package.json`](../../../../package.json) and the generated
wrapper manifest in [`package-manifests.ts`](../../../../scripts/delivery/npm/package-manifests.ts)
require Node.js 22.18 or later. The native executable requires neither Node.js
nor an installed .NET runtime. State no minimum operating-system versions
without an accepted source that defines them.

For stable, recommend fresh qualification of the exact release commit on all six matching
hosts, with seven public npm packages, six portable archives, and checksums
agreeing on source and version. Smoke the exact stable version and `latest`, then
upgrade from the published beta while checking overwrite companions, custom
files, removal choices, and recovery/restoration. Packaged CI journeys cover
fresh local tarball installs. The public beta 1-to-beta 2 upgrade is useful
existing evidence, but stable still needs its own published-beta upgrade check.
Keep beta 2 proof in Task 59.
Record stable qualification and publication in a future stable-release Task.

Prepare concise release notes and limitations from verified behavior. A fresh
dependency review is a proportionate recommendation, not an existing accepted
gate. These records establish no current vulnerability finding.

## Keep separate or defer

- Recommend keeping [Task 55](../../working/cli-development/tasks/task55-alternative-root.md) separate from the current readiness sequence. This is a scheduling recommendation, not an accepted exclusion from 1.0.
- Defer [Task 40](../../working/cli-development/tasks/task40-capture-coverage.md), [Task 48](../../working/cli-development/tasks/task48-scoping-for-extension-routes.md), and [Task 58](../../working/cli-development/tasks/task58-demo-evals.md) unless an accepted compatibility or release need changes their priority.
- Park partial Extension merging in [Task 36](../../working/cli-development/tasks/task36-extension-merge-and-guards.md) pending a specific user need and maintainer decision. It conflicts with the package's whole-file ownership model. Treat host `AGENTS.md` and `CLAUDE.md` guard migration as a separate decision, without implying mixed ownership of whole files or regions.
- Coordinate [Task 65](../../working/cli-development/tasks/task65-where-open-tasks-live.md) with [Local Planning](../../working/local-planning.md). Task placement is planning hygiene, not runtime readiness.

Task 62's repository Doctor reports 12 blocked `reference.target-alias`
findings caused by an automatic ID collision between the flat
`.agents/memory/working/cli-development/tasks/beta-follow-ups.md` record and
the nested
`.agents/memory/working/cli-development/tasks/beta-follow-ups/_beta-follow-ups.md`
entrypoint. These findings concern maintainer-repository identity and
navigation. They do not indicate a failure on a fresh install or a Task 62
regression. Treat the naming and route choice as separate repository hygiene.

## Proposed sequence and remaining decisions

Use these three waves to coordinate work, not to predeclare release blockers:

1. **Close defects and choose stable behavior:** complete Task 64, decide Task 63's direct-edit policy, freeze Task 47's traversal contract, and record the compatibility promise. Exit when each accepted behavior agrees across implementation, contract, and reproducible evidence.
2. **Finish bounded journeys and content:** run Tasks 53, 54, and 44 in order. Coordinate 32, 34, and 39 by file ownership, then run 37. Implement Task 47 after its traversal and write limits are frozen. Exit when shipped prose, examples, Entries, and CLI results match demonstrated behavior.
3. **Qualify stable:** run the exact candidate's host, package, checksum, stable channel, and beta upgrade checks above. Record stable evidence in the future stable-release Task. Exit only with evidence from that candidate and its public artifacts.

The maintainer still needs to decide Task 63's direct-edit policy and the 1.0
compatibility promise. Task 47's remaining contract needs to be frozen, and any
deferral requires a maintainer change to its accepted priority. Tasks 53 and 54
will add fresh keep/scope/remove decisions. These stable criteria are
recommendations, not accepted gates. This document remains contextual until the
maintainer accepts a specific next scope.
