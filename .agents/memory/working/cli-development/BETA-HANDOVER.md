---
open-forge:
  description: Jumpstart for a fresh chat taking over beta preparation, with the state, the order, the traps that defeated earlier agents, and the decisions still open
  tags: [Memory, Working, CLI, Handover, Beta, KeepInMind, Contextual, Active]
---

# Beta preparation — handover

Written 2026-09-17 for a new session picking this up cold.

## The bar

**Everything a user touches is polished, and there are no stupid bugs.**
Architecture work — the project split, the test split, duplication removal — is
explicitly second priority, however much it wants doing.

## Where things stand

- Branch `feature/render-improvements`, **204 commits unpushed. Never push.**
- All four gates green: unit **3,210 / 0 / 0**, integration **2,226 total / 0
  failed / 17 skipped**, end-to-end **163 / 0 / 0**, `dotnet format whitespace`
  exactly **5** pre-existing errors.
- Commands are built at
  `artifacts/bin/OpenForge.Cli/release/OpenForge.Cli.exe`.
- **Never run `open-forge doctor` against this repository** — about 258 MB of
  output. Scratch workspaces only.

## The order

[The task index](tasks/_tasks.md) opens with the full ordering and the reason
each task sits where it does. In short: journeys, then the output audit, then
the two tasks that change what a fresh install contains, then the prose, then
the output sweep. The safety-net tasks — end-to-end observability and capture
coverage — sit above the architecture work because they decide whether we would
find out when something breaks.

## The work this handover is for

Define **scenarios** and **journeys**, per command, with the expected output at
each detail level; use them to find and fix defects; then turn the settled ones
into end-to-end tests.

- [Scenario template](../../../templates/scenarios/scenario.md) and
  [User Flow template](../../../templates/scenarios/user-flow.md) — copy-ready, in
  the Templates route, written for any subject rather than for this CLI
- [A worked scenario, from a real defect](tasks/task41/scenario-example.md)
- [The journeys to define, as a backlog](tasks/task41/journeys-a-c.md), and
  [D–F](tasks/task41/journeys-d-f.md), [G–H](tasks/task41/journeys-g-h.md)
- [What the first run found](tasks/task41/run-1.md)

**Journey** is the path, **scenario** is the unit. Scenario is the BDD word and
already the harness's own — `ReadOutputScenario` carries one action and its
expected outcome — so an authored scenario maps onto a capture and a contract
row without translation. Nothing renames: a test's situation id is the scenario
id, and no capture path moves.

There is deliberately **no third word** for the gap between expected and actual.
A run records a verdict per scenario, and `unclear` is a valid verdict and the
most useful one. "Finding" is the product's own word and "observation" is a
memory route, and inventing a third noun to dodge them made the vocabulary worse
rather than better.

## Where these live

The templates are generic on purpose, so the capability can be lifted into an
Extension later without carrying this repository with it. Neither mentions a
command, a detail level or a CLI.

**A written scenario is a specification, so it belongs in crystallized memory**,
not in a task. Tasks carry the work; crystallized carries the accepted answer.
The natural home is beside the contract it specifies, under
`.agents/memory/crystallized/documents/cli/contracts/<command>/`, so the
guarantee and the situations that demonstrate it sit together. The journeys in
this task folder are a **backlog of what to define**, not the definitions.

## Read this before writing any expected output

Three failure modes let defects reach a beta-candidate build with every gate
green. They are recorded in full in
[run-1.md](tasks/task41/run-1.md); the short version:

1. **A code was reviewed only where its sentence is true.**
   `index.metadata-incomplete` has six captures, all of a genuinely unreadable
   file. The same code fires for a file with no frontmatter, where "could not be
   read completely" is false, and that has no capture. One code, several
   scenarios — found four times in this repository now.
2. **Behaviour was out of scope.** The output revamp specified the wording of
   each status, never whether the status was correct, and named diagnosis work
   as a non-goal. So `doctor` inventing a failed Framework update was nobody's
   job to catch.
3. **A capture proves the output did not change; it cannot prove it is true.**
   A doubled resolution line and a wrong file count were both captured,
   reviewed, and shipped. Reviewers compared output to output. Nobody counted
   the files.

**The practical consequence: write what the output should be before running the
command.** Recording what it currently prints and calling that expected is how a
defect becomes a contract.

## Traps that defeated earlier agents

- **A message is not read alone.** `RouteCreateReportSelector`,
  `RouteUpdateReportSelector` and `LibraryAttachReportSelector` interpolate the
  first finding's message into the headline. Nine proposed improvements were
  rejected for producing "... so nothing was changed. Nothing was changed."
  Always check the rendered frame.
- **Crystallized sources stay current.** A shipped message must match its row in
  `.agents/memory/crystallized/documents/cli/contracts/<command>/interface.md`.
  A unit invariant now fails the build when a captured message and its row
  disagree. Contracts are maintained, not frozen; "frozen" only ever meant that
  stabilised output strings are not to be reworded on a whim.
- **No internal vocabulary in user text**: `lifecycle`, `residual`, `preflight`,
  `projection`, `lease`, `occupant`, `provenance`, `topology`, `semantic`,
  `trusted`, `payload`.
- **Exhaustiveness tests will catch missing wiring.** Adding a finding code
  touches the enum, the definitions, a second wire vocabulary under
  `Presentation`, the selector, the wording, the contract row and two domain
  tests. Let them fail and follow them.
- **Only about a quarter of finding codes appear in any capture.** 604 of 813.
  So a green suite says much less than it looks like it says.

## The agent fleet is currently broken

worker-watch runs fail in 4–7 seconds. `worker-watch doctor` originally reported
`spawnSync codex ENOENT`; the daemon was spawning the bare name, which Node
cannot resolve on Windows, and `.cmd` then failed `EINVAL` under Node 26.
Pointing at the native binary fixes the spawn — `codexVersion` reports
`codex-cli 0.154.0` — but `codexLoginStatus` comes back **empty from the
daemon's environment** while the same binary reports "Logged in using ChatGPT"
from an ordinary shell. Runs still fail.

To keep the binary fix across a restart:

    export WW_CODEX_BIN="C:\nvm4w\nodejs\node_modules\@openai\codex\node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\bin\codex.exe"
    worker-watch daemon stop && worker-watch daemon start

The login half needs the maintainer. Until it is fixed, scenarios are run by
hand, which works and is how the first eight defects were found.

## Open decisions, all the maintainer's

1. **Does `index` continue past a file with thin metadata?** The maintainer's
   stated intent is yes; the accepted G4 catalogue says no. This is a change to
   accepted design, not a bug fix, and it blocks the most common user journey.
2. **Does "no metadata yet" get its own finding code**, separate from "metadata
   could not be read"?
3. **`extension-update.managed-divergence` wording** — the code is reachable and
   its row is now honest, but the sentence has not been reviewed.
4. Whether `ideas`, `observations`, `documents` and `decisions` move into one
   planning Extension or a smaller one that planning depends on
   ([Task 42](tasks/task42-minimal-core.md)).
