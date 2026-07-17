---
open-forge:
  description: Fresh review found workspace-escape, active-chain, and overwrite omissions in deterministic context lookup
  tags: [Memory, Observation, AgentLearning, Contextual, Candidate, CLI, Routing, Security, Loading]
---

# Observation: Deterministic Context Lookup Does Not Preserve Its Declared Boundaries

Date: 2026-07-12. Source: current `src/cli/cli.ts`, a safe local path-escape proof, generation-11 raw reports, and the Open Forge routing contract.

## Findings

- `find --route` resolves a caller path with `path.join` and no root-containment check. With `src/open-forge` as the selected target, `../../README.md` successfully read the repository README outside that target.
- `find --follow-required` resolves authored Required Routes the same way. A hostile or injected workflow can therefore point outside the workspace and cause an agent to ingest an external file into context or logs.
- `doctor` uses the same unchecked Required Route resolution and treats an existing outside file as valid.
- `find --tag KeepInMind` scans the whole routed tree. The loader contract activates load-policy tags only through loaded parent entrypoints, so global tag search is not the active closeout set.
- `.overwrite.md` companions are excluded from generated entries and deterministic `find` output even though the loader requires reading them after the base. Generation-11 seed-1 missed `loader.overwrite.md`, providing an observed consequence.

## Risk

- Severity is high to critical for secret-bearing workspaces: a route can cross the logical workspace boundary and expose unintended files to an agent or report.
- Even without an escape, the current closeout command can over-load inactive follow-ups and under-load active overwrite instructions.

## Candidate Action

- Centralize containment with resolved-path and realpath checks; reject absolute paths, `..`, drive changes, and symlink escapes in `find`, Required Routes, doctor, and generated-route expansion.
- Add adversarial Windows and POSIX tests.
- Replace the normative global closeout lookup with an active-context resolver that traverses from the loader, emits ancestor contracts and base-plus-overwrite order, follows Required Routes when requested, explains selection, reports cost, and produces a stable context digest.

Suggested promotion destinations after validation: #Core routing/CLI behavior plus a crystallized security decision.
