---
open-forge:
  description: Capture and commit the current output of every command at both current views and both formats before any G4 behavior change
  tags: [Memory, CLI, Task, Plan, G4, Contextual, Archived, Historical]
---

# 01 — Before snapshots

> Read [00 — G4 conventions](00-conventions.md) first.

> Historical baseline record: the snapshots captured the pre-G4 presentation
> vocabulary. The merged CLI now uses one schema-3 report with `--format`,
> `--detail`, and `--detail-filter`; the older view and format terms below
> describe what was true at capture time.

## Goal

The current output of every command, in every reachable status the seeded
workspaces can produce, at both current views and in both formats, is
committed as reviewed snapshot files in one commit that changes no production
code. Every later G4 diff is reviewed against these files.

## Depends on / Blocks

- Depends on: nothing.
- Blocks: 03, 05, every command subtask.

## References

- [Phase 7 scenarios](../../../../working/cli-development/tasks/task30/phase-7-scenarios.md) for the AOT-safe snapshot
  boundary and its normalization rules.
- [Model-level snapshot testing](../../../../emerging/analysis/cli-experience-audit/model-level-snapshot-testing.md)
  for the tool shape: a string comparison anchored at `[CallerFilePath]`, an
  environment-variable update switch, no reflection.
- Existing tool: the Imprint package wired in `911470d2` and the
  `__snapshots__` folders under `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/`.
  Reuse it if it satisfies the AOT and update requirements; otherwise finish
  the in-house tool described in the analysis.
- The fable capture script in the proposal is design evidence only. It ran a
  managed build of the tree in a scratch workspace, not the test boundary.
- Seeded states per command are listed in each command subtask under
  "Situations". Use those names.

## Preconditions

- [x] Three suites green at the starting commit.
- [x] `artifacts/publish/open-forge-dev/Release` rebuilt from the same commit
      (`dotnet build src/cli/root/OpenForge.Cli/OpenForge.Cli.csproj -c Release --no-restore`).

## Steps

1. [x] Add or finish the in-process snapshot helper in TestSupport:
       `MatchSnapshot(actual, [CallerFilePath], [CallerMemberName], name)`,
       ordinal comparison, `OPENFORGE_SNAPSHOT_UPDATE=1` overwrites. Normalize
       only the workspace absolute path, the version string, SHA-256 hashes and
       line endings, each replaced by a named placeholder. Verify: unit green;
       a deliberately changed expected file fails with a readable diff.
2. [x] Add one integration test class per command under the mirrored path that
       seeds each situation named in the command subtask, runs the real
       operation in-process, renders text at `compact` and `expanded` and JSON
       at both views, and matches four snapshot files. Doctor uses the unit
       fixtures only; it is never run against this repository. Verify:
       integration green.
3. [x] Run once with the update switch, commit the generated files with the
       tests, and nothing else. Commit message: `Captured command output before the G4 rendering revamp`.
4. [x] Record the count of snapshot files per command in this file.

## Expected result

One commit adding tests and `__snapshots__` files. `git diff --stat` for that
commit touches only `src/cli/tests/`.

## Acceptance

- [x] Every command has at least the situations named in its subtask.
- [x] The three suites are green and the counts are recorded.
- [x] No production file changed.

Coverage acceptance applies to the 340 currently reachable situations, including the disclosed current-refusal and staged-cancellation boundaries. The sole named exception is Library Attach `links-unsupported`, whose finding has no current emitting path; its required future implementation remains in task 35. No snapshot claims that unreachable behavior exists.

## Changes ledger

- Test support: existing renderer fragments only -> explicit-update, caller-anchored command output snapshots reuse the pinned Imprint 1.0.0 package. No existing tests or expectations are removed.

### Execution applicability and baseline

- This slice changes only test support, fixtures, reviewed output files, and this record. Production behavior and authored catalogues remain unchanged. Fixtures own their temporary files and lock stores; Git and explicit snapshot regeneration provide recovery.
- The pinned Imprint 1.0.0 package supplies an AOT-compatible string capture, explicit identities, exact comparison options, and readable failure differences. The existing package reference is reused in TestSupport. Exceptional machinery: none.
- Decisive evidence: unit normalization checks, integration snapshot comparison and deliberate mismatch checks, command-local semantic/effect assertions, and all three managed suites. Native qualification follows at the required G4 03 and 40 gates.
- Baseline commit: `5f56199c4e42213938fc0591a605847f9c1de8c9`; tree: `8efe70c460f5c164d531ba69fcdeaaf458c70b9f`; SDK: `10.0.101`; configuration: Release.
- Baseline preparation: `dotnet restore OpenForge.Cli.slnx --source artifacts/delivery/offline-feed -p:NuGetAudit=false`, then `dotnet build OpenForge.Cli.slnx -c Release --no-restore`. Build: 0 warnings, 0 errors; matching development publication rebuilt.
- Each executable ran with `--parallel none --no-ansi --progress off --minimum-expected-tests 1`: Unit 3,366 passed, 0 skipped; Integration 1,849 passed, 17 expected platform skips; EndToEnd 131 passed, 0 skipped. All failed counts were zero. `npm run check:dotnet` reported the five documented pre-existing whitespace errors.
- Baseline assembly SHA-256: Unit `656D70221AC37D784450E8194B50797FF1A6D73B90F03C889B3107A8949AFEE3`; Integration `CEEE29909A4545C87B3C0620B3BDC0E97CDEC64AC317DBD11A4B323CC0525B83`; EndToEnd `3EA40D34D42FF20F2D9829CBF80231D15DB7B50DE8F554CB1A6699D28EC1B64D`.
- Development publication SHA-256: root assembly `64A4C14337DC3840A6D0F0713C87DE1B1A0930181E67609ED6598D8EE723ADAD`; Core `38DCE9CD860A04DE21D871FC7B2F1269568B60AA04C7AAD8AC338AA7EAE26BCA`.
- Per user request, root-owned `/root/g4_shell_runner` uses the default agent role with `gpt-5.6-luna` at `max` for exact mechanical build and test commands only. The implementer selects commands, corrects fixture semantics, and accepts evidence.
- The snapshot-only staging receipt contains 1,409 files: 1,360 snapshots, 48 C# files, and the TestSupport project file. No production files or task records are staged. Long snapshot identities require per-command `git -c core.longpaths=true`; no global Git setting is changed.
- The staged source-only whitespace check passes. Checking the complete snapshot diff reports 54 whitespace diagnostics from faithfully captured trailing spaces, blank lines, and unified-diff rows in current CLI output. These are intentional byte-exact evidence, not source-formatting defects; formatters must not trim or rewrite snapshot bytes.
- Root applied `dotnet format whitespace --include` only to the staged test C# paths. The inspected changes split object-initializer members onto separate lines without changing values, requests, assertions, or strings. Snapshot files were excluded. The pre-existing five formatter diagnostics remain at `ReferencesOperation.cs` lines 459/460 and `ExtensionListApplicationIntegrationTests.cs` lines 83/84/85.

### Final verification

- Root committed the tests-only snapshot boundary as `5c388e7c5747a8a229591807f1b3693e4dd7b14e`, tree `878b646ed748662f9be7cd0bcb4da7dd0f5f3441`, with message `Captured command output before the G4 rendering revamp`: 1,409 files, 80,094 insertions, two deletions, all under `src/cli/tests/`. This task's evidence record is intentionally outside that snapshot-only commit.
- `dotnet build OpenForge.Cli.slnx -c Release --no-restore`: passed with zero warnings and errors; receipt `artifacts/g4-01-rebuild.log`.
- Ordinary `artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1`: 3,382 passed, zero failed/skipped; receipt `artifacts/g4-01-unit.log`. After the terminal-period helper correction, all five focused normalization tests passed again; receipt `artifacts/g4-01-normalization.log`.
- Ordinary `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1`: 2,197 total, 2,180 passed, zero failed, 17 expected platform skips; receipt `artifacts/g4-01-integration-final.log`. This verifies the final corrected baselines with snapshot updates disabled.
- Ordinary `artifacts/bin/OpenForge.Cli.EndToEndTests/release/OpenForge.Cli.EndToEndTests.exe --parallel none --no-ansi --progress off --minimum-expected-tests 1`: 131 passed, zero failed/skipped; receipt `artifacts/g4-01-e2e-final.log`.
- The final formatter verification returns exit 2 with exactly the five pre-existing diagnostics identified above, and no new diagnostics; receipt `artifacts/g4-01-format-final.log`. Snapshot whitespace remains deliberately exact.

### Named-situation coverage and snapshot inventory

The new command captures contain 340 named situations and 1,360 snapshot files. Each situation has compact and expanded Human output and both requested JSON views. Names map directly to the `Situations` lists in tasks 10–37 and to the new caller-adjacent snapshot filenames. Current parser-level JSON requests that still produce shell text are explicitly identified below. Existing snapshots and helper-test expected files are excluded from these counts.

| Command task | Captured situations | New snapshot files |
| --- | ---: | ---: |
| 10 Status | 12 | 48 |
| 11 Doctor | 11 | 44 |
| 12 Install | 12 | 48 |
| 13 Update | 11 | 44 |
| 14 Index | 11 | 44 |
| 15 Repair | 13 | 52 |
| 16 Cleanup | 9 | 36 |
| 17 Context | 14 | 56 |
| 18 Find | 12 | 48 |
| 19 References | 11 | 44 |
| 20 Route List | 11 | 44 |
| 21 Route Inspect | 13 | 52 |
| 22 Route Init | 11 | 44 |
| 23 Route Create | 14 | 56 |
| 24 Route Update | 13 | 52 |
| 25 Route Move | 14 | 56 |
| 26 Route Remove | 12 | 48 |
| 27 Extension List | 11 | 44 |
| 28 Extension Inspect | 10 | 40 |
| 29 Extension Create | 11 | 44 |
| 30 Extension Install | 18 | 72 |
| 31 Extension Update | 15 | 60 |
| 32 Extension Remove | 13 | 52 |
| 33 Library List | 10 | 40 |
| 34 Library Inspect | 11 | 44 |
| 35 Library Attach | 12 | 48 |
| 36 Library Sync | 14 | 56 |
| 37 Library Detach | 11 | 44 |
| **Total** | **340** | **1,360** |

The only uncaptured authored situation is Library Attach `links-unsupported`, whose current absence of an emitting production path is documented below. All other named situations have four files, including the explicitly disclosed current-refusal prompt cases and the two deterministic staged-cancellation boundaries. The final inventory compares only staged new snapshot filenames with the authored situation names; Status's prose references to future view names are not situations.

- wave 1: the JSON snapshot pretty-printer wrote `Environment.NewLine` -> it writes `
`. `CommandOutputSnapshotFormatting.PrettyPrintJson` built its `Utf8JsonWriter` with `Indented = true` and no `NewLine`, and that property defaults to `Environment.NewLine`, so every `*.json.*` capture was produced with CRLF on Windows. `.gitattributes` declares `* text=auto eol=lf`, so the committed bytes are LF, and `CommandOutputSnapshot` compares with `IgnoreLineEndings = false`. The three could not hold at once: on any fresh checkout every JSON capture failed. Adding `NewLine = "
"` to the writer options keeps a capture identical on every platform.

- wave 1: snapshot comparison policy was declared twice with `IgnoreLineEndings = false` -> one
  `CommandOutputSnapshot.OutputComparison` with `IgnoreLineEndings = true`. `MatchSnapshot` and
  `MatchDetailSnapshot` each built their own `SnapshotComparison`, which is two answers to one
  structural question. They are now one value, and it ignores line endings, so a capture taken on
  any platform agrees with the bytes `.gitattributes` `eol=lf` checks out. Content, case and
  trailing whitespace stay exact: a one-character content edit to a committed snapshot still
  fails, and a whole-file CRLF conversion now passes. Both were verified deliberately.

## Divergences observed

- The first complete ordinary Integration run exposed six successful Update JSON snapshots whose `next.reason` ended the exact retained recovery path with a period. All other JSON properties matched. The normalizer now recognizes a sentence-final period only when followed by end of text, whitespace, or a closing quote; it preserves neighboring `.other` filenames. The existing independent recovery-path oracle covers plain and JSON spellings, terminal punctuation, and unrelated identities. This corrects the already authorized exact-path normalization rather than suppressing a new volatile field.
- The exact 26-case Update/Extension Update recapture changed 18 files: twelve JSON views changed only `/next/reason`, and six expanded Human views changed only the identical sentence's exact recovery path. The first ordinary run stopped each failing case at compact JSON before reaching expanded Human; it did not establish that later view had passed. Root inspected all 18 differences and confirmed no other changes.
- Imprint stores the required situation filenames beneath its caller/test identity directories rather than directly beneath the command directory. The filenames remain `<situation>.<view>.txt` and `<situation>.json.<view>.txt`; no production meaning changes.
- The narrower normalization list omitted retained recovery bundles. `UpdateApplicationOperation` creates a fresh operation GUID and `RecoveryBundleStore` places its bundle under current-user LocalApplicationData. The Phase 7 named-platform-fields boundary permits replacing only the exact result-returned recovery path, including its JSON-encoded spelling, with `<recovery-bundle>`. Tests assert the returned path exists and has the retained disposition before replacing it. Arbitrary GUIDs and other path-shaped text remain exact. Documentation propagation must include this named normalization field.
- Current Update with a retained retired file returns `Attention`; the proposed catalogue uses the future status spelling. Before snapshots retain the observed current result and make no status correction.
- Current Context section selection also applies to retained startup layers. A requested section present on the selected guide still returns `Attention` when startup layers lack that section. The before snapshot preserves these findings separately from the wholly missing-section case.
- Current `RouteListDefinitions` requires `Incomplete` for missing metadata and malformed Loader findings; its named situations retain exit 3 before G4 instead of the catalogue's proposed warning/blocked outcomes.
- Extension sources must live outside the workspace: the production source reader rejects overlap. The exact separately owned fixture source root and its JSON spelling become `<extension-source>` under the same Phase 7 named-platform-fields boundary. Every such read capture asserts that source bytes are unchanged; arbitrary external paths remain exact.
- Current Extension Inspect reports an unknown stable ID as package-unavailable (`Incomplete`) and a missing explicit source with retained installed facts as `Attention`. A newer available version with unchanged content remains `Complete`: current findings derive content relations rather than promoting version difference alone. Snapshots retain those existing outcomes.
- Current Library List treats malformed and unreadable ownership as informational `Complete` observations, and a missing registered source folder as `Incomplete`; these differ from proposed catalogue severities and remain unchanged in before snapshots.
- Cleanup seeds fixed operation GUIDs and replaces only each exact returned candidate path with its distinct `<recovery-bundle-N>` identity. Tests assert initial existence and final removal or preserved bytes. Its unavailable-store seed replaces only the exact returned owned bucket path with `<recovery-store>` and asserts its collision-file bytes remain intact.
- Current Route Move classifies moving into itself or its own descendants as `Blocked`. Route Remove treats an absent source as a `Complete` no-op. Before snapshots preserve those outcomes rather than introducing the proposed invalid-input classification.
- Extension Remove's current single-answer selection prompt is repeated during application revalidation; end-of-input on that second question blocks without effects. Its `select-prompt` before snapshot records this defect. Extension Update has no selection prompt: its planner refuses multiple packages without IDs or `--all` even when interaction is allowed. Its separately named attempted-interactive-selection test retains the catalogue situation `select-prompt` while asserting `Invalid`, `SelectionRequired`, empty prompt output, and unchanged bytes. This records current refusal for prompt-capable input; successful selection is future 04/31 behavior.

- Current Library attach, sync, and detach classify an unavailable workspace lock as `Failed` through their preflight unexpected-failure evidence. Successful Library application exposes its recovery path with `Removed`, unlike the retained-only paths in other families. Exact returned recovery paths are normalized only after asserting owned-bucket containment and existence matching the typed `Removed`/`Retained` state.
- Current Library Sync recreates a missing registered link when the complete source still supplies it (`Complete`); Detach refuses a missing link (`Blocked`). The snapshots assert those distinct real effects.
- Current Find, Context, and References classify leaf/category logical-ID ambiguity as `Blocked`. Context frontmatter selection silently omits the host document's absent frontmatter and remains `Complete`. The before snapshots preserve these existing outcomes.
- The real Windows partial-write fixtures keep a read-sharing handle on one exact later target. Planning, recovery preparation, and revalidation read its bytes normally; replacement or deletion is denied deterministically after an earlier real effect. Tests separately assert the earlier effect, unchanged denied target, and actual recovery evidence. Other platforms skip these specifically Windows sharing cases instead of manufacturing an error.
- Install now covers all twelve named situations with 48 snapshots. Its force-eligible occupied generated entrypoint uses the existing embedded payload fixture. The partial-write case verifies an earlier Loader effect, unchanged held Memory target, no ownership publication, and a retained bundle. The exact owned recovery-bucket collision returns current `Incomplete`; it renders no external store path. The successful forced replacement removes its bundle and leaves zero recovery candidates.
- Current Library partial application can expose recovery state `Prepared` when a bundle exists and cleanup was not attempted (`LibraryMutationCompletionProjection.Recovery`). These captures preserve that state and assert the exact owned returned bundle exists, alongside `Removed`/`Retained` checks in other situations.
- Route Inspect performs an actual source-selection prompt. Route Update, Move, and Remove have no interactive selector in their current operation factories. Their explicitly named attempted-interactive tests supply prompt-capable CLI input and retain the authored ambiguity-prompt situation names, but assert current refusal, exact ambiguity diagnostic IDs, no prompt output, and unchanged bytes. Working selection for these commands remains G4 04 behavior.
- Stream evidence is independently fixed by the requested format and each test's expected exit: Human 0/2/3 uses stdout; Human 1/4/5/130 uses stderr; typed JSON uses stdout. Eight current parser-level invalid-input situations instead emit plain shell diagnostics to stderr even when JSON was requested; those cases explicitly declare this boundary. The helper asserts the expected completion target, populated expected stream, empty opposite stream, and empty separate prompt output.
- Cleanup's `cancelled-partial` capture uses a disclosed staged integration boundary: real catalogue and dry-run preflight, actual workspace lease and deletion session, one actual successful deletion followed by token cancellation and an actual cancelled retained deletion. Production `CleanupDeletionProgress` and `CleanupResultBuilder` form effects, findings, status, next action, and residuals. The fixture maps independently asserted lease, revalidation, and verification outcomes to their existing boundary views. It does not claim to deterministically time cancellation inside the top-level application loop.
- Library Attach's `interrupted-partial` capture also discloses its staged integration boundary: actual production observations, planning, permissions, recovery preparation, and real link application are followed by an actual record write receiving a cancelled token. The real receipts and source-effect facts feed production completion. The test verifies the link remains, the record was not published, source bytes remain intact, and recovery evidence exists. It does not claim cancellation was timed inside the top-level command loop.
- The current Library Attach partial-cancellation renderer labels record publication `failed` even though its actual record receipt was cancelled before starting; the overall status remains `Interrupted` and recovery is `Prepared`. Extension Create's actual partial failure reports two intended effects and one applied manifest effect. Cleanup's cancellation reports the first deletion verified and both later candidates not started and retained. These current strings and distinctions are preserved.
- Cleanup's JSON lease `operationId` derives from the selected workspace identity and changes with each owned temporary workspace. The fixture asserts every candidate's typed lease ID equals `CleanupPlanner.LeaseOperationId(request)` before replacing only that exact lowercase-N JSON member value with `<cleanup-lease-id>`. Unquoted IDs, other members, longer hexadecimal values, uppercase variants, and fixed candidate provenance IDs remain exact. An independent literal Unit oracle verifies this named-field normalization.
- Library Attach's `LinkCapabilityUnavailable` finding is declared and mapped but has no emitting site in Attach or shared Library planning/completion. The separate `LibraryOperationalContributor.ReadLinkCapability` fact reports supported links on Windows, Linux, and macOS, but Attach does not consume it. Therefore `links-unsupported` is currently unreachable through the real Attach command, regardless of host; G4 35 must connect the requested capability refusal. No unsupported result, misleading conditional test, or machine-wide privilege change is manufactured.
- Source-unreadable Library fixtures deny only directory enumeration for the current Windows identity on one exact owned source directory. The support scope validates containment, proves enumeration is denied, restores the original access descriptor in `Dispose`, and verifies exact restoration before byte comparisons and fixture cleanup. The Extension Create unreadable case first creates its exact scaffold, then denies enumeration of that owned package directory: its explicit catalogue root alone need not be enumerated, so root enumeration denial would not establish a command failure.
- Extension Create's partial-write seed applies a current-user `CreateDirectories` denial inherited only by the first generation of newly created catalogue directories. The catalogue itself remains usable, allowing the actual operation to create its package and manifest before payload directory creation fails. The root access descriptor is restored and verified; only the exact created scaffold paths are cleaned. No global identity or permission policy changes are made.
- Repair's Library recovery case uses an actual verified residual bundle and actual automatic inverse application. It returns current `Complete`, preserves the original evidence bundle and source bytes, and reports the actual Library receipt. Original and forward bundle paths retain distinct normalization identities; a forward preparation is required to have been removed before its path is normalized.

- wave 1: the plan and the Task 30 G4 records described 24 integration failures as stale before-baselines "captured before the native output for those commands merged", and scheduled a snapshot refresh to repair them. That premise was wrong. Regenerating all five affected classes with `OPENFORGE_SNAPSHOT_UPDATE=1` changed 108 files, and every one was identical to the committed bytes once line endings were normalised: `git diff --name-only` reported zero changes while `git status` reported 108, because Git normalises through `eol=lf` and the harness does not. There was no stale content anywhere in Install, Library list, Extension list, Context or the composed Index capture. The refresh could not have worked either: committing regenerated CRLF bytes stores LF again through `eol=lf`, restoring the failure on the next checkout. Fixed at the cause in the writer instead; no snapshot file was changed.

## Rollback

Revert the single commit.
