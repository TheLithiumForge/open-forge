import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  pathExists as exists,
  repoRoot,
  requireSuccess,
  runProcess,
  useTestSandbox
} from "../../tests/support/index.ts";
import {
  buildTreeManifest,
  finalizeRun,
  prepareRun,
  renderResult,
  renderRun,
  sha256,
  stableStringify,
  validateResultDocument,
  validateRun,
  validateRunSpecDocument,
} from "./runner.ts";

const sandbox = useTestSandbox("open-forge-benchmark");
const coreRatingIds = [
  "directive-compliance",
  "memory-growth",
  "routing-behavior",
  "communication",
  "product-fidelity",
];

describe("benchmark harness P0", () => {
  test("refuses an undeclared collision and permits the exact declared override", async () => {
    const fixture = await createFixture();
    const overlay = path.join(fixture.repo, "overlay");
    await fs.mkdir(path.join(overlay, ".agents"), { recursive: true });
    await fs.writeFile(path.join(overlay, ".agents", "loader.md"), "overlay loader\n");
    fixture.spec.components.push({ name: "overlay", source: "overlay", destination: "" });
    await writeJson(fixture.specPath, fixture.spec);

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("Undeclared composition collision");
    expect(await directoryEntriesOrEmpty(fixture.runsRoot)).toEqual([]);

    fixture.spec.overrides.push({ path: ".agents/loader.md", from: "base", to: "overlay" });
    await writeJson(fixture.specPath, fixture.spec);
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    expect(await fs.readFile(path.join(prepared.workspaceDir, ".agents", "loader.md"), "utf8")).toBe("overlay loader\n");
  });

  test("uses NFC and case-folded target identity for portable collision checks", async () => {
    const fixture = await createFixture();
    await fs.mkdir(path.join(fixture.repo, "unicode-a"));
    await fs.mkdir(path.join(fixture.repo, "unicode-b"));
    await fs.writeFile(path.join(fixture.repo, "unicode-a", "Café.txt"), "one\n");
    await fs.writeFile(path.join(fixture.repo, "unicode-b", "cafe\u0301.TXT"), "two\n");
    fixture.spec.components = [
      { name: "unicode-a", source: "unicode-a", destination: "" },
      { name: "unicode-b", source: "unicode-b", destination: "" },
    ];
    await writeJson(fixture.specPath, fixture.spec);
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("Undeclared composition collision");
  });

  test("rejects Windows-unsafe component paths on every host", async () => {
    const fixture = await createFixture();
    const unsafe = path.join(fixture.repo, "unsafe");
    await fs.mkdir(unsafe);
    await fs.writeFile(path.join(unsafe, "CON.txt"), "not portable\n");
    fixture.spec.components = [{ name: "unsafe", source: "unsafe", destination: "" }];
    await writeJson(fixture.specPath, fixture.spec);

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("not portable across Windows");
  });

  test("rejects a planned parent file even when another path sorts between parent and child", async () => {
    const fixture = await createFixture();
    await fs.writeFile(path.join(fixture.repo, "parent-source"), "parent file\n");
    await fs.writeFile(path.join(fixture.repo, "spacer-source"), "sorting spacer\n");
    await fs.writeFile(path.join(fixture.repo, "child-source"), "nested file\n");
    fixture.spec.components = [
      { name: "parent", source: "parent-source", destination: "a" },
      { name: "spacer", source: "spacer-source", destination: "a-b" },
      { name: "child", source: "child-source", destination: "a/x" },
    ];
    await writeJson(fixture.specPath, fixture.spec);

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("a is also a parent file of a/x");
    expect(await directoryEntriesOrEmpty(fixture.runsRoot)).toEqual([]);
  });

  test("allocates unique atomic UUID claims for concurrent prepares", async () => {
    const fixture = await createFixture();
    const [left, right] = await Promise.all([
      prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot }),
      prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot }),
    ]);
    expect(left.runId).not.toBe(right.runId);
    expect(left.runDir).not.toBe(right.runDir);
    expect(left.ownerToken).not.toBe(right.ownerToken);
    expect(await fs.stat(path.join(left.evidenceDir, "claim.json"))).toBeTruthy();
    expect(await fs.stat(path.join(right.evidenceDir, "claim.json"))).toBeTruthy();
  });

  test("rejects an overlapping runs root before creating it", async () => {
    const fixture = await createFixture();
    const overlapping = path.join(fixture.repo, "must-not-be-created");
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: overlapping })).rejects.toThrow("must not overlap sourceRepo");
    expect(await exists(overlapping)).toBe(false);
  });

  test("rejects a non-existent runs root projected through a link into sourceRepo before creating it", async () => {
    const fixture = await createFixture();
    const runsAlias = path.join(fixture.root, "runs-alias");
    await fs.symlink(fixture.repo, runsAlias, process.platform === "win32" ? "junction" : "dir");
    const requested = path.join(runsAlias, "must-not-be-created", "nested");

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: requested })).rejects.toThrow("must not overlap sourceRepo");
    expect(await exists(path.join(fixture.repo, "must-not-be-created"))).toBe(false);
  });

  test("canonicalizes a linked sourceRepo before creating an overlapping runs root", async () => {
    const fixture = await createFixture();
    const sourceAlias = path.join(fixture.root, "source-alias");
    await fs.symlink(fixture.repo, sourceAlias, process.platform === "win32" ? "junction" : "dir");
    fixture.spec.sourceRepo = sourceAlias;
    await writeJson(fixture.specPath, fixture.spec);
    const requested = path.join(fixture.repo, "canonical-must-not-be-created");

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: requested })).rejects.toThrow("must not overlap sourceRepo");
    expect(await exists(requested)).toBe(false);
  });

  test("rejects an experiment run directory link before claiming outside runsRoot", async () => {
    const fixture = await createFixture();
    const outside = path.join(fixture.root, "outside-experiment");
    await fs.mkdir(fixture.runsRoot);
    await fs.mkdir(outside);
    await fs.symlink(outside, path.join(fixture.runsRoot, fixture.spec.experimentId), process.platform === "win32" ? "junction" : "dir");

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("Experiment run directory must be an existing non-linked directory");
    expect(await directoryEntriesOrEmpty(outside)).toEqual([]);
  });

  test("tree hashes are stable and change after a one-byte edit", async () => {
    const root = await createTemporaryRoot();
    await fs.mkdir(path.join(root, "tree"));
    await fs.writeFile(path.join(root, "tree", "a.txt"), "abc");
    await fs.writeFile(path.join(root, "tree", "b.txt"), "xyz");
    const first = await buildTreeManifest(path.join(root, "tree"));
    const second = await buildTreeManifest(path.join(root, "tree"));
    expect(first).toEqual(second);
    await fs.writeFile(path.join(root, "tree", "a.txt"), "abd");
    const changed = await buildTreeManifest(path.join(root, "tree"));
    expect(changed.digest).not.toBe(first.digest);
  });

  test("rejects hard-linked files in a worker tree", async () => {
    const root = await createTemporaryRoot();
    const tree = path.join(root, "hardlink-tree");
    await fs.mkdir(tree);
    await fs.writeFile(path.join(tree, "source.txt"), "shared bytes\n");
    await fs.link(path.join(tree, "source.txt"), path.join(tree, "alias.txt"));

    await expect(buildTreeManifest(tree)).rejects.toThrow("hard-linked file");
  });

  test("executes the snapshotted CLI, ignores a fake PATH command, and keeps the canary outside the worker tree", async () => {
    const fixture = await createFixture();
    const fakeBin = path.join(fixture.root, "fake-bin");
    const sentinel = path.join(fixture.root, "fake-open-forge-ran");
    await fs.mkdir(fakeBin);
    await fs.writeFile(path.join(fakeBin, "open-forge.cmd"), `@echo off\r\necho fake>"${sentinel}"\r\nexit /b 99\r\n`);
    await fs.writeFile(path.join(fakeBin, "open-forge"), `#!/bin/sh\necho fake > "${sentinel}"\nexit 99\n`);
    await fs.chmod(path.join(fakeBin, "open-forge"), 0o755).catch(() => undefined);
    const previousPath = process.env.PATH;
    process.env.PATH = `${fakeBin}${path.delimiter}${previousPath ?? ""}`;
    let prepared;
    try {
      prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    } finally {
      process.env.PATH = previousPath;
    }

    expect(await fs.readFile(path.join(prepared.workspaceDir, ".agents", "generated-index.md"), "utf8")).toBe("generated by snapshotted cli\n");
    expect(await exists(sentinel)).toBe(false);
    const canary = (await fs.readFile(path.join(prepared.evidenceDir, "canary.txt"), "utf8")).trim();
    const manifest = await buildTreeManifest(prepared.workspaceDir, { allowGitDirectory: true });
    expect(manifest.entries.some((entry) => entry.path.startsWith(".git/"))).toBe(false);
    for (const entry of manifest.entries.filter((item) => item.type === "file")) {
      expect((await fs.readFile(path.join(prepared.workspaceDir, ...entry.path.split("/")), "utf8")).includes(canary)).toBe(false);
    }
    expect(path.relative(prepared.workspaceDir, prepared.evidenceDir).startsWith("..")) .toBe(true);
  });

  test("refuses orchestration-only artifacts and reserved package directories in visible composition", async () => {
    const fixture = await createFixture();
    fixture.spec.components = [{ name: "leak", source: "inputs", destination: "leaked" }];
    await writeJson(fixture.specPath, fixture.spec);
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("orchestration-only");

    const packageRoot = path.join(fixture.repo, "whole-package");
    await fs.mkdir(path.join(packageRoot, "orchestrator"), { recursive: true });
    await fs.writeFile(path.join(packageRoot, "payload.txt"), "payload\n");
    await fs.writeFile(path.join(packageRoot, "orchestrator", "rubric.md"), "secret rubric\n");
    fixture.spec.components = [{ name: "package", source: "whole-package", destination: "" }];
    await writeJson(fixture.specPath, fixture.spec);
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("reserved orchestrator");
  });

  test("resolves linked component ancestry before applying reserved-content checks", async () => {
    const fixture = await createFixture();
    const hidden = path.join(fixture.repo, ".git", "hidden-component");
    const alias = path.join(fixture.repo, "component-alias");
    await fs.mkdir(hidden, { recursive: true });
    await fs.writeFile(path.join(hidden, "secret.txt"), "must not become worker input\n");
    await fs.symlink(path.join(fixture.repo, ".git"), alias, process.platform === "win32" ? "junction" : "dir");
    fixture.spec.components = [{ name: "linked", source: "component-alias/hidden-component", destination: "" }];
    await writeJson(fixture.specPath, fixture.spec);

    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("reserved .git");
    expect(await directoryEntriesOrEmpty(fixture.runsRoot)).toEqual([]);
  });

  test("does not treat a reserved sourceRepo parent name as worker-visible content", async () => {
    const parent = await createTemporaryRoot();
    const reservedParent = path.join(parent, "orchestrator");
    await fs.mkdir(reservedParent);
    const fixture = await createFixture(reservedParent);

    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    expect(await fs.readFile(path.join(prepared.workspaceDir, ".agents", "loader.md"), "utf8")).toBe("base loader\n");
  });

  test("rejects a CLI-created worker junction or symlink before Git baseline creation", async () => {
    const fixture = await createFixture();
    await fs.writeFile(path.join(fixture.repo, "benchmark-cli.mjs"), `import fs from "node:fs/promises";
import path from "node:path";
const [, workspace] = process.argv.slice(2);
const evidence = path.resolve(workspace, "..", "evidence");
await fs.symlink(evidence, path.join(workspace, "evidence-link"), process.platform === "win32" ? "junction" : "dir");
`);
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("symbolic link or junction");
  });

  test("rejects a CLI-created .git directory before runner baseline initialization", async () => {
    const fixture = await createFixture();
    await fs.writeFile(path.join(fixture.repo, "benchmark-cli.mjs"), `import fs from "node:fs/promises";
import path from "node:path";
const [, workspace] = process.argv.slice(2);
await fs.mkdir(path.join(workspace, ".git", "hooks"), { recursive: true });
await fs.writeFile(path.join(workspace, ".git", "hooks", "pre-commit"), "malicious hook");
`);
    await expect(prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot })).rejects.toThrow("pre-existing .git directory");
  });

  test("records a detected canary exposure as invalid evidence instead of overclaiming success", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const canary = await fs.readFile(path.join(prepared.evidenceDir, "canary.txt"), "utf8");
    await fs.writeFile(path.join(prepared.workspaceDir, "leaked-context.txt"), canary);
    const result = await finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath: await createEvaluation(fixture.root),
    });
    expect(result.status).toBe("invalid");
    expect((result.eligibility as Record<string, unknown>).engineeringEligible).toBe(false);
    expect((result.eligibility as Record<string, unknown>).causalReasonCodes).toContain("CANARY_EXPOSED");
    expect((await validateRun(prepared.runDir, validationCredentials(prepared))).ok).toBe(true);
  });

  test("forces a canary-contaminated aborted evaluation to invalid consistently", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const canary = await fs.readFile(path.join(prepared.evidenceDir, "canary.txt"), "utf8");
    await fs.writeFile(path.join(prepared.workspaceDir, "aborted-leak.txt"), canary);
    const result = await finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath: await createEvaluation(fixture.root, { status: "aborted" }),
    });

    expect(result.status).toBe("invalid");
    expect(await validateRun(prepared.runDir, validationCredentials(prepared))).toEqual({ ok: true, errors: [] });
  });

  test("rejects a workspace root replaced by a symlink or junction", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const relocated = path.join(fixture.root, "relocated-workspace");
    await fs.rename(prepared.workspaceDir, relocated);
    await fs.symlink(relocated, prepared.workspaceDir, process.platform === "win32" ? "junction" : "dir");
    const evaluationPath = await createEvaluation(fixture.root);

    await expect(finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath,
    })).rejects.toThrow("Workspace directory must be an existing non-linked directory");
    expect(await exists(path.join(prepared.evidenceDir, "finalize.lock.json"))).toBe(false);
    const validation = await validateRun(prepared.runDir, validationCredentials(prepared));
    expect(validation.ok).toBe(false);
    expect(validation.errors[0]).toContain("run layout");
  });

  test("binds a claim to one canonical run path so a copied run cannot finalize", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const copiedRun = path.join(fixture.root, "copied-run");
    await fs.cp(prepared.runDir, copiedRun, { recursive: true });
    const evaluationPath = await createEvaluation(fixture.root);

    await expect(finalizeRun({
      runDir: copiedRun,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath,
    })).rejects.toThrow("does not match the canonical path bound into claim.json");
    expect(await exists(path.join(copiedRun, "evidence", "finalize.lock.json"))).toBe(false);
  });

  test("refuses finalization by a non-owner without taking the finalize lock", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const evaluationPath = await createEvaluation(fixture.root);
    await expect(finalizeRun({ runDir: prepared.runDir, ownerToken: "not-the-owner", preparedRootSha256: prepared.preparedRootSha256, evaluationPath })).rejects.toThrow("Owner token");
    expect(await exists(path.join(prepared.evidenceDir, "finalize.lock.json"))).toBe(false);
  });

  test("rejects evaluation documents and trace sources controlled by the worker workspace", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const externalEvaluation = await createEvaluation(fixture.root);
    const internalEvaluation = path.join(prepared.workspaceDir, "worker-evaluation.json");
    await fs.copyFile(externalEvaluation, internalEvaluation);
    await expect(finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath: internalEvaluation,
    })).rejects.toThrow("Evaluation input must be external");

    const internalTrace = path.join(prepared.workspaceDir, "worker-isolation-trace.log");
    await fs.writeFile(internalTrace, "worker-authored isolation claim\n");
    const evaluation = JSON.parse(await fs.readFile(externalEvaluation, "utf8"));
    evaluation.traces[0].path = internalTrace;
    await fs.writeFile(externalEvaluation, `${JSON.stringify(evaluation, null, 2)}\n`);
    await expect(finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath: externalEvaluation,
    })).rejects.toThrow("Evaluation trace worker-trace must be external");
    expect(await exists(path.join(prepared.evidenceDir, "finalize.lock.json"))).toBe(false);
  });

  test("detects prepared evidence tampering before taking the finalize lock", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const evaluationPath = await createEvaluation(fixture.root);
    await expect(finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: "0".repeat(64),
      evaluationPath,
    })).rejects.toThrow("Prepared root does not match");
    expect(await exists(path.join(prepared.evidenceDir, "finalize.lock.json"))).toBe(false);
    const sealedSpecPath = path.join(prepared.evidenceDir, "spec.json");
    const sealedSpec = JSON.parse(await fs.readFile(sealedSpecPath, "utf8"));
    sealedSpec.model.id = "tampered-model";
    await fs.writeFile(sealedSpecPath, stableStringify(sealedSpec));

    const validation = await validateRun(prepared.runDir, validationCredentials(prepared));
    expect(validation.ok).toBe(false);
    expect(validation.errors.some((error) => error.includes("Prepared sealed file was changed"))).toBe(true);
    await expect(finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath,
    })).rejects.toThrow("Prepared sealed file was changed");
    expect(await exists(path.join(prepared.evidenceDir, "finalize.lock.json"))).toBe(false);
  });

  test("detects live worker workspace drift while a run is still prepared", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    expect(await validateRun(prepared.runDir, validationCredentials(prepared))).toEqual({ ok: true, errors: [] });

    await fs.appendFile(path.join(prepared.workspaceDir, ".agents", "loader.md"), "pre-worker drift\n");

    const validation = await validateRun(prepared.runDir, validationCredentials(prepared));
    expect(validation.ok).toBe(false);
    expect(validation.errors).toContain(
      "prepared worker workspace: worker workspace does not match the sealed prepared worker-visible manifest",
    );
  });

  test("allows only one same-owner finalizer to acquire the immutable lock", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const evaluationPath = await createEvaluation(fixture.root);
    const attempts = await Promise.allSettled([
      finalizeRun({ runDir: prepared.runDir, ownerToken: prepared.ownerToken, preparedRootSha256: prepared.preparedRootSha256, evaluationPath }),
      finalizeRun({ runDir: prepared.runDir, ownerToken: prepared.ownerToken, preparedRootSha256: prepared.preparedRootSha256, evaluationPath }),
    ]);
    expect(attempts.filter((attempt) => attempt.status === "fulfilled")).toHaveLength(1);
    expect(attempts.filter((attempt) => attempt.status === "rejected")).toHaveLength(1);
    expect(String((attempts.find((attempt) => attempt.status === "rejected") as PromiseRejectedResult).reason)).toContain("already claimed");
    expect((await validateRun(prepared.runDir, validationCredentials(prepared))).ok).toBe(true);
  });

  test("records the worker delta, enforces schemas and checksums, and renders deterministically", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    await fs.appendFile(path.join(prepared.workspaceDir, ".agents", "loader.md"), "worker change\n");
    await fs.writeFile(path.join(prepared.workspaceDir, "untracked-output.txt"), "untracked worker bytes\n");
    const evaluationPath = await createEvaluation(fixture.root);
    const result = await finalizeRun({ runDir: prepared.runDir, ownerToken: prepared.ownerToken, preparedRootSha256: prepared.preparedRootSha256, evaluationPath });

    expect(result.status).toBe("complete");
    const inputManifest = JSON.parse(await fs.readFile(path.join(prepared.evidenceDir, "input-manifest.json"), "utf8"));
    expect(inputManifest.artifacts.harness.snapshotPath).toMatch(/^snapshot\/meta\/harness/);
    expect(inputManifest.artifacts.harness.sha256).toBe((result.hashes as Record<string, unknown>).harnessSha256);
    expect(await fs.readFile(path.join(prepared.evidenceDir, ...inputManifest.artifacts.harness.snapshotPath.split("/")))).toBeTruthy();
    expect((result.eligibility as Record<string, unknown>).engineeringEligible).toBe(true);
    expect((result.eligibility as Record<string, unknown>).causalEligible).toBe(false);
    const finalProvenance = JSON.parse(await fs.readFile(path.join(prepared.evidenceDir, "workspace-final-provenance.json"), "utf8"));
    expect(finalProvenance.baselineDiffSha256).not.toBe(sha256(""));
    expect(await fs.readFile(path.join(prepared.evidenceDir, "workspace-final", "git-diff-baseline.patch"), "utf8")).toContain("worker change");
    expect(await fs.readFile(path.join(prepared.evidenceDir, "final-workspace", "untracked-output.txt"), "utf8")).toBe("untracked worker bytes\n");
    expect(finalProvenance.status).toContain("? untracked-output.txt");

    expect(await validateRun(prepared.runDir, validationCredentials(prepared))).toEqual({ ok: true, errors: [] });
    const firstRender = await renderRun(prepared.runDir);
    const secondRender = await renderRun(prepared.runDir);
    expect(firstRender).toBe(secondRender);
    expect(await fs.readFile(path.join(prepared.evidenceDir, "report.md"), "utf8")).toBe(firstRender);

    const malformed = structuredClone(result) as Record<string, any>;
    malformed.eligibility.publicEligible = true;
    expect(() => validateResultDocument(malformed)).toThrow("may not mark evidence publicEligible");

    const resultPath = path.join(prepared.evidenceDir, "result.json");
    const forged = JSON.parse(await fs.readFile(resultPath, "utf8"));
    forged.status = "invalid";
    forged.model.id = "forged-model";
    forged.controls.planned = !forged.controls.planned;
    forged.hashes.finalWorkerTreeDigest = "0".repeat(64);
    await fs.writeFile(resultPath, stableStringify(forged));
    await fs.writeFile(path.join(prepared.evidenceDir, "report.md"), renderResult(forged));
    await regenerateTransparentChecksums(prepared.evidenceDir);
    const tampered = await validateRun(prepared.runDir, validationCredentials(prepared));
    expect(tampered.ok).toBe(false);
    expect(tampered.errors.some((error) => error.includes("deterministic derivation"))).toBe(true);
    expect(tampered.errors.some((error) => error.includes("Final seal"))).toBe(true);
  });

  test("requires one fixed 0-2 rating for every core dimension", async () => {
    const fixture = await createFixture();
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const evaluationPath = await createEvaluation(fixture.root, { omitRating: "communication" });
    await expect(finalizeRun({ runDir: prepared.runDir, ownerToken: prepared.ownerToken, preparedRootSha256: prepared.preparedRootSha256, evaluationPath })).rejects.toThrow("Missing required core subjective rating: communication");
  });

  test("validates and prepares the explicitly unrun workflow-first behavior scenarios", async () => {
    const sourceScenarioDir = path.join(repoRoot, "benchmarks", "harness", "scenarios", "workflow-first");
    const sourceRepo = repoRoot;
    const cases: Array<{
      spec: string;
      caseId: string;
      workflows: string[];
      absent: string[];
      currentTruth?: string;
    }> = [
      {
        spec: "run-spec.exact-architecture-greenfield.json",
        caseId: "exact-architecture-greenfield",
        workflows: ["architecture/_architecture.md"],
        absent: ["vision/_vision.md", "implementation/_implementation.md"],
      },
      {
        spec: "run-spec.exact-vision-support.json",
        caseId: "exact-vision-support",
        workflows: ["vision/_vision.md"],
        absent: ["architecture/_architecture.md", "implementation/_implementation.md"],
      },
      {
        spec: "run-spec.no-match-direct-choice.json",
        caseId: "no-match-direct-choice",
        workflows: ["vision/_vision.md", "architecture/_architecture.md"],
        absent: ["implementation/_implementation.md"],
      },
      {
        spec: "run-spec.explicit-no-workflow.json",
        caseId: "explicit-no-workflow",
        workflows: ["vision/_vision.md", "architecture/_architecture.md"],
        absent: ["implementation/_implementation.md"],
      },
      {
        spec: "run-spec.ordered-handoffs.json",
        caseId: "ordered-handoffs",
        workflows: ["vision/_vision.md", "architecture/_architecture.md", "implementation/_implementation.md"],
        absent: [],
      },
      {
        spec: "run-spec.direct-delivery-sufficient-truth.json",
        caseId: "direct-delivery-sufficient-truth",
        workflows: [
          "brainstorming/_brainstorming.md",
          "vision/_vision.md",
          "architecture/_architecture.md",
          "planning/_planning.md",
          "task-creation/_task-creation.md",
          "implementation/_implementation.md",
          "testing/_testing.md",
        ],
        absent: [],
        currentTruth: "accepted-note-cli-slice.md",
      },
      {
        spec: "run-spec.missing-architecture-prerequisite.json",
        caseId: "missing-architecture-prerequisite",
        workflows: [
          "brainstorming/_brainstorming.md",
          "vision/_vision.md",
          "architecture/_architecture.md",
          "planning/_planning.md",
          "task-creation/_task-creation.md",
          "implementation/_implementation.md",
          "testing/_testing.md",
        ],
        absent: [],
        currentTruth: "accepted-note-cli-vision.md",
      },
    ];
    const temporary = await createTemporaryRoot();
    const isolatedRepo = path.join(temporary, "source-repo");
    const scenarioDir = path.join(isolatedRepo, "benchmarks", "harness", "scenarios", "workflow-first");
    const runsRoot = path.join(temporary, "workflow-first-runs");
    const seenCaseIds = new Set<string>();

    await fs.mkdir(path.join(isolatedRepo, "src", "cli"), { recursive: true });
    await fs.mkdir(path.dirname(scenarioDir), { recursive: true });
    await fs.copyFile(path.join(sourceRepo, "src", "cli", "cli.ts"), path.join(isolatedRepo, "src", "cli", "cli.ts"));
    await fs.cp(path.join(sourceRepo, "src", "open-forge"), path.join(isolatedRepo, "src", "open-forge"), { recursive: true });
    for (const extension of [
      "architecture-capability",
      "architecture-workflow",
      "vision-capability",
      "vision-workflow",
      "planning-capability",
      "brainstorming-workflow",
      "planning-workflows",
      "implementation-capability",
      "implementation-workflow",
      "quality-capability",
      "testing-workflow",
    ]) {
      await fs.cp(
        path.join(sourceRepo, "src", "extensions", extension),
        path.join(isolatedRepo, "src", "extensions", extension),
        { recursive: true },
      );
    }
    await fs.cp(sourceScenarioDir, scenarioDir, { recursive: true });
    await run(isolatedRepo, ["git", "init", "--quiet"]);
    await run(isolatedRepo, ["git", "config", "user.name", "Benchmark Scenario Test"]);
    await run(isolatedRepo, ["git", "config", "user.email", "benchmark-scenario-test@example.invalid"]);
    await run(isolatedRepo, ["git", "add", "-A"]);
    await run(isolatedRepo, ["git", "commit", "--quiet", "-m", "isolated scenario fixture"]);

    for (const scenario of cases) {
      const specPath = path.join(scenarioDir, scenario.spec);
      const sourceDocument = JSON.parse(await fs.readFile(specPath, "utf8"));
      const spec = validateRunSpecDocument(sourceDocument, scenarioDir);

      expect(spec.caseId).toBe(scenario.caseId);
      expect(seenCaseIds.has(spec.caseId)).toBe(false);
      seenCaseIds.add(spec.caseId);
      expect(spec.evidenceClass).toBe("engineering-smoke");
      expect(spec.model).toMatchObject({ provider: "unassigned", id: "replace-before-run", revision: "unrun" });
      expect(spec.runtime).toMatchObject({
        name: "unassigned-external-worker",
        version: "unrun",
        settings: { scenarioStatus: "unrun" },
      });
      expect(spec.isolation).toEqual({
        freshContext: "unverified",
        inheritedContext: "unknown",
        workerReceivesOnlyWorkspace: false,
        network: "unknown",
        filesystem: "unknown",
      });
      expect(spec.controls).toEqual({
        planned: false,
        randomized: false,
        declaredTreatmentOnly: false,
        replicatePlanned: false,
        planId: null,
        planSha256: null,
      });

      for (const declaredPath of [
        spec.sourceRepo,
        spec.inputs.cli.path,
        spec.inputs.prompt.path,
        spec.inputs.rubric.path,
        ...spec.components.map((component) => component.source),
      ]) {
        expect(await fs.stat(declaredPath)).toBeTruthy();
      }

      const prepared = await prepareRun({ specPath, runsRoot });
      expect(await validateRun(prepared.runDir, validationCredentials(prepared))).toEqual({ ok: true, errors: [] });
      expect(await fs.readFile(prepared.workerPromptPath, "utf8")).toBe(await fs.readFile(spec.inputs.prompt.path, "utf8"));
      await run(isolatedRepo, [process.execPath, path.join(isolatedRepo, "src", "cli", "cli.ts"), "doctor", prepared.workspaceDir]);

      if (scenario.currentTruth) {
        const decisions = path.join(prepared.workspaceDir, ".agents", "memory", "crystallized", "decisions");
        expect(await exists(path.join(decisions, scenario.currentTruth))).toBe(true);
        expect(await fs.readFile(path.join(decisions, "_decisions.md"), "utf8")).toContain(`- \`${scenario.currentTruth}\``);
      }

      for (const workflow of scenario.workflows) {
        const workflowPath = path.join(prepared.workspaceDir, ".agents", "workflows", ...workflow.split("/"));
        expect(await exists(workflowPath)).toBe(true);
        const expectedPhase = workflow.startsWith("brainstorming/")
          ? "PhaseDiscovery"
          : workflow.startsWith("vision/") || workflow.startsWith("architecture/")
            ? "PhaseDefinition"
            : workflow.startsWith("planning/") || workflow.startsWith("task-creation/")
              ? "PhasePlanning"
              : workflow.startsWith("implementation/")
                ? "PhaseDelivery"
                : "PhaseVerification";
        expect(await fs.readFile(workflowPath, "utf8")).toContain(`Workflow, ${expectedPhase}`);
      }
      for (const workflow of scenario.absent) {
        expect(await exists(path.join(prepared.workspaceDir, ".agents", "workflows", ...workflow.split("/")))).toBe(false);
      }
    }

    expect(seenCaseIds.size).toBe(cases.length);
  }, 120_000);

  test("never grants P0 causal eligibility from declared controls and a text isolation receipt", async () => {
    const fixture = await createFixture();
    fixture.spec.evidenceClass = "confirmatory";
    fixture.spec.isolation = {
      freshContext: "verified",
      inheritedContext: "none",
      workerReceivesOnlyWorkspace: true,
      network: "disabled",
      filesystem: "workspace-only",
    };
    fixture.spec.controls = {
      planned: true,
      randomized: true,
      declaredTreatmentOnly: true,
      replicatePlanned: true,
      planId: "fixture-plan",
      planSha256: "a".repeat(64),
    };
    await writeJson(fixture.specPath, fixture.spec);
    const prepared = await prepareRun({ specPath: fixture.specPath, runsRoot: fixture.runsRoot });
    const result = await finalizeRun({
      runDir: prepared.runDir,
      ownerToken: prepared.ownerToken,
      preparedRootSha256: prepared.preparedRootSha256,
      evaluationPath: await createEvaluation(fixture.root, { includeIsolationReceipt: true }),
    });
    const eligibility = result.eligibility as Record<string, unknown>;
    expect(eligibility.causalEligible).toBe(false);
    expect(eligibility.causalReasonCodes).toContain("P0_CAUSAL_CONTROL_VERIFIER_UNAVAILABLE");
  });
});

interface Fixture {
  root: string;
  repo: string;
  runsRoot: string;
  specPath: string;
  spec: Record<string, any>;
}

async function createTemporaryRoot(): Promise<string> {
  return sandbox.createDirectory("case");
}

async function createFixture(providedRoot?: string): Promise<Fixture> {
  const root = providedRoot ?? await createTemporaryRoot();
  const repo = path.join(root, "source-repo");
  const runsRoot = path.join(root, "external-runs");
  await fs.mkdir(path.join(repo, "payload", ".agents"), { recursive: true });
  await fs.mkdir(path.join(repo, "inputs"), { recursive: true });
  await fs.writeFile(path.join(repo, "payload", ".agents", "loader.md"), "base loader\n");
  await fs.writeFile(path.join(repo, "inputs", "worker-prompt.md"), "worker prompt\n");
  await fs.writeFile(path.join(repo, "inputs", "rubric.md"), "orchestrator rubric\n");
  await fs.writeFile(path.join(repo, "benchmark-cli.mjs"), `import fs from "node:fs/promises";
import path from "node:path";
const [command, workspace] = process.argv.slice(2);
if (command !== "index" || !workspace) throw new Error("expected index and workspace");
await fs.mkdir(path.join(workspace, ".agents"), { recursive: true });
await fs.writeFile(path.join(workspace, ".agents", "generated-index.md"), "generated by snapshotted cli\\n");
process.stdout.write("indexed\\n");
`);
  const specPath = path.join(repo, "run-spec.json");
  const spec: Record<string, any> = {
    schemaVersion: 1,
    experimentId: "p0-test",
    caseId: "seed-1",
    armId: "routed",
    replicate: 0,
    evidenceClass: "engineering-smoke",
    sourceRepo: ".",
    components: [{ name: "base", source: "payload", destination: "" }],
    overrides: [],
    inputs: {
      cli: { path: "benchmark-cli.mjs", invocation: ["{runtime}", "{cli}", "index", "{workspace}"] },
      prompt: { path: "inputs/worker-prompt.md" },
      rubric: { path: "inputs/rubric.md" },
    },
    model: { provider: "test", id: "deterministic-fixture", revision: "1", settings: {} },
    runtime: { name: "bun-test", version: Bun.version, adapter: "manual", settings: {} },
    isolation: {
      freshContext: "unverified",
      inheritedContext: "unknown",
      workerReceivesOnlyWorkspace: true,
      network: "unknown",
      filesystem: "workspace-only",
    },
    controls: {
      planned: false,
      randomized: false,
      declaredTreatmentOnly: false,
      replicatePlanned: false,
      planId: null,
      planSha256: null,
    },
  };
  await writeJson(specPath, spec);
  await run(repo, ["git", "init", "--quiet"]);
  await run(repo, ["git", "config", "user.name", "Benchmark Test"]);
  await run(repo, ["git", "config", "user.email", "benchmark-test@example.invalid"]);
  await run(repo, ["git", "add", "-A"]);
  await run(repo, ["git", "commit", "--quiet", "-m", "fixture"]);
  return { root, repo, runsRoot, specPath, spec };
}

async function createEvaluation(root: string, options: { omitRating?: string; includeIsolationReceipt?: boolean; status?: "complete" | "invalid" | "aborted" } = {}): Promise<string> {
  const tracePath = path.join(root, `worker-trace-${crypto.randomUUID()}.log`);
  await fs.writeFile(tracePath, "worker runtime trace\n");
  const evaluationPath = path.join(root, `evaluation-${crypto.randomUUID()}.json`);
  await writeJson(evaluationPath, {
    schemaVersion: 1,
    status: options.status ?? "complete",
    objectiveAssertions: [
      {
        id: "build",
        status: "pass",
        command: "bun test",
        exitCode: 0,
        durationMs: 12.5,
        evidenceTraceNames: ["worker-trace"],
        note: null,
      },
      ...(options.includeIsolationReceipt ? [{
        id: "isolation-fresh-context",
        status: "pass",
        command: null,
        exitCode: null,
        durationMs: null,
        evidenceTraceNames: ["worker-trace"],
        note: "arbitrary text receipt",
      }] : []),
    ],
    subjectiveRatings: coreRatingIds
      .filter((id) => id !== options.omitRating)
      .map((id) => ({ id, score: 2, maxScore: 2, rationale: `${id} fixture rating` })),
    traces: [{ name: "worker-trace", path: tracePath }],
    notes: ["fixture evaluation"],
  });
  return evaluationPath;
}

async function run(cwd: string, command: string[]): Promise<void> {
  requireSuccess(await runProcess(command, { cwd }), command.join(" "));
}

async function writeJson(file: string, value: unknown): Promise<void> {
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, `${JSON.stringify(value, null, 2)}\n`);
}

function validationCredentials(prepared: { ownerToken: string; preparedRootSha256: string }): { ownerToken: string; preparedRootSha256: string } {
  return { ownerToken: prepared.ownerToken, preparedRootSha256: prepared.preparedRootSha256 };
}

async function regenerateTransparentChecksums(evidenceDir: string): Promise<void> {
  const relativeFiles: string[] = [];
  async function walk(directory: string): Promise<void> {
    const children = await fs.readdir(directory, { withFileTypes: true });
    for (const child of children) {
      const absolute = path.join(directory, child.name);
      const relative = path.relative(evidenceDir, absolute).replaceAll("\\", "/").normalize("NFC");
      if (relative === "checksums.json" || relative === "final-seal.json") continue;
      if (child.isDirectory()) await walk(absolute);
      else if (child.isFile()) relativeFiles.push(relative);
    }
  }
  await walk(evidenceDir);
  relativeFiles.sort((left, right) => left.localeCompare(right, "en"));
  const files = [];
  for (const relative of relativeFiles) {
    const content = await fs.readFile(path.join(evidenceDir, ...relative.split("/")));
    files.push({ path: relative, sha256: sha256(content), bytes: content.byteLength });
  }
  await fs.writeFile(path.join(evidenceDir, "checksums.json"), stableStringify({ schemaVersion: 1, files }));
}

async function directoryEntriesOrEmpty(directory: string): Promise<string[]> {
  return fs.readdir(directory).catch(() => []);
}
