---
open-forge:
  description: Read-only native and catalogue compatibility review of lane B's frozen checked candidate
  tags: [Memory, Working, CLI, Tool, Contextual, Evidence, Review]
---

# Config catalogue review

Config catalogue review is a read-only check of B's frozen configuration candidate against the two V1 native recipes.
For example, a missing CodeGraph telemetry file must give the documented on default without creating an identity file.

Status: Review accepted. The coordinator revalidated both P2 findings and routed them together to B. Native journeys and final user docs wait for checked Root composition.

## Findings

The [native output design](../../../../crystallized/documents/cli/technical-designs/tool-setup.md#native-outputs) requires effective data coverage before skipping the conditional CodeGraph ignore block.
Git checks the supplied pathname. A matching child doesn't show directory coverage. Within one ignore file, the last matching rule controls the result and `!` can include a path again. These are documented [Gitignore rules](https://git-scm.com/docs/gitignore#_description) and [check-ignore behavior](https://git-scm.com/docs/git-check-ignore#_description).
The examples below are source traces using those rules. No new Git or upstream execution was used to reproduce them.

### E-B1

P2, correctness. A name-specific child match can skip required index coverage.

Location: `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolEffectiveIgnore.cs:20`.

For a directory condition, `IsIgnoredAsync` checks only the child named by `CoverageProbeName` and treats exit zero as coverage at line 34.
For example, if root `.gitignore` contains only `*coverage-probe`, `.codegraph/open-forge-ignore-coverage-probe` matches while `.codegraph/index-data` doesn't.
`ToolConfigService.EditAsync` at `ToolConfigService.cs:130` then skips the CodeGraph block, leaving index data unignored.
This can also happen with a partial internal ignore that excludes the probe while including other data.

Smallest fix: don't accept a name-specific match on the synthetic child as directory-data coverage.
Keep the supported root-directory rule and native internal `*` cases.
Treat narrower or uncertain coverage as absent so the declared block can be planned.
Extend `ToolEffectiveIgnoreTests` with a probe-only ignore and a partial internal ignore case.

Confidence: high from the supplied path, the branch result and Git's documented pathname behavior.
The existing three ignore cases cover linked targets, a full root-directory rule and a full internal wildcard.
They don't cover these false-positive matches.

### E-B2

P2, correctness. A prepended ignore block can be overridden by existing rules while Apply reports success.

Location: `src/cli/framework/OpenForge.Cli.Framework/Framework/Tools/Shared/ConfigChanges/ToolMarkedBlocks.cs:43`.

When coverage is absent, every new marked block is prepended.
For example, take an existing root `.gitignore` containing `!.codegraph/` and no internal ignore.
The planner correctly sees absent coverage, then prepends the CodeGraph block containing `.codegraph/` before that negation.
The later negation includes the directory again.
`ToolConfigService.Apply.cs:116` verifies the resulting bytes only, then returns a successful addition receipt.
Reapply also keeps that ineffective block because its bytes match the receipt.

Smallest fix: make conditional ignore placement produce effective coverage before returning a successful addition.
Keep unrelated bytes and guarded span removal.
If the existing rules prevent coverage with the supported edit, return a conflict instead of reporting a successful fix.
Add an owned Git case with the existing later negation, including an unterminated final line.
This is separate from E-B1 because it occurs after the probe correctly reports absent coverage.

Confidence: high from the insertion position, byte-only verification and documented rule order.
The current ignore cases don't exercise a later negation.

## Native compatibility

The [record behavior](../../../../crystallized/documents/cli/contracts/shared/tool-records/behavior.md#native-settings) and the [upstream recipe report](upstream-recipes.md#stored-telemetry) define the native setting check.
The following paths fit those rules on the reviewed source:

| Area             | Checked result                                                                                                                                                                                                                                                                                                                                             |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Stored telemetry | Missing file gives on with `Present=false`. An object without `enabled` gives on with the source present. Boolean on requires string `machine_id`. Off permits string or null. Invalid, unreadable and oversized state blocks without command fallback.                                                                                                    |
| Identity         | The parser checks the identity token kind. It never calls `GetString` on that member, compares its content or puts it in observations or facts. Queue paths stay report-only.                                                                                                                                                                              |
| Overrides        | Null stays unchecked. A checked map without an effective override returns an empty map. `DO_NOT_TRACK` wins when nonempty except exact `0` or case-insensitive `false`. Otherwise nonempty `CODEGRAPH_TELEMETRY` chooses off for those false values and on for every other value. Inputs aren't trimmed. Reports contain only the deciding key and on/off. |
| Native vectors   | Writes require account scope, the CodeGraph allow-list ID, desired off and exactly `telemetry off`. Command reads require the installed invocation, declared bounds and exact status vector. The read removes the two inherited telemetry keys and sets the no-download value. Normal writes use an empty overlay.                                         |
| Home binding     | Root provides the per-profile home. Stored reads use that context. Facts match tool ID, change ID and resolved native home. A changed home reads its own previous value and keeps the earlier home's fact. Reapply keeps the first previous on/off value.                                                                                                  |
| C7 Remove        | Native receipt removal runs no native read or restore. Results keep the tool's account facts and manual arguments. The dropped project receipt doesn't turn intentional kept settings into a failure.                                                                                                                                                      |
| Native inventory | The planner uses C's typed native plan reader through the installed invocation. It keeps the complete detected-client list, filters client supplements by that list and validates returned paths against the declared possible set. It implements no client discovery.                                                                                     |
| Client bindings  | Exact file-valued `<id>-config` bindings expand as complete paths. Directory bindings support profile and legacy supplements. Concrete backup targets use host path comparison and SHA-256 or absence. Report-only directories and telemetry paths don't become arbitrary-tree backups.                                                                    |
| Native commands  | Setup, native plan, dry-run and uninstall use their separate authored vectors and invocation prefixes. CBM's default all-client discovery stays enabled. An install inventory isn't used as an uninstall inventory.                                                                                                                                        |

The `NativeSettings` source, `ToolNativeSettingsTests` and the 25-case `ToolNativeEnvironmentTests` match B-R1 and B-R2.
Root still needs to provide child environment and client bindings that match the reviewed native home and path choices.
This review doesn't test production Root composition or either real native journey.

## Receipts and backups

Apply and Remove start from the complete input receipt set.
They change receipts only after verified effects and keep untouched or unstarted receipts after a later failure.
Native facts stay tool-filtered and keep their original previous values through reapply.
Only per-call completed or removed IDs are returned as deltas.

File writes return pending backup handles instead of completing them.
The shared operation-scoped backup instance keeps one original across native and Open Forge edits.
Git coverage requires tracked raw bytes matching HEAD.
Other existing bytes get a sibling backup, and occupied unrelated siblings block planning.
Completion checks the latest target hash and saved backup before discarding only the unchanged operation backup.
The B-R3 failure path returns the kept failed sibling handle as unverified.
D still has to publish each complete receipt set and finish backups only after publication and final verification.

The [fixed digest rules](../../../../crystallized/documents/cli/contracts/shared/tool-records/interface.md#lock) match `ToolProfileDigest` for native and catalogue inputs.
The projection includes native home policy, setup and uninstall vectors, overlays, bounds, plan metadata, output declarations, effects, stored-reader defaults, command-read metadata and ignore conditions.
Authored JSON keys are ordered ordinally, arrays keep their order and templates are hashed before expansion.
Tested versions, machine paths, observed settings and timestamps are excluded.
No further native or catalogue fit gap was found in these receipt, backup and digest paths.

## Evidence

E's defining documents and catalogue were read at `fd2b1e4e12cf25d70d205db1e4158dc22b6bc8a9`.
B's checked candidate is the uncommitted 39-file slice in `D:/Repositories/open-forge-worktrees/tool-setup-b` on `b22e1ec393280935dea93a312f2a1aa337834ad5`.
Source locations above refer to that B worktree.
Its packet, all 24 production files under the four assigned folders and the relevant semantic test cases were read.

The earlier ten-class logs under `artifacts/lane-b/continuation/` show 138 passed, zero failed and zero skipped.
The grouped review logs under `artifacts/lane-b/review/` show the following affected results:

| Log                      | Tests passed | Failed | Skipped |
| ------------------------ | ------------ | ------ | ------- |
| `native-environment.log` | 25           | 0      | 0       |
| `native-settings.log`    | 11           | 0      | 0       |
| `publication.log`        | 10           | 0      | 0       |
| `backups.log`            | 10           | 0      | 0       |
| `config.log`             | 11           | 0      | 0       |

The grouped build log records zero warnings and errors in `00:05:46.37`.
The packet records exit zero for both .NET checks, and their logs contain the required commands without diagnostics.
The Markdown log records a formatting pass.
The 67 affected tests overlap the earlier population and aren't 67 additional unique cases.
These receipts were consumed without rerunning a build or test.

## Source checkpoint

The checkpoint hashes the candidate's 39 changed or new files, including B's packet and test support.
Sort workspace-relative paths ordinally, hash each file's raw bytes with SHA-256, then hash UTF-8 rows of `path`, NUL, lowercase file hash and LF.
The complete manifest hash is `3dfd5b2341268316ecd3a31f944ff6da078a1b5983e004f24abbe231434b56ed`.
The same check before and after the source review matched.

The production subsets use the same row format:

| Folder           | Files | Manifest SHA-256                                                   |
| ---------------- | ----- | ------------------------------------------------------------------ |
| `ConfigChanges`  | 14    | `6ae13bb76afb7c5e0a2def91a7d7503d57fa3acb27e0b70541493daf8ad952e6` |
| `NativeSettings` | 2     | `c39f681b3d4ac0a0e9be1378304f7b87af246a65d749de75405b142dcc287e61` |
| `Backups`        | 4     | `6766fc8216fe7c07c31c7e2a39a5773dbda1b80b3b85e9191281a1252429d2ad` |
| `Filesystem`     | 4     | `c89e10ec75ea1a20362ce4c103c09eff8e33c5452e064011fbb175368891fe00` |

The two primary finding files have these raw hashes:

| File                                   | SHA-256                                                            |
| -------------------------------------- | ------------------------------------------------------------------ |
| `ConfigChanges/ToolEffectiveIgnore.cs` | `8f9a10b1037348023c784de4eb66e9ac9eb276cc9b89d695d6474a2a06769f5b` |
| `ConfigChanges/ToolMarkedBlocks.cs`    | `48d372238c4275d72e441235734c2a866821e83aad06e21c1ea969786ce226c1` |

## Limits

This is the focused native compatibility review requested by the coordinator.
It isn't the final adherence review or a new acceptance gate for already reviewed unrelated work.
Only E's packet and this new report change.
No product, fixture, peer worktree, shared model or parent Task changes were made.
No native tool, real per-user store, PATH, registry, profile, full suite or payload snapshot was used.
The review ran without a commit. The coordinator then authorized a local commit of this report and E's packet after the required checks.
The integrator needs to Index this one new routed report.
