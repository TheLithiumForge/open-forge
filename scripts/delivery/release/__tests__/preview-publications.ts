import assert from "node:assert/strict";
import { readReleasePublications } from "../release-publications.ts";
import { publishPackages } from "../../npm/publish-packages.ts";

const [, , sha, version] = process.argv;
assert.ok(sha && version);
const publications = readReleasePublications(process.cwd(), "artifacts/release", { sha, dirty: false }, version);
delete process.env["npm_execpath"];
await publishPackages(process.cwd(), publications, "beta", true);
