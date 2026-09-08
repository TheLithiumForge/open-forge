# Development Toolkit

Development Toolkit is the first-party Open Forge Extension.

It adds:

- Six optional Workflows for Vision, Architecture, Planning, Development, Debugging, and Review.
- One specialized Experience Design Skill with focused references for journey mapping, design review, and implementation handoff.
- Nine copy-ready Templates for Vision, Principles, Architecture, Maintenance Contracts, Decisions, Ideas, Analyses, Observations, and Handoffs.

The Workflows are direct routed files. Select one only when its description, tags, and route meaning show that its recipe would significantly help. Work directly when no Workflow adds value.

The Experience Design Skill follows its own `SKILL.md` and can be selected independently when user-experience work needs specialized criteria.

Templates provide starting content only. Copy the most relevant Template, adapt it to the destination, and maintain the result independently. A workspace does not need to use every installed Template.

## Install

Preview the complete package:

```sh
open-forge extension install development-toolkit --dry-run
```

Install it:

```sh
open-forge extension install development-toolkit
```

Review and commit the resulting files and `.agents/open-forge.lifecycle.json`.

The package is one managed unit. After installation, every file is ordinary user-owned workspace content. Remove routes that provide no local value, or use the CLI to remove the package while its recorded files remain unchanged.
