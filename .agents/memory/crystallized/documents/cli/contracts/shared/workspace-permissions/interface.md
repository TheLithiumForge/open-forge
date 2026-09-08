---
open-forge:
  description: Define the exact consumer permission document and truthful permission result coordinates
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Interface, CurrentTruth]
---

# Workspace Permissions Interface

These are accepted current contracts for the replacement CLI, which does not
ship yet. The consuming Extension Install, Update and Remove contracts select
this capability. Library grants have a defined document representation; Library
projection behavior remains governed by its separate command contracts.

## Permission Document

The consumer owns `.agents/open-forge.permissions.json`. Schema version 1 has
exactly `schemaVersion`, `extensions` and `libraries` at the root. Both arrays
are required. Each Extension entry contains exactly `id` and `paths`; each
Library entry contains exactly `id`, `sourceRoot` and `paths`. IDs retain their
existing command grammars. Library source roots retain the accepted contained
workspace-relative grammar. The document is independent of ownership records.

Read strict UTF-8 JSON, rejecting unknown or duplicate properties, unsupported
versions, missing properties, nulls and incorrect types. Reject duplicate
identities, duplicate portable paths within one identity, and invalid paths.
Accept entry and path order supplied by a human; canonical writes sort entries
by ordinal ID and paths by ordinal spelling, use two-space indentation and a
final LF. A write preserves unrelated valid grants semantically. The recovery
bundle preserves exact prior bytes, including formatting. No comments or
trailing commas are supported.

A grant names one canonical, portable, workspace-relative file outside
`.agents/`. No absolute, empty, parent, dot, backslash, wildcard, trailing slash,
non-NFC, device-name or trailing-dot/space alias is accepted. Portable identity
uses the existing invariant case-insensitive target key; divergent spellings of
one portable path are rejected within an entry. A directory grant is invalid.
The document cannot grant ownership, force, source selection, executable trust,
or permission to mutate a Library source. Extension and Library identities are
different subjects even when their IDs match. A Library grant binds sourceRoot.

Absence means no external grants. A malformed, linked, unsafe or unreadable
document never becomes an empty grant set and is never overwritten by approval.
Commands requiring no external destination do not depend on this document.
Read-only commands do not prompt or change it. A package's own permission file
is content and cannot supply consumer approval; its control-file destination is
reserved and cannot be installed.

## Result Coordinates

Each consuming command places the following `permissions` object at its
explicitly declared command-local result location. The shared
process envelope, status precedence and exit mapping remain unchanged. Human
presentation uses this same semantic graph.

| Member     | Meaning                                                                                                |
| ---------- | ------------------------------------------------------------------------------------------------------ |
| `path`     | Always `.agents/open-forge.permissions.json`.                                                          |
| `required` | Ordered objects with `id` and exact `path` for the selected operation.                                 |
| `missing`  | Ordered subset absent from the observed document, retained after approval.                             |
| `decision` | `not-evaluated`, `not-required`, `granted`, `required`, `approved`, or `declined`.                     |
| `action`   | `none`, `create`, or `replace`; the planned control-file action.                                       |
| `outcome`  | `not-requested`, `planned`, `not-started`, `verified`, `verification-failed`, or `completion-unknown`. |

Order pair arrays by ordinal ID then path. `granted` means all required grants
already exist; `approved` means this invocation received explicit Yes. A result
formed before permission determination uses `not-evaluated`, action `none`,
and outcome `not-requested`; retain any known requirements but do not claim an
unknown missing set is complete. `not-required` is reserved for complete
determination that no external grant is needed. Content effects do not include a
duplicate permission effect. Recovery protected paths do include the control
file when it has a planned change.

Map existing ordinary-file receipts without guessing: a NotStarted effect maps
to `not-started`; Applied with Verified maps to `verified`; Applied with Failed
verification, including unavailable verification, maps to `verification-failed`;
an Unknown effect maps to `completion-unknown`. Before application a planned
change has `planned`; when no change is requested use `not-requested`. Only a
verified receipt supports saying the grant was saved. Later content failure
does not alter an already observed permission outcome. Retained bundle location
continues to use the existing recovery object.

See the [Behavior](behavior.md) for approval and write ordering, and the
[Technical Design](../../../technical-designs/workspace-permissions.md) for
concrete realization.
