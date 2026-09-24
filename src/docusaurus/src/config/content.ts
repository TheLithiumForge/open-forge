import { fileURLToPath } from "node:url";
import { join, resolve } from "node:path";
import type { Options as DocsOptions } from "@docusaurus/plugin-content-docs";
import { isInside, repositoryLinks } from "../remark/repository-links";
import { guidesDirectory, guidesRouteBasePath, publishedGuides, repositoryBlobUrl, repositoryEditUrl, repositoryTreeUrl, siteDirectory } from "./site";

// The site's own pages live in `docs/` beside this configuration. The long-form
// guides stay in the repository's `docs/` folder and are published from there,
// so each guide keeps a single source.

const siteRoot = fileURLToPath(new URL("../..", import.meta.url));
const repositoryRoot = resolve(siteRoot, "../..");
const siteDocsRoot = join(siteRoot, "docs");
const guidesRoot = join(repositoryRoot, guidesDirectory);
const guidePaths = new Set(publishedGuides.map((guide) => join(guidesRoot, guide)));

function repositoryLinkPlugin(isPublished: (absolutePath: string) => boolean) {
  return [repositoryLinks, { repositoryRoot, isPublished, blobBaseUrl: repositoryBlobUrl, treeBaseUrl: repositoryTreeUrl }] as const;
}

export const siteDocs = {
  path: "docs",
  routeBasePath: "docs",
  sidebarPath: "./sidebars.ts",
  editUrl: ({ docPath }) => `${repositoryEditUrl}/${siteDirectory}/docs/${docPath}`,
  beforeDefaultRemarkPlugins: [repositoryLinkPlugin((path) => isInside(siteDocsRoot, path))],
} satisfies DocsOptions;

export const guideDocs = {
  id: "guides",
  path: `../../${guidesDirectory}`,
  include: [...publishedGuides],
  routeBasePath: guidesRouteBasePath,
  sidebarPath: "./sidebars-guides.ts",
  editUrl: ({ docPath }) => `${repositoryEditUrl}/${guidesDirectory}/${docPath}`,
  beforeDefaultRemarkPlugins: [repositoryLinkPlugin((path) => guidePaths.has(path))],
} satisfies DocsOptions;
