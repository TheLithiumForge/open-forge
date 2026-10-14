import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

// The guides are published from the repository's `docs/` folder.
const sidebars: SidebarsConfig = {
  guides: [
    { type: "category", label: "How-to guides", items: [{ type: "doc", id: "extensions", label: "Extension guide" }] },
    { type: "category", label: "Contributing", items: [{ type: "doc", id: "development", label: "Development guide" }] },
    { type: "category", label: "Reference", items: [{ type: "doc", id: "cli", label: "Command reference" }] },
  ],
};

export default sidebars;
