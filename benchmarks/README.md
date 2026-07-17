# Open Forge Benchmarks

Reproducible dogfood scenarios for observing agent behavior across framework versions, models, seeds, and declared overlays.

Benchmark packages are versioned with the repository but excluded from the npm package. A package's `payload/` is worker-visible input; its `orchestrator/` material is evaluation-only and must never enter the worker workspace.

## Default Path

Use the developer-only runner documented in [harness/README.md](harness/README.md):

```powershell
bun run build
bun run bench -- prepare --spec <run-spec.json> --runs-root <absolute-external-directory>
bun run bench -- finalize --run <run-directory> --owner-token <token> --prepared-root <sha256> --evaluation <external-evaluation.json>
bun run bench:validate -- --run <run-directory> --owner-token <token> --prepared-root <sha256>
bun run bench -- render --run <run-directory>
```

`prepare` resolves the complete composition before claiming a run, snapshots every worker-visible component and orchestration-only input, runs the snapshotted CLI, and creates a clean Git baseline in an external workspace. It returns an `ownerToken` and `preparedRootSha256`; retain both as external trust inputs for finalization and every authenticated validation. The worker gets only that workspace and the exact prompt. `finalize` captures the final Git/tree evidence and a byte-complete regular-file workspace snapshot, writes canonical `result.json`, generates `report.md`, and authenticates the evidence root with the owner token.

The runner is not an agent scheduler. A human or orchestrator still launches a genuinely fresh worker, observes it, executes independent non-mutating verification, records traces, and supplies the evaluation JSON.

## Design Rules

- Fixed seeds, declared treatments. Do not edit a seed inside a run. Every arm difference must be frozen explicitly in the run spec: component/override composition, prompt, rubric, model, runtime, or settings.
- Worker payload and orchestrator evidence stay physically separate. `.git`, `node_modules`, `orchestrator`, CLI, prompt, rubric, and canary material must not appear in the worker-visible manifest.
- One post-composition baseline commit exists before the worker starts. Final evidence captures committed, staged, and unstaged Git deltas plus every singly linked regular workspace file outside `.git`, including untracked and ignored files; links and special entries invalidate capture.
- Prompt, rubric, model revision, runtime, isolation claims, controls, and source dirty state are explicit and hashed.
- A self-asserted clean context is not isolation evidence. P0 records a trace-linked `isolation-fresh-context` assertion and declared controls as audit inputs, but never converts one run's self-contained bundle into a causal verdict.
- Canonical JSON is truth; Markdown is a deterministic rendered view.

## Seeds

- `seed-0-greenfield` — no initial product truth. Tests discovery, option quality, scope defense, and confirmed memory growth in vision or vision-then-build mode.
- `seed-1-rebuild-small` — compact bookmarks CLI truth. Tests basic routing, directives, and whether seeded semantics reach a working implementation.
- `seed-2-rebuild-medium` — work-journal CLI with date, timezone, append-only storage, and immutable-correction constraints that resist common defaults.
- `seed-3-rebuild-large` — multi-package ledger with scoped routes, API/CLI contracts, security constraints, and a planted specification contradiction that should be surfaced rather than silently resolved.

## Overlays

- `variable-dump-tool` — historical context-dump variable, retained for comparisons with older runs.
- `variable-closeout-command` — loader overwrite exposing the one-command #KeepInMind closeout recheck.
- `variable-sessions-keepinmind` — session route and closeout axiom used with the closeout-command control in generation 11.
- Bundled extension packs — declare the exact pack payloads and dependencies being tested as components. Select `payload/`, not a package root.

## Evidence Tiers

`engineering-smoke` is appropriate for development and regression plumbing. `pilot` and `confirmatory` are declared corpus classifications for runs intended to carry stronger predeclared controls and independently evidenced isolation; P0 records their inputs but does not enforce class-specific minimums. P0 hard-disables both causal and public eligibility because one run cannot verify its own random assignment, treatment separation, replication, or external isolation boundary.

The 20 hand-authored generation reports currently in `benchmarks/results/`, plus the older `pre-harness/` reports, are legacy engineering evidence. They are useful raw observations and historical comparisons, not causal proof, because their provenance and isolation were not captured by this runner.

New durable runs live under the external runs root. A revalidatable run requires the entire run directory, including the live workspace and its `.git` repository, the evidence tree, both external trust inputs, and a compatible Git/runtime environment. A curated evidence-only excerpt may still be inspected or rendered from `result.json`, but it cannot pass the harness's full authenticated validation and must be labeled accordingly. Never copy only the Markdown view.

## Operator Material

- [Harness contract](harness/README.md)
- [Runbook](harness/orchestrator/runbook.md)
- [Worker prompt patterns](harness/orchestrator/worker-prompt-templates.md)
- [Core rubric](harness/orchestrator/rubric-core.md)
- [Evaluation/result guidance](harness/orchestrator/report-template.md)
- [Schemas](harness/schemas)
- [Engineering-smoke example](harness/examples/run-spec.engineering-smoke.example.json)
