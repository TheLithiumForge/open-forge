---
open-forge:
  description: Define the exact consumer permission document and truthful permission result coordinates
  tags: [Memory, Crystallized, CLI, Contract, Shared, Permission, Interface, CurrentTruth]
---

# Workspace Permissions Interface

These are accepted current contracts for the replacement CLI, which does not
ship yet. The consuming Extension Install, Update and Remove and Library Attach, Sync
and Detach contracts select this capability. Explicit Library recovery checks
current grants without prompting or publishing them.

## Permission Document

The consumer owns `.agents/open-forge.permissions.json`. Schema version 1 has
exactly `schemaVersion`, `extensions` and `libraries` at the root. Both arrays
are required. Each Extension entry contains exactly `id` and `paths`; each
Library entry contains exactly `id`, `sourceRoot`, `paths` and `directories`. IDs retain their
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

An exact `paths` grant names one canonical, portable, workspace-relative file outside
`.agents/`. No absolute, empty, parent, dot, backslash, wildcard, trailing slash,
non-NFC, device-name or trailing-dot/space alias is accepted. Portable identity
uses the existing invariant case-insensitive target key; divergent spellings of
one portable path are rejected within an entry. Extension directory grants are invalid. Library `directories` contains canonical
external directories; a grant covers future descendant file leaves, never the
directory itself. `.` and empty/workspace-root grants are invalid. File and
directory scopes retain their separate kinds, with unique portable identity
within each array. Canonical writes sort both arrays. The mandatory Library
`directories` member has no old-format default or compatibility branch.
The document cannot grant ownership, force, source selection, executable trust,
or permission to mutate a Library source. Extension and Library identities are
different subjects even when their IDs match. A Library grant binds sourceRoot.

Absence means no external grants. A malformed, linked, unsafe or unreadable
document never becomes an empty grant set and is never overwritten by approval.
Commands requiring no external destination do not depend on this document.
Read-only commands do not prompt or change it. A package's own permission file
is content and cannot supply consumer approval; its control-file destination is
reserved and cannot be installed.

## Library Scope And Rebinding Coordinates

Library command results retain concrete required/missing destination leaves,
adding `sourceRoot` to each Library `{id, sourceRoot, path}` leaf. Their
`proposedScopes` and `approvedScopes` arrays contain `{id, sourceRoot, kind,
path}`, with kind `file` or `directory`. A directory scope includes all future
descendant files; concrete leaf arrays never stand in for that broader approval.
Order by ordinal ID, source root, path and kind. Empty arrays remain present.

A nullable `rebinding` object contains `previousSourceRoot` and `sourceRoot`
when a selected ID has grants bound to another source. It describes the proposed
subject replacement, not its completion. Explicit Yes populates approved scopes;
only the verified permission outcome proves they were saved. Approval replaces
old-source grants only with the displayed newly approved scopes. It preserves
same-source existing grants and all unrelated subjects. No grant is silently
transferred between source roots.

Extension result objects and their exact-file grammar remain unchanged. Each
Library command declares the location and order of its extended result object.

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
