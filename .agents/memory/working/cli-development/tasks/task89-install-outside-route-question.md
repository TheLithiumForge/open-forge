---
open-forge:
  description: Open Task 89 to let a fresh interactive Install also register one existing folder of the user's own docs as an outside route
  tags: [Memory, Working, CLI, Task, Install, Routing, Include, Wizard, Contextual, Active]
---

# Task 89: Install outside-route question

Status: Implemented on `claude/readme-docs-overhaul-423xd0` for PR #11, pending maintainer review and hosted CI. Design accepted by the maintainer on 2026-10-29.

## Outcome

A fresh interactive Install asks, right after the context-folder question, whether to keep the user's own docs in another folder too. On yes, Install registers that existing folder as an outside route, the same way `route include <folder>` does.

## Accepted design

The maintainer accepted this shape:

1. The wizard asks "Where should Open Forge keep its context files?" as today, with `.agents` recommended.
2. It then asks whether to also set up another folder, such as a personal or team docs workspace. The default is No.
3. On Yes, it asks for an existing folder, such as `docs/team`, and validates it with the folder Include rules.
4. The Install plan lists the registration with the other changes, so one confirmation covers both.
5. Install applies the Framework files first, then runs the existing folder Include operation on the new loader, and reports both results.

The question appears only on a fresh, prompt-capable interactive Install. Preview, JSON, automatic, redirected and `--configure` runs never ask. No new flag is added. Scripts keep running `route include <folder>` after Install.

## Implementation decisions

- One optional text prompt carries the question. Pressing Enter skips it, which gives the accepted default No without a second yes/no prompt.
- The prompt validates the answer with `WorkspaceOutsideRoutePath`, the same rules folder Include uses. The resolver then requires an existing directory, so a refused answer stops before any plan.
- The root host composes the registration from the folder Include operation through an `InstallOutsideRouteRegistration` delegate. Install never imports `Commands.Route.Shared`, and no inclusion code was promoted.
- Registration runs only after a complete Install. It has its own lease and recovery. A failure never rolls back and never changes Install's status, findings or exit. The receipt reports the retry command.
- The plan names the folder in the setup recap. The receipt replaces that line with the registration outcome. JSON output is unchanged, because JSON requests never ask.

## Evidence

Local Linux x64 managed Release with SDK 10.0.403, running as uid 0:

- Build: 0 warnings, 0 errors.
- Unit project: 5,624 passed.
- Integration `Commands.Install*`, `Commands.Route*` and `Architecture*`: 1,745 passed, 27 platform skips. The two Folder Exclude `UnavailableRegisteredDirectory` cases fail because `SetUnixFileMode(None)` does not deny uid 0. Route Exclude is unchanged.
- New: `InstallOutsideRoutePromptTests` (3) and `InstallOutsideRouteSetupIntegrationTests` (10), with reviewed prompt and receipt snapshots.
- `InstallCompositionIntegrationTests` scripts gained one empty line for the new question.

Native AOT and the six-platform gate run in hosted CI.

## Not decided yet

- Whether a later version adds a flag, such as `--include <folder>`, for unattended Install.
