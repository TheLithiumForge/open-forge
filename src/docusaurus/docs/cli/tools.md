---
title: External tools
description: The planned external tool command group, its consent and workspace records.
---

# External tools
`open-forge tool` is a planned command group that installs reviewed external programs and configures their workspace use.

You install a supported pin, build its index through the external program, then choose its role. Installation doesn't choose a provider or build an index. The V1 core contracts are decided. Command implementation and concrete CodeGraph and Codebase Memory adapters are still pending.

## Commands

The planned commands are `list`, `inspect`, `install`, `configure`, `status`, `update`, `remove` and `choose`, in that order. The core has an empty production catalogue. Local sources can select supported definitions but can't import code or admit another pin.

## Consent

Write commands preview the same plan with `--dry-run`. `--automatic` gives final confirmation only. Downloads, declared session daemons and workspace destinations need their separate consent or grants. Missing consent blocks changes and reports what you need to provide.

## Workspace records

`.agents/open-forge.json` records installed pins and active roles. `.agents/open-forge.lock.json` keeps verified receipts, hashes and generated member values. Required usage guidance verifies before active MCP members are published. Unrelated host entries stay unchanged.

## Keep removals

Tool writers honor recorded removal exclusions. If you remove required guidance, Configure and Choose block until every covering exclusion is cleared. Tool Remove can release the registration without restoring missing content.

Full command and consent details are in the [external tool reference](/guides/tools). Use the installed CLI's help to check command availability.
