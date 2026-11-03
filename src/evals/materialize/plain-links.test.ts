import assert from "node:assert/strict";
import { test } from "node:test";
import { withoutLinks } from "./plain-links.ts";

test("plain docs keep a link's text and drop its target, but keep outside sources and code blocks", () => {
  const source = [
    "See the [rules](rules.md) and the [moves section](rules.md#moves), or [this heading](#local).",
    "React lists its [stable releases](https://react.dev/versions).",
    "```",
    "[kept](literal.md)",
    "```",
  ].join("\n");
  const plain = withoutLinks(Buffer.from(source)).toString();
  assert.equal(plain.split("\n")[0], "See the rules and the moves section, or this heading.");
  assert.equal(plain.split("\n")[1], "React lists its [stable releases](https://react.dev/versions).");
  assert.equal(plain.split("\n")[3], "[kept](literal.md)");
  assert.equal(withoutLinks(Buffer.from(plain)).toString(), plain, "Removing links twice changes nothing.");
});
