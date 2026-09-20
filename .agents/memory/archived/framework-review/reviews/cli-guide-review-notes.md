---
open-forge:
  description: Recorded CLI guide rewrite checks and the remaining executable verification boundary
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# CLI guide review notes

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

The public guide in `docs/cli.md` was rewritten from the accepted command
definitions and contracts at `develop` commit
`2b54fdc598a48a12e44772a5cb323cf45e9e5a73`. The review worktree's CLI source
is older than that comparison surface, so this guide was not validated by
executing a CLI binary.

Before release, the CLI implementation chat should build the selected CLI
baseline and compare `open-forge --help` plus one read-only command from each
family with the guide. Recheck at least global flags, source-reference
resolution, route metadata option delimiters, extension source separation,
Library argument order, status streams, and exit codes.

The guide deliberately does not describe these prepared follow-up behaviors as
current:

- scoped continuity loading;
- synchronization between the source Extension catalogue and the embedded
  CLI catalogue; and
- package publication or final delivery evidence.

The source catalogue and embedded catalogue therefore still need separate
release evidence. The current first-party source catalogue is the evidence for
the package structure described by the public Extension guide; it does not by
itself prove that an omitted `--source` exposes every source-catalogue package.

The guide keeps the public `#extension-operations` heading because
`docs/extensions.md` currently links to it. The parent docs rewrite should
update or remove any remaining references to the old
`#new-cli-contract-not-shipping` section when it replaces `docs/extensions.md`.

No CLI implementation, build, test, package publication, global link, or
worktree mutation was performed for this documentation task.
