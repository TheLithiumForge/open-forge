---
open-forge:
  description: Issue 24 package alias implementation, local gates and Windows installation smoke receipt
  tags: [Memory, Archived, Contextual, Historical, CLI, Delivery]
---

# Package Aliases
Package aliases are delivery channels that install the selected Open Forge release through the unscoped npm name and PyPI.

For example, `0.9.0-beta.18` gives npm `open-forge@0.9.0-beta.18` and wheel version `0.9.0b18`. The [distribution contract](../../crystallized/documents/cli/distribution.md#alias-and-pypi) records the maintainer's decision through issue #24 on 2026-11-12.

Status: implemented with passing local build, focused tests and Windows installation smoke. CI qualification and publication are pending.

## Scope

The work uses `feature/package-aliases`, based on `1ee8ab02987658fdd07fbc87269aa13bb9990ac3`. It changes delivery TypeScript, workflows, install notes, the shipped CLI Skill and its workspace copy. It changes no C# behavior. Commits stay local.

This record came from the active Issue 24 checkpoint. It is archived because local implementation is complete. The contracts define the channels. This receipt keeps the test results and limits.

## Implementation

Staging generates the alias from the selected version. Its exact scoped dependency provides the launcher and native packages. Release publication orders native packages, the scoped wrapper and the alias.

TypeScript writes one deterministic wheel for each of the six existing targets. Each wheel takes its native bytes from npm staging. Collection checks metadata, native bytes and archive identity before release preparation. Node buffers, SHA-256, CRC-32 and deflate provide the ZIP writer. Python is used for the installed entry point and smoke venv. No Python build tooling or new dependency is added.

The release job prepares checked wheels. A separate OIDC job uploads them only when `PUBLISH_PYPI=true` and the destination is `all`. The [delivery guide](../../../../scripts/delivery/README.md#pypi-setup) gives the one-time publisher setup. The switch defaults off. `smoke:public --aliases` is opt-in and requires both new channels to exist first.

## Local Gates

All commands ran from PowerShell. The final local gate follows the lane rules updated on 2026-11-12 evening. Those rules require a build and focused tests, leave complete and native suites to CI and prohibit a new local native build.

The local results are:

| Gate | Result |
| --- | --- |
| `npm run test:delivery` | 180 passed, one declared Windows exclusion |
| `npm run check:delivery` | TypeScript, lint and formatting passed |
| `npm run test:package-layout` | Seven passed |
| `npm run build -- --no-restore` | Passed, zero warnings and errors |
| Focused payload Unit | 78 passed |
| Focused Status and payload Integration | 11 passed |
| `npm run check:docs` | 13 passed |
| Docusaurus typecheck and build | Passed |
| Local npm alias installation | Passed with packed tarballs |
| Local Windows wheel installation | Passed in a new Python venv |

The tools were Node 26.12.1, npm 11.20.0, .NET SDK 10.0.101 and Python 3.13.14. Root and site `npm ci` and offline .NET restore passed before testing.

The working root is `D:/Repositories/open-forge-worktrees/package-aliases`. The managed build uses Release configuration on Windows x64. Focused tests run serially against managed .NET, without snapshot-update mode. Their final execution takes about three seconds for Unit and 29 seconds for Integration. Full-suite evidence below predates the final focused gate and keeps its own limits.

The managed artifacts used by the focused gate have these SHA-256 identities:

| Artifact | SHA-256 |
| --- | --- |
| Development `OpenForge.Cli.dll` | `b233e796d9034bb428406d5deae6ab19826c856f3f7dc2e2523d8f6f1caee010` |
| Development `OpenForge.Cli.Framework.dll` | `e43f4567823979a9e4766837fdfe60dd1ca24169463f36fcfda0d2d29d7a7568` |
| Unit test assembly | `2d036712d8b0cea4d397a88d4ecb70c139cfe271da0cc5de9fe6187303685c1a` |
| Integration test assembly | `ec43c6c0f26c36d7f06c008fd5a89b24e9f98e5357354ce6007f43e01f2e7c1e` |

The focused managed commands are:

```powershell
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-namespace 'OpenForge.Cli.Core.UnitTests.Framework.Distribution*' --minimum-expected-tests 78 --parallel none --fail-warns on --fail-skips on --no-ansi --progress off
dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class OpenForge.Cli.IntegrationTests.Commands.Status.StatusBeforeOutputSnapshotTests --filter-class OpenForge.Cli.IntegrationTests.Framework.Distribution.WorkspaceLayoutPayloadRenderingIntegrationTests --minimum-expected-tests 11 --parallel none --fail-warns on --fail-skips on --no-ansi --progress off
```

## Windows Smoke

The updated lane rule prevents a fresh native build. The smoke therefore uses the already published `@thelithiumforge/open-forge-win-x64@0.9.0-beta.18` binary as its fixture. Its SHA-256 is `f8510a954aa8407dc0c6e862d0145fe9e40ca9c92dba28b2b7364c91d075b888`. This checks installation packaging. It does not qualify a native build of this branch.

The fixture was downloaded with `npm pack`, then extracted with Windows `tar`. `npm run build:launcher -- --outDir artifacts/alias-smoke-source/launcher` compiled the current launcher. A temporary TypeScript execution artifact called `packNpmPackages()` with the fixture, selected version and compiled launcher. It then called `createWheel()` with `stagedNative` and the repository license. The tarballs and wheel are under `artifacts/alias-smoke-packages`.

The installed checks ran with:

```powershell
npm run smoke:aliases -- --from artifacts/alias-smoke-packages --rid win-x64 --version 0.9.0-beta.18
```

Both the packed alias and `open_forge-0.9.0b18-py3-none-win_amd64.whl` returned `0.9.0-beta.18`. The command installs the three local npm tarballs offline and the wheel with pip `--no-index --no-deps`. It removes its temporary npm installation and venv afterward. Normal CI packaging can use `npm run smoke:aliases` directly after qualified `pack` output exists.

The first wheel smoke failed during venv creation because npm's isolated Windows profile prevented Store Python from starting its venv interpreter. npm keeps its isolated home. Python now keeps its OS profile and uses the temporary venv, with user site packages disabled. The next smoke passed.

## Snapshot Review

The shipped Skill gains one install paragraph of 536 characters and 134 estimated tokens. These reviewed baselines changed:

| Snapshot group | Files | Reason |
| --- | --- | --- |
| Status `HealthyWithExtension` | Seven | All-routed size and startup share |
| Status `InstalledWorkspace` | 42 | The same size and share change across six cases |
| Status `RecoveryBundlePresent` | Seven | The same size and share change with a recovery bundle |
| Payload `CustomGuidanceMatchesReviewedTemplates` | One | The added install paragraph |

All 57 changes were reviewed and accepted. The targeted update ran serially. Diagnostic templates stayed unchanged. The EndToEnd `default-layout.json` fixture changes only the encoded CLI Skill entry. The prior decoded bytes exactly matched the current Skill with that paragraph removed. Other fixture entries stayed the same.

## Earlier Full Runs

Before the lane rules changed, complete managed Unit passed 5,627 tests. Complete Integration initially had five expected-data failures from the Skill change. After the reviewed updates, it passed with 4,900 tests and 60 declared platform exclusions. Both complete suites passed again in later runs.

The first complete EndToEnd run had 470 passes, ten failures and one platform exclusion, out of 481. Seven failures were process or terminal timeouts. Two compared the old default-layout Skill fixture. One failed the outside-install workspace-state comparison. The failed CTRF file also contained an invalid control character. The raw report and log are under `artifacts/delivery/failed-parallel-public`.

After the fixture update, each failed case passed one isolated serial retry. Their names and results are in `artifacts/delivery/isolated-public/summary.json`. The timeout cases were F29 canonical folder with Index first, F08 ambiguous successors with choice 0, F08 unsafe navigation, Help concepts, F25 overwrite, F02 native Skill and explicit-frontmatter terminal installation. The remaining retries covered both default-root fixture cases and outside-install with `.agents`. Those retries do not turn the complete run into a pass.

A second normal run repeated startup timeouts and was stopped. Its log is under `artifacts/delivery/failed-parallel-public-second`. A later complete EndToEnd attempt selected four collection threads without a test filter. It failed the outside-install state comparison with `knowledge/main` and exceeded the existing 20-minute deadline. It was stopped after the lane rule changed. Its partial evidence is under `artifacts/delivery/qualified-managed/public`. No assertion or runner default was changed.

The temporary complete-suite helper first set the Integration minimum to its total population of 4,960. The runner counts executed cases for that option, so it rejected the otherwise passing 4,900 results with 60 exclusions. That was an invocation error. The corrected helper used minimum one and checked the exact report total separately. Earlier evidence is under `artifacts/delivery/qualified-managed-minimum-mismatch`.

The complete-suite attempts used the normal managed assembly paths, `--parallel collections`, `--fail-warns on`, `--fail-skips off`, `--report-xunit-ctrf` and the existing watched-process deadline. The four-thread attempt added `--max-threads 4` only to EndToEnd. The final complete Unit and Integration report totals are 5,627 and 4,960. Temporary execution helpers were removed after this receipt recorded their inputs, commands, results and limits.

## Maintainer Follow-up

CI needs to qualify all six native targets and their installed journeys. The outside-install state-comparison failures and local process timeouts need CI results before release. No complete EndToEnd pass or fresh native qualification is recorded here.

Before publishing PyPI, configure its trusted publisher and GitHub environment, then enable the repository variable. Verify the project name is available to the maintainer. No package, GitHub release, branch or pull request was published during this task.
