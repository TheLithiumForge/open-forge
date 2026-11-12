# forge

`forge` gives local development and CI the same build, test and delivery
commands. It coordinates the repository's .NET and npm tools. Run it from the
repository root; no global install or separate build of this tool is needed.

```sh
npx forge --help
npx forge pack --help
```

Arguments go directly after the command. Both `--help` and `-h` work. Root npm
aliases remain available, such as `npm run build`; aliases need the usual
separator for flags: `npm run pack -- --skip-tests`.

Every command accepts `--log <file>`. It creates the parent directory, prints
stdout and stderr as usual, and records both streams in that file. A failed
command keeps its exit code.

## Start from a fresh checkout

Install Node/npm as specified in [package.json](../../package.json), the SDK in
[global.json](../../global.json), and your platform's native toolchain if you
want Native AOT output. The [development guide](../../docs/development.md#build-and-test)
has platform prerequisites. Node runs this TypeScript tool directly.

```sh
npx forge setup
npx forge test --no-restore
npx forge dist --no-restore --plan
npx forge dist --no-restore
```

`setup` runs `npm ci` and .NET restore. It replaces `node_modules`; it does not
install Node, the SDK or a native toolchain. `--offline` uses cached dependencies
and fails if required packages are unavailable. Builds restore by default;
`--no-restore` reuses an earlier restore. Choose one of these flags, not both.

## Choose a stage

| Command                    | What it does                                                                 |
| -------------------------- | ---------------------------------------------------------------------------- |
| `setup`                    | Install npm dependencies and restore .NET.                                   |
| `restore`                  | Restore .NET only.                                                           |
| `build`                    | Build the managed solution and development CLI in Release mode.              |
| `test`                     | Build and run managed unit, integration and public-command tests.            |
| `build:native`             | Build the current host's native CLI and test executables.                    |
| `test:built`               | Test existing native-build artifacts in six execution modes.                 |
| `pack`                     | Package existing artifacts and test their npm installation.                  |
| `dist`                     | Run `build:native`, `test:built`, then `pack`.                               |
| `dist:wrapper`             | Build and pack only the npm wrapper, without .NET or native files.           |
| `version`                  | Bump the root version and synchronize .NET, without committing or tagging.   |
| `clean`                    | Remove owned .NET and delivery outputs.                                      |
| `verify`                   | Run delivery, package, .NET and documentation checks in order.               |
| `check`                    | Check formatting, TypeScript and lint.                                       |
| `check:fast`               | Check TypeScript and lint.                                                   |
| `check:delivery`           | Check delivery TypeScript, lint and formatting.                              |
| `check:dotnet`             | Check .NET whitespace, then formatting and analyzer style.                   |
| `ci:prepare`               | Write runner SDK paths and macOS temporary paths to `GITHUB_ENV`.            |
| `ci:reuse`                 | On a direct push, reuse another branch's Build for the same commit.          |
| `release:checkpoint`       | Print the publication plan and wait 60 seconds before uploads.               |
| `release:restore-channels` | Preview or apply npm channel recovery for all seven packages.                |
| `ci:bundle`                | Archive the published CLI for `--rid` with its license and checksum.         |
| `ci:checks`                | Run the Build workflow's shared setup and verification.                      |
| `ci:job`                   | Run one Build matrix job for `--rid` with the workflow's logs.               |
| `docs:build`               | Install, type-check and build the documentation site.                        |
| `ci:watch`                 | Watch all GitHub workflow runs for `--sha`, or the commit of `--run`.        |
| `ci:timings`               | Compare step timings across recent Build runs and list slow or failed steps. |
| `smoke:public`             | Install and check an exact published `--version` in a fresh workspace.       |
| `gate:wsl`                 | Run the three managed test tiers for `--commit` in a WSL clone.              |

`dist --plan` shows stage names and effective arguments without executing them.
Each stage reports start, success or failure. Execution stops at the failed
stage and identifies the command or log. GitHub Actions uses separate
build/test/pack steps with those same commands.

The six `test:built` modes run on **one host**: unit tests once, integration
tests twice, and public-command tests three times. They exercise managed and
native combinations. Linux does not execute macOS or Windows tests.

Native commands detect the current host. `--rid` makes that choice explicit
and must match it. Supported RIDs are `linux-x64`, `linux-arm64`, `osx-x64`,
`osx-arm64`, `win-x64` and `win-arm64`.

## Pack without tests

From existing `build:native` artifacts:

```sh
npx forge pack --skip-tests
```

Or build and pack in one command:

```sh
npx forge dist --skip-tests --no-restore
```

The build still compiles test executables. Neither command executes tests.
Source and artifact integrity checks remain active. These archives are marked
untested and cannot enter native publication or release collection. To qualify
the same build later, run `test:built`, then ordinary `pack`.

Normal `pack` does not run .NET tests or rebuild: it requires passing
`test:built` reports and runs the npm installation check itself.

## Publish a wrapper for selected targets

The wrapper can be prepared without native artifacts on your machine:

```sh
npm ci --ignore-scripts
npx forge dist:wrapper --targets linux-x64,osx-x64,win-x64
npx forge publish:wrapper --tag preview --dry-run
```

That version lists exactly the three selected x64 packages as optional
dependencies. Omitting `--targets` selects all six. This option changes the
wrapper's dependency list; it does not cross-compile native executables.

On each selected host, build/test/pack its native package, then preview its
independent upload with `npx forge publish:native --tag preview --dry-run`.
Publish all listed native packages at the same version before the wrapper.
The wrapper provides the `open-forge` npm command; native packages contain the
executable payload. Both include the license.

`--dry-run` validates local artifacts and prints a plan without registry
contact. **Removing it uploads to the configured npm registry.** Actual upload
requires committed matching source, current packages, credentials and an
explicit tag. Prereleases cannot use `latest` as that tag, but until a package
has a stable version, a newly published prerelease also becomes `latest`.
Existing versions warn and skip
without retagging; an existing wrapper with different dependencies rejects the
run. Changing a published wrapper's targets requires a new version.

## Collect a release from several hosts

Pass the same `--targets` to `pack` or `dist` on each selected host. Copy each
host's package output into `artifacts/release-input/package-<RID>/`, then run:

```sh
npx forge release:collect artifacts/release-input artifacts/release --targets linux-x64,osx-x64,win-x64
npx forge publish:release --from artifacts/release --tag preview --dry-run
```

Collection requires tested packages with matching source, version and target
selection. It replaces the output directory and records the selection in
`release.json`. Omitted target artifacts are not required. Publication consumes
that exact selection, checks it before uploads, and publishes the wrapper last.
Uploads are not atomic; rerun the same publication to fill missing versions.
The default Actions workflows still build and release all six targets.

`release:select` is a GitHub Actions helper, not a local release command.
Its help describes the workflow environment it reads.

## Reuse Builds and cancel publication

Release reuses the most recent successful `build.yml` run for the exact source
commit with all six `package-<RID>` artifacts present and unexpired. Only runs
started by a push or by hand in this repository count. A pull request run
builds a merge commit and a fork can change the workflow, so neither can stand
in for a commit's own Build. A supplied `BUILD_RUN_ID` must pass the same
checks. If no run qualifies, Release builds once.

On direct branch pushes, `ci:reuse` skips duplicate checks and builds when
another branch's trusted Build of that commit has passed. When that Build
started earlier and is still running, `ci:reuse` waits for it, for up to 90
minutes. It skips only if that Build passes. If it fails, is cancelled or runs
too long, this push builds the commit itself, so a branch never shows green
without a passing Build. Pull requests, manual runs and calls from Release
always build. A newer push to a pull request cancels that pull request's older
Build. Direct pushes, manual runs and Release calls are never cancelled or
queued by another run.

If only one host fails, open the Build run in GitHub Actions and choose
**Re-run failed jobs**. Successful hosts keep their artifacts. After the retry
passes, Release can reuse that run without rebuilding those hosts.

```sh
npx forge ci:reuse --help
npx forge release:checkpoint --wait 60
npx forge release:restore-channels --version 0.9.0-beta.11 --to 0.9.0-beta.10
```

`release:checkpoint` prints the target, channel, version, source commit, every
npm package and every GitHub release file from the collected release. It waits
60 seconds by default. `--from <directory>`, `--target <github|npm|all>` and
`--tag <channel>` select its inputs. `--wait <seconds>` changes the wait.
The publish job runs it after collection and before either destination.
Cancel during this wait to publish nothing.

During npm publication, `publish:release` handles SIGINT and SIGTERM, starts no
further upload or tag change, and lets the current effect finish or fail. It
reports published packages, an interrupted upload whose final state is unknown,
and packages not started, then exits non-zero. A completed upload remains
published. Force cancellation can prevent this report. Inspect npm and GitHub
before recovery because publication across packages and destinations is not a
transaction.

`release:restore-channels` prints `npm dist-tag add` commands for all seven
packages without registry contact. Its default tags are `latest` and the bad
version's prerelease channel, or just `latest` for a stable version. Use
`--channels latest,beta` to choose tags explicitly. Add `--apply` only after
reviewing the plan. It checks that `--to` exists for every package before
changing any tag. It never unpublishes or deprecates a version.

For GitHub recovery, open the affected release under the repository's
**Releases** page. Use **Edit** to mark it as a prerelease, remove its latest
designation or explain the withdrawal in its notes. If it should be removed,
use **Delete** on that release. Edit the previous release's latest designation
as needed and inspect the source tag separately. These manual actions do not
restore npm channels or undo downloaded files.

## Version and clean up

```sh
npx forge version patch
npx forge version prerelease --preid beta
npx forge version 0.1.0-beta.1
```

These are alternatives, not a sequence. npm updates `package.json` and
`package-lock.json`; the lifecycle hook updates `Directory.Build.props`.
Review and commit those files, then rebuild and repack. `version:sync` is that
hook and normally needs no manual invocation. `--sha` on `build:native`, `dist`
or `dist:wrapper` stamps an artifact with the current commit without bumping the
root version.

| Output                                           | Location                                    |
| ------------------------------------------------ | ------------------------------------------- |
| Managed development CLI                          | `artifacts/publish/open-forge-dev/Release/` |
| Native executables                               | `artifacts/publish/<RID>/`                  |
| Host reports and delivery metadata               | `artifacts/delivery/<RID>/`                 |
| Portable archive, two npm tarballs and checksums | `artifacts/delivery/<RID>/packages/`        |
| Standalone wrapper                               | `artifacts/delivery/wrapper/packages/`      |
| Collected release                                | The output passed to `release:collect`.     |

Run `npx forge clean` to remove `artifacts/bin`, `artifacts/obj`,
`artifacts/publish` and owned host/wrapper delivery outputs and managed reports. Dependencies,
offline caches, logs, collected releases and npm links remain. Restore again
before using `--no-restore`. Run commands sequentially within one worktree;
builds and packs replace their own output directories.

## Download development binaries

The GitHub Actions **Build** workflow produces a `binary-<RID>` artifact for each
platform whose native build succeeds. Open the workflow run, choose the artifact
for your platform, then extract its portable archive. It contains `open-forge`
(`open-forge.exe` on Windows), the license and a development-build notice.
The download also includes a SHA-256 checksum. Artifacts remain available for
14 days; creating them does not publish a release or an npm package.

Binaries are uploaded before the test gate. Check the run's test results before
using them: an available binary does not mean its tests passed. The separate
`package-<RID>` artifacts require successful tests and package-install checks.

Dependency setup restores each native executable for its host runtime and
self-contained configuration before `--no-restore` builds. Integration and public reports
allow only explicit exclusions for another operating system. Those exclusions
remain counted separately from passes; unexpected skips and capability failures
still fail qualification.

CI source paths are mapped for reproducibility; snapshot lookup resolves those
paths back into the checkout. Existing Windows snapshots retain their contents.
On Unix, comparison adapts expected placeholder-rooted physical path separators
and the exact held-lock error code for that OS. Received output is never rewritten
by this comparison, so failure artifacts preserve platform evidence. Windows-only
cleanup deletion snapshots belong to their own explicitly excluded test.

macOS jobs use the physical runner temporary directory. This keeps test roots
outside the system `/var` alias and leaves room for Unix-domain socket paths.
Recovery and lock safety checks continue to reject symlinked storage chains.
Diagnostic uploads include received snapshot files when comparisons fail.

Windows denial fixtures protect their temporary DACL while the denial is active.
Restoration verifies ordered access-rule bytes and inheritance protection; an
OS-added auto-inheritance bookkeeping flag is not treated as a permission change.
The real access-denial probes and required Windows scenarios remain mandatory.

## Run workflow steps locally

The workflow commands are ordinary repository commands. Preview a job's stages,
then run the same job on a matching host:

```sh
npx forge ci:checks --plan
npx forge ci:job --rid win-x64 --plan
npx forge ci:job --rid win-x64
npx forge docs:build --plan
```

| Workflow step                                | Local command                                                                                   |
| -------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| Build: prepare runner environment            | `npx forge ci:prepare`                                                                          |
| Build: select another branch's run           | `npx forge ci:reuse` with the workflow environment                                              |
| Build: install dependencies                  | `npx forge setup`                                                                               |
| Build: shared verification                   | `npx forge verify`                                                                              |
| Build: show delivery plan                    | `npx forge dist --rid <RID> --no-restore --plan`                                                |
| Build: native artifacts                      | `npx forge build:native --rid <RID> --no-restore --log artifacts/delivery/logs/<RID>/build.log` |
| Build: downloadable binary                   | `npx forge ci:bundle --rid <RID> --log artifacts/delivery/logs/<RID>/bundle.log`                |
| Build: managed and native tests              | `npx forge test:built --rid <RID> --log artifacts/delivery/logs/<RID>/test.log`                 |
| Build: package and installation check        | `npx forge pack --rid <RID> --log artifacts/delivery/logs/<RID>/pack.log`                       |
| Documentation: install, type-check and build | `npx forge docs:build`                                                                          |
| Release: select workflow source              | `npx forge release:select` with the workflow environment                                        |
| Release: collect packages                    | `npx forge release:collect artifacts/release-input artifacts/release`                           |
| Release: publication checkpoint              | `npx forge release:checkpoint --wait 60`                                                        |
| Release: npm publication                     | `npx forge publish:release --tag <channel> --dry-run` for a local preview                       |

`ci:checks` runs `setup` and `verify`. `ci:job` runs `setup`, the delivery plan,
native build, binary bundle, tests and pack. `--no-restore` reuses an earlier
setup. `--offline` uses cached dependencies for setup and build. These two
options cannot be combined. Each composite command accepts `--plan` and stops
at the first failed stage, naming it.

`ci:prepare` runs before npm dependencies exist. It writes `DOTNET_INSTALL_DIR`
and `DOTNET_ROOT` below `RUNNER_TEMP/dotnet`, plus the physical `RUNNER_TEMP`
path as macOS `TMPDIR`. Without `GITHUB_ENV`, it prints the assignments and
changes nothing. `ci:bundle --rid <RID>` uses existing publish output and
`tar` from PATH. It writes the same development notice and `SHA256SUMS` as
the hosted bundle.

`docs:build` runs `npm ci`, `npm run typecheck` and `npm run build` in
`src/docusaurus`. Hosted checkout, tool installation, artifact upload and
Pages deployment stay in GitHub Actions. Release publication keeps its workflow
secrets and existing order. Windows workflow commands use runner-default
PowerShell so filesystem tests retain their ACL denial evidence.

## Diagnose slow, stalled and failed suites

Each `test:built` and `test` suite writes its runner output to
`execution.log` in its report folder and runs under three safeguards:

- xUnit names any test still running after two minutes, and repeats the
  report while it runs. The suite prints new reports at each two-minute check.
- Every two minutes the suite prints how long it has run and how much it has
  logged.
- A suite still running after 30 minutes is stopped with every process it
  started. Before stopping it, the runner lists running processes in
  `processes.txt` next to the log, then prints the log tail and fails.

A failed suite names each failed test. A passing suite prints its three slowest
tests. In GitHub Actions, failed tests and hung tests also become annotations,
so they appear on the run page and in `ci:watch` without a signed-in log
download. Each passing suite also leaves a notice with its time, so suite times
compare across Builds. `test:delivery` and `test:package-layout` annotate their failed tests
the same way through [github-test-reporter.ts](github-test-reporter.ts), so a
failed `checks` job names its test too. Any other failed stage becomes one
annotation with its exit status. Each native publish writes
`artifacts/delivery/logs/<RID>/publish-<folder>.log`, and its failure annotation
carries that log's last error and linker lines. On osx-arm64, the native
Integration tests link with Apple's classic linker (`-Wl,-ld_classic`), because
the default linker asserts on that large executable. The shipped CLI keeps the
default linker. The longest suite, public, takes under half of the test step, which
has taken up to 22 minutes on osx-x64. The 20-minute deadline is about twice
that suite's time on the slowest host. With the slowest build and suites
observed so far, a stall in the last suite reaches its deadline by about minute
85, inside the Build job limit of 90 minutes, so the diagnostics artifact is
uploaded.

## Follow and qualify a delivery

```sh
npx forge ci:watch --sha <commit>
npx forge ci:watch --run 38043270935
npx forge ci:timings --limit 20
npx forge smoke:public --version 0.9.0-beta.11
npx forge gate:wsl --commit <sha>
```

`ci:watch` prints job states when they change. It uses the public GitHub API,
adding `GITHUB_TOKEN` only when set, and succeeds only when every run for the
commit completes successfully. Without a token, GitHub allows 60 requests an
hour, so it polls every three minutes. With `GITHUB_TOKEN` it polls every 15
seconds. When GitHub reports a rate limit, it waits until the reset time GitHub
gives and continues. When a job fails, it prints that job's annotations, which
name failed and hung tests.

`ci:timings` reads the latest completed Build runs, 10 by default and at most
50, with about one request per run plus one. Very large runs and rate-limit
retries add requests. It prints the median and slowest
successful time of each step that takes 30 seconds or more, grouped by job and
RID. It then lists failed and cancelled steps, and steps that took more than
twice their median. It keeps every attempt, so a retried host's failed attempt
stays visible. Per-suite times are in each job's `test.log`.

`smoke:public` contacts npm and installs the exact version in a new temporary
directory. It checks the version, unattended Essentials installation, current
Status, and a guidance configuration preview. Each check reports its name.
The temporary workspace and data home remain available for inspection.

`gate:wsl` requires Windows and WSL with Node >=22.18, Git and .NET. It
starts Node through the WSL login shell, so a per-user install such as nvm or
Volta is found. `--commit` accepts any name Git resolves on the host, such as a
short SHA. The gate reuses a clone in the WSL home, fetches the current branch
from the host repository, and checks out that commit. Restore
uses an empty local source with `NuGetAudit=false`. It builds Release and runs
the managed unit, integration and public-command tiers. Summaries, execution
logs and distinct skip reasons go to `artifacts/delivery/logs/wsl/<commit>/`.
Unexpected skips fail the gate using [platform-skips.ts](platform-skips.ts).

## Maintain the scripts

[commands.ts](commands.ts) defines commands, options, usage, guidance and examples.
[cli.ts](cli.ts) dispatches them; [options.ts](options.ts) parses shared options
and renders help. [dist-plan.ts](dist-plan.ts) declares the pipeline.
The [ci](ci/) modules declare workflow plans and implement runner and operator
commands. [process-log.ts](process-log.ts) records command output at dispatch.
Focused modules implement each stage. [npm](npm/) contains package preparation
and the launcher; [release](release/) contains collection and release helpers.

Keep the entry point dependency-free so help and setup work before npm install.
To check delivery code and its owned behavior, run `npm run check:delivery` and
`npm run test:delivery`. Package layout and installed-command checks are also
available as `npm run test:package-layout` and `npm run test:package-manager`.
