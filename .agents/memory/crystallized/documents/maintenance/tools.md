---
open-forge:
  description: Review each tool catalogue entry against its upstream release before changing it
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool entry review

Tool entry review is a checklist for adding a tool to the catalogue or moving an entry to a newer tested version.
For example, moving CodeGraph past 1.6.2 means checking its README and CLI for changed install, setup and uninstall flags.

Status: Lean checklist decided on 17 November 2026.

## Source

The [Tool setup design](../cli/technical-designs/tool-setup.md) holds the current entries and their evidence.

## Checklist

Before adding or updating an entry, check the upstream release at its tag:

1. Record the ID, name, purpose, category, upstream URL and licence.
2. Use the documented install method. Pin the tested version when the installer allows it, and say so when it doesn't.
3. Record the package manager or runner the install method needs.
4. Check that `--version` prints a version number.
5. Record the tool's own project setup, if any, with one plain sentence about what it does. Say when its installer changes PATH or account-wide files.
6. Record the tool's own uninstall commands and one plain sentence about what they remove.
7. Keep Open Forge's integration changes additive and self-identifying.
8. Update the design's evidence and the user documentation in the same change.

Open Forge doesn't rewrite what a tool's own setup writes.
