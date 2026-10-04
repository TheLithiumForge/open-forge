---
open-forge:
  description: "Historical record: Task 47 follow-up to make default Index reach routed resources through native Skills"
  tags: [Memory, CLI, Task, Skill, Index, Contextual, Archived, Historical]
---

# Task 47 follow-up — default Skill indexing

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/tasks/task47-default-skill-indexing.md` after the maintainer selected Memory cleanup. Final receipt explicitly says no Task47 requirement remains; all six runtime modes and installed Planning passed.

This record preserves historical evidence. The [current CLI development route](../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

**Reviewed on 2026-09-28:** [review](../analysis/open-task-review/task47-entrypoint-reachability.md). Recommendation:
Do before 1.0. The review names any details in this record that are out of date.

## Outcome and authority

Recorded at the maintainer's request on 2026-09-22 under
[Task 47](task47-entrypoint-reachability.md), coordinated with
[Task 46](task46-routed-skill-resources.md). The desired outcome is for default
Index to reach a native `SKILL.md` and the routed resource catalogues beneath
that Skill without requiring the user to name the catalogue manually.

The initial 2026-09-22 instruction recorded a future capability. The
2026-10-01 frozen selection contract below subsequently authorized its bounded
implementation. The independent completion receipt records qualification of
that exact candidate. The separate [lossless wording proposal](../../beta-preparation/src-wording-proposal.md)
is historical work.

## Current evidence

`use-workflow` is provided by the optional `workflows` Extension. Core provides
the Skills route, not the installed selector. The native Skill points to
`references/_references.md`; other Extensions add scoped recipe catalogues.

The current [Index selection resolver](../../../../../src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSelectionResolver.cs)
starts its default selection from Loader roots and recognized entrypoint
closures. The [published recovery journey](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs)
now proves that plain Index reaches the immediate reference catalogues of a
rooted native Skill, updates generated Entries, preserves authored bytes, and
leaves scratch Doctor clean. Explicitly indexing a catalogue remains supported;
selecting the native Skill leaf retains parent-only behavior. Detached fixtures
remain detached. The earlier Doctor correction supplies accurate targeted
advice where an explicit catalogue remains necessary.

That correction is complete. This task concerns automatic reachability, not a
return to a misleading bare Index recovery suggestion.

## Accepted user journey

A user installs Core and an Extension supplying a native Skill, then adds a
valid document beneath one of the Skill's routed reference catalogues. They
run `open-forge index` with no source argument. The relevant generated Entries
include the document; Doctor no longer reports that catalogue as stale.
The Skill instructions and authored document bytes stay unchanged. Repeating
Index makes no further changes.

This journey is implemented within the frozen command-local selection
contract below and independently verified through the installed package.

## Initial specification questions

These initial questions are resolved by the frozen selection contract and
qualification receipts below. No product-scope decision remains open.

- Define how a discovered native Skill connects to its resource catalogues:
  recognized catalogue descendants, explicit contained Markdown links, or a
  bounded combination. Do not equate every link with a route or scan the whole
  workspace merely to make the example pass.
- Distinguish selecting `SKILL.md` as a traversal boundary from rewriting it.
  Native frontmatter and instructions must remain intact. Whether any authored
  Entries section in a Skill is supported needs an explicit contract; do not
  invent one or inject a generated section by default.
- Define discovery outside the standard Skills route, detached/unexposed Skills,
  multiple or absent catalogues, native metadata, compatibility entrypoint names,
  nested Skills, cycles, duplicate reachability, aliases and physical containment.
- Align Index selection/counts/dry-run output with Doctor, route inspection and
  context selection where they share topology facts. Index reachability does not
  automatically make reference bodies #LoadNow or authorize recipe execution.
- Preserve bounded filesystem effects and performance. Define how invalid,
  inaccessible or escaping resources are diagnosed without unauthorized writes.

## Completed specification and verification work

1. Trace Skill classification, discovery, generated-navigation topology and
   Index selection. Resolve Task 46's metadata questions once, with one shared
   model for the affected consumers.
2. Freeze before/after outcomes for default and explicit selection. Include the
   existing targeted-catalogue success case as preservation evidence.
3. Specify the journey above plus no-catalogue, multiple-catalogue, cycle/alias,
   malformed/unreadable and escaping-path cases only where supported by the
   accepted model. Agree on messages, status/exits, streams and JSON facts before
   authoring expectations.
4. Implement one bounded change after selection. Use pure topology/mapping tests
   where no real filesystem is needed and real-OS integration evidence where it
   is. No filesystem mock layer or broad filesystem abstraction is implied.
5. Verify dry-run makes no writes, apply only changes permitted generated
   interiors, authored bytes are preserved, repeated Index is idempotent, and
   Doctor agrees. Run the applicable managed/native gates and update contracts,
   help and current task state together.

## Current state

Task 47 “Entrypoint reachability” (phase 3/3): milestone 4/4. Independently complete and ready for Root integration on the beta4 base. The exact accepted source, tests, and contracts were reconstructed and independently qualified in a_1bec444bebb4. Full managed populations, supported-host win-x64 NativeAOT populations, packaging, and the installed Planning journey passed. Root owns the squash merge, combined wave acceptance, and release qualification.

Review budget: 1/1 consumed. Repairs: 0. Councils: 0. The review requested no repair. Any product, test, or contract repair requires a Root mutation slot.

## Execution Capsule: root freeze, 2026-10-01

**Status:** Task 47 “Entrypoint reachability” (phase 3/3): milestone 3/4. Root accepted the transferred candidate for inclusion in combined qualification. Root's focused integration, documentation, and physical checks are complete. The final combined managed, native, and package gates remain, so the task is not closed. **Profile:** Assured bounded public-selection change. Frozen implementation and one fresh whole-task review are complete.

**Authority and budget:** Root retains architecture, integration, acceptance, and the full managed/native gate. All edits and deterministic patch transfers are Luna/max-owned. No child packet workers or Codex app task threads are assigned. This does not limit authorized Task Mastermind coordination. Allow one fresh whole-task review by GPT-6.1 Sol at high or GPT-6 Astra at medium, one grouped correction, and zero councils. The completed GPT-6.1 Sol/high review covered the whole task, including production structure and test/evidence lenses. It found no material findings. Review budget: 1/1 consumed. Repairs: 0. Councils: 0. No repair was requested. Any product, test, or contract repair requires a Root mutation slot.

**Frozen selection contract:**

- Add a command-local bridge in Index selection only. Eligible native Skills must already be structurally reachable under the standard `.agents/skills` route, including routed scopes. Discover catalogue entrypoints only in immediate child directories using the existing typed formation, then follow each entrypoint's normal closure.
- Expand default selection and explicit Loader or entrypoint closures. An explicitly selected Skill leaf retains parent-only behavior. Zero catalogues are a no-op. Multiple catalogues are supported.
- Do not traverse arbitrary Markdown links, recursively search gaps, add scans, or change shared runtime topology or activation. Skills are never region targets. Keep `SKILL.md` and authored bytes unchanged. Writes stay inside generated catalogue `Entries` interiors.
- Preserve the five entrypoint forms (`_{folder-name}.md`, `index.md`, `_index.md`, `references.md`, `_references.md`), order, aliases, ambiguity, physical containment within `WORKSPACE`, and existing malformed-source finding and skip semantics. Detached fixtures remain detached.

**Packets:**

- **A, Luna/max code and tests:** `src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSelectionResolver.cs`, with a local `IndexSkillCatalogueTraversal.cs` only if needed; Index selection unit tests, Index integration tests, `PublishedDoctorGeneratedNavigationProcessTests.cs`, and `PublishedIndexProcessTests.cs`. Do not change source topology, parsing, metadata, or safety.
- **B, Luna/max documents (contract packet complete):** `.agents/memory/crystallized/documents/cli/contracts/index-candidate/{interface,behavior,technical-design}.md`, `.agents/memory/crystallized/documents/maintenance/payload/agents/{skills,workflows}.md`, and the two Task 47 records. Reconcile only precise runtime-inheritance clauses. Root retains shared loading documents, payload prose, `docs/cli.md`, and the project ledger. Return literal suggestions for those protected sources when needed. Do not edit them.
- Each C# author and reviewer independently reads the complete current `.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md`.

**Evidence and execution:** Cover a focused pure selection matrix, then a real scratch-workspace check for byte preservation, dry-run, idempotence, metadata, and alias boundaries. Cover the installed planning-process journey with scratch Doctor only. Do not use remote restore. If assets are missing, report the exact prerequisites. Run code tests as `dotnet run --project <exact-test-csproj> --configuration Release --no-restore [Unit/Integration: -p:OpenForgeSkipDevelopmentPublish=true] -- --filter-class <exact-class> --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off`. EndToEnd builds its own managed CLI.

## Folded in on 2026-09-28

- **From [Task 46](task46-routed-skill-resources.md):** can a native Skill host
  routed resources, and on what terms? Freeze the metadata rule with it. The
  thirteen-warning Doctor report and the metadata question are resolved.
- **From [Task 43](task43-workflows-as-skill.md):** adding a workflow recipe
  still needs a manual `index` step.
- **Historical contract conflict:** Workflow Support and Skills used different
  loader-to-resource inheritance wording. The frozen contract documents now
  distinguish Index's command-local selection from runtime activation.
- **Possibly the same root cause:** the documentation review found that
  `extension update` reports the `use-workflow` catalogue's `Entries` as updated
  when the file doesn't change. See
  [Task 64](task64-cli-defects-and-contract-drift.md).

## Qualification and review receipt — 2026-10-01

**Original receipt status:** At the author-receipt baseline, Task 47 “Entrypoint reachability” (phase 3/3): milestone 3/4. The frozen implementation, focused author qualification, and one fresh whole-task review were complete. Root's later acceptance for combined qualification and the current remaining gates are recorded in the Root addendum. Review budget: 1/1 consumed. Repairs: 0. Councils: 0.

Code author r_5ccb64248b73 (a_390948de2166, gpt-6-luna/max) completed six code and test files. Docs author r_47d626b3b0d7 (a_0664b65ade70) completed seven documentation files. Both transfer runs targeted branch ww/a_9ea9832e868f at base 2e0da10a5657c084b2847245775858123e09a5ed.

Docs transfer r_a4123669bf85 used artifacts/task47-transfer/task47-docs-r_47d626b3b0d7.patch from transfer worktree a_7262fdf639b8, SHA256 FD630D409630B41B67EDAFD4A386C20B77DE3C5B21A1DB89044B35E7D9792613. Code transfer r_4fe224e75c39 used artifacts/task47-transfer/task47-code-r_5ccb64248b73.patch from the same transfer worktree, SHA256 CFCFC2CD033D41A06A4869036D11E55040CB4C1465BF62D9EDD5D00BF9B832D0. All 13 transferred files matched the author SHA256 values. Real indexes remained unchanged.

**Code and test paths:**

- src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSelectionResolver.cs
- src/cli/operations/OpenForge.Cli.Operations/Commands/Index/Shared/Selection/IndexSkillCatalogueTraversal.cs
- src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedDoctorGeneratedNavigationProcessTests.cs
- src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexOperationWorkspace.cs
- src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Index/IndexSkillCatalogueIntegrationTests.cs
- src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Index/Shared/Selection/IndexSelectionResolverTests.cs

**Contract paths:**

- .agents/memory/crystallized/documents/cli/contracts/index-candidate/behavior.md
- .agents/memory/crystallized/documents/cli/contracts/index-candidate/interface.md
- .agents/memory/crystallized/documents/cli/contracts/index-candidate/technical-design.md
- .agents/memory/crystallized/documents/maintenance/payload/agents/skills.md
- .agents/memory/crystallized/documents/maintenance/payload/agents/workflows.md

The seven documentation files also included the two Task47 records. All 13 original and final SHA256 values are recorded in the [ignored qualification receipt](../../../../../artifacts/task47/qualification-receipt.json).

**Focused qualification:** The author reported 17 passing unit tests, one passing integration test, and eight passing PublishedDoctorGeneratedNavigationProcessTests, with zero failures or skips in the successful runs. The exact commands and per-run counts are in the qualification receipt. An initial build-server access failure was resolved with --disable-build-servers. No permissions changed and no remote restore was used. The author corrected initial fixture and assertion failures before the final successful runs.

**Fresh review:** Review run r_c88df85ae4a8, agent a_32f2ebe4dd8d, effective model gpt-6.1-sol/high, found no material findings. The reviewer independently loaded the full C# directive trio, checked all 13 paths and direct consumers, hashes, indexes, diff, and actual author test logs. The reviewer did not rerun tests.

**Original focused-receipt limits:** The author-focused receipt did not demonstrate real physical alias or containment behavior, or malformed metadata behavior, through the new bridge. Root's later physical and integrated checks and dispositions are recorded in the Root addendum. The final combined managed, native, and package gates remain. No product scope decision is unresolved. The review requested no repair. Any product, test, or contract repair requires a Root mutation slot.

## Root qualification addendum (2026-10-01)

Root accepted Task 47 for inclusion in combined qualification based on source
trace `r_6e927cfe0548` and an independent read of
`SourceCatalogue.ProjectIssue` and existing alias compatibility. The source
trace records a bounded interpretation of the same-master QA, not a second
fresh whole-task review. The existing review `r_c88df85ae4a8` remains the sole
fresh review, with no material findings. Review budget: 1/1. Repairs: 0.
Councils: 0.

The original 13-file candidate transfer `r_ee4eaa84b49a` remains valid. Root
verified all 13 hashes before these receipt-only updates. The original
[`qualification-receipt.json`](../../../../../artifacts/task47/qualification-receipt.json)
and transfer receipt remain unchanged.

**Focused integration:** Run `r_071b9f003432` passed 17 unit tests, one
integration test, and eight published-process tests, for 26 passed, zero failed,
and zero skipped. All command exits were zero. Logs are in
`artifacts/task47/integration-qualification`. The initial author 26-pass
receipt remains separate evidence. These runs are not reported as 52 unique
tests.

**Documentation:** Corrected run `r_e3a4ea779382` passed all 13
`RepositoryMarkdownTests`, with zero failures or skips and exit 0. Initial run
`r_c20352533e1a` used the wrong working directory and ran zero tests. The
corrected run leaves no current documentation blocker.

**Physical QA:** Run `r_b2c9df586ae2` matched all six source and test hashes to
the reviewed candidate. Its fresh Release build had zero warnings and errors.
The assembly SHA-256 is
`7BC75A4EAB6CD0B38EA4B840098F1908ACD446ECE94D6314876CE213BDFC1E3B`. The
receipt is
`C:/Users/Tedy/.worker-watch/worktrees/a_390948de2166/artifacts/task47-physical-qualification/run-04a606154cc641c496be26a9e750dc9c/qualification-receipt.json`.

Root accepted these bounded dispositions without a product patch:

- An outside junction was excluded before formation. The valid inside
  catalogue was the only path written, and outside hashes stayed unchanged. A
  new public warning is not required for an excluded unselected directory.
  The unsupported QA prompt is resolved by the unchanged
  `PathComponentWalker`, `SourceCatalogueReader`, and
  `SourceCatalogue.ProjectIssue` behavior.
- Both inside aliases are logically eligible. Existing compatible identity
  collapses them to ordinal alpha, yielding one catalogue effect and zero
  effects on repeat. Eligibility does not depend only on a physical target.
  Native Skills, overwrites, and recipes remain unchanged.
- Missing name or description leaves both baseline and candidate incomplete
  with exit 3. The existing metadata warning remains, and hashes do not
  change.

Root's focused integration, documentation, and physical checks are complete.
Task 47 is accepted for inclusion in the combined qualification, not closed.
Final combined managed, native, and package gates remain. The original
candidate source remains frozen.

## Independent completion receipt (2026-10-02)

Task 47 “Entrypoint reachability” (phase 3/3): milestone 4/4. Completion owner
a_1bec444bebb4, run r_54dbdc5b958b, GPT-6.1 Sol/high, independently completed
the accepted candidate on base `2e0da10a5657c084b2847245775858123e09a5ed`.
The user's completion assignment supersedes the older author roster for this
completion. Root retains integration and wave acceptance.

The 13-file inventory contains six source/test files, five contract documents,
and these two Task records. All eleven frozen source/test/contract hashes match
the accepted integrated candidate and original author trees. The two records
contain receipt-only reconciliation. Both formerly untracked Task sources are
included. No other Task, shared control record, version field, prompt file, or
ignored output is part of the patch. Version remains `0.9.0-beta.4`.

Fresh focused qualification passed 17 unit, one integration, and eight
published-process tests: 26 passed, zero failed or skipped. This is independent
evidence for this reconstructed candidate; it overlaps the full populations.

| Runtime mode       | Passed | Failed | OS exclusions |
| ------------------ | -----: | -----: | ------------: |
| unit               |   3882 |      0 |             0 |
| integration        |   2669 |      0 |            17 |
| public             |    264 |      0 |             0 |
| native-integration |   2669 |      0 |            17 |
| native-public      |    264 |      0 |             0 |
| public-native      |    264 |      0 |             0 |

All six modes used the repository delivery gate. Every exclusion was checked
against `scripts/delivery/platform-skips.ts`; no unexpected skip or warning was
accepted. Native evidence is for this supported Windows win-x64 host. Other
release hosts remain Root's release responsibility.

The Release solution build, native CLI/integration/public publications, scoped
whitespace and analyzer verification, tested packaging, offline local npm
installation, and installed Planning journey all exited 0. Builds and cached
restores disabled build servers and used `-m:1`. The run commands used already
built assemblies. Restore used repository `NuGet.Config`, an empty local feed,
and the existing package cache; no new dependency or network restore occurred.
A fresh NuGet vulnerability audit was not run.

Four scratch scenarios passed against the fresh native CLI: outside-junction
exclusion, inside-alias collapse, missing-name metadata, and missing-description
metadata. Dry-run preserved hashes; successful apply preserved native Skills,
recipes, overwrites, and authored prefixes; repeat was a no-op. Incomplete
metadata returned exit 3 with only planned/not-started effects and unchanged
hashes. The installed package Planning journey also proved generated Entries,
byte preservation, idempotence, and scratch Doctor with zero findings. Doctor
was never run in this repository.

Earlier failed commands remain in the receipt: an invalid `dotnet run` argument
ran zero tests; two nonstandard temporary paths affected repository/snapshot
fixtures; and a scratch assertion confused effect rows with applied writes.
The standard Windows task-owned temporary directory and corrected scratch
accounting passed without changing product code or test expectations.

Exact commands, exits, reports, OS exclusion names, before/after SHA-256 values,
artifact identities, and patch-application evidence are in
`artifacts/task47/sol-completion/completion-receipt.json`. The owned binary patch
and readable diff are `task47-owned.patch` and `task47-owned.diff` in that
directory. Final document verification and `git diff --check` are recorded there.
The patch is checked in reverse and applied to an exact scratch base, with all
13 resulting hashes compared to this worktree.

Review budget stays 1/1 consumed (r_c88df85ae4a8, no material findings). Repairs
remain 0; councils remain 0; no additional review phase was created. No Task47
requirement remains. Root's combined integration/wave/release work is separate.
No real index, develop branch, commit, merge, push, or publication was changed.
