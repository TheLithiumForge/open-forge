import assert from "node:assert/strict";
import { test } from "node:test";
import { packageReadme, RepositoryUrl, SiteUrl } from "../npm/package-readme.ts";

test("package README links resolve against the repository", () => {
  assert.equal(packageReadme("See [the guide](docs/cli.md)."), `See [the guide](${RepositoryUrl}/blob/main/docs/cli.md).`);
  assert.equal(packageReadme("[Size](docs/development.md#measure-context-size)"), `[Size](${RepositoryUrl}/blob/main/docs/development.md#measure-context-size)`);
  assert.equal(packageReadme("[Demos](demos/)"), `[Demos](${RepositoryUrl}/tree/main/demos/)`);
  assert.equal(packageReadme("[License](./LICENSE)"), `[License](${RepositoryUrl}/blob/main/LICENSE)`);
});

test("package README images come from the documentation site", () => {
  const html = '<source srcset="src/docusaurus/static/img/map-dark.svg"><img src="src/docusaurus/static/img/map-light.svg">';
  assert.equal(packageReadme(html), `<source srcset="${SiteUrl}/img/map-dark.svg"><img src="${SiteUrl}/img/map-light.svg">`);
});

test("package README keeps anchors, absolute URLs, and code blocks unchanged", () => {
  const unchanged = ["[Skip ahead](#manually-from-a-clone)", "[Site](https://thelithiumforge.github.io/open-forge/)", "```md\n- [Rule](testing.md) - #Directive\n```"];
  for (const text of unchanged) assert.equal(packageReadme(text), text);
});
