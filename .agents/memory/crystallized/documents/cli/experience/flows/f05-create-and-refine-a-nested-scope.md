---
open-forge:
  description: Create and refine a nested scope
  tags: [Memory, Document, CLI, UserFlow, Evergreen]
---

# F05: Create and refine a nested scope

**Who:** A maintainer organizing team guidance by hand.

**Behavior:** The current CLI implements this journey. The [Task record](../../../../../working/cli-development/tasks/task79-forgiving-maintenance.md) retains design decisions and historical reproduction evidence.

## Contract

The [maintenance design](../../technical-designs/forgiving-maintenance.md) and command contracts define completion, rename proof, protected sources and the next action.

## Starting point

ROUTES has guidance and no team/new-topic chain.

## Flow

Carry the actual state between steps. Fork only for an explicit alternative.

| Step | Action                                                                                                               | Scenario                                                   | State passed forward                                                              |
| ---: | -------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------- | --------------------------------------------------------------------------------- |
|    1 | Write `.agents/guidance/team/new-topic/notes.md` with a body and no frontmatter.                                     | [X34](../scenarios/experience.md#x34)                      | The note exists behind two missing entrypoints.                                   |
|    2 | Run `open-forge index --dry-run`, then `open-forge index`.                                                           | [C05-14](../scenarios/commands/c05-index.md#c05-14)        | Index completes both entrypoints, the note metadata and all direct-child Entries. |
|    3 | Run `open-forge route update guidance/team/new-topic/notes --tag Guidance --tag Team`.                               | [C15-02](../scenarios/commands/c15-route-update.md#c15-02) | Only the intentional tag patch changes supplied metadata.                         |
|    4 | Run `open-forge route inspect guidance/team/new-topic/notes` and `open-forge context guidance/team/new-topic/notes`. | [X34](../scenarios/experience.md#x34)                      | Identity, inherited scope and body are usable.                                    |
|    5 | Run `open-forge index`, then `open-forge doctor`.                                                                    | [C02-14](../scenarios/commands/c02-doctor.md#c02-14)       | The repeated maintenance does not churn files and Doctor is clean.                |

## Alternatives and recovery

### Create through the CLI

Existing Route Create missing-parent behavior remains supported. See [C14-09](../scenarios/commands/c14-route-create.md#c14-09). Explicit Init remains available with its existing draft defaults. See [C13-01](../scenarios/commands/c13-route-init.md#c13-01).

### Removed route, unknown root or duplicate entrypoints

Do not invent a root, clear exclusions or choose an entrypoint. Follow the actual boundary action. See [X35](../scenarios/experience.md#x35) and [X36](../scenarios/experience.md#x36).

Index preserves protected sources and completes only eligible absent fields and rooted chains. Incomplete results state completed and skipped work separately. Edit-and-index instructions keep an explicit workspace. Doctor reports eligible completion consistently and prints no Next when clean.

## Final result

Files are discoverable and links resolve after all named choices or edits. Authored bodies and existing metadata survive. No guessed target is adopted.

## Verification

Observe the actual resulting state independently of the report. Record stdout, stderr, exit, changed paths, body and existing-field bytes, then run the printed next action. A snapshot alone does not prove preservation or a clean Doctor.
