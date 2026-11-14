---
open-forge:
  description: Resume issue 23 tool core implementation, verification and unresolved maintainer decisions
  tags: [Memory, Working, Checkpoint, Active, KeepInMind, CLI, Tool]
---

# Tool command core
The Tool command core is an implementation task that adds the eight external-program commands without concrete production tools.

The selected sources are issue #23 and both comments, the complete external specification and the latest maintainer decisions. This worktree stays on `feature/tool-command`, above updated Comment Links. The overseer handles pushes and PR #26. Earlier checkpoint details and capture lists are kept in Git at `0822e0ed8:.agents/memory/working/checkpoints/tool-command.md`.

Done: Contracts, models, schemas, catalogue selection and the three read commands are committed. Generic process, ignore and backup mechanics are committed. The supply increment is gated and ready to commit.
Now: Finish the verified-download contracts increment, then implement the five writers.
Next: Build stores, extraction, bindings, references, telemetry choice, backup reporting and Repair restoration, then Install, Configure, Choose, Update and Remove with consent and preview parity.
Blocker: Choose integration switching and Unix Node archive links need maintainer decisions. Independent native artifact and writer work can continue.

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

| Commit | Result |
| --- | --- |
| `dee176dc7` | Initial contracts, rebased from the original contracts commit |
| `6b70cdec5` | Models, schemas and exact catalogue selection |
| `b8edc22a9` | List, Inspect and Status |
| `5fa5a0bcf` | Plain-backup contracts replacing the discarded bundle design |
| `87cecdbea` | Sibling backup mechanics |
| `9adb49e08` | Overseer's whitespace formatting on updated Comment Links |
| `de4065cd9` | Tool-owned lifecycle contracts and docs |
| `d8c7ba60b` | Lifecycle vectors and separate execution records, agent parser removed |
| `489e785d6` | Linux clean-checkout fix for three ignored supply model files |
| `0822e0ed8` | Bounded process host, Git backup selection and shared ignore mechanics |

The Linux failure was caused by `Models/Artifacts` matching the Windows artifact ignore rule. All three types now live in tracked `Models/Supply`. No discarded parser package, context or seam survives.

## Verification

CI is the full gate. Local evidence uses Release managed builds, the authorized `NuGetAudit=false` override and no native publication. Integration keeps JSON reflection disabled. The SDK is 10.0.101. Never run Doctor on this worktree. The latest lane update permits only touched or created tests and their affected captures.

Recent gates are:

| Increment | Build | Unit | Integration | Other checks |
| --- | --- | --- | --- | --- |
| Tool-owned contracts | 0 warnings/errors | Unchanged | 66 passed | Both format checks, site and Index passed |
| Lifecycle models | 0 warnings/errors | 139 passed | 52 passed | Both format checks and four schemas passed |
| Linux source fix | 0 warnings/errors | 48 passed | Unchanged | Both format checks passed |
| Generic mechanics | Both builds, 0 warnings/errors | 18 passed | 79 passed | Both format checks passed, no snapshots changed |
| Supply recipes | Both builds, 0 warnings/errors | 7 passed | 8 passed | Both format checks, four schemas, site and Index passed |

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

The pending writer work is:

1. Add per-user stores, safe extraction, bindings, complete shared references and local process consent. Use local hashed test artifacts only.
2. Add the visible telemetry choice and effective Inspect output. Keep private Node, frozen npm closure and official-source checks on their accepted seams.
3. Add Status and Doctor backup reporting and bounded Root Repair restoration before writer effects. Only settings, lock and ignore are Tool backup targets.
4. Add all five typed writer leaves, complete plans, consent, final confirmation, expected-state revalidation and paired control publication.
5. Add managed ignore planning, all removal exclusions, ownership collisions, root Remove coexistence and final-reference release.
6. Replace unavailable store observations with verified bindings and resolve external instructions. Native completion adapters remain on the next branch.

Original stash `60d034fef70b076bcb05a488d668f09fd538ddce` is untouched. Its rebased reporting/restoration copy is `15825724e67c04a66957378d2316925e28c92e85`. Restore selected reporting and restoration files carefully. They still reference the removed member model and old host backup targets. The generic mechanics checkpoint `76a2b372ae9af705bd28eef5174a9a2061a4448c` is already applied and committed. Don't apply it again.

## Not decided yet

- Whether Choose switches agent integrations through declared tool commands or changes role metadata only. A user question is pending. Don't implement the dependent flow before the answer.
- Whether exact reviewed unused Node launcher links can be omitted from Unix archives. The current spec rejects archive links. Upstream packaging creates npm/npx links. A user question is pending. Don't silently relax extraction.
- Proposed `open-forge tool exec <tool> -- <args>` remains specification only.
