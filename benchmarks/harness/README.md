# Benchmark Harness

The developer-only P0 runner turns a declared benchmark composition into a reproducible external workspace and a tamper-checkable evidence bundle. It does not launch, schedule, or supervise an agent.

## Commands

```powershell
bun run benchmarks/harness/runner.ts prepare --spec <run-spec.json> --runs-root <absolute-external-directory>
bun run benchmarks/harness/runner.ts finalize --run <run-directory> --owner-token <token> --prepared-root <sha256> --evaluation <external-evaluation.json>
bun run benchmarks/harness/runner.ts validate --run <run-directory> --owner-token <token> --prepared-root <sha256>
bun run benchmarks/harness/runner.ts render --run <run-directory>
```

`bun run bench -- ...` and `bun run bench:validate -- --run <run-directory> --owner-token <token> --prepared-root <sha256>` are package-script aliases.

`prepare` prints canonical JSON containing the run id, external run directory, worker workspace, snapshotted prompt path, evidence directory, `ownerToken`, and `preparedRootSha256`. The token authenticates the prepared and final roots; it is not one-time and is required for every later validation. Retain both values in an operator-controlled record outside the worker and run bundle. Run the worker with only `workspace/` and the exact snapshotted prompt, collect independent traces outside all protected roots, then finalize once.

## Composition Contract

- `sourceRepo` and every input path resolve relative to the run-spec file. `sourceRepo` must be an existing Git repository with a valid `HEAD` and tree, and Git must be available throughout preparation, finalization, and validation.
- `components` are ordered worker-visible inputs. Select a package's `payload/` or core `src/open-forge`; package roots containing `.git`, `node_modules`, or `orchestrator` material are rejected.
- Worker target paths are relative, contained, NFC-normalized, Windows-portable, and compared case-insensitively after normalization. A collision must have one exact `{ "path", "from", "to" }` override. Unused, reversed, ambiguous, undeclared, or file-as-parent paths fail before the run is claimed.
- `inputs.cli.path`, `inputs.prompt.path`, `inputs.rubric.path`, the executing harness source, the source spec, and every component file are copied and hashed under `evidence/snapshot/`.
- CLI invocation is shell-free and must use `{cli}` and `{workspace}`. Use `[{runtime}, {cli}, ...]` for scripts that need the current absolute runtime executable.
- `--runs-root` must be absolute and must not contain, equal, or sit inside `sourceRepo`, including through existing symlink or junction ancestry. The experiment, claimed run, `workspace/`, and `evidence/` directories must remain non-linked direct descendants of that canonical root.

The normal portable CLI invocation is:

```json
["{runtime}", "{cli}", "index", "{workspace}"]
```

The runner executes the snapshotted CLI for composition, records stdout/stderr/exit/duration and executable hashes, fingerprints and snapshots its own producer source, initializes a clean Git baseline in the worker workspace, and records the source repository's exact HEAD, tree, porcelain status, staged diff, and unstaged diff. Validation requires harness bytes matching that recorded producer, so retain the snapshot and invoke it with a compatible runtime when validating after a harness upgrade. That CLI is not provisioned to the worker. If a scenario expects an `open-forge` command, the orchestration platform must separately provision and declare the exact tool without exposing the runner snapshot or source repository, then capture its availability and identity in an external trace. A dirty source is acceptable as exact engineering provenance; it is never silently described as a clean commit.

The composition CLI is trusted operator code, not sandboxed worker input. It runs with the harness process's filesystem, environment, and network authority. Use only reviewed run specs and CLI artifacts; the source-repository before/after capture detects source drift but cannot contain arbitrary executable side effects elsewhere.

## Evidence Classes

- `engineering-smoke` records development and regression plumbing evidence. `engineeringEligible` means only that evaluation status is `complete` and no canary exposure was detected.
- `pilot` and `confirmatory` are intended corpus classifications. P0 records their declared controls and evidence but does not enforce class-specific isolation or control minimums.
- `causalEligible` and `publicEligible` are always false in P0. Publishing or generalizing a corpus requires a separate review.

An isolation claim in JSON is not proof by itself. A causal candidate for later review needs a passing objective assertion named `isolation-fresh-context`, at least one referenced trace, and a matching copied trace record, but P0 still reports no causal verdict. Canary exposure makes the run invalid. Prompt, rubric, CLI, `.git`, and orchestration material are excluded from the worker-visible manifest.

## Outputs And Truth

Each run has sibling `workspace/` and `evidence/` directories. `claim.json` binds ownership to the one canonical runs-root/experiment/run path, so copying or moving a run invalidates finalization and validation rather than creating a second lock domain. `evidence/result.json` is the canonical result. `evidence/report.md` is generated from it, and `render` reproduces that view. Prepared and final seals authenticate the evidence roots to the holder of the owner token; checksums alone are tamper-evident, not proof of who produced them. `validate` checks executable contracts, hashes, manifests, provenance, workspace deltas, rendering, state, checksums, and seals without repairing anything. It needs the intact run directory, both external trust inputs, the workspace's `.git` repository, and a compatible installed Git that can reproduce the recorded repository queries.

The final workspace capture includes porcelain status, staged and unstaged diffs, the tracked baseline delta, the baseline-to-HEAD diff, and `evidence/final-workspace/`, a byte-for-byte copy of every singly linked regular workspace file outside `.git`. Symbolic links, junctions, special entries, and hard-linked files invalidate capture. The snapshot therefore preserves untracked and ignored output that Git diffs omit. Use only sanitized, disposable workspaces: secrets, `.env` files, caches, dependency trees, and large build products are copied if the worker leaves them there, increasing disclosure and bundle-size risk.

Start from [run-spec.engineering-smoke.example.json](examples/run-spec.engineering-smoke.example.json) and [evaluation.engineering-smoke.example.json](examples/evaluation.engineering-smoke.example.json). The evaluation file and every referenced trace must be external, singly linked regular files outside the source repository, run directory, workspace, and evidence tree; trace paths resolve relative to the evaluation file. The checked-in evaluation is a shape template, so copy it and any trace fixtures to an independent external directory before finalizing. It deliberately records failed isolation and false control claims, making it useful for plumbing tests but never causal evidence.

The [workflow-first scenario set](scenarios/workflow-first/README.md) adds seven explicitly **UNRUN** external-worker definitions for exact workflow selection, no-match choice, explicit opt-out, ordered handoffs, provenance/accord-aware vision and greenfield architecture support derivation, direct delivery from sufficient current truth, and one concrete missing-architecture prerequisite. Automated closure tests validate their run-specs and prepare their worker-visible compositions through the real CLI in OS temporary directories. That is plumbing evidence only; behavioral conclusions require fresh external model runs with interaction and tool traces under the frozen rubric.

The detailed operator sequence is in [orchestrator/runbook.md](orchestrator/runbook.md). JSON schemas in [schemas](schemas) are structural aids; the executable runner validation is authoritative for cross-field uniqueness, references, path safety, and eligibility rules.
