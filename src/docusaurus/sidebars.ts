import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

const sidebars: SidebarsConfig = {
  docs: [
    "intro",
    {
      type: "category",
      label: "Start here",
      collapsed: false,
      items: ["getting-started/installation", "getting-started/ten-minute-guide"],
    },
    {
      type: "category",
      label: "Try a demo",
      link: { type: "doc", id: "demos/index" },
      items: ["demos/greenfield", "demos/brownfield"],
    },
    {
      type: "category",
      label: "Guides",
      items: ["getting-started/greenfield-and-brownfield", "getting-started/your-own-docs", "getting-started/share-with-your-team", "getting-started/grow-your-framework"],
    },
    {
      type: "category",
      label: "Understand Open Forge",
      items: [
        "highlights",
        {
          type: "category",
          label: "Concepts",
          link: { type: "doc", id: "concepts/index" },
          items: ["concepts/routing", "concepts/scopes", "concepts/loading-and-tags", "concepts/core-categories", "concepts/memory", "concepts/customizing"],
        },
        { type: "doc", id: "getting-started/first-task", label: "How agents load context" },
        "limits",
      ],
    },
    {
      type: "category",
      label: "Using the CLI",
      link: { type: "doc", id: "cli/index" },
      items: ["cli/flows"],
    },
    {
      type: "category",
      label: "Extensions",
      link: { type: "doc", id: "extensions/index" },
      items: [
        "extensions/reading-installed-files",
        {
          type: "category",
          label: "Packages",
          collapsed: false,
          items: [
            "extensions/workflows",
            "extensions/core-templates",
            "extensions/collaboration",
            "extensions/planning",
            "extensions/project-documents",
            "extensions/document-flow",
            "extensions/scenarios",
            "extensions/observations-and-handoffs",
            "extensions/development",
            "extensions/orchestration",
            "extensions/development-toolkit",
          ],
        },
      ],
    },
    {
      type: "category",
      label: "Reference",
      items: ["glossary", { type: "link", label: "Command reference", href: "/guides/cli" }],
    },
  ],
};

export default sidebars;
