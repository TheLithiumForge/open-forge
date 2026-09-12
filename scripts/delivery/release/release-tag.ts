import assert from "node:assert/strict";

export async function validateReleaseTag(read: (path: string) => Promise<Response>, tag: string, sha: string): Promise<void> {
  const response = await read(`git/ref/tags/${encodeURIComponent(tag)}`);
  if (response.status === 404) return;
  let target = await tagObject(response);
  while (target.type === "tag") target = await tagObject(await read(`git/tags/${encodeURIComponent(target.sha)}`));
  assert.equal(target.sha, sha, "The existing release tag must resolve to the selected commit.");
}

async function tagObject(response: Response): Promise<{ type: "commit" | "tag"; sha: string }> {
  assert.ok(response.ok, `Unable to inspect release tag (${response.status}).`);
  const value: unknown = await response.json();
  assert.ok(typeof value === "object" && value !== null && "object" in value);
  const target = value.object;
  assert.ok(typeof target === "object" && target !== null && "type" in target && "sha" in target);
  assert.ok(target.type === "commit" || target.type === "tag", "The release tag must resolve to a commit.");
  assert.ok(typeof target.sha === "string" && target.sha.length > 0, "Missing release tag object identity.");
  return { type: target.type, sha: target.sha };
}
