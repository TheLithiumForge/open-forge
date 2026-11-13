---
title: External tools
description: Read external tool definitions and recorded workspace installs.
---

# External tools
`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

You install a supported pin, build its index through the external program, then choose its role. Installation doesn't choose a provider or build an index. `list`, `inspect` and `status` are available. The five write commands and concrete CodeGraph and Codebase Memory adapters are pending.

## Commands

The V1 contract defines `list`, `inspect`, `install`, `configure`, `status`, `update`, `remove` and `choose`, in that order. The core has an empty production catalogue. Local sources can select supported definitions but can't import code or admit another pin.

## Read tools

Run `open-forge tool list --format json` to read the catalogue and recorded installs. With no tool records, the empty core catalogue returns `completed` with empty `available` and `installed` arrays. `tool inspect <id> --source <path>` explains one local definition. Unreviewed pins return `blocked` with the readable definition.

Run `open-forge tool status` for passive file and configuration checks. It starts no process or database session. An index file alone reports `present-unverified`. Missing receipts keep the settings entry and report unavailable verification.

## Consent

Write commands preview the same plan with `--dry-run`. `--automatic` gives final confirmation only. Downloads, declared session daemons and workspace destinations need their separate consent or grants. Missing consent blocks changes and reports what you need to provide.

## Workspace records

`.agents/open-forge.json` records installed pins and active roles. `.agents/open-forge.lock.json` keeps verified receipts, hashes, lifecycle commands and their results. Tool-owned installers handle both hosts. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Backup reporting and Repair restoration are pending.

## Keep removals

Tool writers check declared output paths against recorded removal exclusions before starting a command. Configure can't restore excluded output. Choose changes metadata only. Tool Remove can release the registration without restoring missing content.

Full command and consent details are in the [external tool reference](/guides/tools). Use the installed CLI's help to check command availability.
