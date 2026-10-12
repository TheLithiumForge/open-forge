---
open-forge:
  description: How the build, test, package and release system is organized, what runs locally and in GitHub, and the rules that keep every pipeline step runnable from the repository
  responsibility: Define the current structure and rules of the repository delivery pipeline and point to the guides that operate it
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Build, Delivery, Pipeline, Release]
---

# Delivery System

## What does this system do, and where does each part live?

The delivery system builds, tests, packages and releases the Open Forge CLI. Every step is a `forge` command in the repository, so the same step runs on a developer machine and in GitHub Actions.

| Part        | Location                                                  | Role                                                                                         |
| ----------- | --------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `forge`     | [`scripts/delivery/`](../../../../../scripts/delivery/)   | TypeScript commands for setup, build, test, packaging, release, CI steps and operator checks |
| npm scripts | [`package.json`](../../../../../package.json)             | `npm run <name>` aliases for `forge` commands and leaf tools such as `typecheck`             |
| Workflows   | [`.github/workflows/`](../../../../../.github/workflows/) | Runner selection, hosted tool setup, artifact transfer, secrets and Pages deployment         |

## Rules

- A workflow `run:` step is one `npm run <command>` with arguments. Matrix values are passed as `${{ matrix.rid }}`, which reads the same in bash and PowerShell. The one exception is `git config --global core.longpaths true`, which runs before checkout.
- Logic lives in `forge` commands, written as readable TypeScript with pure functions and small utilities under the [TypeScript Directive](../../../../directives/open-forge/typescript/typescript-source-structure.md). Shell scripts and JavaScript files are not part of the system.
- Every workflow job has a local command. Only hosted tool setup, artifact upload and download, secret-backed npm publication, GitHub release creation and Pages deployment stay in GitHub. Publication has a local `--dry-run` preview.
- Windows runs workflow steps under the runner-default PowerShell. Git Bash can enable backup and restore privileges that bypass the ACL denials the filesystem tests exercise.
- A test skipped on one platform must give a reason declared in [`platform-skips.ts`](../../../../../scripts/delivery/platform-skips.ts). An undeclared reason fails the delivery report.
- `test:built` runs only against the commit and working tree that `build:native` built. Commit before building, and do not edit sources until the tests finish.
- Commands that call `npm run`, such as `ci:job` and `smoke:public`, run through `npx forge` or `npm run`, which supply npm's environment.
- Each test suite runs under a 20-minute deadline, prints progress every two minutes, and has xUnit name any test running longer than two minutes. At the deadline it records running processes, stops the suite's process tree and fails. Failed and hung tests become GitHub annotations, which are readable without signing in. The `node:test` checks in `verify` annotate their failed tests the same way. The Build job limit is 90 minutes. With the slowest build and suites observed so far, a stall in the last suite still reaches its deadline before that limit. The [delivery guide](../../../../../scripts/delivery/README.md#diagnose-slow-stalled-and-failed-suites) gives the details.

## What does each check prove?

Every required check below proves something no other check covers. Removing one
removes that evidence.

| Check                                 | Runs                         | Proves                                                                                                                                                                                                                         |
| ------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `check:delivery`                      | `checks`, Linux              | Delivery TypeScript compiles under strict settings and passes lint and Prettier                                                                                                                                                |
| `test:delivery`                       | `checks`, Linux              | Delivery commands behave as documented: plans, packaging, release selection and reuse, publication order and cancellation, suite diagnostics                                                                                   |
| `test:package-layout`                 | `checks`, Linux              | npm package staging places the launcher, license and native files where installation expects them                                                                                                                              |
| `check:dotnet`                        | `checks`, Linux              | .NET whitespace, formatting and analyzer style                                                                                                                                                                                 |
| `check:docs`                          | `checks`, Linux              | Repository Markdown passes the document gate: links, frontmatter and inventory                                                                                                                                                 |
| `build:native`                        | Each of six hosts            | The managed Release build and the Native AOT CLI and test executables compile for that RID. AOT cannot build for another operating system, and the tests must run on the platform they prove, so each RID uses a matching host |
| Unit suite                            | Each host                    | Core logic, in about 11 seconds                                                                                                                                                                                                |
| Integration suite, managed and native | Each host                    | Commands against that host's real filesystem, permissions and processes. The native run proves AOT trimming keeps the same behavior                                                                                            |
| Public suite, three modes             | Each host                    | Public commands as users run them: managed tests against the managed CLI, native tests, and managed tests against the native CLI that ships                                                                                    |
| `ci:bundle`                           | Each host                    | The downloadable archive and checksum for that RID                                                                                                                                                                             |
| `pack`                                | Each host                    | Packages come only from tested artifacts, and the installed npm launcher starts the native binary                                                                                                                              |
| `docs:build`                          | `docs.yml`                   | The documentation site type-checks and builds                                                                                                                                                                                  |
| `gate:wsl`                            | Local, before a release push | The managed tiers pass on Linux from a clean clone, and no skip reason is undeclared                                                                                                                                           |
| `smoke:public`                        | Local, after publication     | The published npm version installs and runs Essentials installation, Status and a configuration preview                                                                                                                        |

Platform behavior differs where it matters: Windows ACL denials and ConPTY,
Unix permissions and sockets, and macOS temporary paths. Each such test either
runs on its platform or is skipped elsewhere with a reason declared in
[`platform-skips.ts`](../../../../../scripts/delivery/platform-skips.ts).

## Pipelines

| Workflow      | Trigger                                                                           | Jobs                                                                                                                                                                                                                                                                                          | Local command                                                                          |
| ------------- | --------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| `build.yml`   | Push to `main` or `develop`, pull requests, manual, or called by Release          | `reuse` checks direct pushes for another branch's Build of the same commit. Otherwise `checks` runs `setup` and `verify` once. One matrix job per RID runs `ci:prepare`, `setup`, the delivery plan, `build:native`, `ci:bundle`, `test:built` and `pack`                                     | `ci:reuse`, `npx forge ci:checks`, `npx forge ci:job --rid <rid>`                      |
| `release.yml` | Version tag `v*`, or manual with a source ref, destination and optional Build run | `select` checks the tag and source and reuses the newest qualified exact-commit Build. Without one, `build` calls `build.yml`. `publish` collects packages, prints the plan and waits 60 seconds, publishes the six platform packages before the main package, and creates the GitHub release | `release:select`, `release:collect`, `release:checkpoint`, `publish:release --dry-run` |
| `docs.yml`    | Documentation changes on pull requests and `main`                                 | Builds the site, and deploys it to Pages from `main`                                                                                                                                                                                                                                          | `npx forge docs:build`                                                                 |

Each Build job writes `build.log`, `bundle.log`, `test.log` and `pack.log` to `artifacts/delivery/logs/<rid>/`, and uploads packages, the downloadable binary and diagnostics. The [CLI Distribution](distribution.md#publication-boundary) document defines the package graph and the complete-release rules that `publish` enforces.

Release reuse requires a successful `build.yml` run for the exact source commit
and all six unexpired package artifacts, including when a run ID is supplied.
Only push and manual runs in this repository qualify. A pull request run builds
a merge commit and a fork can change the workflow, so neither stands in for a
commit's own Build.

A direct push reuses another branch's passed Build of the same commit. It waits
for an earlier one that is still running and reuses it only if it passes, so a
branch never shows green without a passing Build. A Build that fails, is
cancelled or runs past 90 minutes makes the push build the commit itself.
Pull requests, manual runs and Release calls always build. Retry failed hosts
with GitHub's **Re-run failed jobs**, then reuse the completed run.

Cancelling during the publication checkpoint uploads nothing. During npm
publication, SIGINT or SIGTERM stops further uploads after the current effect
settles and reports partial results with a non-zero exit. Published uploads
remain present. `release:restore-channels` previews npm channel recovery for all
seven packages and applies it only with `--apply`. The [delivery guide](../../../../../scripts/delivery/README.md#reuse-builds-and-cancel-publication)
explains recovery, including manual GitHub release changes.

## Qualifying a release

Run these around a release, in order:

1. `npx forge gate:wsl --commit <sha>` before pushing, to run the managed tiers on Linux from a WSL clone and catch undeclared platform skips.
2. `npx forge ci:watch --sha <sha>` after pushing, to follow every hosted run for the commit. A failed job's annotations name its failed tests.
3. `npx forge smoke:public --version <version>` after publication, to install the exact npm version and check it.

## How long does a Build take?

`npx forge ci:timings` compares step times across recent Build runs. Native compilation and the six test suites take most of each job. osx-x64 is the slowest host and sets the Build's length: in the ten runs to 2026-10-11 its job took a median of 48 minutes and at most 59. The shared `checks` job adds about five minutes before the matrix starts. [Task 71](../../../archived/cli-development/tasks/task71-streamline-build-release-pipeline.md#timing-analysis) records the full analysis.

## Guides and open work

- The [`forge` guide](../../../../../scripts/delivery/README.md) lists every command, its options, and the workflow-step-to-command table.
- The [development guide](../../../../../docs/development.md#ci-and-releases) explains the contributor workflow, release triggers and npm credentials.
- [Task 71](../../../archived/cli-development/tasks/task71-streamline-build-release-pipeline.md) records the pipeline decisions and their evidence.
