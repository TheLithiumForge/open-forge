---
open-forge:
  description: Task 30 phase 7-S slice 71 adding the two seeded workspace journeys that have no coverage at any boundary
  tags: [Memory, Working, CLI, Task, Subtask, Testing, Scenarios, Contextual, Active]
---

# 71 — The two uncovered journeys

## Outcome

The `dirty` and `relocated` seeded workspace shapes gain in-process journeys, so
every shape this phase names has evidence somewhere.

## Depends on

[70](70-coverage-matrix.md), which is merged. Its journey matrix decides where
each of these belongs and draws a distinction this slice must respect.

## What is missing, measured

Integration files mentioning each seeded shape on 2026-09-17:

| Shape | Files | Disposition |
| --- | ---: | --- |
| `malformed` | 66 | covered |
| `external` | 64 | covered |
| `nested` | 20 | covered |
| **`dirty`** | **0** | **this slice** |
| **`relocated` workspace** | **0** | **this slice** |

**Read slice 70's journey matrix before starting.** It separates two things that
share a word:

- **Relocated workspace shape** — no coverage. This slice owns it.
- **Relocated published artifact** — already deliberately proven by
  `PublishedEmbeddedPayloadProcessTests.RelocatedArtifactReachesEmbeddedPayload`
  and `.RelocatedArtifactReachesEmbeddedExtensions`, which copy the published
  artifact to prove deployment-relative embedded-resource discovery. **Do not
  duplicate that at the in-process boundary; it is process evidence by design.**

Note also that individual drift cases exist today but do not form the named
seeded `dirty` journey. Reuse those fixtures where they fit rather than seeding a
parallel shape.

## Actionable boundary

- Both journeys belong at the in-process `CliCoreApplication.RunAsync` boundary
  unless the thing being proven is genuinely process-only. Slice 70's markers
  (`I-run`, `I-result`, `P`) are the vocabulary; say which you used and why.
- Assert the contracts this phase names, where each is genuinely distinct for
  the journey: encoding, finding subject and `Next` behaviour, exit status,
  write-freedom, and recovery consequence. Do not assert all five mechanically
  where a journey does not exercise one.
- **Every scenario owns an isolated mutable fixture** under the OS temp
  directory. No scenario may depend on the repository checkout or the user's
  profile.
- Adding a journey must not change any command's output. If seeding one of these
  shapes reveals a behaviour that looks wrong, **record it and stop on that
  item** — this slice adds evidence, it does not change behaviour.
- If a shape turns out to be already covered under a different name, say so and
  do not add a duplicate. The measurement counts filename mentions, which is a
  starting point, not proof of absence.

## Acceptance

- A `dirty` workspace journey and a `relocated` workspace journey exist at the
  boundary slice 70's matrix names, each with an isolated fixture.
- Slice 70's journey matrix is updated so those two rows no longer read `G`.
- No command output changed and no capture regenerated.
- All four gates green.

## Changes ledger

- `dirty workspace`: no named seeded `I-run` journey -> `WorkspaceShapeJourneyIntegrationTests.DirtyWorkspaceBlocksInstallWithoutWrites` reuses `InstallOperationWorkspace`, changes one managed file, and proves blocked exit 5, the `install.managed-divergence` subject and `Next`, strict UTF-8 JSON, no workspace writes, and not-required recovery.
- `relocated workspace`: no named seeded `I-run` journey -> `WorkspaceShapeJourneyIntegrationTests.RelocatedWorkspaceRemainsCurrentWithoutWrites` moves an installed isolated fixture to a sibling path, proves composed Status remains complete with the explicit relocated path and no findings/`Next`/recovery, then restores the fixture.
- `coverage matrix`: dirty and relocated workspace rows read `G` -> both read `I-run`; the deliberately process-only relocated published-artifact row remains `P`.
- `command output and captures`: unchanged -> unchanged; the new journeys assert live JSON at `CliCoreApplication.RunAsync` and do not use snapshot or capture regeneration.

## Divergences observed

- `measurement`: slice 70 records 3,205 unit tests and 2,223 integration tests with 17 skips; the live preflight was 3,206 unit tests and 2,224 integration tests with 17 skips. The phase's filename-mention counts still showed dirty 0 and relocated 0 before this addition, so the two journeys were not already present under another filename.
- `fixture naming`: the existing drift evidence uses command-specific fixture names rather than a `dirty` journey name. `InstallOperationWorkspace` was reused for the seeded dirty and relocated shapes; no parallel fixture shape was introduced.
- `gate counts`: the live preflight was unit 3,206 and integration 2,224 / 17 skipped -> the two new integration journeys produced unit 3,206 and integration 2,226 / 17 skipped, with 0 failures; whitespace remained at the five documented pre-existing errors.
- `build execution`: the first post-edit shared-artifact build encountered transient access/contention errors after the source compiled; after removing only the generated `task71` scratch outputs and stopping stale workers from the earlier verification run, the full shared release gates passed. The initial isolated IntegrationTests build and focused two-test run also passed.
