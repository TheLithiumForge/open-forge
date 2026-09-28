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

**Now:** package fix integrated, release not started. The integration snapshot refresh recorded in [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md) should land before the release.

**Check on the published page:** npm resolves relative README links and images
against the `repository` field. The README's diagram uses a `<picture>` element
with relative paths. If npm doesn't show it, switch its image paths to absolute
`raw.githubusercontent.com` URLs.

## Folded in on 2026-09-28

- **From [Task 60](../../../archived/cli-development/tasks/task60-cli-skill.md): refresh the integration snapshots.**
  The Install and Status snapshots still record the 14-file payload, so the
  Build is red on `main` and `develop`. This blocks the release. Merge
  [Task 61](task61-documentation-accuracy-and-voice.md) first, because it
  changes the shipped Skill, so the snapshots are refreshed once. The refresh
  needs a machine or CI run where the integration suite executes.
- **The npm page shows the `latest` version's README.** `latest` still points at
  `0.9.0-beta.1`, which has no README. See the
  [review](../../../emerging/analysis/open-task-review/task59-beta-2-release.md) for the dist-tag options.
