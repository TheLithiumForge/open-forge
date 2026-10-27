---
open-forge:
  description: Remaining Task 85 Delivery 2 to move an established main context folder, unstarted and awaiting maintainer selection
  tags: [Memory, Working, Task, CLI, Install, Settings, Layout, Contextual]
---

# Task 85: Configurable main root

Status: Delivery 1 is complete and accepted in PR #9, merged as `77a24b2c0`. Its implementation history and acceptance receipt are [Archived](../../../archived/cli-development/tasks/task85-configurable-main-root.md). Delivery 2 has not started and is not selected by the beta18 release.

## Remaining outcome

Move an established installation's context folder through a reviewed `install --configure --main-root <directory>` plan. The same Task identity stays open until that accepted outcome ships. Current Delivery 1 refuses a different established root before effects, including with `--configure`.

## Accepted boundaries

The [main root council](../../../emerging/analysis/main-root-council/record.md) and [Delivery 1 plan](../../../emerging/analysis/main-root-council/plan.md) retain the design and Delivery 2 outline. Before implementation, plan and qualify migration's safety conditions:

- Inventory real ignored and untracked content and reject collisions or unsafe links.
- Protect both locations with ignore rules throughout the move, verify copied bytes before removing originals and preserve later edits during restoration.
- Translate Install-owned ignore rules, keep recorded recovery paths literal and rewrite affected included-file links.
- Preserve fixed settings, ownership, native Skills and workspace-root runtime storage.
- Refuse Library source intersections or mappings that cannot retain one destination root and unchanged suffixes.

Editing `mainRoot` by hand moves no files and grants no migration authority. No Delivery 2 implementation, fixture, qualification or release is claimed. Resume only when the maintainer selects it and its plan is ready.

## Completed Delivery 1

Choosing an absent or empty main root at first installation works across command families. The default remains `.agents`, native Skills remain in `.agents/skills` and controls stay fixed. Task 86 merged as `164fc9667`, Task 85 Delivery 1 as `77a24b2c0`, and Task 87 as `fba519f35`. The [beta18 receipt](../../../archived/cli-development/tasks/task87-routes-outside-main-root.md#beta18-release-2026-10-27) owns combined acceptance and release evidence.
