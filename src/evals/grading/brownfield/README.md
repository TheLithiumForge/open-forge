# Brownfield grading
Brownfield grading is a verifier runner that checks Layer 4 changes against their case and the frozen Layer 3 rules.

A combined case can pass its repair checks while its feature checks fail. Each check keeps its own result and area, so one group doesn't hide another group's evidence.

## Case inputs

The loader reads `case.json` from the named case folder. The path fields are `request`, `specDelta`, `seedPatch`, `solutionPatch`, `checks` and `answers`. They default to the filenames in the shared Layer 4 case format. Repair cases omit `specDelta`. Feature cases omit `seedPatch`. Paths stay inside the case folder.

The runner reads `supersedes` as clause IDs and `diffScope` as project-relative globs. An optional `alt-solution.patch` beside `solution.patch` gives a second solution for case validation. The source, product documents and case inputs are hashed before the batch. Case records, patches, answer banks and check files stay outside every project.

## Change checks

Each case's `checks/` folder provides `checks.json` and, when a check needs one, `worker.ts`. The manifest has `schemaVersion: 1` and a `checks` array. Each entry has a distinct `id` and `kind`, either `feature` or `repair`. A case has 10 to 20 checks. Combined cases have both groups.

A clause check sets `"method": "clause"` and names one `clause`, such as `R10.2`, that the full frozen Layer 3 checker asserts. It takes that clause's result from the regression run, and the clause leaves the regression set, as a superseded one does. It needs no worker. A case whose checks are all clause checks needs 3 to 20 of them, with distinct clauses, and no `worker.ts`. When the regression run fails, every clause check reports an error with its message.

The verifier starts the trusted worker with Node. Its one argument names an input JSON file with these fields:

- `schemaVersion` - `1`.
- `project` - The submitted source folder.
- `html` - The saved built page, or `null` after a failed build.
- `report` - The output JSON file the worker writes.
- `checks` - The declared IDs and groups.
- `changeType` - `F`, `R` or `FR`.

The worker reads source as data and checks the page through Playwright. It must never import or execute submitted Node code as the checker account. The container has no network or credentials. The checker keeps its private harness and evidence in folders the build account can't enter.

Write `schemaVersion: 1` and a `checks` array to the report. Each declared ID appears once with `status` and nonempty `evidence`. Status is `pass`, `fail`, `error` or `blocked`. A worker decides each check's prerequisites. Missing features mustn't block unrelated repair checks. Missing, duplicate and unknown outcomes reject the report.

Reports leave the container through `copyOutPlain`, then pass the report parser. Submitted reports never stand in for checker output.

## Gates

The regression runner executes the full frozen Layer 3 checker. Grading removes the named superseded clauses before aggregating each check. Clauses the checker lists as limits and clauses left to review are reported as pending, outside the regression gate and coverage. Unexpected errors still fail the gate. Every gradable frozen clause must pass. Other gradable clauses in the same check still count. Feature, repair and regression checks use separate areas. The saved `brownfield.json` artifact reports coverage for each group apart and lists pending clauses.

The diff gate compares against the saved setup manifest. It reports added, changed and deleted paths and whether each matches `diffScope`. Files ignored by the starting project's saved `.gitignore` rules stay outside the comparison. Editing `.gitignore` counts and can't hide other changes. Installed packages, distributions and browser reports at the project root are also outside the comparison. In Open Forge cells, changes to installed Open Forge files, routes and `.agents/` work records are reported and allowed. Other cells get no Open Forge exemption. The protected-input gate checks external hashes before and after verification and reports private grader files found in the submitted project.

Rechecks use the saved setup manifest. They append assessments through the existing history reader.

## Case validation

Before running agents, check one case from the repository root:

```powershell
npm run evals -- validate-case --case l4-f01
```

The command builds the starting project and runs every gate. It then applies `solution.patch` to a fresh starting snapshot and repeats the build. Before running the solution gates, it runs the project's own `npm test` and `npm run test:e2e` as the build account in the verifier. Their output files exercise the saved ignore rules. Both commands must pass with reported tests. If `alt-solution.patch` exists, the command repeats these steps on another fresh starting snapshot. Project code runs only in verifier containers. The command uses no models or sign-ins.

Each change check declares `expected.starting` as `pass`, `fail` or `blocked` and `expected.solution` as `pass`. At least one check in each change group must fail on the starting project. Errors can't count as expected failures.

Illustrative change check entry:

```json
{ "id": "stale-callback", "kind": "repair", "expected": { "starting": "fail", "solution": "pass" } }
```

This entry requires the planted defect to fail the check and each solution to pass it. Gradable frozen regression checks default to `pass` in every phase. For repair and combined cases, the manifest's optional `regressionExpectations` object maps affected `regression-<Layer 3 check ID>` keys to starting statuses. Unknown, pending or superseded IDs are rejected. Feature cases must start with passing frozen regressions. Every project must build, typecheck and pass the diff and protected-input guards. Every solution check must pass.

The command writes `checks/validation.json` with the input hashes, the patch for each phase, expected statuses, actual statuses, evidence and pending clauses. Paths in evidence are removed before saving. Any mismatch exits with code `1`. The report is generated output, so the case loader and grading fingerprints leave it out of their inputs.

To check every case a plan lists, run:

```powershell
npm run evals -- validate-case --all --plan src/evals/experiments/neon-pairs-l4-check.json
```

Every listed case loads before any build starts. The command checks every loaded case and exits with code `1` if any case has a mismatch.

## Run plans

The Layer 4 plans list `l4-f01`, `l4-r01` and `l4-fr01`. The CLI loads every listed case before installing setups or starting attempts. Each case gets its own batch and answer bank. Cases run in plan order and attempts keep the batch's random ordering.

Once the case folders and their known-solution validation are ready, run from the repository root:

```powershell
npm run evals -- qualify --plan src/evals/experiments/neon-pairs-l4-check.json
npm run evals -- run --plan src/evals/experiments/neon-pairs-l4-check.json
npm run evals -- run --plan src/evals/experiments/neon-pairs-l4-full.json
```

The check plan runs once per case and setup. The full plan runs three times per case and setup. Both use Docker and five parallel attempts within each case.
