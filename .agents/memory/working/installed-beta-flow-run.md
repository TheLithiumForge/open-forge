---
open-forge:
  description: Installed beta user-flow verification on Windows and WSL, with platform limits and reproduction commands
  tags: [Memory, Working, CLI, RunRecord, Evidence, Contextual]
---

# Installed beta flow run

## Result

All 26 flow families were exercised successfully against the installed beta on
both platforms. Windows passed all 82 existing journey cases. WSL passed 66
existing journey cases and four manual Linux equivalents for the F19/F20 flows
whose automated fixtures are entirely Windows-specific. The other 12 skipped
Windows-specific branches remain platform exclusions, not Linux passes.

The broader public suite established 247 passing Windows cases after the
documented fixture correction, and 231 passing WSL cases with 16 approved
platform exclusions. No CLI defect or regression was found. Production source,
automated assertions and platform skip rules were unchanged.

## Scope

The maintainer requested parallel execution of all user flows against the
installed CLI on Windows and WSL on 2026-09-24. This run selects all 26
[flow families](../crystallized/documents/cli/experience/flows/_flows.md), their
82 existing journey cases, and the remaining public-process tests. It does not
claim to execute every scenario in the broader 438-case source collection.

Both installed commands were updated to the published `0.9.0-beta.1` package.
Windows had stale WSL-created development-package links that its Node process
could not follow. Those exact links were preserved with `.pre-beta-link`
suffixes before installing the public beta. WSL had an older development
version. These were local installation problems, not beta test failures.

## Artifact identity and isolation

The managed EndToEnd runner was freshly built at
`33125be678905ae94e89eb718e75bddb5aea5060`, using .NET SDK `10.0.101`.
The `src/cli` tree is `424005fd31374b22953c366320e2fe4aff46bedf`, identical to
the released `v0.9.0-beta.1` source. Build succeeded with no warnings or errors.
The runner DLL SHA-256 is
`ddece73a378e5ab87847676dcd77103b1b092d05dc4e3aa376b150f635bb313f`.

The runner and repository solution marker were copied to an owned execution
root for each platform. Its normal publication-discovery layout selects the
installed npm native executable, with a version marker obtained from that
executable. Tests and assertions were not edited. The initial Windows run used
a symbolic link to the installed executable. Relocation tests require an
ordinary file, so their focused rerun used a byte-identical copy. WSL uses an
ordinary copy from its installed package. The npm command launcher is checked
separately. No locally rebuilt product executable supplies this evidence.

Native SHA-256 identities:

| Platform | Installed native executable |
| --- | --- |
| Windows x64 | `cf0ddf7b05ad3bda5facb2a0a9d51c41621559d5df057e8f15d2a0f405a8d39b` |
| WSL Linux x64 | `bab1e7180b062ee9c946d83900181a1655c0850100ed157788e8cf081d0eb14f` |

Each test owns its workspace and redirects product stores through the existing
test support. Collections run in parallel. All manual CLI calls also use owned
workspaces and `OPENFORGE_DATA_HOME`. Doctor is never run on the real repository.

## Windows results

All 82 journey cases passed across F01–F26, with no skips. Both supplemental
Root Remove recovery journeys passed. In the full 247-case public suite, 245
passed initially and two relocation fixtures failed before invoking the CLI
because their setup rejects symbolic links. After replacing only the test
publication link with a verified copy, both relocation cases passed. Effective
coverage is 247 passed, zero unresolved failures and zero skips.

The globally installed `open-forge` command also passed version reporting,
Framework installation, Core Templates installation, Status and Doctor. Doctor
reported no problems across six checks, 19 links and 19 routes.

Raw evidence is under `artifacts/installed-beta-flows/windows/`: `run.log`,
`results/results.json`, `relocation-run.log`, `relocation-results/results.json`,
`launcher-smoke.log`, and the explicitly combined `summary.json`. Original
fixture failures remain visible in the first report.

## WSL results

The canonical suite passed with 247 cases: 231 passed, 16 Windows-specific
skips, and no failed tests. It ran in 14.889 seconds. The repository's
`qualifyReport(report, "linux")` independently accepted every case and skip.
WSL used .NET SDK `10.0.112` and .NET runtime `10.0.12`.

The first suite's minimum-execution guard was incorrectly set to
240, above Linux's expected 231 executed cases. The canonical rerun uses 231
without changing test selection or expectations. The original result remains
separate from that rerun.

All three supplemental Linux F20 checks passed using the installed npm launcher:
a live OS lock blocks A without changing its workspace, B remains independent,
release permits retry, a killed holder releases ownership without deleting the
persistent lock file, and retry includes input authored after contention.
The fixture observes lock files created by installation, then uses `fcntl.flock`
to hold the actual OS lock. It does not infer ownership merely from a pathname.

The supplemental Linux F19 flow also passed. Making only Beta's entrypoint
directory mode `0555` caused a real partial index failure after Alpha changed.
Beta, authored children, settings and ownership stayed byte-identical, and a
real recovery archive remained. Doctor reported stale generated content and
recovery evidence. Restoring permissions and Alpha's prior bytes allowed a
successful retry. Cleanup preview left the workspace and recovery bytes intact.

The canonical runner exited zero. The globally installed Volta command passed
version reporting, Framework installation, Status and Doctor. A subsequent
Core Templates installation and Doctor check also passed.

The canonical Linux fixtures ran on WSL's native filesystem. An earlier F20
attempt also passed on the mounted Windows drive but is not the native-filesystem
receipt. Both are kept distinct. Raw WSL reports and manual command captures are
under `artifacts/installed-beta-flows/wsl/native-linux/`. `receipt.json` records
the exact command, zero exit, versions, hashes, class counts and all skip reasons.
`results-linux.json` and `run-linux.stdout.log` are the canonical suite report
and log. The two manual summaries and command logs retain their original native
paths. The earlier mounted-drive attempt is explicitly superseded.

### Journey coverage

The Windows column contains automated passes. The WSL column contains automated
passes plus Windows-only exclusions. Manual Linux checks supplement F19 and F20
without changing the automated counts or marking skipped tests as passed.

| Flow | Windows passed | WSL passed / excluded | Additional Linux check |
| --- | ---: | ---: | --- |
| F01 | 4 | 4 / 0 | |
| F02 | 2 | 2 / 0 | |
| F03 | 4 | 3 / 1 | |
| F04 | 3 | 3 / 0 | |
| F05 | 4 | 4 / 0 | |
| F06 | 2 | 2 / 0 | |
| F07 | 4 | 3 / 1 | |
| F08 | 4 | 4 / 0 | |
| F09 | 4 | 3 / 1 | |
| F10 | 2 | 2 / 0 | |
| F11 | 5 | 5 / 0 | |
| F12 | 3 | 2 / 1 | |
| F13 | 4 | 3 / 1 | |
| F14 | 7 | 5 / 2 | |
| F15 | 5 | 4 / 1 | |
| F16 | 3 | 2 / 1 | |
| F17 | 3 | 3 / 0 | |
| F18 | 3 | 3 / 0 | |
| F19 | 1 | 0 / 1 | Permission fault, diagnosis, continuation and cleanup preview passed |
| F20 | 3 | 0 / 3 | Live lock, crashed holder and changed-input retries passed |
| F21 | 2 | 1 / 1 | |
| F22 | 2 | 2 / 0 | |
| F23 | 4 | 2 / 2 | |
| F24 | 2 | 2 / 0 | |
| F25 | 1 | 1 / 0 | |
| F26 | 1 | 1 / 0 | |

## Reproduction

Install the beta with `npm install -g @thelithiumforge/open-forge@beta
--ignore-scripts --no-audit --no-fund` on each platform. With Volta, use its
package-install command when updating a Volta-managed global command.

Build the independently runnable EndToEnd project once from the recorded source:

```sh
dotnet build src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj --configuration Release --no-restore --nologo -v quiet
```

Copy the complete `artifacts/bin/OpenForge.Cli.EndToEndTests/release/` output
to `<owned-root>/runner/`, and copy `OpenForge.Cli.slnx` to `<owned-root>/`.
Copy the installed platform package's `bin/open-forge[.exe]` to
`<owned-root>/artifacts/publish/open-forge-dev/Release/open-forge-dev[.exe]`.
Verify the source and destination SHA-256 values match. Write its actual
`--version` result to the adjacent `open-forge-dev.version` file. The expected
version compiled into this runner is `0.9.0-beta.1`.

Run the complete public suite in that isolated layout:

```sh
dotnet <owned-root>/runner/OpenForge.Cli.EndToEndTests.dll --parallel collections --minimum-expected-tests 240 --fail-warns on --fail-skips off --no-ansi --progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json --results-directory <owned-root>/results
```

The Windows fixture correction reran the same command with
`--filter-class '*PublishedEmbeddedPayloadProcessTests'`, a minimum count of
two, and a separate `relocation-results` directory. No other selection or
expectation changed.

On Linux, replace the minimum count with `231`, the known executed count after
the 16 platform exclusions. Independently verify all 247 case records and each
skip reason with `qualifyReport` from `scripts/delivery/test-report.ts`.

### Linux lock fixture

Use the installed npm launcher in owned A/B workspaces with one shared, isolated
`OPENFORGE_DATA_HOME`. Install A and B separately and observe the new lock file
after each install. Add distinct Markdown notes with a description and tags
under each workspace's `.agents/guidance/`. Hold A's observed zero-byte lock
with a child Python process using this exact ownership primitive:

```python
import fcntl, sys
f = open(sys.argv[1], "r+b")
fcntl.flock(f, fcntl.LOCK_EX)
print("LOCKED", flush=True)
sys.stdin.readline()
fcntl.flock(f, fcntl.LOCK_UN)
print("RELEASED", flush=True)
```

Wait for `LOCKED` and assert the holder is alive. Run `index` in A: expect exit
5, empty stdout, a human error on stderr, and unchanged file hashes. Run
`index` in B: expect exit zero and B's note in its guidance Entries while A
stays unchanged. Send a line to the holder and wait for successful exit. Retry
A and verify its note appears while both zero-byte lock files remain.

Repeat with a fresh A/B pair and kill the holder after the blocked command.
Verify the persistent file remains and A's retry succeeds. With another fresh
pair, author a second A note after the blocked command, release the holder,
and verify retry indexes that new note. These are the same three F20 outcomes
checked by the Windows journey class, with Linux OS locking in place of its
Windows-only holder fixture.

### Linux partial-operation fixture

Use a fresh owned workspace, isolated `OPENFORGE_DATA_HOME`, and a non-root
Linux user. Install the Framework, then seed Alpha and Beta guidance folders
as in the tracked
[F19 journey fixture](../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Journeys/F19PartialOperationJourneyTests.cs):
each folder has its named entrypoint and a described/tagged `seed.md`. Set the
workspace settings to `{"allowInstallPaths":[]}`. Run `index` on the two
entrypoints, then add described/tagged `new.md` children to both. Capture the
workspace file hashes, settings, ownership bytes and current recovery inventory.

Run `chmod 0555 .agents/guidance/beta`, then index the Alpha and Beta
entrypoints in that order. Require exit 1, empty stdout, a human partial-failure
error, changed Alpha Entries, unchanged Beta, unchanged authored files/settings/
ownership, and a newly retained recovery ZIP. In this run the report said it
stopped after one of three files. Inspect the archive's manifest and ensure its
prior payloads exist. The retained ZIP's SHA-256 for this execution was
`8c4a951a900f4825b6eee3d774da7addfea6440e29c66c85bedf7dc595a5384a`.

Restore Beta's directory to `0755` and its entrypoint to `0644`. Run human and
full JSON Doctor on the same resulting workspace. Require exit 2, stale
generated-region and recovery findings, and no workspace/recovery changes.
Restore only Alpha's recorded original bytes, then repeat indexing for both
entrypoints. Require exit zero, both new children in their Entries, unchanged
authored files/settings/ownership, and the same retained ZIP hash. Finally run
`cleanup --dry-run --format=json`. Require the bundle to be listed as
`would-be-removed` while both workspace and recovery inventories remain unchanged.
