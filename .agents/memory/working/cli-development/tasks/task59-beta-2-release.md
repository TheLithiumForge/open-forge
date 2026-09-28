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

**Direction:** the maintainer asked for the package fix and for a beta 2 after
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

**Now:** package fix integrated, release not started. The integration snapshot
refresh is verified locally and included in the local `develop` repair commit.

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
