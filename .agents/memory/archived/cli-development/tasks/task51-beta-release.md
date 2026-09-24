---
open-forge:
  description: Hosted build qualification and publication of the first public beta
  tags: [Memory, Archived, CLI, Task, Beta, Release, Contextual, Historical, Complete]
---

# Task 51: Public beta release

## Completed outcome

Version `0.9.0-beta.1` is published on GitHub and npm after all six native hosts
passed qualification. Fresh public npm installations by exact version and by
`@beta` passed Framework installation, Core Templates installation, `status`,
and `doctor` on Windows x64. The installed native executable matches the
qualified build and GitHub download byte for byte.

Archived after release verification on 2026-09-24. The current distribution
state is maintained in [CLI Distribution](../../../crystallized/documents/cli/distribution.md).
This record retains the fixes, authorization and release evidence.

## Accepted scope

On 2026-09-24 the maintainer authorized checking GitHub Actions, fixing pipelines
directly on `develop`, committing and pushing those fixes, and publishing usable
beta packages to GitHub and npm after successful qualification.

Use `0.9.0-beta.1`, npm channel `beta`, and a GitHub prerelease. Retain all six
supported native targets. Do not publish untested packages or bypass failing
checks. Follow the repository's plain past-tense commit subjects.

## Initial state

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

## Initial verification

- `npm run check:delivery`: passed after `npm ci --ignore-scripts --no-audit --no-fund`.
- `npm run test:delivery`: 50 passed, none failed or skipped.
- `npm run test:package-layout`: all seven corrected tests passed, none skipped.
- Push the candidate to `develop`; require the exact candidate's shared checks,
  all six native jobs, and their installed-package journeys to pass.
- Publish through `release.yml`, then verify all seven npm packages, beta tags,
  GitHub archives/checksums, and a fresh install from the public registry.

No publication had occurred at that checkpoint. The default branch is `main`
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

## Hosted qualification and publication

Release source: `89438d39015aee71f21e6d2d5a67427f7ab7e520`.
[Build 35942428820](https://github.com/TheLithiumForge/open-forge/actions/runs/35942428820)
passed shared checks and all six native hosts. Every host passed Unit,
Integration, public, native Integration, native public, and public-against-native
execution, followed by its installed npm package journey.

| Host | Unit passed | Integration passed / excluded, each runtime | Public passed / excluded, each of three modes |
| --- | --- | --- | --- |
| Linux x64 and ARM64 | 3,424 each | 2,474 / 22 | 231 / 16 |
| macOS x64 and ARM64 | 3,424 each | 2,468 / 28 | 231 / 16 |
| Windows x64 and ARM64 | 3,424 each | 2,479 / 17 | 247 / 0 |

The `v0.9.0-beta.1` tag points to that source. Its first push registered the
Release workflow. A subsequent REST dispatch could reuse the successful Build
even though the default branch lacked manual-dispatch controls. The redundant
tag-triggered rebuild was cancelled after the replacement dispatch was queued.

[GitHub publication](https://github.com/TheLithiumForge/open-forge/actions/runs/35945159597)
passed and produced the [public prerelease](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.1).
All six portable archive digests match the published `SHA256SUMS`. The downloaded
Windows executable reports `0.9.0-beta.1` and matches the qualified native hash
`cf0ddf7b05ad3bda5facb2a0a9d51c41621559d5df057e8f15d2a0f405a8d39b`.
That exact executable installed the Framework and all seven Core Templates in
an owned scratch workspace, then passed `status` and `doctor` with no problems.
The downloaded Windows/Linux packages also passed checksum checks and contain
identical npm launcher file contents.

The first npm attempt failed before uploading its first package because the
token lacked the required 2FA publishing capability. The maintainer updated
`NPM_TOKEN`. [The retry](https://github.com/TheLithiumForge/open-forge/actions/runs/35945333127)
passed collection but returned E404 creating the first scoped package. A further
attempt, run `35945639195`, again received the 2FA E403. The maintainer corrected
the scope permissions and replaced the repository secret after that run began.

[Run 35945963855](https://github.com/TheLithiumForge/open-forge/actions/runs/35945963855)
used the newest secret and successfully published all six native packages,
followed by the main package. It reused Build `35942428820` at the unchanged
release tag and SHA. Public exact-version endpoints expose all seven packages
as `0.9.0-beta.1`, and the main package depends on exactly those six native
packages at that version. All seven public `beta` tags match. The registry also
reports `latest` for this initial version, although every upload explicitly
used `--tag beta`.

Some package listings briefly returned E404 after publication while exact-version
and dist-tag endpoints already succeeded. All seven full package listings later
resolved successfully with the expected version, beta tag and dependency graph.
Fresh isolated public-registry installations succeeded both for the exact
version and for `@beta`, without install scripts. Each ran `--version`, `--help`,
Framework installation, Core Templates installation, `status`, and `doctor`.
The checks asserted the installed Framework files and all seven templates plus
their entrypoint. Doctor reported no problems across six checks, 19 links and
19 routes. The installed Windows native SHA-256 matches the qualified hash above.

The GitHub release notes now contain the verified npm beta installation command,
portable archive instructions, and the qualification link. README installation
uses `@beta`, and current distribution/development documentation reflects the
completed six-host qualification and publication. No code or release-tag change
was needed for this documentation closeout.
