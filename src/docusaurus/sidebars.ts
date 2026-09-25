import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

const sidebars: SidebarsConfig = {
  docs: [
    "intro",
    {
      type: "category",
      label: "Getting started",
      collapsed: false,
      items: ["getting-started/installation", "getting-started/greenfield-and-brownfield", "getting-started/first-task", "getting-started/grow-your-framework"],
    },
    "highlights",
    {
      type: "category",
      label: "Concepts",
      link: { type: "doc", id: "concepts/index" },
      items: ["concepts/routing", "concepts/scopes", "concepts/loading-and-tags", "concepts/core-categories", "concepts/memory", "concepts/customizing"],
    },
    {
      type: "category",
      label: "Using the CLI",
      link: { type: "doc", id: "cli/index" },
      items: ["cli/flows", { type: "link", label: "Command reference", href: "/guides/cli" }],
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
      label: "Demos",
      link: { type: "doc", id: "demos/index" },
      items: ["demos/greenfield", "demos/brownfield"],
    },
    "glossary",
    "limits",
  ],
};

export default sidebars;
