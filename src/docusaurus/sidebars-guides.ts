import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

// The guides are published from the repository's `docs/` folder.
const sidebars: SidebarsConfig = {
  guides: [
    { type: "doc", id: "cli", label: "CLI reference" },
    { type: "doc", id: "extensions", label: "Extension guide" },
    { type: "doc", id: "development", label: "Development guide" },
  ],
};

export default sidebars;
