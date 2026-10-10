---
open-forge:
  description: How the build, test, package and release system is organized, what runs locally and in GitHub, and the rules that keep every pipeline step runnable from the repository
  responsibility: Define the current structure and rules of the repository delivery pipeline and point to the guides that operate it
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Build, Delivery, Pipeline, Release]
---

# Delivery System

## What does this system do, and where does each part live?

The delivery system builds, tests, packages and releases the Open Forge CLI. Every step is a `forge` command in the repository, so the same step runs on a developer machine and in GitHub Actions.

| Part | Location | Role |
| ---- | -------- | ---- |
| `forge` | [`scripts/delivery/`](../../../../../scripts/delivery/) | TypeScript commands for setup, build, test, packaging, release, CI steps and operator checks |
| npm scripts | [`package.json`](../../../../../package.json) | `npm run <name>` aliases for `forge` commands and leaf tools such as `typecheck` |
| Workflows | [`.github/workflows/`](../../../../../.github/workflows/) | Runner selection, hosted tool setup, artifact transfer, secrets and Pages deployment |

## Rules

- A workflow `run:` step is one `npm run <command>` with arguments. Matrix values are passed as `${{ matrix.rid }}`, which reads the same in bash and PowerShell. The one exception is `git config --global core.longpaths true`, which runs before checkout.
- Logic lives in `forge` commands, written as readable TypeScript with pure functions and small utilities under the [TypeScript Directive](../../../../directives/open-forge/typescript/typescript-source-structure.md). Shell scripts and JavaScript files are not part of the system.
- Every workflow job has a local command. Only hosted tool setup, artifact upload and download, secret-backed npm publication, GitHub release creation and Pages deployment stay in GitHub. Publication has a local `--dry-run` preview.
- Windows runs workflow steps under the runner-default PowerShell. Git Bash can enable backup and restore privileges that bypass the ACL denials the filesystem tests exercise.
- A test skipped on one platform must give a reason declared in [`platform-skips.ts`](../../../../../scripts/delivery/platform-skips.ts). An undeclared reason fails the delivery report.
- `test:built` runs only against the commit and working tree that `build:native` built. Commit before building, and do not edit sources until the tests finish.
- Commands that call `npm run`, such as `ci:job` and `smoke:public`, run through `npx forge` or `npm run`, which supply npm's environment.

## Pipelines

| Workflow | Trigger | Jobs | Local command |
| -------- | ------- | ---- | ------------- |
| `build.yml` | Push to `main` or `develop`, pull requests, manual, or called by Release | `checks` runs `setup` and `verify` once. One matrix job per RID runs `ci:prepare`, `setup`, the delivery plan, `build:native`, `ci:bundle`, `test:built` and `pack` | `npx forge ci:checks`, `npx forge ci:job --rid <rid>` |
| `release.yml` | Version tag `v*`, or manual with a source ref, destination and optional Build run | `select` checks the tag and source. `build` calls `build.yml` unless a successful Build run for the exact commit is supplied. `publish` collects the packages, publishes the six platform packages before the main package, and creates the GitHub release | `release:select`, `release:collect`, `publish:release --dry-run` |
| `docs.yml` | Documentation changes on pull requests and `main` | Builds the site, and deploys it to Pages from `main` | `npx forge docs:build` |

Each Build job writes `build.log`, `bundle.log`, `test.log` and `pack.log` to `artifacts/delivery/logs/<rid>/`, and uploads packages, the downloadable binary and diagnostics. The [CLI Distribution](distribution.md#publication-boundary) document defines the package graph and the complete-release rules that `publish` enforces.

## Qualifying a release

Run these around a release, in order:

1. `npx forge gate:wsl --commit <sha>` before pushing, to run the managed tiers on Linux from a WSL clone and catch undeclared platform skips.
2. `npx forge ci:watch --sha <sha>` after pushing, to follow every hosted run for the commit.
3. `npx forge smoke:public --version <version>` after publication, to install the exact npm version and check it.

## Guides and open work

- The [`forge` guide](../../../../../scripts/delivery/README.md) lists every command, its options, and the workflow-step-to-command table.
- The [development guide](../../../../../docs/development.md#ci-and-releases) explains the contributor workflow, release triggers and npm credentials.
- [Task 71](../../../working/cli-development/tasks/task71-streamline-build-release-pipeline.md) holds the open pipeline work: timing and stall diagnosis, reuse of Build artifacts for releases, retrying only failed hosts, and release cancellation.
