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
