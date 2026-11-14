---
open-forge:
  description: Define tool install inputs, artifact consent and inactive registration
  tags: [Memory, Document, CLI, Contract, Tool, Install, Interface, CurrentTruth]
---

# tool install interface

`tool install` is a write command that installs one exact reviewed pin and registers it without choosing a role.

Installation verifies official upstream bytes and runs the tool's own pinned installer. Repeating it repairs missing or changed immutable files and reruns that installer. Verified local artifacts can avoid another download. Indexing is an external step. Matching foreign binaries aren't adopted.

Status: V1 core contract decided.

## Syntax

Proposed:

```text
open-forge tool install [<id>] [--source <path>] [--tool-version <exact>] [--host <claude-code|codex>...] [--code-root <path>] [--allow-download] [--prompt-hook <yes|no>] [--allow-session-daemon] [--allow-path <path>...] [--automatic] [--dry-run] [global flags]
```

The [group inputs](../_tool.md#inputs) define defaults and repetition. The help purpose is "Install and register one tool pin without choosing a role."

## Plan

Resolve the exact source and pin. Validate release admission, target, paths, exclusions, current receipts, disk space and process compatibility before effects. Transfers report each exact URL, size and digest. `--allow-download` covers only these descriptors for this invocation. `--automatic` gives final confirmation only.

The tool's installer registers its entries and native configuration. Open Forge then applies only the definition-declared command rewrites and declined-component removal. It verifies those changes before recording success. Install doesn't choose a role or build an index. The settings entry has the verified pin and an empty `roles` array for a new registration. Unrelated role assignments stay unchanged.

The shared [tool operations](../../../shared-operation-contract.md#tool-operations) define complete preview and apply data, consent, permissions, recovery and verification. `role` is null. `pin` identifies the exact selected version. Expected `needs-index` is information.

## Conformance

Required cases are:

- Verified local fixture artifacts and offline immutable-install reuse.
- Same-pin re-verification, changed or missing file restoration and installer execution.
- Official-source enforcement, private runtime paths and frozen npm dependency installation.
- Exact descriptor consent missing, supplied and changed between invocations.
- Unsupported tuples, candidate admission and unqualified native targets.
- Traversal, links, duplicate extraction destinations and hash failures.
- No roles chosen and no external index built.
- Paired settings and lock publication after verification.
- Equal preview and apply decisions before effects.
- Interruption at every effect with prior bytes kept and unfinished work reported.

The [behavior](behavior.md) defines installation ordering.

## Team setup

Without an ID, Install selects every declared settings entry at its exact pin and verifies it against the lock. It uses the same separate consent, optional-component choices and preview rules as named Install. No catalogue default replaces the declared pin.

Select declared IDs in ordinal order. Missing or mismatched lock evidence blocks that pin before effects. Report each pin separately in `tools`, including its plan, result and unfinished work. An empty declaration set completes with an empty `tools` array and no effects. Source, version, host and root overrides require a named ID. Saved component choices apply to team restore. If a declared component has no saved choice, non-interactive restoration requires its flag. This includes `--prompt-hook yes|no` for a prompt hook.
