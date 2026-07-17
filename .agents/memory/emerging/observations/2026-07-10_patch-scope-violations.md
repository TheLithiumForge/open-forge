---
open-forge:
  description: Scope intent repeatedly loses to tool and process working-directory defaults; v7 patch failures recurred in a 2026-07-12 build diagnostic
  tags: [Memory, Observation, Contextual, Candidate, Directive, Tooling, Benchmark]
---

# Observation: Scope Violations Recur At Tool And Process Boundaries

Date: 2026-07-10. Source: the three v7 build-seed run reports in `benchmarks/results/` (Codex GPT-5, no variables), each independently verified by the orchestrator. Scope: one model, one harness; strong recurrence signal, not proof across runtimes.

- In three of three build seeds, the worker's first `apply_patch` call resolved paths against the parent repos folder and created an accidental sibling project directory before correcting itself; the same failure appeared earlier in the v6 worker run, making this four occurrences.
- Routing was not the cause: the same workers read the right routes, followed seeded opinions with high fidelity, and self-reported the violation unprompted every time.
- The failure lives at the editing-tool boundary: workspace-scope instructions are prose, while the patch tool's working-directory default is operational.
- Maintainer diagnosis 2026-07-10: the orchestrator runs from a root folder that subspaces one workspace per agent, so worker tool sessions start with the root as working directory. This is a harness-environment artifact, not a framework routing gap.
- Harness fixes applied 2026-07-10: worker prompt templates now pin and verify the workspace root before the first write, the scope-control directive gained an operational verify-working-directory axiom, the orchestrator prompt requires absolute workspace paths at spawn, and the rubric scores first-attempt scope correctness (a corrected violation caps the dimension at 1).
- Validated 2026-07-10 by the v8 generation: zero scope violations across all three build seeds (v8 synthesis). Lesson generalizes as "operational rules beat prose scope at tool boundaries" - promotion candidate for the future defaults extension.
- Secondary recurrence, also fixed 2026-07-10: workers hand-edited generated index regions because they did not know the CLI existed; the harness now ships an index-regeneration directive stating the global `open-forge` CLI and the exact fallback rule.

## Fresh Recurrence: 2026-07-12

- During the multi-perspective framework assessment, a diagnostic created and validated a safe temporary copy but did not switch the shell process into it before invoking `bun run build`.
- The build therefore ran in the repository and regenerated ignored `dist/` output twice. No tracked file changed; a corrected temporary-directory run succeeded and produced identical hashes across two builds.
- The failure was not path computation or repository routing. The command execution boundary retained the caller's working directory.
- This broadens the observation beyond patch tools: safe target paths do not constrain a process unless the tool call itself receives or asserts the intended working directory.
- Promotion candidate: a runtime/tooling safety directive or adapter should require an explicit working directory on every mutating command and verify the resolved directory immediately before execution. Prose workspace scope remains necessary but is not sufficient.
