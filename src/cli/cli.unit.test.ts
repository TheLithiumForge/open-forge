import { describe, expect, test } from "bun:test";
import {
  cliTestInternals,
  createExtensionSelectionState,
  toggleExtensionSelection,
  type ExtensionDependencyInfo
} from "./cli.ts";

const augmentationHost = `# Loader

<!-- open-forge-augment.workflow-selection:start -->
<!-- open-forge-augment.workflow-selection:end -->
`;

describe("extension catalogue selection state", () => {
  const extensions: ExtensionDependencyInfo[] = [
    { id: "base-skill", dependencies: [] },
    { id: "shared-workflow", dependencies: ["base-skill"] },
    { id: "feature-flow", dependencies: ["shared-workflow"] },
    { id: "review-flow", dependencies: ["shared-workflow"] }
  ];

  test("auto-selects transitive dependencies as required", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow"]);

    expect([...state.direct]).toEqual(["feature-flow"]);
    expect([...state.required].sort()).toEqual(["base-skill", "shared-workflow"]);
  });

  test("keeps directly selected dependencies direct and locks required-only entries", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow", "shared-workflow"]);
    const locked = toggleExtensionSelection(extensions, state, "base-skill");

    expect([...state.direct]).toEqual(["feature-flow", "shared-workflow"]);
    expect([...state.required]).toEqual(["base-skill"]);
    expect(locked).toBe(state);
  });

  test("releases dependencies only after their last dependent is deselected", () => {
    let state = createExtensionSelectionState(extensions, ["feature-flow", "review-flow"]);
    state = toggleExtensionSelection(extensions, state, "feature-flow");

    expect([...state.direct]).toEqual(["review-flow"]);
    expect([...state.required].sort()).toEqual(["base-skill", "shared-workflow"]);

    state = toggleExtensionSelection(extensions, state, "review-flow");
    expect([...state.direct]).toEqual([]);
    expect([...state.required]).toEqual([]);
  });

  test("downgrades a deselected direct dependency to required while it is still needed", () => {
    const state = createExtensionSelectionState(extensions, ["feature-flow", "shared-workflow"]);
    const next = toggleExtensionSelection(extensions, state, "shared-workflow");

    expect([...next.direct]).toEqual(["feature-flow"]);
    expect([...next.required].sort()).toEqual(["base-skill", "shared-workflow"]);
  });
});

describe("extension augmentation mechanics", () => {
  test("sets and parses owned blocks in stable extension-id order", () => {
    const withZeta = cliTestInternals.setAugmentationBlock(
      augmentationHost,
      "workflow-selection",
      "zeta-extension",
      "Zeta guidance.",
      "fixture loader"
    );
    const composed = cliTestInternals.setAugmentationBlock(
      withZeta,
      "workflow-selection",
      "alpha-extension",
      "Alpha guidance.",
      "fixture loader"
    );

    const slot = cliTestInternals.parseAugmentationSlots(composed, "fixture loader").get("workflow-selection");
    expect([...slot!.blocks.keys()]).toEqual(["alpha-extension", "zeta-extension"]);
    expect(slot!.blocks.get("alpha-extension")!.content).toBe("Alpha guidance.");
    expect(composed.indexOf("alpha-extension:start")).toBeLessThan(composed.indexOf("zeta-extension:start"));
  });

  test("replaces one owner without duplicating its block", () => {
    const initial = cliTestInternals.setAugmentationBlock(
      augmentationHost,
      "workflow-selection",
      "alpha-extension",
      "Old guidance.",
      "fixture loader"
    );
    const updated = cliTestInternals.setAugmentationBlock(
      initial,
      "workflow-selection",
      "alpha-extension",
      "New guidance.",
      "fixture loader"
    );

    expect(updated).not.toContain("Old guidance.");
    expect(updated).toContain("New guidance.");
    expect(updated.match(/alpha-extension:start/g)).toHaveLength(1);
  });

  test("removes only the matching unmodified owner and refuses local edits", () => {
    const alpha = cliTestInternals.setAugmentationBlock(
      augmentationHost,
      "workflow-selection",
      "alpha-extension",
      "Alpha guidance.",
      "fixture loader"
    );
    const composed = cliTestInternals.setAugmentationBlock(
      alpha,
      "workflow-selection",
      "zeta-extension",
      "Zeta guidance.",
      "fixture loader"
    );
    const installed = {
      target: ".agents/loader.md",
      slot: "workflow-selection",
      sha256: cliTestInternals.sha256("Alpha guidance.")
    };

    const removed = cliTestInternals.removeAugmentationBlock(composed, installed, "alpha-extension", "fixture loader");
    expect(removed).not.toContain("alpha-extension:start");
    expect(removed).toContain("zeta-extension:start");

    const locallyEdited = composed.replace("Alpha guidance.", "Locally edited guidance.");
    expect(() => cliTestInternals.removeAugmentationBlock(
      locallyEdited,
      installed,
      "alpha-extension",
      "fixture loader"
    )).toThrow("locally modified");
  });

  test("refuses malformed slots and unowned slot content", () => {
    expect(() => cliTestInternals.parseAugmentationSlots(
      augmentationHost.replace("<!-- open-forge-augment.workflow-selection:end -->", "Manual content\n<!-- open-forge-augment.workflow-selection:end -->"),
      "fixture loader"
    )).toThrow("unowned content");
    expect(() => cliTestInternals.setAugmentationBlock(
      augmentationHost,
      "missing-slot",
      "alpha-extension",
      "Guidance.",
      "fixture loader"
    )).toThrow("does not declare augmentation slot");
  });
});

describe("extension ownership receipt mechanics", () => {
  test("hashes authored entrypoint bytes while excluding generated Entries metadata", () => {
    const original = `# Managed Route

Authored contract v1.

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`;
    const generatedChange = cliTestInternals.updateGeneratedIndexRegion(
      original,
      "- `local.md` - Local route - #Local",
      "fixture entrypoint"
    );
    const authoredChange = original.replace("Authored contract v1.", "Authored contract v2.");

    expect(cliTestInternals.extensionOwnedFileSha256("fixture.md", generatedChange)).toBe(
      cliTestInternals.extensionOwnedFileSha256("fixture.md", original)
    );
    expect(cliTestInternals.extensionOwnedFileSha256("fixture.md", authoredChange)).not.toBe(
      cliTestInternals.extensionOwnedFileSha256("fixture.md", original)
    );
  });

  test("validates reciprocity and normalizes deterministic ordering", () => {
    const digestA = "a".repeat(64);
    const digestB = "b".repeat(64);
    const receipt = {
      schema: 1 as const,
      roots: ["zeta-extension", "alpha-extension"],
      extensions: {
        "zeta-extension": {
          version: "1.0.0",
          dependencies: ["alpha-extension"],
          files: ["zeta.md"],
          augmentations: []
        },
        "alpha-extension": {
          version: null,
          dependencies: [],
          files: ["alpha.md"],
          augmentations: []
        }
      },
      files: {
        "zeta.md": { sha256: digestB, owners: ["zeta-extension"] },
        "alpha.md": { sha256: digestA, owners: ["alpha-extension"] }
      }
    };

    expect(() => cliTestInternals.validateExtensionReceiptIntegrity(receipt, "fixture receipt")).not.toThrow();
    const normalized = cliTestInternals.normalizeExtensionReceipt(receipt);
    expect(normalized.roots).toEqual(["alpha-extension", "zeta-extension"]);
    expect(Object.keys(normalized.extensions)).toEqual(["alpha-extension", "zeta-extension"]);
    expect(Object.keys(normalized.files)).toEqual(["alpha.md", "zeta.md"]);
  });

  test("rejects duplicate roots and non-reciprocal file ownership", () => {
    const receipt = {
      schema: 1 as const,
      roots: ["alpha-extension", "alpha-extension"],
      extensions: {
        "alpha-extension": {
          version: null,
          dependencies: [],
          files: ["alpha.md"],
          augmentations: []
        }
      },
      files: {
        "alpha.md": { sha256: "a".repeat(64), owners: [] }
      }
    };

    expect(() => cliTestInternals.validateExtensionReceiptIntegrity(receipt, "fixture receipt")).toThrow("duplicate");
    receipt.roots = ["alpha-extension"];
    expect(() => cliTestInternals.validateExtensionReceiptIntegrity(receipt, "fixture receipt")).toThrow("does not reciprocally list its owner");
  });
});

describe("Markdown contract mechanics", () => {
  const workflow = `# Delivery

## Mode

iterative

## Goal

- outcome: deliver one change

## Required Routes

- none

## Constraints

- none

## Steps

1. Implement the change.

## Loop

Repeat until verified.

## Outputs

- verified change

## Completion

- [ ] verification passes
`;

  test("accepts the complete ordered workflow contract and catches invalid mode/constraints", () => {
    expect(cliTestInternals.validateWorkflowDocument(workflow)).toEqual([]);
    const invalid = workflow
      .replace("iterative", "goal-seeking")
      .replace("## Constraints\n\n- none", "## Constraints\n\n- none\n- Preserve bytes.");
    expect(cliTestInternals.validateWorkflowDocument(invalid)).toEqual(expect.arrayContaining([
      expect.stringContaining("Mode must be linear or iterative"),
      expect.stringContaining("cannot mix none/inherited")
    ]));
  });

  test("requires substantive direct directives and rejects legacy applicability", () => {
    expect(cliTestInternals.validateDirectiveDocument("# Safety\n\n## Axioms\n\n- Preserve user work.\n", false)).toEqual([]);
    expect(cliTestInternals.validateDirectiveDocument("# Safety\n\n## Applies To\n\n- src/**\n\n## Axioms\n\n- inherited\n", false)).toEqual(expect.arrayContaining([
      expect.stringContaining("scope belongs to routing"),
      expect.stringContaining("must be substantive")
    ]));
  });

  test("updates only a bounded generated index region", () => {
    const original = `# Routes

Stable contract.

## Entries

<!-- open-forge:generated-index:start -->
- old.md - Old route - #Old
<!-- open-forge:generated-index:end -->
`;
    const updated = cliTestInternals.updateGeneratedIndexRegion(original, "- `new.md` - New route - #New", "fixture.md");
    expect(updated).toContain("Stable contract.");
    expect(updated).toContain("- `new.md` - New route - #New");
    expect(updated).not.toContain("Old route");
  });
});

describe("portable extension paths", () => {
  test("accepts ordinary portable manifest and payload paths", () => {
    expect(() => cliTestInternals.assertPortableManifestPath(
      ".agents/loader.md",
      "extension.json",
      "augmentations[0].target"
    )).not.toThrow();
    expect(() => cliTestInternals.assertPortablePayloadPath(
      ".agents/patterns/review/_review.md",
      "review-pack"
    )).not.toThrow();
  });

  for (const invalid of [
    "CON/file.md",
    "folder/NUL.txt",
    "folder/COM1.log",
    "folder/trailing-dot.",
    "folder/trailing-space ",
    "folder/bad?.md",
    "folder/bad<name>.md"
  ]) {
    test(`rejects Windows-unsafe path ${JSON.stringify(invalid)}`, () => {
      expect(() => cliTestInternals.assertPortableManifestPath(
        invalid,
        "extension.json",
        "augmentations[0].source"
      )).toThrow("not portable across Windows");
      expect(() => cliTestInternals.assertPortablePayloadPath(invalid, "fixture-pack")).toThrow(
        "not portable across Windows"
      );
    });
  }

  test("rejects a literal backslash as a payload or manifest path separator", () => {
    const invalid = "folder/literal\\backslash.md";
    expect(() => cliTestInternals.assertPortableManifestPath(
      invalid,
      "extension.json",
      "augmentations[0].source"
    )).toThrow("must be a portable relative path");
    expect(() => cliTestInternals.assertPortablePayloadPath(invalid, "fixture-pack")).toThrow(
      "must be a portable relative path"
    );
  });
});
