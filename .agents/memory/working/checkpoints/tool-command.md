---
open-forge:
  description: Resume issue 23 tool core implementation, verification and unresolved maintainer decisions
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool]
---

# Tool command core

The Tool command core is an implementation task that adds the eight external-program commands without concrete production tools.

The selected sources are issue #23 and both comments, the complete external specification and the latest maintainer decisions. This worktree stays on `feature/tool-command`, above updated Comment Links. The overseer handles pushes and PR #26. Earlier checkpoint details and capture lists are kept in Git at `0822e0ed8:.agents/memory/working/checkpoints/tool-command.md`.

Done: Contracts, models, schemas, catalogue selection and the three read commands are committed. Generic process, ignore and backup mechanics and the supply recipes are committed.
Now: Rebased onto Comment Links `75f6677ad`. Final V1 decisions are recorded in accepted contracts. Tool Run, the new generic features and all five writer leaves are unimplemented. V1 is incomplete.
Next: Implement Install with the gated capabilities and wire passive store observations. Then implement Configure, Choose, Update and Remove with consent and preview parity. The telemetry choice is recorded and reported.
Blocker: Unix Node archive links still need a decision. The latest maintainer direction settles Tool Run, declared rewrites and active-role guidance.

## Decisions

The latest decisions are:

- Open Forge distributes reviewed recipes and never ships, mirrors or rehosts tool bytes.
- Native tools use official GitHub release assets, pinned hashes and build attestations where published.
- Node tools use a private official portable runtime pinned from signed checksums. The bundled npm uses a complete pinned registry lock with `npm ci`, no resolution and no package scripts. The user's Node and PATH stay unchanged. musl is unsupported.
- Tool-owned commands install, uninstall and clean their indexes. They handle agent files and required usage Skills. Open Forge edits only its settings, lock and managed ignore section for tools.
- Tool self-updates stay off. Telemetry defaults off with a visible opt-in.
- Same-pin Install verifies and repairs immutable bytes and reruns the installer. Status diagnoses and Inspect shows paths. No Tool repair or open-path command is added.
- Update selects reviewed pins only. Keep previously reviewed pins for exact downgrades. Keep tool and Node versions while any workspace references them.
- Git workspaces create no `.bak`, including for dirty or untracked files. Non-Git writes use sibling backups. Root Repair handles interrupted Tool control-file backups. Recovery v1 stays unchanged.
- Tool Exec is proposed only. Concrete CodeGraph and Codebase Memory definitions and adapters stay on the next branch.

## Commits

The current history includes these review boundaries:

| Commit      | Result                                                                  |
| ----------- | ----------------------------------------------------------------------- |
| `dee176dc7` | Initial contracts, rebased from the original contracts commit           |
| `6b70cdec5` | Models, schemas and exact catalogue selection                           |
| `b8edc22a9` | List, Inspect and Status                                                |
| `5fa5a0bcf` | Plain-backup contracts replacing the discarded bundle design            |
| `87cecdbea` | Sibling backup mechanics                                                |
| `9adb49e08` | Overseer's whitespace formatting on updated Comment Links               |
| `de4065cd9` | Tool-owned lifecycle contracts and docs                                 |
| `d8c7ba60b` | Lifecycle vectors and separate execution records, agent parser removed  |
| `489e785d6` | Linux clean-checkout fix for three ignored supply model files           |
| `0822e0ed8` | Bounded process host, Git backup selection and shared ignore mechanics  |
| `391ed70f6` | Verified upstream supply recipes, admission checklist and source checks |
| `c28e4c5fb` | Verified extraction, physical store areas and reference registry        |
| `11247f2a4` | Backup reporting and bounded Root Repair restoration                    |
| `b712682e0` | Verified store publication, attachment and final release                |
| `0cb6fd810` | Ordered command journaling and paired control-record publication        |
| `e9e178ee7` | Saved telemetry choice, applied receipt and passive reporting           |

The Linux failure was caused by `Models/Artifacts` matching the Windows artifact ignore rule. All three types now live in tracked `Models/Supply`. No discarded parser package, context or seam survives.

## Verification

CI is the full gate. Local evidence uses Release managed builds, the authorized `NuGetAudit=false` override and no native publication. Integration keeps JSON reflection disabled. The SDK is 10.0.101. Never run Doctor on this worktree. The latest lane update permits only touched or created tests and their affected captures.

Recent gates are:

| Increment                   | Build                                | Unit       | Integration | Other checks                                              |
| --------------------------- | ------------------------------------ | ---------- | ----------- | --------------------------------------------------------- |
| Tool-owned contracts        | 0 warnings/errors                    | Unchanged  | 66 passed   | Both format checks, site and Index passed                 |
| Lifecycle models            | 0 warnings/errors                    | 139 passed | 52 passed   | Both format checks and four schemas passed                |
| Linux source fix            | 0 warnings/errors                    | 48 passed  | Unchanged   | Both format checks passed                                 |
| Generic mechanics           | Both builds, 0 warnings/errors       | 18 passed  | 79 passed   | Both format checks passed, no snapshots changed           |
| Supply recipes              | Both builds, 0 warnings/errors       | 7 passed   | 8 passed    | Both format checks, four schemas, site and Index passed   |
| Store foundation            | Both builds, 0 warnings/errors       | 8 passed   | 21 passed   | Both format checks passed, no snapshots changed           |
| Backup reporting and Repair | Both builds, 0 warnings/errors       | 1 passed   | 23 passed   | Both format checks, site and Index passed                 |
| Store publication           | Integration build, 0 warnings/errors | Unchanged  | 19 passed   | Both format checks passed, no snapshots changed           |
| Execution records           | Both builds, 0 warnings/errors       | 6 passed   | 18 passed   | Both format checks passed, no snapshots changed           |
| Telemetry choice            | Both builds, 0 warnings/errors       | 10 passed  | 18 passed   | Both format checks, four schemas, site and Index passed   |
| Request preparation         | Unit build, 0 warnings/errors        | 19 passed  | Unchanged   | Both format checks and Index passed, no snapshots changed |

Every listed run had zero final failures and skips. The supply Unit selection is `*ToolArtifactSourceTests`. Its initial failure used a synthetic non-GitHub repository in the positive GitHub scenario. The fixture was corrected before the final run. The mechanics build corrected one xUnit analyzer violation. Neither was a flake.

The supply capture selection uses these exact method filters:

- `*ToolReadCommandsTests.HelpStopsBeforeWorkspace`
- `*StatusBeforeOutputSnapshotTests.HealthyWithExtension`
- `*StatusBeforeOutputSnapshotTests.InstalledWorkspace`
- `*StatusBeforeOutputSnapshotTests.RecoveryBundlePresent`
- `*StatusOutsideIntegrationTests.OutsideHumanAndJsonViewsAreReviewed`
- `*IncludedFileReportingIntegrationTests.ReportsIncludedFiles`

Build each matching test project with `dotnet build <project> --configuration Release --no-restore -p:PublishAot=false -p:OpenForgeSkipDevelopmentPublish=true -p:NuGetAudit=false --verbosity quiet`. Integration also uses `-p:JsonSerializerIsReflectionEnabledByDefault=false`. Run the matching Release DLL under `artifacts/bin/<project>/release/` with the selected class or method filters and `--minimum-expected-tests 1 --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory <owned-report-directory>`.

Supply reports are under `artifacts/tool-command/restart-supply-*`. Accepted 56 Status captures for Skill byte, token and ratio totals only, plus three help captures for supply guidance. The help test now uses the actual build version when normalizing. Its old `test` placeholder incorrectly altered the word attestation. No findings, statuses or other semantic JSON values changed in Status captures. The site passed with its existing Node localStorage warning. Index checked 199 files and planned no remaining changes.

Writing review R12-R15 was corrected and passed recheck. Choose's unconfirmed metadata-only behavior is now proposed. Attestations keep the where-published condition. npm SRI is separate from native/Node archive SHA-256. Optional per-artifact `url` and its source-check seam support Node and registry URLs in one recipe.

## Resume

The store foundation adds the per-platform locator, physical store areas, a bounded file lease and a source-generated reference registry. Initialization uses an atomic directory rename. Later changes require the store lease and expected prior bytes. Missing or inconsistent references block access. The archive reader verifies pinned bytes before extraction and rejects traversal, duplicate destinations, file-parent conflicts, links and special members. Extraction uses an empty private directory. The existing backup session and the registry share one byte-replacement helper.

Foundation evidence is under `artifacts/tool-command/store-foundation-*`. Unit selects the new `ToolArtifactIntegrityTests` and `ToolStoreLocatorTests`. Integration selects all methods in the new `ToolArchiveExtractorTests` and `ToolStoreRegistryTests`, plus only `ToolBackupSessionTests.SharedReplacementKeepsBackupContract`. The final runs executed 8 and 21 tests with zero failures or skips. An initial registry fixture exposed a missing-root read bug, which was fixed. A zero-test mixed class/method filter was replaced with method-only filters. Neither result was counted as a passing gate. No snapshots changed. Writing review R16 corrected the distinction between initial atomic publication and later leased changes.

The pending writer work is:

1. Store publication, attachment and final release are gated. Wire them into writers and passive observations. Local process-consent selection still needs its caller. Use local hashed test artifacts only.
2. The visible telemetry choice and Inspect output are gated. Apply the choice in writer profiles. Keep private Node, frozen npm closure and official-source checks on their accepted seams.
3. Backup reporting and bounded Root Repair restoration are gated. Only settings, lock and ignore are Tool backup targets.
4. Add all five typed writer leaves, complete plans, consent, final confirmation, expected-state revalidation and paired control publication.
5. Add managed ignore planning, all removal exclusions, ownership collisions, root Remove coexistence and final-reference release.
6. Replace unavailable store observations with verified bindings and resolve external instructions. Native completion adapters remain on the next branch.

Original stash `60d034fef70b076bcb05a488d668f09fd538ddce` is untouched. Its rebased reporting/restoration copy is `15825724e67c04a66957378d2316925e28c92e85`. The reporting and restoration portions were restored selectively and adapted. Don't apply either stash wholesale or restore the discarded member model and host backup targets. The generic mechanics checkpoint `76a2b372ae9af705bd28eef5174a9a2061a4448c` is already applied and committed. Don't apply it again.

## Backup increment

The current increment restores the reporting checkpoint selectively. It reads sibling backups only at settings, lock and `.gitignore`, including when current records are invalid. Reports keep saved contents private and show workspace-relative paths. Tool readers and root Status and Doctor point to the scoped Repair preview. No agent-file backup target survives.

Root Repair restores the control group before other work. Automatic selection or interactive confirmation is required. Explicit relinks need a separate invocation after restoration. Validate saved control bytes, matching resulting Tool pins, current ignore-file permission and removal exclusions. Recheck all current control records and both saved and target bytes under the workspace lease. Verify the group before deleting backups. Known kept backups warn. Unknown removal fails. Tool backup findings are separate from recovery v1. Preview includes target restoration and later backup deletion.

The final Integration build passed with reflection disabled, zero warnings and zero errors. The latest affected run before the final result refinements executed 23 tests with 19 passes and four expected measurement snapshot failures. Accepted 56 Status captures for Skill byte, character, token and ratio totals only. Structured comparison kept every other JSON value. Accepted 37 new reporting, preview and help captures after review. The final preview refreshed four JSON captures for backup-deletion effects only. Final managed Unit passed 1/1, Integration passed 23/23, both with zero skips. Both builds had zero warnings and errors. Both required format checks and the site build passed. The site kept its existing Node localStorage warning. Index refreshed the checkpoint description while keeping both active entries. Its final preview is expected to have zero changes. Reproduction selects `*RepairFiniteMappingTests.FindingDefinitionsAreComplete` in Unit. Integration uses method filters for `*ToolBackupReportingTests.*`, `*ToolBackupRepairTests.*` and the five affected Status/Included methods listed above, excluding the unchanged Tool help test.

Reports and logs use `artifacts/tool-command/tool-backup-*`. Development corrections fixed the Doctor finding category, the public Status code oracle, explicit standard detail for Doctor warning rows and fixture replacement through the real file API. These were deterministic failures, not flakes. Writing review R17-R18 and the final result-clause review passed. An incidental Windows default-encoding rewrite was corrected by restoring untouched contract bytes from HEAD with explicit UTF-8. Both Repair contract diffs now contain additions only.

## Store publication

The publisher now writes pending metadata before atomic member moves and rechecks destination expectations before each move. Pending metadata keeps the source stage and prior verification history. Foreign members and new changed pending bytes block retry. Previously verified immutable members can be repaired. A fresh stage can finish an interrupted first publication or interrupted repair. Final member verification includes Unix executable bits. Source-stage cleanup keeps a cause when unfinished.

Attach rechecks the complete dependency closure. Nonfinal release keeps shared bytes. Final release checks complete references, known members and hashes before deleting recognized files and empty directories. Missing unit or workspace reference coverage blocks deletion. Parent removal releases reciprocal dependency references. The helpers don't run tools or choose command policy.

Managed Integration build passed with reflection disabled, zero warnings and zero errors. The 19 new `ToolStorePublisherTests` cases passed with zero skips. Select only that class with the standard Release runner flags above. Both required format checks passed. No snapshots changed. Evidence is under `artifacts/tool-command/tool-publication-*`. The initial six failures came from trying to snapshot the fixture's own exclusively held lock. The fixture snapshot now excludes only that exact lock and still compares directories, metadata and all data bytes. This was deterministic, not a flake. Writing review R20 clarified unknown paths versus changed registered bytes.

## Execution records

This increment uses managed file replacement, the existing workspace lease and the bounded process host. It adds no dependency, native bridge or exceptional machinery. The supported cases are malformed records, failed commands, cancellation, interrupted writes and cooperating-process changes. Concurrent edits are kept. Tool-owned external effects have no guessed inverse. CI covers the full managed and Native AOT gates. Local Unit cases cover ordered execution. Integration cases use owned files, leases and child processes.

Paired publication validates the settings and lock before writes and rechecks both expected files under the workspace lease. Git workspaces create no sibling backups. Other workspaces use the existing backup session. Failure restores only bytes still matching this operation's writes and reports unfinished recovery. First publication creates only the fixed control parent. Execution records don't claim a successful installation.

The command executor records the complete proposed list before launch. The process host reports actual child creation before the executor saves started evidence. Each result is saved before another command can start. A failed command or failed record write stops the sequence. In-memory results keep an outcome that couldn't be saved. Lifecycle decoding now rejects a null command record before summary creation.

Both managed Release builds passed with zero warnings and errors. Unit passed 6/6 in the new `ToolCommandExecutorTests`. Integration passed 18/18 using method filters for `*ToolRecordPublisherTests.*`, `*ToolProcessRunnerTests.RefusesChangedExecutable`, `*ToolProcessRunnerTests.ReportsActualStart` and `*ToolProcessRunnerTests.ReapsChildAfterNotificationFailure`. Both runs had zero skips. Reports and logs use `artifacts/tool-command/tool-execution-*`. Both required format checks passed. No snapshots changed. An initial fixture teardown tried to delete files after its newly created parent had already been removed. The fixture now checks existence before deletion. This was deterministic, not a flake. Writing review R21-R22 corrected recovery limits and plan-before-consent order and passed recheck.

## Telemetry choice

The saved `tools.<id>.telemetry` boolean defaults to `false`. Inspect reports it independently of receipt availability. Unreadable settings produce an unavailable choice instead of an assumed off value. The lock records the applied choice. Active-tool Status reports drift when desired and applied choices differ. The pending writers still need to apply the choice through their reviewed profiles. No running tool changes in this increment.

This increment reuses source-generated codecs and the read-only command pipeline. It adds no dependency or exceptional machinery. Unit covers authored values, defaulting, malformed values and receipt compatibility. Integration uses owned workspaces to check passive observations and rendered results. CI covers full managed and Native AOT gates.

Both Release builds passed with zero warnings and errors. Unit passed 10/10, selecting `*ToolTelemetrySettingsTests.*` and `*ToolReceiptTests.KeepsAppliedTelemetry`. Integration passed 18/18, selecting `*ToolTelemetryReadTests.*`, `*ToolReadCommandsTests.ReadOutput`, `*ToolReadCommandsTests.HelpStopsBeforeWorkspace` and the five affected Status/Included methods under Verification. Both runs had zero skips. Evidence is under `artifacts/tool-command/tool-telemetry-*`.

Accepted three new on, off and unavailable human captures. Accepted eight candidate captures for the telemetry text row and JSON field and three help captures for the setting guidance. Accepted 56 Status captures for payload bytes, characters, tokens and ratios only. Structured comparison kept all other JSON values. The initial rendering fixture assumed a blocked report used stdout and tried to overwrite through a creation-only fixture helper. It now captures stderr and creates a separate workspace per scenario. A shared presentation fixture gives every target the same candidate data, so telemetry captures contain no unrelated platform warning. These were deterministic fixture failures, not flakes. Both required format checks, four schemas, site and Index passed. The site kept its existing Node localStorage warning. Writing review R23 corrected saved versus effective wording and passed recheck.

## Request preparation

Lifecycle request preparation now renders declared commands through one template reader shared with manifest validation. Variable names have one code definition. Values are substituted once. Repeated arguments stay separate, and each host gets independent argument and environment collections. Host-scoped commands use ordinal host order. Workspace-scoped commands produce one request. Unknown actions and hosts, missing used values, malformed templates, escaping paths and invalid index directory segments block preparation.

The caller provides the declared lifecycle, resolved executable facts and complete reviewed profile environment. This helper selects no artifact, verifies no store binding and grants no permission. It creates no file or process. Writer planners still need to call it after resolving their facts, before consent. The executor still needs its command-leaf callers.

This increment reuses the BCL regex source generator, path APIs and existing template validation. It adds no dependency or exceptional machinery. Pure Unit cases cover request construction and template admission. CI covers the complete managed and Native AOT gates. The final managed Release Unit build passed with zero warnings and errors. The 19 new `ToolLifecycleProcessFactoryTests` cases passed with zero skips. Reproduce with the Unit build and standard runner flags under Verification, selecting `*ToolLifecycleProcessFactoryTests.*`. Evidence is under `artifacts/tool-command/tool-preparation-*`. No Integration test or snapshot changed. Both required format checks and Index passed. Writing review found no material issue.

The deadline checkpoint leaves these required parts unfinished:

- All five writer bindings, requests, planners, results, renderers and help availability.
- Download, signature and attestation orchestration, store publication and declared-command invocation from Install and Update. Same-pin repair must rerun the installer.
- Configure, Choose and Remove flows, local process consent, destination grants, final confirmation and equal preview decisions.
- Applying telemetry through reviewed profiles, managed ignore planning, removal exclusions, ownership checks and root Remove coexistence.
- Verified passive account-store bindings, resolved Inspect paths and external instructions, shared-reference release from writer flows and private Node with the frozen npm preparation pipeline.

Concrete CodeGraph and Codebase Memory definitions and adapters are excluded from this branch as directed. Tool Exec stays proposed only. No push was made. The original and rebased reporting stashes are kept as described under Resume.

## Superseded questions

The earlier Choose question is answered. Keep the specified checked integration switch and test it with a second synthetic provider. Tool Run is required V1 behavior. General private Node runtime preparation waits for the maintainer, including the Unix launcher-link question. The latest narrowed step covers the rebase, specification and shared source contracts only.

## Final decision update

The final 2026-11-13 direction is recorded in the external-tools design with issue #23 history links, the Tool Run contracts, affected tool contracts, workspace-state decision and admission maintenance. V1 admits CodeGraph only. Codebase Memory stays blocked research. Tool Run replaces the proposed Tool Exec. Team install restores declared pins from the lock. Origin, optional components, declared command rewrites and conditional role lines are required. The help pointer is withdrawn and no pointer or template commit was made.

Backup branch `backup/tool-command-before-comment-links-20261113-1808` keeps the pre-rebase head. Rebase preserved the newer encoded Repair relink and weak-label rules plus Tool backup restoration. Conflicting generated captures and default-layout bytes are regenerated from this tree. The original stash stays untouched. No push was made.

The current implementation still lacks Tool Run, its fast verified dispatch and missing-tool installation offer, origin fields and grouping, generic command rewrites and drift reports, optional-component choices, bare team install and active-role managed-block lines. These are required continuation work, along with the five writers already listed.

## Narrowed qualification

Done: Rebase onto `75f6677ad` is complete. The checkpoint index keeps Comment Links and Tool Command. The Repair prose conflict keeps encoded relinks, weak evidence labels and Tool backup restoration. Generated values were refreshed from fresh output. The default-layout fixture comes from an isolated install with all account stores redirected. It contains 23 files. Status accepts 56 refreshed captures for payload measurements and the added CLI Skill Tool reference. Tool help accepts three captures for the final Run, registration, consent and removal wording.

Now: The specification and contracts record the final decisions with issue history. Runtime command work stays pending. The shared source contract increment follows this documentation checkpoint. No Tool writer or Tool Run binding is added in this step.

Next: Finish the shared model and interface increment, then hand off Dispatch, Registration and Writers as described in the Tool lane contracts. Concrete CodeGraph definitions and adapters follow on their separate branch. Codebase Memory has no V1 definition, Skill or public tool page. Private Node preparation waits for the maintainer. Add no help pointer.

Blocker: None for the narrowed step. Full managed and Native AOT gates belong to CI. No push is authorized.

The fresh Release Integration and EndToEnd builds pass without warnings or errors. Focused Integration verification passes 22 cases, with zero skips. This includes the five former snapshot failures, the unaffected outside and included-file captures, Tool backup Repair and two encoded Repair relink cases. Both published default-layout compatibility cases pass. Evidence is under `artifacts/tool-command/narrow-*`. The writing review closes R24-R33. Both required format checks pass. The documentation site build passes with the existing Node localStorage warning. Index preview has zero effects and findings. Changed relative link destinations resolve.
