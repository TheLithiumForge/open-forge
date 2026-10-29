import type { Config } from "@docusaurus/types";
import type { Options as PresetOptions } from "@docusaurus/preset-classic";
import { guideDocs, siteDocs } from "./src/config/content";
import { organizationName, projectName, siteBaseUrl, siteUrl } from "./src/config/site";
import { themeConfig } from "./src/config/theme";

const config: Config = {
  title: "Open Forge",
  tagline: "A Markdown framework that tells coding agents which of your project's rules, docs and decisions to read for each task, and keeps what they learn for the next one.",
  favicon: "img/favicon.svg",

  // Opt into the v4 defaults, but keep the standard webpack bundler rather than
  // adding the optional Rspack-based @docusaurus/faster dependency.
  future: { v4: true, faster: false },

  url: siteUrl,
  baseUrl: siteBaseUrl,
  organizationName,
  projectName,
  trailingSlash: false,

  onBrokenLinks: "throw",
  onBrokenAnchors: "throw",
  markdown: {
    hooks: { onBrokenMarkdownLinks: "throw" },
  },

  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      {
        docs: siteDocs,
        blog: false,
        theme: { customCss: "./src/css/custom.css" },
      } satisfies PresetOptions,
    ],
  ],

  plugins: [["@docusaurus/plugin-content-docs", guideDocs]],

  // Local search: the index is built with the site and searched in the browser,
  // with no external service. The optional AI answer widget is not installed.
  themes: [
    [
      "@easyops-cn/docusaurus-search-local",
      {
        hashed: true,
        indexBlog: false,
        indexPages: false,
        docsRouteBasePath: [siteDocs.routeBasePath, guideDocs.routeBasePath],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],

  themeConfig,
};

export default config;
