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

## Supply

Open Forge distributes reviewed recipes and never ships, mirrors or rehosts tool bytes. Native assets come from the project's own GitHub release with pinned SHA-256 and build-attestation evidence where published. Node tools use official portable archives, pinned from signed checksums, and a private versioned runtime. The bundled npm runs `npm ci` from a complete pinned registry lock. Your own Node and PATH stay unchanged. musl/Alpine has no official Node build.

A source, size, hash, required signature or attestation mismatch blocks installation. Tools never self-update. Telemetry defaults off with a visible per-tool opt-in and appears in Inspect.

## Workspace records

`.agents/open-forge.json` records installed pins and active roles. `.agents/open-forge.lock.json` keeps verified receipts, hashes, lifecycle commands and their results. Tool-owned installers handle both hosts. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Backup reporting and Repair restoration are pending.

The user store keeps tool and private Node versions while any workspace references them. Update selects only reviewed catalogue pins, including exact older pins for downgrades. The catalogue keeps previously reviewed pins. The last update or removal releases an old version after a complete reference check.

## Reinstall

Running Install again on the same pin verifies and repairs immutable bytes and reruns its installer. Use Status to diagnose and Inspect to find resolved paths. There is no separate Tool repair or open-path command.

## Keep removals

Tool writers check declared output paths against recorded removal exclusions before starting a command. Configure can't restore excluded output. Choose never restores excluded output. Tool Remove can release the registration without restoring missing content.

Each active tool requires a usage Skill with its purpose, commands, limits and trust notes. The tool's installer registers MCP and supplies the Skill. Removal uses its stored cleanup and uninstall commands, then releases references and unused Tool ignore rules.

Proposed for maintainer review: `open-forge tool exec <tool> -- <args>` runs the pinned tool and runtime from the current directory without PATH changes or symlinks. It isn't implemented.

Full command and consent details are in the [external tool reference](/guides/tools). Use the installed CLI's help to check command availability.
