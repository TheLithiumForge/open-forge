# Development Toolkit

Development Toolkit is the single first-party Open Forge Extension.

It adds:

- Six optional Workflows for Vision, Architecture, Planning, Development, Debugging, and Review.
- One specialized Experience Design Skill with focused references for journey mapping, design review, and implementation handoff.
- Nine copy-ready Templates for Vision, Principles, Architecture, Maintenance Contracts, Decisions, Ideas, Analyses, Observations, and Handoffs.

The Workflows are direct routed files. Select one only when its visible `description`, tags, and `route` meaning show that its recipe would materially help. Direct execution remains valid.

The Experience Design Skill follows its native `SKILL.md` contract and may be selected independently when user-experience work needs specialized criteria.

Templates provide starting content only. Copy the most relevant Template, adapt it to the destination, and maintain the result independently. Installing the Extension does not require every workspace to use every Template.

## Install

Preview the complete package:

```sh
open-forge extend development-toolkit --dry-run
```

Install it:

```sh
open-forge extend development-toolkit
```

Review and commit the resulting files and `open-forge.extensions.json`.

The package is intentionally one managed unit. After installation, every file is ordinary user-owned workspace content. Remove routes that provide no local value, or remove the managed package through the CLI while its recorded files remain unchanged.
