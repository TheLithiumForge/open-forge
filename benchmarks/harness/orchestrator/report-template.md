# Evaluation And Report Contract

The P0 runner no longer accepts a hand-authored Markdown report as evidence. Supply runner-valid evaluation JSON to `finalize`; `evidence/result.json` becomes canonical and `evidence/report.md` is generated from it.

Start from `benchmarks/harness/examples/evaluation.engineering-smoke.example.json`, but copy it to an independent directory outside the source repository and run directory before use. The JSON schema is a structural aid; executable runner validation is authoritative for uniqueness, references, path safety, ratings, and cross-field rules.

## Evaluation Input

- `status`: `complete`, `invalid`, or `aborted`.
- `objectiveAssertions`: reproducible checks with status, exact command when applicable, exit code, duration, linked trace names, and a bounded note.
- `subjectiveRatings`: exactly one of each core 0–2 rating — `directive-compliance`, `memory-growth`, `routing-behavior`, `communication`, `product-fidelity` — plus optional unique `seed-*` ratings.
- `traces`: named external, singly linked regular files outside the source repository and the entire run directory. Relative trace paths resolve from the external evaluation file. The finalizer copies, hashes, and renames them deterministically.
- `notes`: concise caveats or interpretation context, never a substitute for trace evidence.

Every rating rationale should identify observable evidence and explain why the adjacent score was not chosen. Do not encode an execution failure, contamination, or invalid design only as a low rating; use objective assertions and run status too.

For evidence intended for later causal review, include a passing objective assertion whose id is exactly `isolation-fresh-context` and whose `evidenceTraceNames` references at least one declared trace. The trace must independently support the claim. This receipt and the run spec's control fields are audit inputs only; P0 itself always leaves causal eligibility false.

## Generated Outputs

`finalize` writes:

- `evidence/evaluation.json` — normalized evaluation with copied trace records
- `evidence/result.json` — canonical run result and eligibility
- `evidence/report.md` — deterministic human view
- `evidence/checksums.json` — hashes and byte counts for the sealed evidence tree
- `evidence/final-workspace/` — all final regular workspace files outside `.git`, including untracked and ignored output
- `evidence/workspace-final/` and `workspace-final-provenance.json` — committed, staged, and unstaged Git evidence
- `evidence/final-seal.json` — owner-authenticated checksum root
- `evidence/snapshot/meta/harness.*` — exact harness producer source required for same-semantics validation

Use `bun run bench -- render --run <runDir>` to reproduce the Markdown view and `bun run bench:validate -- --run <runDir> --owner-token <ownerToken> --prepared-root <preparedRootSha256>` to check the intact run. Full validation also needs the live sibling workspace and its `.git` repository, both external trust inputs, and a compatible Git/runtime environment. Never edit generated outputs to improve presentation.
