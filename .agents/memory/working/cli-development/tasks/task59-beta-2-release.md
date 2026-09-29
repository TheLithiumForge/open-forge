---
open-forge:
  description: Open Task 59 to publish 0.9.0-beta.2 once the documentation site is merged, so the npm package shows the README, the website, and the repository
  tags: [Memory, Working, Task, Release, Beta, Package, Contextual, Active]
---

# Task 59 — Beta 2 release

**Reviewed on 2026-09-28:** [review](../../../emerging/analysis/open-task-review/task59-beta-2-release.md). Recommendation:
Do next. The review names any details in this record that are out of date.

## Outcome

Recorded at the maintainer's request on 2026-09-25. The published
`0.9.0-beta.1` npm package has no README, no website link, and no repository
link, so its npm page tells a visitor almost nothing. Beta 2 ships the
documentation site work and a package page that explains Open Forge and links
to the site and GitHub.

**Original direction:** the maintainer asked for the package fix and for a beta 2 after
the documentation work is merged. Merging, tagging, and publishing are the
maintainer's steps. This record authorizes none of them.

**Already integrated** into local `develop` and `main` with Task 57:

- The wrapper package now includes the repository `README.md` and declares
  `homepage` (the documentation site), `repository`, `bugs`, and `keywords`.
  The six native packages carry the same links.
- `wrapper-artifact.ts` expects the README in the packed wrapper, and the
  staging, packing, and publication tests assert the README and the links.
- `npm run test:delivery` passed 50 of 50 and `npm run test:package-layout`
  passed 7 of 7 on Windows, with Windows `tar` ahead of Git's GNU `tar` on the
  `PATH`. GNU `tar` misreads `C:\` temporary paths, so those tests are meant for
  Linux CI or bsdtar.

**Done when:**

- [ ] The documentation work (Tasks 52 and 57) is merged into `develop` and
      `main`, and GitHub Pages serves the site.
- [ ] The version is bumped with `npm run version:bump -- prerelease --preid beta`,
      reviewed, and committed.
- [ ] A `v0.9.0-beta.2` tag runs the release workflow, and all six native hosts
      qualify.
- [ ] The npm page shows the README, the website, and the repository, and
      `npm install -g @thelithiumforge/open-forge@beta` installs beta 2.

## Current State

### Build and test repair, 2026-09-28

The maintainer requested a local CLI build and test repair. This bounded
follow-up refreshes stale payload expectations after the shipped CLI Skill was
added. It does not authorize release steps.

Execution capsule: routine, reversible test maintenance on
`codex/cli-build-tests`, based on `a8bbf4677a62b4ee67de27bf437db97d760dcd82`.
The CLI manages local user Markdown, but this change affects only test
expectations and this receipt. Existing real temporary workspaces, generated
serialization and snapshot support provide the required evidence. Exceptional
machinery: none. Preserve production behavior and independent safety assertions.
Run focused Integration evidence for changed expectations, then the complete
managed build and test command. Native AOT is not triggered by an expectation-only
refresh with unchanged production, resources and build configuration.

Baseline: the Release build passed with zero warnings and errors. Integration
executed 2,496 cases: 2,454 passed, 25 failed, and 17 were platform skips.
Failures comprise one stale directory-count assertion and 24 snapshot cases
covering Install, Status and Update.

The repair updates 261 snapshot files and the Install serialization directory
count from 11 to 12. Reviewed changes represent the added shipped CLI Skill,
its parent directory, the resulting effects and inventory counts, and current
shipped-text measurements. Production behavior and snapshot comparison rules
are unchanged.

Reproduction from the repair worktree root:

- `npm run restore -- --offline` prepared cached .NET dependencies without feeds.
- With `OPENFORGE_SNAPSHOT_UPDATE=1`, run
  `dotnet artifacts/bin/OpenForge.Cli.IntegrationTests/release/OpenForge.Cli.IntegrationTests.dll --filter-class '*InstallBeforeOutputSnapshotTests' '*StatusBeforeOutputSnapshotTests' '*UpdateBeforeOutputSnapshotTests' '*UpdateDirectoryRestorationIntegrationTests' --parallel none --minimum-expected-tests 1 --no-ansi --progress off --results-directory artifacts/test-results/snapshot-refresh`.
  This explicit refresh passed 77 cases. Unset the variable before verification.
- `npm test -- --no-restore` rebuilds the Release development executable and
  verifies all three managed test projects against the checked-in expectations.
- `dotnet format whitespace OpenForge.Cli.slnx --no-restore --verify-no-changes --include src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Serialization/InstallGeneratedSerializationTests.cs`
  and `git diff --check` passed.

The host used .NET SDK 10.0.101 and Node v26.3.1 on Windows x64. No Native AOT
qualification or fresh dependency vulnerability audit is claimed.
Final managed verification completed on 2026-09-29 with exit 0. The fresh Release
build reported zero warnings and errors. Unit passed 3,424 of 3,424 cases,
Integration passed 2,479 of 2,496 with 17 expected platform exclusions, and
EndToEnd passed 247 of 247 against the worktree's freshly published
`open-forge-dev` version `0.9.0-beta.1`. All 6,167 discovered cases were accounted
for, with zero failures. Snapshot updates were disabled for this final run.
The reviewed patch was applied to the original checkout and included in one
local `develop` commit with the maintainer's authorization.
The workspace doctor check remains blocked by 12 existing
`reference.target-alias` findings concerning the `beta-follow-ups` routes.
Those paths and references are unchanged by this repair.

**Prior checkpoint:** the package fix and verified integration snapshot refresh
are included locally. The release authorization and current state follow below.

**Check on the published page:** npm resolves relative README links and images
against the `repository` field. The README's diagram uses a `<picture>` element
with relative paths. If npm doesn't show it, switch its image paths to absolute
`raw.githubusercontent.com` URLs.

## Folded in on 2026-09-28

- **From [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md): refresh the integration snapshots.**
  The old Install, Status and Update snapshots recorded the 14-asset payload.
  The local build and test repair above refreshes them after
  [Task 61](task61-documentation-accuracy-and-voice.md), with complete managed
  evidence. The repair is included in the local `develop` commit.
- **The npm page shows the `latest` version's README.** `latest` still points at
  `0.9.0-beta.1`, which has no README. See the
  [review](../../../emerging/analysis/open-task-review/task59-beta-2-release.md) for the dist-tag options.

## Authorized delivery, 2026-09-29

The maintainer explicitly authorized confirming the documentation and website,
squash merging Task 62 into `develop`, pushing `develop`, and checking the full
build and test results. Beta 2 may be released only after those checks pass.
After publication, assess what remains for a good 1.0.0 release. This direction
supersedes the earlier record's withheld integration and publication boundary.

Execution uses Managed Delivery with root retaining integration and publication.
The completed Task 62 implementation and independent review are retained without
another whole-change review. A Luna max read-only helper checks the release
procedure. No parallel worker may mutate Git refs, version files or release state.

Task 59 “Beta 2 release” (phase 1/3): milestone 1/5.

The phases are integration, release qualification/publication, and published
verification. Milestones are documentation readiness, develop integration,
complete CI qualification, beta publication, and published-package/site checks.
The later 1.0.0 assessment is a separate analysis outcome, not authority to
implement the remaining backlog.

The candidate is the verified `task62-applyto` worktree based on
`d7e8a6d261dd85ac4086584121758ff143640226`. Task 62 passed all six local Windows
managed/native modes, documentation type checking/build, its one independent
review, and the native CLI smoke journey. All 9331 test executions passed with
34 expected platform exclusions. The source and receipts are recorded in
[Task 62 execution](task62/execution.md).

The repository's Build workflow qualifies six native targets and packaging.
Documentation deploys only from `main`. After verified develop integration,
promote the accepted release candidate to `main` as part of the authorized beta
release and website update. Preserve unrelated local files. Publish through the
existing Release workflow, which requires successful builds and publishes the
six native npm packages before the wrapper. Do not bypass its gates or change
release-channel policy. The intended version/tag are `0.9.0-beta.2` and
`v0.9.0-beta.2`.

Current boundary: docs/site source verified; packaging preflight and integration
preparation active. No beta 2 tag or package has been published. Record exact
commit, CI run, release URL, package versions and website evidence as they become
available. Existing repository Doctor findings are unrelated to this release
candidate and were not increased by Task 62.

### Candidate preparation

The committed Task 62 candidate `e11109d09c3e7cea5fb60e70cdf0a8504b202087`
passed a fresh Windows native build, all six test modes (9331 passes and 34
expected platform exclusions), packing, and installation of the generated npm
packages. Delivery checks passed 55 tests, package-layout checks passed seven,
and the documentation type check and production build passed.

Prepare `0.9.0-beta.2` before the final CI run so that CI qualifies the version
to be published. The normal version command changed only `package.json`,
`package-lock.json`, and `Directory.Build.props`, with no dependency changes or
tag creation. The prepared candidate is undergoing version-specific local
qualification before squash integration. The release remains conditional on the
complete CI gate and the documentation deployment.

The existing publication policy assigns the `beta` tag and also advances
`latest` while no stable version exists. Verify both tags after publication.
The normal tag-triggered Release workflow will perform its own six-host
qualification before publishing. The checked remote `main` is behind the
candidate, so earlier statements that it already contains the current
documentation do not establish deployment readiness.
