# Benchmark Runbook

Orchestrator-only material. Never select this directory as a worker-visible component.

## 1. Freeze The Run

Before `prepare`:

1. Choose the case, arm, replicate, evidence class, model revision, runtime, isolation boundary, and controls.
2. Name every ordered worker-visible component by its exact `payload/` or core `src/open-forge` path.
3. Declare each intentional collision as one exact `{path, from, to}` override. Do not use broad last-writer-wins rules.
4. Freeze the exact prompt and rubric files. If core and seed criteria both apply, create one predeclared rubric artifact rather than changing criteria after observation.
5. For pilot or confirmatory work, freeze the experiment plan and record its id and SHA-256 before any run starts.
6. Build the CLI before the orchestrator session. Treat the built artifact as an input; do not rebuild it between arms.
7. Confirm that `sourceRepo` is an existing Git repository with a valid `HEAD` and tree, and that Git will remain available through validation.
8. Review the run spec and composition CLI as trusted code. The harness does not sandbox this preparation step; it executes with the operator process's filesystem, environment, and network authority.

Use `benchmarks/harness/examples/run-spec.engineering-smoke.example.json` only as a plumbing example. Its controls and isolation fields are deliberately ineligible for causal claims.

## 2. Prepare

```powershell
bun run bench -- prepare --spec <run-spec.json> --runs-root <absolute-external-directory>
```

The runs root must be absolute, external, and non-overlapping with the source repository through real path and existing symlink/junction ancestry. Its experiment, run, workspace, and evidence descendants must remain direct non-linked directories; moving or replacing one invalidates the run. `prepare` prints canonical JSON with `runDir`, `workspaceDir`, `evidenceDir`, `workerPromptPath`, `ownerToken`, and `preparedRootSha256`.

- Store the owner token and prepared-root digest in an operator-controlled record outside the worker and run bundle. The token authorizes finalization and authenticates every future validation; losing either value makes full validation impossible.
- Do not edit `evidence/` or the composed workspace before the worker starts.
- Confirm the prepared state with `bun run bench:validate -- --run <runDir> --owner-token <ownerToken> --prepared-root <preparedRootSha256>` if desired. A prepared run has no result yet; validation authenticates the sealed inputs and baseline and checks that the live worker workspace has not drifted.
- Invalid specs, composition failures, bad source repositories, and other pre-claim failures create no run. A failure after the atomic claim may preserve an invalid partial run; do not reuse it.

The runner snapshots and hashes all inputs, executes the snapshotted CLI without a shell, records CLI and source-repository provenance, then creates the worker workspace's Git baseline.

## 3. Launch A Fresh Worker

The runner does not launch the agent.

1. Create a genuinely fresh worker context using the declared model/runtime settings.
2. Set its working directory to `workspaceDir`.
3. Give it the exact contents of the snapshotted prompt and only the worker workspace as filesystem context.
4. Do not expose `evidenceDir`, owner token, source repository, run spec, CLI, rubric, orchestration material, prior benchmark conversations, or other arms.
5. Capture a platform/runtime trace that independently demonstrates the context and filesystem boundary. A prose assertion by the orchestrator or worker is not an isolation receipt.
6. While the worker runs, do not coach it. For interactive seed-0 work, answer only according to the frozen persona script and record the interaction as a trace.

If the platform necessarily adds system or tool context, record it in runtime settings and the isolation trace. Do not mark `freshContext`, `workerReceivesOnlyWorkspace`, or filesystem/network controls stronger than the evidence supports.

The runner uses its snapshotted CLI to compose the workspace but does not give that artifact to the worker. If the frozen scenario expects an `open-forge` command, provision the exact tool independently through the declared worker runtime, keep the runner snapshot and source repository hidden, and record the provisioned executable's availability and identity in an external trace.

## 4. Verify And Evaluate

After the worker stops:

1. Preserve the workspace as observed. Run only independent checks known not to update snapshots, generated files, dependencies, caches tracked by the workspace, or external state.
2. Store verification output, interaction logs, and the isolation receipt in an independent external directory, outside the source repository and the entire run directory; the finalizer copies declared trace files into evidence.
3. Score each core rating exactly once on the fixed 0–2 scale: `directive-compliance`, `memory-growth`, `routing-behavior`, `communication`, and `product-fidelity`.
4. Add seed-specific ratings only with `seed-` ids.
5. Record objective assertions with status, command, exit code, duration, trace names, and a bounded note. A causal candidate needs a passing `isolation-fresh-context` assertion linked to at least one real trace.
6. Mark the run `invalid` or `aborted` when warranted; do not conceal contamination, missing evidence, or execution failure by lowering a subjective score.

Use `benchmarks/harness/examples/evaluation.engineering-smoke.example.json` only as a shape example. Copy it to the independent external directory before use; do not pass the checked-in file to `finalize`. Trace paths are resolved relative to that external evaluation file. The schemas provide structural checks, while the runner is authoritative for cross-field uniqueness, trace references, path constraints, ratings, and eligibility.

## 5. Finalize, Validate, Render

```powershell
bun run bench -- finalize --run <runDir> --owner-token <ownerToken> --prepared-root <preparedRootSha256> --evaluation <externalEvaluation.json>
bun run bench:validate -- --run <runDir> --owner-token <ownerToken> --prepared-root <preparedRootSha256>
bun run bench -- render --run <runDir>
```

Finalization is exclusive and non-stealable. The claim is bound to its canonical runs-root/experiment/run path; copying or moving a prepared run does not create another valid lock domain. Never delete or replace a finalization lock merely to retry; preserve the run for investigation and start a new run when recovery is uncertain.

`finalize` requires the source repository at its recorded path. It captures the final worker tree and Git state, copies every singly linked regular workspace file outside `.git`, copies and hashes traces, computes eligibility, writes canonical `evidence/result.json`, derives `evidence/report.md`, and authenticates the evidence root. `validate` is read-only and must pass before the result is used.

Final workspace capture includes ignored files, dependency trees, caches, build products, and secrets if present, but rejects symbolic links, junctions, special entries, and hard-linked files. Use a sanitized disposable workspace, keep credentials out of it, and account for the worst-case bundle size before finalizing. Preserve the complete run directory, including `workspace/.git`, the snapshotted harness producer, both external trust inputs, and a compatible Git/runtime environment for future authenticated validation. A subset containing only evidence or a rendered report is an archival excerpt, not a revalidatable run.

Do not hand-edit `report.md`. If an evaluation is wrong, preserve the bad run and create a corrected run; finalized evidence is an audit record, not a mutable draft.

## Role Boundaries

- Developer: defines the harness, schemas, planned experiment artifacts, and built CLI.
- Orchestrator: prepares, launches, observes, independently verifies, evaluates, finalizes, and interprets eligibility.
- Worker: receives only its workspace and exact prompt, performs the scenario, and reports to the orchestrator.
- Persona: an orchestrator role used only when a frozen interactive script requires it.

## Interpretation

- `engineeringEligible` means the evaluation says `complete` and no canary contamination was detected. It is not a verification-quality or product-quality verdict.
- `engineering-smoke`, `pilot`, and `confirmatory` are declared classifications in P0. The runner does not yet enforce stronger class-specific control or isolation minimums.
- `causalEligible` is always false in P0. The run records candidate prerequisites, but a separate control/corpus verifier must establish assignment, treatment separation, replication, and isolation across runs.
- `publicEligible` is always false in P0. A separate corpus review must address replication, selection, attrition, statistical interpretation, and disclosure before public claims.
