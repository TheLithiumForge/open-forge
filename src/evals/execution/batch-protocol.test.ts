import assert from "node:assert/strict";
import { test } from "node:test";
import { userDocuments } from "./batch.ts";
import type { BrownfieldCase } from "../materialize/brownfield-case.ts";
import type { BrownfieldInputs } from "../materialize/brownfield-inputs.ts";

const Request = "A request", Document = "A document";
const brownfield = (item: Pick<BrownfieldCase, "documents">) => ({ case: item }) as unknown as BrownfieldInputs;

test("the simulated user reads a suite's documents, but only the request when a case brings its own pack", () => {
  assert.deepEqual(userDocuments({ prompt: Request }), [Request]);
  assert.deepEqual(userDocuments({ prompt: Request, documents: [Document] }), [Request, Document]);
  assert.deepEqual(userDocuments({ prompt: Request, documents: [Document], brownfield: brownfield({}) }), [Request, Document]);
  assert.deepEqual(userDocuments({ prompt: Request, documents: [Document], brownfield: brownfield({ documents: "docs-pack" }) }), [Request]);
});
