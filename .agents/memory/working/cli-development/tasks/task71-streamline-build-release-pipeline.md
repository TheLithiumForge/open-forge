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

Status: Task 71 “Streamline build and release delivery” (phase 1/1): milestone 2/3 — commands and workflow changes verified locally, hosted verification next.

Milestones: M1 design, M2 commands and workflow changes with local evidence, M3 hosted verification.

### Decisions

- **R1 Release reuse.** When no Build run is supplied, `release:select` finds a successful `build.yml` run for the exact commit whose six `package-<rid>` artifacts are present and not expired, validates it with the existing rules, and passes its ID on, so the Release skips its own build. Without such a run, the Release builds as before.
- **R2 No duplicate branch builds.** A first `reuse` job in `build.yml` skips the remaining jobs on a push when a `build.yml` run for the same commit from another branch has passed or is still running, and names that run. Pull requests, manual runs and calls from Release always build.
- **R3 Retry only failed hosts.** A failed platform is retried with GitHub's "Re-run failed jobs" on the Build run. A later Release reuses the completed run under R1, so green hosts are not rebuilt. This is documented, not automated.
- **C1 Last moment to cancel.** The publish job prints the complete publication plan, then waits 60 seconds before the first upload. Cancelling during that wait publishes nothing.
- **C2 Cancellation between uploads.** `publish:release` handles SIGINT and SIGTERM. It does not start another upload after a signal, lets the current upload finish or fail, and reports which packages were published and which were not.
- **C3 Restoring channels.** `release:restore-channels --version <bad> --to <previous>` points the npm `latest` and channel tags of all seven packages back at the previous version. It prints the commands by default and runs them only with `--apply`. The guide covers deleting or editing the GitHub release by hand.

### Release horizon local evidence

- `npm run check:delivery` and `npm run test:delivery` passed: 141 tests, covering reusable-run choice, the reuse decision, the checkpoint plan, stop requests before the first, middle and last upload, and restore command lists.
- Integration review found one defect: two branches pushed together would each defer to the other's unfinished run, so neither would build. A push now defers only to an earlier unfinished run, with a test for both orders.
- `npx forge release:restore-channels --version 0.9.0-beta.11 --to 0.9.0-beta.10` printed the 14 dist-tag commands and changed nothing.
- The worker sandbox could not write `.agents`, so the two document changes arrived as a reviewed patch and were applied during integration.

## Current state

The TypeScript pipeline horizon is complete. The release reuse and cancellation horizon is active. Steps 1 to 4 and 7 remain open.
