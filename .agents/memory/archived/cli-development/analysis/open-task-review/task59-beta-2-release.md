---
open-forge:
  description: "Historical record: Review of Task 59 Beta 2 release, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation"
  tags: [Memory, Analysis, TaskReview, Contextual, Archived, Historical]
---

# Task 59 Beta 2 Release Review

## Archive Status

Archived on 2026-10-04 from `.agents/memory/emerging/analysis/open-task-review/task59-beta-2-release.md` after the maintainer selected Memory cleanup. Superseded analytical baseline; preserve original bodies and current residue owners.

This record preserves historical evidence. The [current CLI development route](../../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

## Question

Is Task 59 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 59](../../tasks/task59-beta-2-release.md)

## Current Conclusion

**Recommendation:** Do next.

**Size:** Small, because the package fix is done and what remains is a snapshot refresh plus the maintainer's release steps.

The release is worth doing now. It gives the npm page a README and ships the `open-forge-cli` Skill. It can't start yet, because the Build workflow is red on `main` and `develop`. The cause is the integration snapshot refresh that [Task 60](../../tasks/task60-cli-skill.md) left open. Two findings also change the plan. The `latest` npm tag points to beta 1, so the package page won't change unless the maintainer moves it. The Task 61 branch also changes the README and the shipped Skill.

## What It Implies

Users installing `@beta` get the CLI Skill and the current documentation links. The npm page can explain Open Forge and link to the site and the repository. The Framework payload gains the Skill already on `main`, with no other Core change. For maintenance, the pipeline gets exercised once more before 1.0.

## State Today

Verified against the repository and public services:

- The wrapper manifest in `scripts/delivery/npm/package-manifests.ts` packs `README.md` and declares `homepage`, `repository`, `bugs`, and `keywords`. The native manifests share the links. The version is still `0.9.0-beta.1` in `package.json` and `Directory.Build.props`.
- **The record is stale about pushing.** Git's reflog shows `origin/main` and `origin/develop` updated by push to `ebf50034` on 2026-09-26. Local `main` and `develop` match them. The Documentation workflow succeeded, and `https://thelithiumforge.github.io/open-forge/` serves the site. The first done-when item is met in substance.
- **Build is red.** Run `36198874706` on `main` failed on all six hosts at the test step. The Linux log reports 23 integration failures. The failures visible in the excerpt are snapshot mismatches in `UpdateBeforeOutputSnapshotTests`. The Status snapshots still record 14 Framework files, the count from before the Skill. The release workflow calls this same Build.
- **npm tags:** the registry shows `beta` and `latest` both at `0.9.0-beta.1`, with no README, homepage, or repository. `release-selection.ts` publishes a prerelease to its channel, and `publish-packages.ts` refuses `latest` for a prerelease. Beta 2 will therefore move only `beta`.

## Dependencies

- **Blocked by:** a green Build, which needs Task 60's snapshot refresh. Integrating Task 61 first is strongly preferred, since it changes the README and the shipped `SKILL.md`.
- **Blocks:** users receiving the CLI Skill and corrected docs through npm, and the path to 1.0.
- **Overlaps with:** Task 60 for the snapshots, Task 61 for the README and Skill text, and Task 52 for Pages.

## Remaining Work

1. Integrate Task 61 into `develop` and `main`.
2. Refresh the integration snapshots with `OPENFORGE_SNAPSHOT_UPDATE=1` on a host where the suite runs. Review each diff and commit it. The payload changes on the Task 61 branch may shift token counts, so refresh once, after it merges.
3. Push, and confirm Build passes on all six hosts.
4. Run `npm run version:bump -- prerelease --preid beta`, review the change to `0.9.0-beta.2`, commit, and push.
5. Push the `v0.9.0-beta.2` tag and watch the release workflow publish six native packages, then the wrapper, then the GitHub release.
6. Decide the `latest` tag. Either run `npm dist-tag add @thelithiumforge/open-forge@0.9.0-beta.2 latest`, or accept that the npm page keeps showing beta 1 until 1.0.
7. Check the npm page. If the `<picture>` diagram's relative paths don't render, switch them to absolute `raw.githubusercontent.com` URLs.
8. Smoke-test `npm install -g @thelithiumforge/open-forge@beta`.

## Pros And Cons

| Pros                                                    | Cons                                                              |
| ------------------------------------------------------- | ----------------------------------------------------------------- |
| The npm page explains the product and links to the site | Another prerelease means more maintainer steps before 1.0         |
| Ships the CLI Skill, which beta 1 lacks                 | The visible fix fails unless `latest` moves                       |
| Exercises the release pipeline before 1.0               | Releasing before Task 61 ships text that Task 61 already corrects |

## Risks And Open Questions

- Accepting refreshed snapshots without reading them could hide a real regression. Review every count and output change.
- I read only part of the failure log. If any of the 23 failures isn't a snapshot count, fix it before the release.
- Moving `latest` to a prerelease means a plain `npm install` gets beta 2. That is already true for beta 1.
- The maintainer decides whether beta 2 also waits for other 1.0 work, or ships as soon as Build is green.

## Next Check

**Action:** after Task 61 merges, refresh and review the integration snapshots, and confirm a green Build on `main`.

**Would change the conclusion:** a Build failure that isn't a snapshot count, or a maintainer choice to skip beta 2 and go straight to 1.0.

**Acceptance needed:** the maintainer, for merging, tagging, publishing, and the `latest` tag.
