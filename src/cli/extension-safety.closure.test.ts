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

  test("unmanaged overlay cannot erase a retained augmentation block", async () => {
    const target = await sandbox.createDirectory("retained-block-target");
    await installCore(target);
    const guest = await sandbox.createDirectory("retained-block-guest");
    await writeExtensionPackage(guest, {
      id: "routing-guest",
      name: "Routing Guest",
      augmentations: [{
        target: ".agents/loader.md",
        slot: "workflow-selection",
        source: "augmentations/guidance.md"
      }]
    }, {}, { "augmentations/guidance.md": "- Guest guidance.\n" });
    requireSuccess(await runCli(["extend", guest, target]), "install routing guest");

    const overlay = await sandbox.createDirectory("retained-block-overlay");
    await writeExtensionPackage(overlay, { name: "Unmanaged Loader Overlay" }, {
      ".agents/loader.md": "# Replacement loader without the occupied slot\n"
    });
    const before = await snapshotTreeState(target);
    const rejected = await runCli(["extend", overlay, target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("would modify or remove installed extension routing-guest augmentation block");
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("host update cannot remove a retained guest slot or block", async () => {
    const target = await sandbox.createDirectory("host-update-target");
    await installCore(target);
    const host = await sandbox.createDirectory("host-update-host");
    await writeExtensionPackage(host, { id: "slot-host", name: "Slot Host" }, {
      ".agents/patterns/host/host.md": augmentationHost("shared-rules")
    });
    requireSuccess(await runCli(["extend", host, target]), "install slot host");

    const guest = await sandbox.createDirectory("host-update-guest");
    await writeExtensionPackage(guest, {
      id: "slot-guest",
      name: "Slot Guest",
      augmentations: [{
        target: ".agents/patterns/host/host.md",
        slot: "shared-rules",
        source: "augmentations/rules.md"
      }]
    }, {}, { "augmentations/rules.md": "- Guest-owned rule.\n" });
    requireSuccess(await runCli(["extend", guest, target]), "install slot guest");

    await writeText(path.join(host, "payload", ".agents", "patterns", "host", "host.md"), "# Host without its former slot\n");
    const before = await snapshotTreeState(target);
    const rejected = await runCli(["extend", host, target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("would modify or remove installed extension slot-guest augmentation block");
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

  test("Core reinstall cannot remove an occupied scoped slot", async () => {
    const target = await sandbox.createDirectory("core-occupied-slot-target");
    await installCore(target);
    const scopedTarget = path.join(
      target,
      ".agents",
      "memory",
      "customer-facing",
      "mobile-app",
      "crystallized",
      "platform",
      "documents",
      "_documents.md"
    );
    await writeText(scopedTarget, scopedDocumentHost());

    const guest = await sandbox.createDirectory("core-occupied-slot-guest");
    await writeExtensionPackage(guest, {
      id: "scoped-doc-guest",
      name: "Scoped Document Guest",
      augmentations: [{
        target: ".agents/memory/customer-facing/mobile-app/crystallized/platform/documents/_documents.md",
        slot: "document-rules",
        source: "augmentations/document-rules.md"
      }]
    }, {}, { "augmentations/document-rules.md": "- Preserve the guest rule.\n" });
    requireSuccess(await runCli(["extend", guest, target]), "install scoped document guest");

    const before = await snapshotTreeState(target);
    const rejected = await runCli(["install", target]);

    expect(rejected.exitCode).toBe(1);
    expect(rejected.stderr).toContain("would modify or remove installed extension scoped-doc-guest augmentation block");
    expect(await snapshotTreeState(target)).toEqual(before);
  });

  test("payload-contained augmentation fragment is materialized only as an owned block", async () => {
    const target = await sandbox.createDirectory("payload-fragment-target");
    await installCore(target);
    const extension = await sandbox.createDirectory("payload-fragment-extension");
    await writeExtensionPackage(extension, {
      id: "payload-fragment",
      name: "Payload Fragment",
      augmentations: [{
        target: ".agents/loader.md",
        slot: "workflow-selection",
        source: "payload/fragments/guidance.md"
      }]
    }, { "fragments/guidance.md": "- Payload-contained guidance.\n" });

    const result = await runCli(["extend", extension, target]);

    expect(result.exitCode).toBe(0);
    expect(await pathExists(path.join(target, "fragments", "guidance.md"))).toBe(false);
    expect(await fs.readFile(path.join(target, ".agents", "loader.md"), "utf8")).toContain("Payload-contained guidance.");
  });

  test("managed payload permits empty slots but rejects pre-owned and malformed blocks", async () => {
    const target = await sandbox.createDirectory("managed-marker-target");
    await installCore(target);
    const emptySlot = await sandbox.createDirectory("managed-empty-slot");
    await writeExtensionPackage(emptySlot, { id: "empty-slot", name: "Empty Slot" }, {
      ".agents/patterns/slot-fixtures/empty.md": augmentationHost("shared")
    });
    expect((await runCli(["extend", emptySlot, target])).exitCode).toBe(0);

    const preowned = await sandbox.createDirectory("managed-preowned-slot");
    await writeExtensionPackage(preowned, { id: "preowned-slot", name: "Preowned Slot" }, {
      ".agents/patterns/slot-fixtures/preowned.md": [
        "# Preowned",
        "",
        "<!-- open-forge-augment.shared:start -->",
        "<!-- open-forge-extension.some-owner:start -->",
        "- Pre-owned content.",
        "<!-- open-forge-extension.some-owner:end -->",
        "<!-- open-forge-augment.shared:end -->",
        ""
      ].join("\n")
    });
    const before = await snapshotTreeState(target);
    const preownedResult = await runCli(["extend", preowned, target]);
    expect(preownedResult.exitCode).toBe(1);
    expect(preownedResult.stderr).toContain("may not ship pre-owned extension blocks");
    expect(await snapshotTreeState(target)).toEqual(before);

    const malformed = await sandbox.createDirectory("managed-malformed-slot");
    await writeExtensionPackage(malformed, { id: "malformed-slot", name: "Malformed Slot" }, {
      ".agents/patterns/slot-fixtures/malformed.md": "# Malformed\n\n<!-- open-forge-augment.shared:start -->\n"
    });
    const malformedResult = await runCli(["extend", malformed, target]);
    expect(malformedResult.exitCode).toBe(1);
    expect(malformedResult.stderr).toContain("incomplete augmentation marker pair");
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
    expect(result.stderr).toContain("Bundled extension package root is a symbolic link or junction");
    expect(await snapshotTreeState(target)).toEqual({});
  });

  test("local dependencies and augmentations require a stable manifest id", async () => {
    const target = await sandbox.createDirectory("stable-id-target");
    await installCore(target);
    const dependencyPack = await sandbox.createDirectory("stable-id-dependency-pack");
    await writeExtensionPackage(dependencyPack, {
      name: "Idless Dependencies",
      dependencies: ["reliability-defaults"]
    });
    const dependencyResult = await runCli(["extend", dependencyPack, target]);
    expect(dependencyResult.exitCode).toBe(1);
    expect(dependencyResult.stderr).toContain("needs a stable id when dependencies or augmentations are declared");

    const augmentationPack = await sandbox.createDirectory("stable-id-augmentation-pack");
    await writeExtensionPackage(augmentationPack, {
      name: "Idless Augmentation",
      augmentations: [{
        target: ".agents/loader.md",
        slot: "workflow-selection",
        source: "augmentations/guidance.md"
      }]
    }, {}, { "augmentations/guidance.md": "- Idless guidance.\n" });
    const augmentationResult = await runCli(["extend", augmentationPack, target]);
    expect(augmentationResult.exitCode).toBe(1);
    expect(augmentationResult.stderr).toContain("needs a stable id when dependencies or augmentations are declared");
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
    expect(indexed).toContain("`local-note.md` - Locally owned routed child");

    await writeText(
      path.join(extension, "payload", ...entrypointRelative.split("/")),
      managedRouteEntrypoint("Authored contract v2.")
    );
    requireSuccess(await runCli(["extend", extension, target]), "update managed route after standalone index");
    const updated = await fs.readFile(entrypoint, "utf8");
    expect(updated).toContain("Authored contract v2.");
    expect(updated).not.toContain("Authored contract v1.");
    expect(updated).toContain("`local-note.md` - Locally owned routed child");
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
    expect(coreIndexed).toContain("`local-note.md` - Locally owned routed child");

    await writeText(
      path.join(extension, "payload", ...entrypointRelative.split("/")),
      managedRouteEntrypoint("Core-index contract v2.")
    );
    requireSuccess(await runCli(["extend", extension, target]), "update managed route after Core reindex");
    const updated = await fs.readFile(entrypoint, "utf8");
    expect(updated).toContain("Core-index contract v2.");
    expect(updated).not.toContain("Core-index contract v1.");
    expect(updated).toContain("`local-note.md` - Locally owned routed child");
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
    expect(rejected.stderr).toContain("retained routed descendant");
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
  payloadFiles: Record<string, string> = {},
  packageFiles: Record<string, string> = {}
): Promise<void> {
  await writeText(path.join(packageRoot, "extension.json"), `${JSON.stringify({
    description: "Lifecycle safety fixture",
    version: "1.0.0",
    dependencies: [],
    augmentations: [],
    ...manifest
  }, null, 2)}\n`);
  for (const [relative, contents] of Object.entries(payloadFiles)) {
    await writeText(path.join(packageRoot, "payload", ...relative.split("/")), contents);
  }
  for (const [relative, contents] of Object.entries(packageFiles)) {
    await writeText(path.join(packageRoot, ...relative.split("/")), contents);
  }
}

function augmentationHost(slot: string): string {
  return [
    "# Augmentation Host",
    "",
    `<!-- open-forge-augment.${slot}:start -->`,
    `<!-- open-forge-augment.${slot}:end -->`,
    ""
  ].join("\n");
}

function scopedDocumentHost(): string {
  return [
    "# Scoped Documents",
    "",
    "<!-- open-forge-augment.document-rules:start -->",
    "<!-- open-forge-augment.document-rules:end -->",
    "",
    "## Entries",
    "",
    "<!-- open-forge:generated-index:start -->",
    "- none - No entries - #Empty",
    "<!-- open-forge:generated-index:end -->",
    ""
  ].join("\n");
}

function transactionEntrypoint(): string {
  return `---
open-forge:
  description: Transaction rollback fixture
  tags: [Extension, Pattern]
---

# Transaction

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

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

- inherited - No local axioms; loaded ancestor axioms remain active.

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
