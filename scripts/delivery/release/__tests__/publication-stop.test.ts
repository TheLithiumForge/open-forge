import assert from "node:assert/strict";
import { test } from "node:test";
import { publicationStop, PublicationSignal } from "../publication-stop.ts";
import { publishPackages } from "../../npm/publish-packages.ts";
import { MainPackageName } from "../../package-model.ts";
import { LatestTag } from "../../npm/dist-tag.ts";

const Publication = { name: MainPackageName, version: "1.0.0", tarball: "fixture.tgz", sha256: "fixture", dirty: false };

for (const signal of Object.values(PublicationSignal)) {
  test(`${signal} stops release publication before the first upload and restores handlers`, async (context) => {
    const before = process.listeners(signal);
    const lines: string[] = [];
    context.mock.method(process.stderr, "write", (chunk: string) => {
      lines.push(chunk);
      return true;
    });
    const stop = publicationStop();
    try {
      assert.equal(stop.requested(), false);
      // Emit the process event deterministically. No real process or remote publication is interrupted.
      process.emit(signal);
      assert.equal(stop.requested(), true);
      await assert.rejects(publishPackages(process.cwd(), [Publication], LatestTag, false, stop.requested), /publication cancelled/u);
      assert.match(lines.join(""), /Published: none/u);
      assert.match(lines.join(""), new RegExp(`Not started: ${MainPackageName}`, "u"));
    } finally {
      stop.dispose();
    }
    assert.deepEqual(process.listeners(signal), before);
  });
}
