---
open-forge:
  description: "Historical record: Keep the extension selection wizard readable in small terminals and preserve a clear current step while navigating or resizing"
  tags: [Memory, Task, CLI, Presentation, Wizard, Contextual, Archived, Historical]
---

# Task 72: Extension wizard terminal layout

## Archive Status

Archived on 2026-10-04 from `.agents/memory/working/cli-development/tasks/task72-extension-wizard-terminal-layout.md` after the maintainer selected Memory cleanup. Root accepts bounded wizard correction locally; final combined six modes and focused ConPTY evidence complete.

This record preserves historical evidence. The [current CLI development route](../../../working/cli-development/_cli-development.md) and its open Tasks define current work. Local qualification in this record does not establish later integration or publication.

The correction subsequently shipped in beta6 after all six hosted platforms passed. [Task 70](task70-existing-workspace-adoption-during-installation.md#beta-6-release-complete-2026-10-04) retains the completed integration and public release receipt.

## Outcome

The maintainer reported this issue on 2026-10-02 after demonstrating Open
Forge. During Extension installation, the wizard shifts text around. In a
small terminal it becomes hard to tell where the user is or which step is
active. The maintainer suggested clearing the view instead.

The maintainer selected implementation and verification again on 2026-10-03.
The issue is present on local develop and in the public develop host source;
the suspected other-worktree correction could not be found.

The intended result is a stable, readable current wizard view while choosing
packages and moving between steps. Prefer a fresh view for each step using
the supported terminal clearing or redraw surface. Keep the current question,
focused option, selections, dependency marks, and controls understandable.
Final plan review and confirmation must remain visible before any write.

## Plan

1. Reproduce `open-forge extension install` without package IDs in an
   interactive terminal. Capture navigation, selection changes, and step
   transitions at normal and small sizes, including about 80 by 24 and
   40 by 12 characters. Resize during selection as well.
2. Inspect the shared prompt renderer and terminal adapter. Establish which
   supported clearing or redraw behavior can keep the active view coherent
   when lines wrap or the viewport is shorter than the package list.
3. Implement the bounded layout correction. Keep package selection,
   dependency handling, permission prompts, plan confirmation, cancellation,
   and filesystem effects unchanged.
4. Verify the interactive journey at the captured sizes, with long package
   labels and more choices than fit on screen. Check any other prompts that
   consume a changed shared renderer.
5. Verify the numbered-input fallback, redirected output, `--format json`,
   `--automatic`, and `--dry-run`. Cursor control and screen clearing must
   remain confined to a capable interactive terminal.

## Relevant sources

- [CLI interaction guidance](../../../../guidance/cli-design.md)
- [Public interaction reference](../../../../../docs/cli.md#prompts-and-noninteractive-runs)

Implementation and evidence paths, relative to the repository root:

- Shared prompt renderer: `src/cli/rendering/OpenForge.Cli.Rendering/Presentation/Shared/Prompts/`
- Terminal interaction: `src/cli/shell/OpenForge.Cli.Shell/Shell/Interaction/`
- Host terminal adapter: `src/cli/root/OpenForge.Cli/Hosting/Shared/Interaction/`
- Prompt unit evidence: `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Presentation/Shared/Prompts/`

## Completion evidence

- The active question and selection remain readable while navigating and
  resizing, with no stale options or leftover wrapped lines obscuring them.
- The user can identify the current step, selection, dependencies, and next
  action in the small-terminal captures.
- The plan remains available for review before confirmation, and cancellation
  writes nothing.
- Focused automated evidence and real terminal captures cover the corrected
  behavior. Noninteractive output stays free of terminal control sequences.

## Current state

Selected in the onboarding/preset integration wave on 2026-10-03, superseding
the earlier skip. The read-only investigation checked local develop `de54c1d`,
local origin/develop `634ab070`, and 119 registered worktrees without finding
the fix. At that baseline Windows disabled redraw, redraw elsewhere counted
explicit newlines rather than wrapped rows, and every choice and description was
printed without a viewport limit. Permission context was printed separately
from choices.

The shared host now supplies BCL console geometry and clearing callbacks through
the neutral Shell boundary. Selection views keep the question, focus, position,
dependencies and controls within the current viewport; permission details are
pageable. Incapable or very small terminals use the numbered line fallback.
Final plan review stays outside selection clearing. The accepted shared contract
and stale Extension Install wizard wording are aligned.

The isolated owner has 46/46 focused Unit passes, 141 Integration passes plus one
declared Unix-only platform exclusion, and 7/7 public checks against a fresh
Windows Native AOT CLI. Three actual ConPTY journeys cover 80x24 and 40x12 apply,
and 80-to-40-to-80 resizing followed by final-plan cancellation; four cover
automatic, JSON, dry-run and redirected output. The 23-path delta is integrated
with a checked patch and exact hashes. Real Unix TTY and stdout-only-redirection
with TTY stdin/stderr remain outside this local qualification.

Root accepts the correction locally after the one holistic review and grouped
correction. All six combined Windows x64 modes pass: 4,088 managed Unit,
2,760 managed and 2,760 native Integration passes (17 declared Unix-only
exclusions in each), and 281 passes in each of the three public modes. The
frozen-source receipt is `artifacts/qualification/onboarding-final/receipt.json`
in Root's worktree. No release or real Unix TTY acceptance is claimed.
Focused evidence is retained in the wizard worktree under
`artifacts/wizard-viewport-receipt.json` and `artifacts/wizard-terminal-captures/`.

Root owns Task state and integration under the budgets and protected boundaries
in [Task 73's execution capsule](task73-layered-adoption-and-installation-choices.md#active-execution-capsule-2026-10-03).
Release inclusion remains unselected.

The subsequent Configure horizon also passes all six combined modes: 4,100 Unit,
2,796 managed and native Integration (17 declared platform exclusions each),
and 289 in each public process mode. Final solution formatting passes. ConPTY
repaint controls and blank rows exposed test marker synchronization, corrected
in the existing transcript helper without changing raw captures, assertions or
the deadline. Both failed captures and deterministic replays remain available;
all seven focused cases pass with both rebuilt runners. A whitespace-only Unit
fixture correction has a fresh full Unit pass. Root's final receipt is
`artifacts/qualification/configure-format-final/receipt.json`; the prior
predecessor receipt remains intact.
