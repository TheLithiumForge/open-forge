---
open-forge:
  description: Streamline hosted build and release delivery while preserving required platform coverage and making stalled suites diagnosable
  tags: [Memory, Working, Task, CLI, Pipeline, Build, Release, Contextual, Active]
---

# Task 71: Streamline build and release delivery

## Outcome

The maintainer requested this follow-up during beta5 publication on 2026-10-02.
Review the Windows-only and other-platform steps, explain which differences
are necessary, reduce avoidable repeated work, and make unusually slow or
stalled jobs visible and recoverable. Recording this task does not begin its
implementation.

## Evidence

Build `36995867259` on commit `634ab07051e670e3f9bcc1b6603ac39a91f4febc`
completed five platform jobs successfully. Its original Linux ARM64 job
`110803462713` exceeded the configured one-hour limit and was cancelled by
GitHub. Native compilation took about 13 minutes. The test step did not
complete after more than 50 minutes. No failed assertion was reported, and
the completed-job log download returned `BlobNotFound`. The exact cause is
unconfirmed. Only this host was retried, with diagnostic logging enabled.
The retry passed all suites and package qualification without source changes.

The prior four successful Linux ARM64 jobs took 16.1 to 18.0 minutes overall
and 4.1 to 4.4 minutes for their test steps. Comparison runs are
`36816172673`, `36813142591`, `36809625271`, and `36656845585`.
The other beta5 hosts completed in 18.9 to 39.6 minutes.

The source currently explains the Windows split: Git Bash can enable backup
and restore privileges, bypassing ACL denials used by filesystem tests.
Windows test and package steps therefore use PowerShell and preserve the
native exit code. This reason must be evaluated before consolidating shells.

`scripts/delivery/test-suites.ts` disables progress and redirects each suite's
output to an execution log. `scripts/delivery/process.ts` waits synchronously
without a per-suite timeout. These choices limit live diagnosis. They do not
establish the cause of the ARM64 stall.

Detailed comparison and recovery receipts are preserved in worktree
`a_6ecaa48917f3`, under `artifacts/root-beta5-qualification/`:
`hosted-duration-comparison.json`, `linux-arm64-slow-job.json`, and
`arm64-recovery.json`.

## Plan

1. Map each required check to the behavior and platform it proves, including
   managed, native, package, site and public upgrade evidence.
2. Compare stage and suite timings across recent successful and failed runs.
   Investigate the ARM64 stall if usable diagnostics become available.
3. Propose simpler shared orchestration with only necessary platform-specific
   behavior. Preserve exit codes, ACL evidence and exact artifact provenance.
4. Evaluate suite deadlines, periodic progress, process diagnostics and
   diagnostic retention before job cancellation.
5. Avoid redundant main/develop builds and reuse successful exact-commit
   artifacts for release. Retry failed hosts without repeating green hosts.
6. Investigate release cancellation. During Release `37003970027`, normal and
   force cancellation marked the run cancelled, yet npm and GitHub publication
   completed. Require clear cancellation boundaries between external effects
   and a documented way to restore public channels after partial publication.
7. Agree the bounded design, implement it, and validate the changed delivery
   behavior proportionately. Do not reduce required coverage by assumption.
8. Replace every inline shell `run:` block in the workflows with a
   `scripts/delivery` TypeScript command, written as readable pure functions
   and small utilities. The maintainer asked for this on 2026-10-10 ("the
   pipelines should too be run on ts scripts, since I can't stand bash"). Keep
   the PowerShell-only Windows test boundary above unless the TypeScript runner
   proves it preserves the ACL evidence.

Beta10 adds one data point for steps 2 and 5. [Build 37983060029](https://github.com/TheLithiumForge/open-forge/actions/runs/37983060029)
failed only the Windows ARM64 test step, and its logs and diagnostics needed a
signed-in session. The tag-triggered [Release 37990754560](https://github.com/TheLithiumForge/open-forge/actions/runs/37990754560)
rebuilt and retested the same commit and passed on all six platforms.

## Horizon: TypeScript pipeline steps and local parity

The maintainer selected step 8 on 2026-10-10 and stated the goal: "we should do everything a pipeline does locally as well", with "all the scripts in the repo such that we can easily check them out and run them when needed". This horizon covers step 8 and that parity. Steps 1 to 7 stay open.

Status: Task 71 “Streamline build and release delivery” (phase 1/1): milestone 3/3 — TypeScript pipeline horizon complete.

Milestones: M1 design, M2 commands and workflow changes with local evidence, M3 hosted verification.

### Decisions

- **P1 No shell in workflows.** Every `run:` step is one `npm run <command>` with arguments. Values that differ by matrix row use `${{ matrix.rid }}`, which reads the same in bash and PowerShell. The Build job drops its bash default and its paired bash and PowerShell steps. Windows then runs every step under the runner default PowerShell, which keeps the ACL evidence the Git Bash note above protects. The only exception is `git config --global core.longpaths true` before checkout, which runs before the repository exists.
- **P2 Commands, not inline logic.** Runner preparation (`ci:prepare`: .NET SDK isolation and the macOS physical temporary root, written to `GITHUB_ENV`) and the downloadable binary bundle (`ci:bundle`: archive plus `SHA256SUMS`) become `forge` commands. `ci:prepare` imports only Node built-ins, because it runs before `npm ci`.
- **P3 Logging.** A shared `--log <file>` option on `forge` commands writes the command output to the file while still printing it, replacing `tee` and `Tee-Object`.
- **P4 Composite checks.** `verify`, `check`, `check:fast` and `check:delivery` become `forge` commands that run their steps in order from data, instead of `&&` chains in `package.json`.
- **P5 Local parity.** `ci:checks` runs the Build `checks` job and `ci:job --rid <rid>` runs one Build matrix job in the same order with the same logs. `docs:build` runs the documentation build job. Release publication stays in the workflow, because it needs repository secrets. Its selection and collection commands already run locally.
- **P6 Operator tools.** The ad hoc helpers used for beta9 to beta11 become repository commands: `gate:wsl` runs the managed tiers for one commit in a WSL clone, `ci:watch` follows hosted runs for a commit through the public GitHub API, and `smoke:public` installs an exact published version in a temporary directory and checks it.

### Local evidence

- `npm run check:delivery` and `npm run test:delivery` passed: 96 tests, no failures or skips.
- `npx forge ci:job --rid win-x64` ran every stage on Windows: delivery plan, `build:native`, `ci:bundle`, `test:built` (unit 4,539, integration and native-integration 2,980 each with 17 platform exclusions, public, native-public and public-native 330 each) and `pack`, writing `build.log`, `bundle.log`, `test.log` and `pack.log` under `artifacts/delivery/logs/win-x64/`. The bundle holds `open-forge.exe`, `LICENSE` and `README.txt`, and its `SHA256SUMS` line matches `sha256sum`. A source edit during the first run made `test:built` refuse the stale build, and `ci:job` stopped and named that stage. `test:built` and `pack` then passed on the unchanged tree.
- `npx forge gate:wsl --commit 54aeb780b` cloned the commit into WSL and passed Unit, Integration (2,967 passed, 30 skipped) and EndToEnd (297 passed, 33 skipped) with no undeclared skip reason. Two fixes came from this run: it now resolves short commit names on the host, and it starts Node through the WSL login shell, because `wsl.exe --exec` does not load per-user Node managers.
- `npx forge smoke:public --version 0.9.0-beta.11` passed its four checks against the published package.
- `npx forge ci:watch --run 38043270935` reported every job of the beta11 runs as successful and exited 0.
- Commands that call `npm run` must be started through `npx forge` or `npm run`, which supply npm's environment. Direct `node scripts/delivery/cli.ts` reports that requirement.

### Hosted evidence

[Build 38080012366](https://github.com/TheLithiumForge/open-forge/actions/runs/38080012366) ran the new `build.yml` for `6a9107d75` and passed on all six platforms, with the new `Prepare runner environment`, bundle, test and pack steps. Following it exposed one defect: `ci:watch` polled every 15 seconds and exhausted GitHub's 60 anonymous requests an hour. It now polls every three minutes without a token, reads a finished run's jobs once, and waits out a reported rate limit. This horizon is complete.

## Horizon: release reuse and cancellation boundaries

The maintainer selected steps 5 and 6 on 2026-10-10.

Status: Task 71 “Streamline build and release delivery” (phase 1/1): milestone 2/3 — Build reuse verified on GitHub, release-side checks wait for the next tagged release.

Milestones: M1 design, M2 commands and workflow changes with local evidence, M3 hosted verification.

### Decisions

- **R1 Release reuse.** When no Build run is supplied, `release:select` finds a successful `build.yml` run for the exact commit whose six `package-<rid>` artifacts are present and not expired, validates it, and passes its ID on, so the Release skips its own build. Only runs started by a push or by hand in this repository qualify, for supplied runs too. Without such a run, the Release builds as before.
- **R2 No duplicate branch builds.** A first `reuse` job in `build.yml` skips the remaining jobs on a push when a trusted `build.yml` run for the same commit from another branch has passed. When that run started earlier and is still running, the job waits up to 90 minutes and skips only if it passes. Otherwise this push builds. Pull requests, manual runs and calls from Release always build.
- **R3 Retry only failed hosts.** A failed platform is retried with GitHub's "Re-run failed jobs" on the Build run. A later Release reuses the completed run under R1, so green hosts are not rebuilt. This is documented, not automated.
- **C1 Last moment to cancel.** The publish job prints the complete publication plan, then waits 60 seconds before the first upload. Cancelling during that wait publishes nothing.
- **C2 Cancellation between uploads.** `publish:release` handles SIGINT and SIGTERM. It does not start another upload after a signal, lets the current upload finish or fail, and reports which packages were published and which were not.
- **C3 Restoring channels.** `release:restore-channels --version <bad> --to <previous>` points the npm `latest` and channel tags of all seven packages back at the previous version. It prints the commands by default and runs them only with `--apply`. The guide covers deleting or editing the GitHub release by hand.

### Release horizon local evidence

- `npm run check:delivery` and `npm run test:delivery` passed: 141 tests, covering reusable-run choice, the reuse decision, the checkpoint plan, stop requests before the first, middle and last upload, and restore command lists.
- Integration review found one defect: two branches pushed together would each defer to the other's unfinished run, so neither would build. A push now defers only to an earlier unfinished run, with a test for both orders.
- `npx forge release:restore-channels --version 0.9.0-beta.11 --to 0.9.0-beta.10` printed the 14 dist-tag commands and changed nothing.
- The worker sandbox could not write `.agents`, so the two document changes arrived as a reviewed patch and were applied during integration.

### Security review

A background review of `454bbfc47` found three issues in the first reuse version. All were accepted and fixed in the next commit.

- **Artifact provenance.** Release could reuse a `pull_request` run of the same commit. GitHub builds a pull request from a merge commit, so those artifacts are not the tagged commit's own Build. Release now accepts only `push` and `workflow_dispatch` runs whose head repository is this repository.
- **Untrusted run trust.** A push could skip its Build by deferring to a pull request or fork run. Branch reuse now applies the same trust rule.
- **Fail-open gate.** A push skipped at once when another branch's Build was still running, so it showed green before anything passed. [Build 38161933644](https://github.com/TheLithiumForge/open-forge/actions/runs/38161933644) on `main` did exactly that while `develop`'s Build ran. The `reuse` job now waits for the earlier run and reuses it only if it passes.

### Release horizon hosted evidence

- [Build 38161906826](https://github.com/TheLithiumForge/open-forge/actions/runs/38161906826) on `develop` built `454bbfc47` and passed on all six platforms. [Build 38161933644](https://github.com/TheLithiumForge/open-forge/actions/runs/38161933644) on `main` skipped at once while it ran, which is the fail-open defect the security review then named.
- After the fix, [Build 38162623919](https://github.com/TheLithiumForge/open-forge/actions/runs/38162623919) on `develop` built `a2d0cee37` and passed on all six platforms. On `main`, [Build 38162644655](https://github.com/TheLithiumForge/open-forge/actions/runs/38162644655) ran its `reuse` job for 52 minutes, from 18:09:42 to 19:02:11 UTC, finished 31 seconds after the `develop` Build passed, and only then skipped checks and builds. R2 now fails closed.
- `ci:watch` followed both commits. Two watches shared GitHub's anonymous limit, reached it, waited out the reported reset of about 34 minutes, and finished with exit 0.
- R1, C1 and C2 run only in `release.yml`. The next tagged release exercises them: `release:select` should report the reused Build, the publish job should show the plan and wait 60 seconds, and publication should report each upload. C3 is verified by its dry run.

## Horizon: timing, stall diagnostics and the check map

The maintainer asked on 2026-10-12 to finish the task completely. This horizon covers steps 1 to 4 and 7.

Status: Task 71 “Streamline build and release delivery” (phase 1/1): milestone 2/3 — implemented with local evidence, hosted verification with the next push.

Milestones: M1 design, M2 commands and workflow changes with local evidence, M3 hosted verification.

### Timing analysis

`npx forge ci:timings --limit 12` read the twelve latest completed Build runs on 2026-10-12. Two were reuse skips, so ten built. Medians and maxima of successful steps, in minutes:

| Host        | Whole job      | Native build   | Test suites            |
| ----------- | -------------- | -------------- | ---------------------- |
| linux-arm64 | 19.7, max 21.1 | 13.1, max 14.2 | 5.7, max 6.0           |
| linux-x64   | 18.5, max 21.7 | 10.0, max 11.7 | 7.5, max 9.1           |
| osx-arm64   | 24.3, max 30.6 | 16.3, max 20.3 | 7.0, max 9.0           |
| osx-x64     | 48.0, max 59.0 | 26.4, max 37.7 | 19.4, max 22.2         |
| win-arm64   | 29.7, max 32.0 | 13.5, max 14.1 | 12.6, max 13.3         |
| win-x64     | 24.4, max 35.3 | 9.8, max 14.5  | 10.2 to 14.8, max 17.8 |

- osx-x64 sets the Build's length and came within one minute of the 60-minute job limit. Intel macOS runners are about twice as slow as the others in both build and tests.
- The shared `checks` job takes 5.0 minutes (`verify` 4.3) before the matrix starts.
- Tests failed on win-arm64 in Builds [37983060029](https://github.com/TheLithiumForge/open-forge/actions/runs/37983060029), [37996572273](https://github.com/TheLithiumForge/open-forge/actions/runs/37996572273) and [38047448623](https://github.com/TheLithiumForge/open-forge/actions/runs/38047448623), and on win-x64 in [38166548020](https://github.com/TheLithiumForge/open-forge/actions/runs/38166548020), a documentation-only commit whose code commit had passed. Each failure's only public annotation was "Process completed with exit code 1". The failing tests could not be named without a signed-in log download, so whether they are one flaky test is unconfirmed.
- The Linux ARM64 stall in the evidence above did not recur. Its slowest test step in these runs was 6.0 minutes. Its cause remains unconfirmed.
- Locally on win-x64, `public` is the longest suite, 226 to 229 seconds of about 530 for all six.

### Decisions

- **D1 Check map.** The [delivery system](../../../crystallized/documents/cli/delivery-system.md#what-does-each-check-prove) maps each required check to what it proves and where it runs. No check is redundant: AOT cannot build for another operating system and the tests must run on the platform they prove, so each RID uses a matching host, and the managed and native modes prove different builds.
- **D2 Timing command.** `ci:timings` reads recent Build runs through the public API, about one request per run plus one, and prints step medians and maxima by job and RID, then failed, cancelled and over-twice-median steps. It keeps every attempt, so a retried host's failure stays visible.
- **D3 Suite safeguards.** Each suite runs asynchronously under a 20-minute deadline with a two-minute heartbeat, and xUnit's `--long-running 120` names hung tests in the log, which the runner echoes at each heartbeat. At the deadline it writes `processes.txt` within a bounded time, stops the process tree (`taskkill /T` on Windows, the process group elsewhere, with the process itself as a fallback), prints the log tail and fails. SIGINT and SIGTERM stop the tree too, so a cancelled local run leaves no suite behind. Passing suites print their three slowest tests.
- **D4 Readable failures.** In GitHub Actions, failed tests become error annotations with their source file and line, including CI's mapped `/_/` source paths, and hung tests become warning annotations. `ci:watch` prints a failed job's annotations. Both are readable without signing in, which the failures above lacked.
- **D5 Job limit.** The Build matrix limit rises from 60 to 90 minutes. osx-x64 needs room beyond its 59-minute maximum. With the slowest build and suites observed, a stall in the last suite reaches its deadline by about minute 85, so the diagnostics artifact uploads. A slower build than any seen so far could still meet the job limit first.
- **D6 Orchestration.** The TypeScript horizon already gave every host the same commands and one matrix job with no paired shell steps. Starting the matrix in parallel with `checks` would save about five minutes but spend six hosts' time on commits that fail cheap checks, so the order stays. Splitting osx-x64 across jobs would transfer artifacts between hosts and weaken provenance, so it stays one job.
- **D7 Remaining shell chains.** `check:dotnet` becomes a `forge` composite of `check:dotnet:whitespace` and `check:dotnet:style`. `apm:install` runs `scripts/agent-tooling/agent-projections/apm-install.ts`, which accepts `--plan`. No npm script chains commands with `&&` any more.

### Diagnostics local evidence

- `npm run check:delivery` and `npm run test:delivery` passed on Windows: 159 tests and one Unix-only skip, including a stalled fixture whose child process is stopped with it at the deadline, an exit-status case, annotation escaping, CTRF failure and slowest-test reading, and timing medians and outliers.
- In the WSL clone at `58da9d59c`, the runner, diagnostics and timing tests passed on Linux, 12 of 12, including the process-group stop at the deadline and on SIGTERM.
- `build:native` and `test:built` at `8498d1af5` first failed six native-integration Install tests together on Windows. The new runner named all six. Two direct re-runs and a second `test:built` passed all six modes: 4,539 unit, 2,980 integration twice and 330 public three times, with 34 declared platform exclusions. The [Windows Install fixture race](potential/windows-install-fixture-race.md) candidate records the failure.
- `npx forge gate:wsl --commit 58da9d59c` passed on Linux: 4,539 unit, 2,967 integration with 30 skips and 297 public with 33 skips, with no undeclared skip reason.
- `npx forge test --no-restore` ran the managed unit, integration and public suites through the new runner on Windows: 4,539, 2,980 and 330 tests passed. `public` printed a heartbeat at two minutes, and each suite printed its three slowest tests.
- `--long-running 2` against the integration suite named tests running longer than two seconds and repeated them every interval. The native test executables accept both runner options.
- `npx forge ci:watch --run 38166548020` printed the failed win-x64 job's annotation.
- `npm run test:agent-tooling` went from 3 of 8 to 7 of 9 passing. The patch output now uses `/` on every platform, and the Git object inspector uses `/dev/null` for its empty global config, because Git for Windows cannot open the Win32 null device name that Node gives. The two remaining failures check `.opencode/tools/inspect-git-objects.ts`, which commit `28c7aa2af` removed on 2026-09-12. Whether to restore the adapter or retire those tests is the maintainer's choice.

### Diagnostics review

An Astra high review of `8498d1af5` found six issues. All were accepted and fixed: Unix cancellation left the detached suite running, a failing or stalled stall report could block the stop, CI's mapped `/_/` paths lost annotation locations, a normally quick step that suddenly took minutes was not an outlier, and three documentation claims overstated timeout headroom, output latency and the request count, and attributed matching hosts to AOT alone.

## Current state

The TypeScript pipeline horizon is complete. The release reuse and cancellation horizon is implemented and its Build reuse is verified on GitHub. Its milestone 3 closes when the next tagged release shows release reuse, the checkpoint and the publication report working. The timing, stall diagnostics and check map horizon is implemented with local evidence and verifies on GitHub with the next push.
