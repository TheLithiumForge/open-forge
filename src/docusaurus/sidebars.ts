import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

const sidebars: SidebarsConfig = {
  docs: [
    "intro",
    {
      type: "category",
      label: "Getting started",
      collapsed: false,
      items: ["getting-started/installation", "getting-started/first-task", "getting-started/grow-your-framework"],
    },
    {
      type: "category",
      label: "Concepts",
      link: { type: "doc", id: "concepts/index" },
      items: ["concepts/routing", "concepts/scopes", "concepts/loading-and-tags", "concepts/core-categories", "concepts/memory", "concepts/customizing"],
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
            "extensions/scenarios",
            "extensions/observations-and-handoffs",
            "extensions/development",
            "extensions/orchestration",
            "extensions/development-toolkit",
          ],
        },
      ],
    },
    "glossary",
    "limits",
  ],
};

export default sidebars;
