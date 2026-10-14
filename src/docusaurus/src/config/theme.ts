import type { ThemeConfig } from "@docusaurus/preset-classic";
import { themes as prismThemes } from "prism-react-renderer";
import { guidesRouteBasePath, npmPackageUrl, releasesUrl, repositoryUrl } from "./site";

const guidesSidebarRoute = `/${guidesRouteBasePath}`;

export const themeConfig = {
  colorMode: {
    defaultMode: "dark",
    respectPrefersColorScheme: true,
  },
  docs: {
    sidebar: { hideable: true },
  },
  navbar: {
    title: "Open Forge",
    logo: { alt: "Open Forge", src: "img/logo.svg" },
    items: [
      { type: "docSidebar", sidebarId: "docs", position: "left", label: "Docs" },
      { to: "/docs/extensions", label: "Extensions", position: "left" },
      { to: "/docs/demos", label: "Demos", position: "left" },
      { to: "/docs/cli", label: "CLI", position: "left" },
      { to: `${guidesSidebarRoute}/development`, label: "Contributing", position: "left" },
      { href: npmPackageUrl, label: "npm", position: "right" },
      { href: repositoryUrl, label: "GitHub", position: "right" },
    ],
  },
  footer: {
    style: "dark",
    links: [
      {
        title: "Learn",
        items: [
          { label: "Introduction", to: "/docs" },
          { label: "Getting started", to: "/docs/getting-started/installation" },
          { label: "Ten-minute guide", to: "/docs/getting-started/ten-minute-guide" },
          { label: "Try a demo", to: "/docs/demos" },
          { label: "Highlights", to: "/docs/highlights" },
          { label: "Concepts", to: "/docs/concepts" },
          { label: "How agents load context", to: "/docs/getting-started/first-task" },
          { label: "Limits", to: "/docs/limits" },
        ],
      },
      {
        title: "Guides and reference",
        items: [
          { label: "New or existing project", to: "/docs/getting-started/greenfield-and-brownfield" },
          { label: "Grow your own framework", to: "/docs/getting-started/grow-your-framework" },
          { label: "Working with the CLI", to: "/docs/cli" },
          { label: "Extensions", to: "/docs/extensions" },
          { label: "Extension guide", to: `${guidesSidebarRoute}/extensions` },
          { label: "Glossary", to: "/docs/glossary" },
          { label: "Command reference", to: `${guidesSidebarRoute}/cli` },
          { label: "Development guide", to: `${guidesSidebarRoute}/development` },
        ],
      },
      {
        title: "Project",
        items: [
          { label: "GitHub", href: repositoryUrl },
          { label: "Releases", href: releasesUrl },
          { label: "npm package", href: npmPackageUrl },
        ],
      },
    ],
    copyright: "Open Forge is MIT licensed. Plain Markdown, owned by you.",
  },
  prism: {
    theme: prismThemes.oneLight,
    darkTheme: prismThemes.oneDark,
    additionalLanguages: ["bash", "json", "yaml", "powershell"],
  },
  tableOfContents: {
    minHeadingLevel: 2,
    maxHeadingLevel: 3,
  },
} satisfies ThemeConfig;
