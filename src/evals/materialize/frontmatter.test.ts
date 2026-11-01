import assert from "node:assert/strict";
import { test } from "node:test";
import { addMetadata, documentMetadata, removeMetadata, splitMarkdown, validateMetadata } from "./frontmatter.ts";
import { stripFrontmatter } from "./strip.ts";

const metadata = { description: "Game requirements", tags: ["LoadNow", "Game"] };
const bodies = ["# Game\nFind the pairs.\n", "\r\n# Café\r\nKeep these bytes.\r\n", "# Cards\n🙂\n\n", "# No final newline"];

test("metadata insertion and stripping keep exact UTF-8 body bytes", () => {
  for (const body of bodies) {
    const bytes = Buffer.from(body);
    const routed = addMetadata(bytes, metadata);
    assert.deepEqual(documentMetadata(routed), metadata);
    assert.deepEqual(stripFrontmatter(routed), bytes);
    assert.deepEqual(removeMetadata(routed, metadata), bytes);
    assert.deepEqual(addMetadata(bytes, metadata), routed);
  }
});

test("foreign YAML keeps its meaning while its CRLF body stays byte identical", () => {
  const bytes = Buffer.from('---\r\ntitle: "Public note"\r\ncustom:\r\n  flags: [true, false]\r\n---\r\n# Note\r\nBody.\r\n');
  const exported = removeMetadata(addMetadata(bytes, metadata), metadata);
  assert.deepEqual(splitMarkdown(exported).body, splitMarkdown(bytes).body);
  assert.deepEqual(splitMarkdown(exported).document?.toJS(), splitMarkdown(bytes).document?.toJS());
  assert.deepEqual(stripFrontmatter(exported), Buffer.from("# Note\r\nBody.\r\n"));
});

test("malformed YAML, duplicate keys, invalid UTF-8 and metadata drift fail", () => {
  for (const text of ["---\nname: x\n", "---\nname: [\n---\n", "---\nname: x\nname: y\n---\n", "---\n- list\n---\n"]) assert.throws(() => splitMarkdown(Buffer.from(text)));
  assert.throws(() => splitMarkdown(Uint8Array.of(255)));
  assert.throws(() => removeMetadata(addMetadata(Buffer.from(bodies[0] ?? ""), metadata), { ...metadata, description: "Changed" }));
  assert.throws(() => addMetadata(addMetadata(Buffer.from(""), metadata), metadata));
  for (const invalid of [{ ...metadata, applyTo: ["src/**"] }, { ...metadata, tags: ["Game", "Game"] }, { ...metadata, description: " " }]) assert.throws(() => validateMetadata(invalid));
});
