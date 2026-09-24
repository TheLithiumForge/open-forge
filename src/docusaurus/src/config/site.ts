// Values shared by the site configuration and the repository link rewriter.

export const repositoryUrl = "https://github.com/TheLithiumForge/open-forge";
export const defaultBranch = "main";
export const repositoryBlobUrl = `${repositoryUrl}/blob/${defaultBranch}`;
export const repositoryTreeUrl = `${repositoryUrl}/tree/${defaultBranch}`;
export const repositoryEditUrl = `${repositoryUrl}/edit/${defaultBranch}`;

// GitHub Pages serves a project repository at https://<owner>.github.io/<repo>/.
// The Pages workflow passes the real address from actions/configure-pages, so a
// custom domain (served from "/") needs no change here. Local builds use the
// project-site address.
const pagesOriginVariable = "DOCS_SITE_ORIGIN";
const pagesBasePathVariable = "DOCS_SITE_BASE_PATH";
const projectSiteOrigin = "https://thelithiumforge.github.io";
const projectSiteBasePath = "/open-forge";

const pagesOrigin = process.env[pagesOriginVariable] ?? "";
const usesPagesAddress = pagesOrigin !== "";

export const siteUrl = usesPagesAddress ? pagesOrigin : projectSiteOrigin;
export const siteBaseUrl = `${usesPagesAddress ? (process.env[pagesBasePathVariable] ?? "") : projectSiteBasePath}/`;
export const organizationName = "TheLithiumForge";
export const projectName = "open-forge";

export const npmPackageUrl = "https://www.npmjs.com/package/@thelithiumforge/open-forge";
export const releasesUrl = `${repositoryUrl}/releases`;

// Repository-relative locations of the content this site publishes.
export const siteDirectory = "src/docusaurus";
export const guidesDirectory = "docs";
export const guidesRouteBasePath = "guides";
export const publishedGuides = ["cli.md", "extensions.md", "development.md"] as const;
