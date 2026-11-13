---
open-forge:
  description: PR 28 resolver, channel recovery and semantic test corrections with focused verification
  tags: [Memory, Archived, Contextual, Historical, CLI, Delivery, Review]
---

# PR 28 Fixes

PR 28 fixes are delivery corrections that make interpreter resolution and first-alias channel recovery work for issue 24.

The maintainer requested A1, A2, A3 and A5 on 2026-11-13 after reviewing `638610d02e49e98e836c9c81aa42d26bd52ac03a`. A4 stays unchanged under the maintainer's separate-issue direction. This receipt replaces the temporary working checkpoint because local implementation and verification are complete. The [distribution contract](../../crystallized/documents/cli/distribution.md) defines the resulting behavior.

Status: local fixes and focused checks complete. CI and publication are pending.

## Findings

The changes address the review findings:

- A1 - the ARM64 Windows payload wheel has compressed `win_amd64.win_arm64` tags with both expanded tags in `WHEEL`. Hardware markers still select the payload. Python starts the executable in another process, so x64 Python on ARM64 hardware can install that ARM64 wheel. The real pip regression resolves the alias, entry and exactly one platform package for x64 hardware, emulated x64 Python and ARM64 Python.
- A2 - recovery reads the previous release graph from its collected `release.json` with `--from`, or from npm with `--registry`. Apply can also read npm. Offline preview without either source stops before registry contact. An absent previous alias never enters the restore preflight. Candidate alias tag removals need separate review and never run through apply. The regression restores all seven previous packages when the alias's previous version is absent. Another regression checks that a missing established package prevents every mutation.
- A3 - the Node snapshot helper and ten baselines are deleted. Delivery tests have no Imprint bridge. Tests check identity, dependencies, metadata, tags, bytes, permissions, hashes and resolver reports directly. Launcher bytes are compared with the generated source for packaging parity. Independent launcher diagnostic and recovery-preview wording needs a maintainer decision about an Imprint bridge.
- A4 - the shipped Skill and encoded EndToEnd fixture are unchanged by this revision.
- A5 - the alias-option test imports `CiCommand.smoke` and `SmokeArguments`. Option registration and tests share the alias definition. Recovery options and its integration test also use named argument definitions.

## Evidence

The working root is `D:/Repositories/open-forge-worktrees/package-aliases`, on `feature/package-aliases`, based on the reviewed head above. The tools are Node 26.12.1, npm 11.20.0, .NET SDK 10.0.101 and Python 3.13.14 on Windows x64. The fix commit carries this receipt and the tested source.

The focused run discovers and executes 31 checks in nine touched or new test files. It passes 26 Unit checks and five Integration checks, counting the resolver parent and its three subtests. Failures, skips and cancellations are zero. The offline recovery integration test passes again after the shared manifest-name change. TypeScript typecheck and changed-source ESLint pass.

The managed Release build passes with zero warnings and errors in 4 minutes 20.67 seconds. Both required .NET formatting commands exit zero. No C# input changes after those checks. Complete delivery, Unit, Integration, EndToEnd and native suites stay in CI under the revised lane rules. This revision accepts no snapshots.

The resolver uses the generated local wheels, synthetic Windows marker environments and pip's interpreter platform selection. Pip runs with dry-run, ignore-installed, no-index and no-cache options. It performs no install, registry access or native execution. Actual ARM64 hardware execution and pipx/uvx runs are outside this regression's evidence.

The logs have these SHA-256 identities:

| Log                                | SHA-256                                                            |
| ---------------------------------- | ------------------------------------------------------------------ |
| `artifacts/pr28-focused-final.log` | `a0d5b8bc1a08abc59f86d881d12364427b1fa6878570c19841d871dc4b9c7a6c` |
| `artifacts/pr28-build.log`         | `deafab9f562a4dd46f380175cc432dde4b2decfaed0e105c6c593620fec3ec5c` |
| `artifacts/pr28-whitespace.log`    | `174c8dc9cf17ac48ea197665f70e728d3d82e72ff83f44c9a8fb05253aab811e` |
| `artifacts/pr28-style.log`         | `ed051c48f8852dfbf01bf04e86486f6fe1353732a5077134c81268a673b2d17f` |

## Startup failures

Before the tag fix, real pip rejected the ARM64 dependency for x64 Python on ARM64 Windows. The two other cases passed. `artifacts/pr28-resolver-before.log` has SHA-256 `f3c469e901ce60129741e75ce395fc9d51c1ef02781f7c74aee1ea82b9999a02`.

Repeated local Windows Store Python starts sometimes timed out before the probe produced output. Interpreter-path discovery did not remove the failure. The final fixture prefers the Python launcher and runs the three independent resolutions in one interpreter process. An isolated resolver run passes all four checks, then the complete focused selection passes all 31. Python discovery's default order is unchanged for production callers. Its launcher preference and fallbacks have two Unit checks.

The representative timeout log is `artifacts/pr28-focused-before-resolved-python.log`, SHA-256 `47cb714558ca3ab7104210ebf3159f381ae2b065c17ebcc2f5227899f8529ef1`. The final isolated resolver log is `artifacts/pr28-resolver-single-process.log`, SHA-256 `844caf74220ed88b907d800a2a55193a94142890bc3b464732abdf74f1c5518e`.

## Reproduce

Run the focused checks from this worktree in PowerShell:

```powershell
node --test --test-concurrency=1 `
  scripts/delivery/pypi/__tests__/resolver.integration.test.ts `
  scripts/delivery/pypi/__tests__/wheel.test.ts `
  scripts/delivery/pypi/__tests__/environment-markers.test.ts `
  scripts/delivery/pypi/__tests__/version.test.ts `
  scripts/delivery/pypi/__tests__/python.test.ts `
  scripts/delivery/ci/__tests__/alias-options.test.ts `
  scripts/delivery/release/__tests__/restore-channel-plan.test.ts `
  scripts/delivery/release/__tests__/previous-release-graph.test.ts `
  scripts/delivery/release/__tests__/restore-channels.integration.test.ts
npm run typecheck:delivery
npm run build -- --no-restore --log artifacts/pr28-build.log
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

## Bootstrap follow-up

CI run `43430339232`, job `checks`, reported the dependency-free help test failure at `4ee52f90f`. `git ls-files` confirms that `option-names.ts` and `ci/smoke-checks.ts` are committed. The test copies a fixed subset of delivery files into a temporary workspace. Its list omitted both modules after the named option definitions introduced those imports.

The fixture now copies both modules. Production code, public behavior and contracts stay unchanged. The same selected test first reproduces `ERR_MODULE_NOT_FOUND`, then passes its top-level help, every command's help, offline npx and `ci:prepare` checks. It discovers and executes one Integration test with zero failures, skips or cancellations in 28.010 seconds. No snapshots change.

`npm ci --ignore-scripts --no-audit --no-fund` and `npm run check:delivery` pass. The delivery check runs TypeScript, ESLint and Prettier. The source is this worktree on `feature/package-aliases`, based on `4ee52f90f`, using the Windows x64 toolchain recorded above. The test log is `artifacts/pr28-bootstrap-final.log`, SHA-256 `cdafb9cc7ca232d34240df8108deea0ce488a92f4714ede092c2b4ab9526427e`. The pre-fix log is `artifacts/pr28-bootstrap-before.log`, SHA-256 `ffa3b8069b8dc625391a45263e5c540c4b2c1c91331602fdd56d02c9d5ba108e`.

The fresh managed Release build passes with zero warnings and errors in 4 minutes 33.62 seconds. Both required .NET formatting checks exit zero before commit. The fresh gate logs have these SHA-256 identities:

| Log                                           | SHA-256                                                            |
| --------------------------------------------- | ------------------------------------------------------------------ |
| `artifacts/pr28-bootstrap-delivery-check.log` | `f2efe4420eaf33dd13e6565bcf499ccdf91f1c99666986476dfaa43bb95ca9e2` |
| `artifacts/pr28-bootstrap-build.log`          | `024badd3c767520d5bf144ca467d5ecf19c4497e3bf53ed8a5ec654440e4846e` |
| `artifacts/pr28-bootstrap-whitespace.log`     | `174c8dc9cf17ac48ea197665f70e728d3d82e72ff83f44c9a8fb05253aab811e` |
| `artifacts/pr28-bootstrap-style.log`          | `ed051c48f8852dfbf01bf04e86486f6fe1353732a5077134c81268a673b2d17f` |

An outer `npm exec --call` invocation caused a separate nested npx usage error during local verification. Direct Node execution with npm's CLI path fixes that invocation. The final test has no runtime flake. Full suites stay in CI.

To reproduce the focused test from PowerShell, give its child processes npm's CLI path:

```powershell
$savedNpmExecpath = $env:npm_execpath
try {
  $env:npm_execpath = Join-Path (Split-Path (Get-Command node).Source) 'node_modules/npm/bin/npm-cli.js'
  node --test --test-name-pattern=bootstrap scripts/delivery/__tests__/cli.integration.test.ts
} finally {
  $env:npm_execpath = $savedNpmExecpath
}
npm run check:delivery
npm run build -- --no-restore --log artifacts/pr28-bootstrap-build.log
npm run check:dotnet:whitespace
npm run check:dotnet:style
```

## Not decided yet

- Should delivery gain an Imprint bridge for independent Python launcher diagnostic and recovery-preview wording baselines? This revision uses the maintainer-authorized semantic assertion fallback.
