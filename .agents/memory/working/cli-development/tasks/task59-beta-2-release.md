---
open-forge:
  description: Record the beta 2 release, package publication, and documentation verification
  tags: [Memory, Working, Task, Release, Beta, Package, Contextual, Complete]
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

**Original direction (2026-09-25):** the maintainer asked for the package fix
and for beta 2 after the documentation work was merged. At that point, merging,
tagging, and publishing remained maintainer steps. The later release
authorization is recorded below.

**Beta 2 package changes, folded in from Task 57:**

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

- [x] The documentation work (Tasks 52 and 57) is merged into `develop` and
      `main`, and GitHub Pages serves the site.
- [x] The version is bumped with `npm run version:bump -- prerelease --preid beta`,
      reviewed, and committed.
- [x] A `v0.9.0-beta.2` tag runs the release workflow, and all six native hosts
      qualify.
- [x] The versioned npm page shows the README, website, and repository links, and
      `npm install -g @thelithiumforge/open-forge@beta` installs beta 2.

## Historical preparation and repair

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
were included locally. The later release authorization and final state follow.

**Package README rendering:** the packaged README uses absolute documentation
site URLs for its diagram assets, so npm does not resolve those image paths
relative to the repository metadata. The exact-version npm page is verified
below.

## Folded in on 2026-09-28

- **From [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md): refresh the integration snapshots.**
  The old Install, Status and Update snapshots recorded the 14-asset payload.
  The local build and test repair above refreshes them after
  [Task 61](task61-documentation-accuracy-and-voice.md), with complete managed
  evidence. The repair is included in the local `develop` commit.
- **Historical npm state on 2026-09-28:** the `latest` tag pointed at
  `0.9.0-beta.1`, which had no README. The publication receipt below confirms
  that the public `beta` and `latest` tags now point at `0.9.0-beta.2`. See the
  [review](../../../emerging/analysis/open-task-review/task59-beta-2-release.md)
  for the original dist-tag options.

## Release authorization and delivery, 2026-09-29

The maintainer explicitly authorized confirming the documentation and website,
squash merging Task 62 into `develop`, pushing `develop`, and checking the full
build and test results. Beta 2 could be released only after those checks passed.
After publication, the remaining work for a good 1.0.0 release would be assessed.
This direction superseded the earlier record's withheld integration and
publication boundary.

Execution uses Managed Delivery with root retaining integration and publication.
The completed Task 62 implementation and independent review are retained without
another whole-change review. A Luna max read-only helper checks the release
procedure. No parallel worker may mutate Git refs, version files or release state.

Task 59 “Beta 2 release” (phase 3/3): milestone 5/5.

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
Documentation deploys only from `main`. The accepted candidate was promoted to
`main` after develop qualification, and the unrelated local files were
preserved. Publication used the existing Release workflow and its six-host
qualification gates. The intended version and tag were `0.9.0-beta.2` and
`v0.9.0-beta.2`.

Current boundary: `0.9.0-beta.2` is published, and all five release milestones
are complete. The final receipts below record release qualification and
published-package verification.

### Pre-integration candidate preparation

The committed Task 62 candidate `e11109d09c3e7cea5fb60e70cdf0a8504b202087`
passed a fresh Windows native build, all six test modes (9331 passes and 34
expected platform exclusions), packing, and installation of the generated npm
packages. Delivery checks passed 55 tests, package-layout checks passed seven,
and the documentation type check and production build passed.

The version was prepared before the final CI run so that CI qualified the
version to be published. The normal version command changed only `package.json`,
`package-lock.json`, and `Directory.Build.props`, with no dependency changes or
tag creation. The prepared candidate passed version-specific local
qualification before squash integration. At that stage, publication still
depended on the complete CI gate and documentation deployment. Both have since
passed, as recorded below.

The existing publication policy assigns the `beta` tag and also advances
`latest` while no stable version exists. Both tags now point to `0.9.0-beta.2`,
as verified below. The tag-triggered Release workflow performed its own
six-host qualification before publishing. At candidate preparation, remote
`main` had not yet received the candidate, so documentation deployment still
needed separate verification. The later promotion and successful deployment
are recorded below.

### Branch integration and CI qualification

The prepared beta 2 candidate passed all six local test modes, with 9331 passes
and 34 expected platform exclusions. Its native build reported no warnings or
errors. Packing and installation of the generated npm packages also passed.
The staged squash tree matched the isolated candidate exactly. Unrelated local
notes remained untouched and outside the commit.

The resulting commit is `00e3ba0294679163d95b42a81295091254debfb7`, pushed to
`develop`. [Build 36511732462](https://github.com/TheLithiumForge/open-forge/actions/runs/36511732462)
completed successfully for that exact commit. All seven jobs passed, including
the six platform jobs and shared verification. The same commit was pushed to
`main`, where [Build 36514567157](https://github.com/TheLithiumForge/open-forge/actions/runs/36514567157)
also completed successfully.

The annotated tag `v0.9.0-beta.2` points to the same commit and was pushed.
That tag triggered the Release workflow, which passed all nine jobs and
published beta 2. The publication receipt follows.

### Documentation deployment

The same candidate was fast-forwarded to `main` and pushed while the last
platform job continued. This allowed the independently verified documentation
to deploy in parallel.

[Documentation 36514567184](https://github.com/TheLithiumForge/open-forge/actions/runs/36514567184)
passed for the candidate. Both the live
[loading guide](https://thelithiumforge.github.io/open-forge/docs/concepts/loading-and-tags)
and [CLI reference](https://thelithiumforge.github.io/open-forge/guides/cli)
returned HTTP 200 and contain `applyTo` and `--for`. Both deployed Framework
diagram files match the verified build byte for byte. The unrelated temporary
directory in the local `main` checkout was preserved.

### Publication and post-publication verification

[Release 36515807876](https://github.com/TheLithiumForge/open-forge/actions/runs/36515807876)
completed successfully with all nine jobs, including all six platform jobs.
GitHub published [release `v0.9.0-beta.2`](https://github.com/TheLithiumForge/open-forge/releases/tag/v0.9.0-beta.2)
at `2026-09-29T03:50:16Z`. The release contains six portable archives and
`SHA256SUMS`. All six archive checksums verified, and the Windows portable and
npm executables are byte-identical.

Registry verification at `2026-09-29T03:51:34Z` confirmed `0.9.0-beta.2` for
the wrapper and all six native packages. The wrapper's `beta` and `latest` tags
point to `0.9.0-beta.2`. All seven packages have the expected homepage,
repository, and bugs metadata, and the wrapper declares the six expected
optional platform dependencies.

A fresh Windows install of the exact version installed Framework and Core
templates. CLI status and Doctor reported zero errors and warnings. A separate
workspace upgraded from public beta 1 through `@beta`. The upgrade completed
with zero errors, preserved custom-file and overwrite hashes, updated the
Loader with `applyTo`, and retained a recovery ZIP whose previous Loader hash
matched the baseline. Upgrade Doctor reported zero errors, zero warnings, and
one informational retained-bundle result. This records the observed upgrade
output without resolving the separate upgrade-contract wording question.

An isolated global `@beta` install reported beta 2 without changing the normal
global installation. The README in both installed packages matched SHA-256
`B52A4A5499D75994AD87828D786BC923167ABA95A33322824DD1D00B4A760717`.
The [exact-version npm page](https://www.npmjs.com/package/@thelithiumforge/open-forge/v/0.9.0-beta.2)
displayed the README, diagram, homepage, and repository links. Checks at 05:58
and around 06:07 local time still showed cached beta 1 content at the bare npm
package URL even though registry `latest` pointed to beta 2. The exact-version
page and registry checks passed. The bare-page display was a transient UI cache
limitation, not a publication failure.

The requested post-release assessment is recorded in the root-authored
[1.0 release readiness analysis](../../../emerging/analysis/one-zero-release-readiness.md).
It is contextual input and does not authorize implementation.
