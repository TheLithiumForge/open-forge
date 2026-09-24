---
open-forge:
  description: Hosted build qualification and publication of the first public beta
  tags: [Memory, Working, CLI, Task, Beta, Release, Contextual, Active]
---

# Task 51: Public beta release

## Accepted scope

On 2026-09-24 the maintainer authorized checking GitHub Actions, fixing pipelines
directly on `develop`, committing and pushing those fixes, and publishing usable
beta packages to GitHub and npm after successful qualification.

Use `0.9.0-beta.1`, npm channel `beta`, and a GitHub prerelease. Retain all six
supported native targets. Do not publish untested packages or bypass failing
checks. Follow the repository's plain past-tense commit subjects.

## Current state

The initial public Build run is
[35936169589](https://github.com/TheLithiumForge/open-forge/actions/runs/35936169589),
for `6c8bec4fce09c9da5285ec9ca97c3bdc87186ac4`. Shared checks passed and native
jobs started. Earlier runs were prevented from starting by GitHub's account
billing restriction. The repository has an `NPM_TOKEN` secret configured;
successful publication must still establish that it has the needed access.

The product version is synchronized through `npm run version:bump -- 0.9.0-beta.1`.
The bump exposed six package layout failures: the tests staged the current
product version but expected a hardcoded `0.0.0` development version. Their
layout fixtures now pass an explicit independent prerelease version. Existing
version tests cover stable and beta SHA qualification separately.

## Verification and next action

- `npm run check:delivery`: passed after `npm ci --ignore-scripts --no-audit --no-fund`.
- `npm run test:delivery`: 50 passed, none failed or skipped.
- `npm run test:package-layout`: all seven corrected tests passed, none skipped.
- Push the candidate to `develop`; require the exact candidate's shared checks,
  all six native jobs, and their installed-package journeys to pass.
- Publish through `release.yml`, then verify all seven npm packages, beta tags,
  GitHub archives/checksums, and a fresh install from the public registry.

No publication has occurred at this checkpoint. The default branch is `main`
and does not yet expose the Release workflow for manual dispatch. A matching
version-tag push can invoke the workflow from the qualified candidate.

## Windows pipeline correction

Candidate `7b99c9fe0` passed shared checks, Linux x64/ARM64 and macOS ARM64.
Both Windows jobs failed the same four Integration cases because directory ACL
denials were ineffective. The existing assertions caught the missing fixture
condition rather than accepting false permission evidence.

Run [35938703904](https://github.com/TheLithiumForge/open-forge/actions/runs/35938703904)
tested an owned directory deny rule under PowerShell on both Windows runners;
both probes passed. The test stages used Git Bash. Its
[MSYS runtime](https://github.com/msys2/msys2-runtime/blob/master/winsup/cygwin/sec_helper.cc)
enables backup/restore privileges at startup for administrative tokens, allowing
descendants to bypass file ACLs.

Windows test and package stages now use PowerShell and explicitly propagate
the native command exit code after writing the log. Unix stages retain Bash.
The diagnostic probe is removed after establishing the shell difference.
No test, expected result, skip rule, product code, dependency or supported
platform changes. Hosted managed/native execution on both Windows architectures
is the decisive verification; local non-elevated execution cannot prove this
runner-specific correction.

## Portable Windows diagnostic evidence

Run [35939237027](https://github.com/TheLithiumForge/open-forge/actions/runs/35939237027)
confirmed the permission correction on both Windows architectures. Their only
remaining Integration failure was the partial Extension Create diagnostic:
its 240-character limit cut the runner's longer temporary path before snapshot
normalization could identify it. The existing capture contained a local-length
fragment (`<temp>/ope...`). A short temporary root also changed the expected
fragment, so changing CI's root alone could not make this evidence portable.

The two diagnostic snapshots now retain complete expected text with the owned
source placeholder. A comparer local to Extension Create expands that coordinate
before applying the independently stated 240-character bound, then compares the
unchanged received diagnostic. The test also checks the complete underlying
denied-path cause. Primary snapshots, filesystem effects, permission assertions,
and the shared normalizer's rejection of ambiguous fragments remain intact.
No production output or skip rule changes.

Focused local evidence on Windows x64, Release managed Integration:

- `dotnet build src/cli/tests/integration/OpenForge.Cli.IntegrationTests/OpenForge.Cli.IntegrationTests.csproj --configuration Release --no-restore --nologo -v quiet`: passed, no warnings.
- `artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.exe --filter-class '*ExtensionCreateBeforeOutputSnapshotTests' --no-ansi --progress off`: 11 passed, none failed or skipped.
- The fully qualified `PartialWriteFailure` method passed separately with both
  short and long owned temporary roots supplied through `TEMP`/`TMP`.
- `npm run check:dotnet` and `npm run format:delivery:check`: passed.
- The focused case also passed with `OPENFORGE_SNAPSHOT_UPDATE=1`; before/after
  hashes confirmed that every expectation retained exactly the same bytes.
  Local toolchain: .NET SDK 10.0.101, Node 26.3.1, npm 11.16.0. Hosted
  qualification uses the workflow's pinned SDK and Node versions.
- Read-only review confirmed exact comparison and identified update-mode path
  leakage as a risk. Template comparisons now remain in verify mode during
  snapshot updates, preventing raw machine paths from replacing the templates.

The earlier candidate also passed macOS Intel, so every Unix platform has now
completed its full managed/native and installed-package gates. A new Build must
qualify all six targets together at the corrected source revision.

The full local Integration run then found one additional corpus invariant:
text snapshots require forward-slash paths. The diagnostic templates now use
that form; their local comparer converts only placeholder-owned path separators
before expansion. The received output remains untouched. The superseded hosted
run was cancelled before qualification rather than published.

The corrected full managed Integration run passed: 2,479 passed, 17 expected
Windows platform exclusions, zero failures among 2,496 discovered tests.
Command: the Release Integration executable with `--parallel collections
--minimum-expected-tests 2400 --fail-warns on --fail-skips off --no-ansi
--progress off --report-xunit-ctrf --report-xunit-ctrf-filename results.json
--results-directory artifacts/beta-release/local-managed-integration-portable`.
The delivery `qualifyReport` validator accepted its CTRF report for `win32`.
Both short-root and long-root focused reruns passed after the separator correction.
