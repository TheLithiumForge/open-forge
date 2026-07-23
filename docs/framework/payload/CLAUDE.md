# Claude Code Entry Bridge

## Description

This descriptor governs `src/open-forge/CLAUDE.md`.

`CLAUDE.md` is the installable root bridge that lets Claude Code enter the canonical Open Forge contract in `AGENTS.md`.

## Represents

The bridge represents harness compatibility, not a second Open Forge configuration or instruction owner.

## Contains

The installed file contains one managed Open Forge block whose only instruction is:

```md
@AGENTS.md
```

Claude Code supports importing `AGENTS.md` from `CLAUDE.md`; the import is resolved relative to the bridge file. The canonical behavior is documented in [Claude Code project memory](https://code.claude.com/docs/en/memory#agentsmd).

## Patch Contract

The CLI creates the bridge when no target `CLAUDE.md` exists.

When a target file already exists, the CLI replaces its marked Open Forge block or appends the block when absent. Text outside the block belongs to the target workspace and remains unchanged.

The source block in `src/open-forge/CLAUDE.md` is the replacement block used by install and update operations.

## Why

Claude Code reads `CLAUDE.md`, not `AGENTS.md`, while Open Forge needs one canonical cross-harness contract. A one-line import provides compatibility without duplicated policy or Windows symlink requirements.

## Alignment Checks

The implementation is aligned when it:

- contains exactly one block bounded by `<!-- open-forge:start -->` and `<!-- open-forge:end -->`
- imports `AGENTS.md` with exactly one `@AGENTS.md` line inside the managed block
- contains no independent Open Forge policy
- preserves target workspace text outside the marked block during install and update
- remains idempotent across repeated installs
