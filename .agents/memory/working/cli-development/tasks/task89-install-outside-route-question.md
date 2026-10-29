---
open-forge:
  description: Open Task 89 to let a fresh interactive Install also register one existing folder of the user's own docs as an outside route
  tags: [Memory, Working, CLI, Task, Install, Routing, Include, Wizard, Contextual, Active]
---

# Task 89: Install outside-route question

Status: Open. Design accepted by the maintainer on 2026-10-29. Implementation not started, because the .NET SDK download host is blocked in the current cloud environment.

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

## Open questions

- The exact wording of the question and its explanation line.
- Whether the plan shows the registration as a separate section or as rows in the Install plan.
- How a failed registration after a successful Install is reported, given that the CLI never rolls back effects.

## Next step

1. Install the .NET SDK once `builds.dotnet.microsoft.com` is allowed.
2. Update the Install Interface and Behavior contracts.
3. Add the prompt, the plan rows and the chained application.
4. Add focused Unit and Integration evidence, and the reviewed prompt snapshot.
