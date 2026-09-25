---
open-forge:
  description: Current CLI task selection and retained task ownership
  tags: [LoadNow, Memory, Working, CLI, Task, Contextual, Active]
---

# CLI Project Control

This ledger owns only current identity, queue state, ownership, and the next
boundary. The ledger as it stood at the beta release is preserved as the
[beta control ledger](../../archived/cli-development/project-control-beta.md).
Older receipts and retired queue state are in the
[archived control ledger](../../archived/cli-development/project-control.md).

## Active task ledger

| ID  | Task                                                                             | State    | Current boundary                                                                     | Owner                   |
| --- | -------------------------------------------------------------------------------- | -------- | ------------------------------------------------------------------------------------ | ----------------------- |
| 30  | [CLI Experience Remediation](tasks/task30-cli-experience-remediation.md)         | Active   | G1/B1/G4 complete and archived; Phase 5-D is next                                    | Root, direct sequential |
| 31  | [Implementation Duplication Removal](tasks/task31-implementation-duplication.md) | Open     | M1/M2/M3/M4 complete; M5 waits for stable structure                                  | Root, direct sequential |
| 52  | [Documentation site](tasks/task52-documentation-site.md)                         | Active   | Integrated into local `develop` and `main`; the maintainer pushes and confirms Pages | Root, direct sequential |
| 53  | [Loading and scoping audit](tasks/task53-loading-and-scoping-audit.md)           | Open     | Recorded for 1.0 polish; not started                                                 | Unassigned              |
| 54  | [Tag trimming](tasks/task54-tag-trimming.md)                                     | Open     | Recorded for 1.0 polish; not started                                                 | Unassigned              |
| 55  | [Alternative root](tasks/task55-alternative-root.md)                             | Open     | Recorded for 1.0 polish; investigation not started                                   | Unassigned              |
| 56  | Memory trim                                                                      | Complete | Completed work archived and integrated into local `develop`                          | Root, direct sequential |
| 57  | [Onboarding and demos](tasks/task57-onboarding-and-demos.md)                     | Complete | Integrated into local `develop` and `main`                                           | Root, direct sequential |
| 58  | [Demo-based evaluations](tasks/task58-demo-evals.md)                             | Open     | Recorded; not started                                                                | Unassigned              |
| 59  | [Beta 2 release](tasks/task59-beta-2-release.md)                                 | Open     | npm package fix integrated; release after the maintainer pushes                      | Maintainer              |
| 60  | [CLI Skill in Core](tasks/task60-cli-skill.md)                                   | Open     | Skill shipped; whether the loader's CLI section shrinks stays open                   | Unassigned              |

The other open Tasks in [Open CLI Tasks](tasks/_tasks.md) are available on
demand. Task 56 has no separate record: its outcome is the archive itself, as
listed in the [Open CLI Tasks](tasks/_tasks.md#archived-on-2026-09-25) route.

## Ownership rules

- The Task record is the current authority for its scope, state, decisions, and
  implementation reality.
- A phase subtask may hold measured analysis data and acceptance evidence, but
  it cannot broaden the parent Task or create a second current plan.
- The Emerging [CLI Experience Audit](../../emerging/analysis/cli-experience-audit/_cli-experience-audit.md)
  and [CLI Design Retrospective](../../emerging/analysis/cli-design-retrospective/_cli-design-retrospective.md)
  are contextual reasoning sources. Do not rewrite them into execution records
  when code findings change.
- Completed work may be recovered from
  [Archived CLI Development](../../archived/cli-development/_cli-development.md),
  but it is not active context.

## Current next action

On 2026-09-25 the maintainer selected [Task 52](tasks/task52-documentation-site.md),
a Docusaurus documentation site, and recorded Tasks 53, 54, and 55 for 1.0
polish. Task 52 is integrated into local `develop`. Merging to `main`, pushing,
and enabling GitHub Pages are the maintainer's steps.

Before that merge, Task 56 moves completed work out of Working and Emerging
Memory. Completed Tasks 38, 45, and 50, the completed Task 30, Task 31, and beta
follow-up packets, and the beta preparation records are archived. Open and
unstarted Tasks stay in place.
