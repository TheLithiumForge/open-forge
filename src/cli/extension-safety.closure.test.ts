import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  pathExists,
  repoPath,
  requireSuccess,
  runCli as executeCli,
  snapshotTreeState,
  useTestSandbox,
  writeText
} from "../../tests/support/index.ts";

const cliFile = repoPath("src", "cli", "cli.ts");
const sandbox = useTestSandbox("open-forge-extension-safety");

describe("extension lifecycle safety closure", () => {
  test("unmanaged overlay cannot replace a receipt-owned file", async () => {
    const target = await sandbox.createDirectory("owned-file-target");
    const catalogue = await sandbox.createDirectory("owned-file-catalogue");
    const managed = path.join(catalogue, "managed-owner");
    await writeExtensionPackage(managed, { id: "managed-owner", name: "Managed Owner" }, {
      ".agents/patterns/managed/rule.md": "# Managed bytes\n"
    });
    await installCore(target);
    requireSuccess(await runCli(["extend", "managed-owner", target], { OPEN_FORGE_EXTENSIONS_ROOT: catalogue }), "install managed owner");

    const overlay = await sandbox.createDirectory("owned-file-overlay");
    await writeExtensionPackage(overlay, { name: "Unmanaged Overlay" }, {
      ".agents/patterns/managed/rule.md": "# Replacement bytes\n"
    });
    const before = await snapshotTreeState(target);
    const rejected = await runCli(["extend", overlay, target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("Unmanaged extension may not replace receipt-owned file");
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("extension removal rejects a linked target root", async () => {
    const target = await sandbox.createDirectory("linked-removal-real-target");
    const managed = await sandbox.createDirectory("linked-removal-owner");
    await writeExtensionPackage(managed, { id: "linked-owner", name: "Linked Owner" }, {
      ".agents/patterns/linked/rule.md": "# Linked owner bytes\n"
    });
    await installCore(target);
    requireSuccess(await runCli(["extend", managed, target]), "install linked owner");

    const aliasParent = await sandbox.createDirectory("linked-removal-alias-parent");
    const alias = path.join(aliasParent, "target-link");
    await fs.symlink(target, alias, process.platform === "win32" ? "junction" : "dir");
    const before = await snapshotTreeState(target);
    const rejected = await runCli(["extend", "--remove", "linked-owner", alias]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("target root is a symbolic link or junction");
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("Core reinstall cannot change a receipt-owned file", async () => {
    const target = await sandbox.createDirectory("core-owned-file-target");
    const managed = await sandbox.createDirectory("core-owned-file-extension");
    await writeExtensionPackage(managed, { id: "loader-owner", name: "Loader Owner" }, {
      ".agents/loader.md": "# Extension-owned loader\n"
    });
    requireSuccess(await runCli(["extend", managed, target]), "install extension-owned loader");

    const before = await snapshotTreeState(target);
    const rejected = await runCli(["install", target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("would modify or remove receipt-owned extension file .agents/loader.md");
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("direct bundled id rejects a linked catalogue package root", async () => {
    const target = await sandbox.createDirectory("bundled-link-target");
    const catalogue = await sandbox.createDirectory("bundled-link-catalogue");
    const outside = await sandbox.createDirectory("bundled-link-outside-package");
    await writeExtensionPackage(outside, { id: "linked-pack", name: "Linked Pack" }, {
      ".agents/patterns/linked-pack/rule.md": "# Linked catalogue bytes\n"
    });
    await fs.symlink(outside, path.join(catalogue, "linked-pack"), process.platform === "win32" ? "junction" : "dir");

    const result = await runCli(["extend", "linked-pack", target], { OPEN_FORGE_EXTENSIONS_ROOT: catalogue });

    expect(result.exitCode).toBe(1);
    expect(result.stderr).toContain("Bundled extension catalogue contains an unsupported linked or special entry");
    expect(await snapshotTreeState(target)).toEqual({});
  });

  test("local dependencies require a stable manifest id", async () => {
    const target = await sandbox.createDirectory("stable-id-target");
    await installCore(target);
    const dependencyPack = await sandbox.createDirectory("stable-id-dependency-pack");
    await writeExtensionPackage(dependencyPack, {
      name: "Idless Dependencies",
      dependencies: ["reliability-defaults"]
    });
    const dependencyResult = await runCli(["extend", dependencyPack, target]);
    expect(dependencyResult.exitCode).toBe(1);
    expect(dependencyResult.stderr).toContain("needs a stable id when dependencies are declared");
  });

  test("standalone index keeps managed entrypoint ownership usable around unmanaged routed children", async () => {
    const target = await sandbox.createDirectory("standalone-index-managed-route-target");
    const extension = await sandbox.createDirectory("standalone-index-managed-route-extension");
    const entrypointRelative = ".agents/patterns/managed-route/_managed-route.md";
    const entrypoint = path.join(target, ...entrypointRelative.split("/"));
    const localChild = path.join(target, ".agents", "patterns", "managed-route", "local-note.md");
    await installCore(target);
    await writeExtensionPackage(extension, { id: "managed-route", name: "Managed Route" }, {
      [entrypointRelative]: managedRouteEntrypoint("Authored contract v1.")
    });
    requireSuccess(await runCli(["extend", extension, target]), "install managed route");
    await writeText(localChild, unmanagedRoutedChild());

    requireSuccess(await runCli(["index", target]), "route unmanaged child through managed entrypoint");
    const indexed = await fs.readFile(entrypoint, "utf8");
    expect(indexed).toContain("Authored contract v1.");
    expect(indexed).toContain("[Locally owned routed child](local-note.md)");

    await writeText(
      path.join(extension, "payload", ...entrypointRelative.split("/")),
      managedRouteEntrypoint("Authored contract v2.")
    );
    requireSuccess(await runCli(["extend", extension, target]), "update managed route after standalone index");
    const updated = await fs.readFile(entrypoint, "utf8");
    expect(updated).toContain("Authored contract v2.");
    expect(updated).not.toContain("Authored contract v1.");
    expect(updated).toContain("[Locally owned routed child](local-note.md)");
    const receipt = JSON.parse(await fs.readFile(path.join(target, "open-forge.extensions.json"), "utf8"));
    expect(receipt.roots).toContain("managed-route");
    expect(receipt.extensions["managed-route"].files).toContain(entrypointRelative);
    expect(receipt.files[entrypointRelative].owners).toEqual(["managed-route"]);
    await expectHealthyTarget(target);

    await fs.rm(localChild);
    requireSuccess(await runCli(["index", target]), "remove local route from managed entrypoint");
    requireSuccess(await runCli(["extend", "--remove", "managed-route", target]), "remove managed route after reindex");
    expect(await pathExists(path.join(target, "open-forge.extensions.json"))).toBe(false);
    await expectHealthyTarget(target);
  });

  test("Core index regeneration does not stale managed entrypoint ownership", async () => {
    const target = await sandbox.createDirectory("core-index-managed-route-target");
    const extension = await sandbox.createDirectory("core-index-managed-route-extension");
    const entrypointRelative = ".agents/patterns/core-managed-route/_core-managed-route.md";
    const entrypoint = path.join(target, ...entrypointRelative.split("/"));
    await installCore(target);
    await writeExtensionPackage(extension, { id: "core-managed-route", name: "Core Managed Route" }, {
      [entrypointRelative]: managedRouteEntrypoint("Core-index contract v1.")
    });
    requireSuccess(await runCli(["extend", extension, target]), "install Core-index managed route");
    await writeText(
      path.join(target, ".agents", "patterns", "core-managed-route", "local-note.md"),
      unmanagedRoutedChild()
    );

    requireSuccess(await runCli(["install", target]), "reinstall Core and regenerate managed entrypoint index");
    const coreIndexed = await fs.readFile(entrypoint, "utf8");
    expect(coreIndexed).toContain("Core-index contract v1.");
    expect(coreIndexed).toContain("[Locally owned routed child](local-note.md)");

    await writeText(
      path.join(extension, "payload", ...entrypointRelative.split("/")),
      managedRouteEntrypoint("Core-index contract v2.")
    );
    requireSuccess(await runCli(["extend", extension, target]), "update managed route after Core reindex");
    const updated = await fs.readFile(entrypoint, "utf8");
    expect(updated).toContain("Core-index contract v2.");
    expect(updated).not.toContain("Core-index contract v1.");
    expect(updated).toContain("[Locally owned routed child](local-note.md)");
    await expectHealthyTarget(target);
  });

  test("removal preserves a managed entrypoint required by an unmanaged routed child", async () => {
    const target = await sandbox.createDirectory("retained-unmanaged-route-target");
    const extension = await sandbox.createDirectory("retained-unmanaged-route-extension");
    const entrypointRelative = ".agents/patterns/retained-route/_retained-route.md";
    const localChildRelative = ".agents/patterns/retained-route/local-note.md";
    await installCore(target);
    await writeExtensionPackage(extension, { id: "retained-route", name: "Retained Route" }, {
      [entrypointRelative]: managedRouteEntrypoint("Retained route contract.")
    });
    requireSuccess(await runCli(["extend", extension, target]), "install retained route");
    await writeText(path.join(target, ...localChildRelative.split("/")), unmanagedRoutedChild());
    requireSuccess(await runCli(["index", target]), "route retained unmanaged child");
    await expectHealthyTarget(target);
    const before = await snapshotTreeState(target);

    const rejected = await runCli(["extend", "--remove", "retained-route", target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("Retained routed content");
    expect(rejected.stderr).toContain(entrypointRelative);
    expect(rejected.stderr).toContain(localChildRelative);
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("injected extension transaction failures restore payload index receipt and tree", async () => {
    const catalogue = await sandbox.createDirectory("transaction-catalogue");
    const extension = path.join(catalogue, "transaction-pack");
    await writeExtensionPackage(extension, { id: "transaction-pack", name: "Transaction Pack" }, {
      ".agents/patterns/transaction/_transaction.md": transactionEntrypoint()
    });

    for (const stage of ["after-payload", "after-index", "after-receipt"] as const) {
      const target = await sandbox.createDirectory(`transaction-${stage}`);
      await installCore(target);
      const before = await snapshotTreeState(target);

      const result = await runCli(["extend", "transaction-pack", target], {
        OPEN_FORGE_EXTENSIONS_ROOT: catalogue,
        OPEN_FORGE_TEST_FAIL_EXTENSION_TRANSACTION: stage
      });

      expect(result.exitCode).toBe(1);
      expect(result.stderr).toContain(`Injected extension transaction failure ${stage}`);
      expect(await snapshotTreeState(target)).toEqual(before);
    }
  }, 30_000);
});

async function runCli(args: string[], env: Record<string, string> = {}) {
  const withPro = (args[0] === "install" || args[0] === "extend") && !args.includes("--pro")
    ? [...args, "--pro"]
    : args;
  return executeCli(cliFile, withPro, { env });
}

async function installCore(target: string): Promise<void> {
  requireSuccess(await runCli(["install", target]), "install Core");
}

async function expectHealthyTarget(target: string): Promise<void> {
  const result = await runCli(["doctor", "--json", target]);
  requireSuccess(result, "validate routed target");
  expect(JSON.parse(result.stdout)).toMatchObject({ errors: 0, warnings: 0 });
}

async function writeExtensionPackage(
  packageRoot: string,
  manifest: Record<string, unknown>,
  payloadFiles: Record<string, string> = {}
): Promise<void> {
  await writeText(path.join(packageRoot, "extension.json"), `${JSON.stringify({
    description: "Lifecycle safety fixture",
    version: "1.0.0",
    dependencies: [],
    ...manifest
  }, null, 2)}\n`);
  for (const [relative, contents] of Object.entries(payloadFiles)) {
    await writeText(path.join(packageRoot, "payload", ...relative.split("/")), contents);
  }
}

function transactionEntrypoint(): string {
  return `---
open-forge:
  description: Transaction rollback fixture
  tags: [Extension, Pattern]
---

# Transaction

## Axioms

- inherited - No local axioms. Loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`;
}

function managedRouteEntrypoint(contract: string): string {
  return `---
open-forge:
  description: Managed route lifecycle fixture
  tags: [Extension, Pattern]
---

# Managed Route

${contract}

## Axioms

- inherited - No local axioms. Loaded ancestor axioms remain active.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`;
}

function unmanagedRoutedChild(): string {
  return `---
open-forge:
  description: Locally owned routed child
  tags: [Pattern, Local]
---

# Local Note

## Shape

- Preserve this local child independently of its managed route host.
`;
}
