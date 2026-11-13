---
title: External tools
description: Read external tool definitions and recorded workspace installs.
---

# External tools

`open-forge tool` is a command group that reads reviewed external tool definitions and recorded workspace installs.

You install a supported pin, build its index through the external program, then choose its role. Installation doesn't choose a provider or build an index. `list`, `inspect` and `status` are available. The five write commands and Tool Run are pending. Concrete CodeGraph adapters follow separately.

## Commands

The V1 contract defines `list`, `inspect`, `install`, `configure`, `status`, `update`, `remove`, `choose` and `run`, in that order. The core has an empty production catalogue. Local sources can select supported definitions but can't import code or admit another pin.

## Read tools

[CodeGraph](codegraph.md) `1.6.2` and [Codebase Memory](codebase-memory.md) `0.12.1` have drafts with verified releases for all six targets. Both are blocked on installer decisions and native qualification. Draft wrappers aren't accepted `--source` inputs. Required Skills and consent inventories are prepared for review. No installer runs while those decisions are pending.

Run `open-forge tool list --format json` to read the catalogue and recorded installs. With no tool records, the empty core catalogue returns `completed` with empty `available` and `installed` arrays. `tool inspect <id> --source <path>` explains one local definition. Unreviewed pins return `blocked` with the readable definition.

Run `open-forge tool status` for passive file and configuration checks. It starts no process or database session. An index file alone reports `present-unverified`. Missing receipts keep the settings entry and report unavailable verification.

## Consent

Write commands preview the same plan with `--dry-run`. `--automatic` gives final confirmation only. Downloads, declared session daemons and workspace destinations need their separate consent or grants. Missing consent blocks changes and reports what you need to provide.

## Supply

Open Forge distributes reviewed recipes and never ships, mirrors or rehosts tool bytes. Native assets come from the project's own GitHub release with pinned SHA-256 and build-attestation evidence where published. Node tools use official portable archives, pinned from signed checksums, and a private versioned runtime. The bundled npm runs `npm ci` from a complete pinned registry lock. Your own Node and PATH stay unchanged. musl/Alpine has no official Node build.

A source, size, hash, required signature or attestation mismatch blocks installation. Tools never self-update. The `tools.<id>.telemetry` setting defaults to `false`. Set it to `true` to opt in. Inspect shows the saved choice, or unavailable if settings cannot be read. Status reports drift for an active tool when that choice differs from its configured receipt. The pending writers apply settings through the reviewed tool profile.

## Workspace records

`.agents/open-forge.json` records installed pins and active roles. `.agents/open-forge.lock.json` keeps verified receipts, hashes, lifecycle commands and their results. Tool-owned installers handle both hosts. Planned Tool writes create no `.bak` in Git workspaces. Without Git, sibling backups keep prior managed-file bytes until changed content validates. Status and Doctor report leftover backups at these control paths. Preview restoration with `open-forge repair --automatic --dry-run`. Review the preview, then apply it with `open-forge repair --automatic`. Check restored files, then run Repair again for other work.

The user store keeps tool and private Node versions while any workspace references them. Update selects only reviewed catalogue pins, including exact older pins for downgrades. The catalogue keeps previously reviewed pins. The last update or removal releases an old version after a complete reference check.

## Reinstall

Running Install again on the same pin verifies and repairs immutable bytes and reruns its installer. Use Status to diagnose and Inspect to find resolved paths. There is no separate Tool repair or open-path command.

## Keep removals

Tool writers check declared output paths against recorded removal exclusions before starting a command. Configure can't restore excluded output. Choose never restores excluded output. Tool Remove can release the registration without restoring missing content.

Each active tool requires a usage Skill with its purpose, commands, limits and trust notes. The tool's installer registers MCP. Open Forge provides the required short usage Skill. Removal uses its stored commands in definition-declared removal order, then releases references and unused Tool ignore rules.

V1 contract, implementation pending: `open-forge tool run <tool> -- <args>` runs the pinned tool and runtime from the current directory without PATH changes or symlinks. It isn't implemented.

Full command and consent details are in the [external tool reference](/guides/tools). Use the installed CLI's help to check command availability.

Bare `open-forge tool install` restores every declared pin against the lock with the same consent. Declared missing tools report this exact next step in Tool Status, workspace Status, Doctor and Run. Non-interactive installs take `--prompt-hook yes|no` for the declared component. Run is required in V1 and its implementation is pending.

## V1 registration

Tools create their own host entries. After Install, Configure or Update, Open Forge applies only the declared command rewrites to JSON command and argument members, TOML command and argument lines, a hook command or a line in a marked block. Unexpected shapes stop the write. Names, markers and unrelated content stay. No TOML parser or independent host entry is added.

Definitions declare first-party or third-party origin. List groups by origin and shows it in a column. Inspect shows the origin, installer entries, rewrites and optional components. Third-party consent identifies upstream software, its repository, licence, official release and authors.

Choose switches recognized role integrations and the required Skill while keeping both installs and indexes. Each active role adds one conditional line in Open Forge's managed `AGENTS.md` and `CLAUDE.md` block. Clearing the role removes that line. No additional help pointer is added.

Tool Run accepts only a declared installed and verified tool ID. It resolves the recorded executable in the verified store and passes the remaining arguments as an array without a shell. It checks recorded size and timestamp on each run. Status re-hashes the files. Paths, shell strings, unknown IDs and changed or missing bytes are refused. Run forwards streams, signals and exit code without successful-run output. Put `--workspace <dir>` before the ID, or use `CLAUDE_PROJECT_DIR`, when the host starts outside the workspace. Otherwise Run searches upward from the current directory.
