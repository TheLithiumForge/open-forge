---
open-forge:
  description: Realize shared destination admission through authored settings and existing mutation receipts
  tags: [Memory, Crystallized, CLI, TechnicalDesign, Permissions, Settings, CurrentTruth]
---

# Workspace Permissions Technical Design

The [Workspace State Files decision](../../../decisions/framework/workspace-state-files.md)
and [shared Interface/Behavior](../contracts/shared/workspace-permissions/interface.md)
define meaning. This design adds no persisted schema, dependency or per-owner grant.

`Framework/Settings` owns authored settings parsing, the shared allow-list policy,
raw observations and explicit key-preserving grant planning/writing.
`Framework/Permissions` is removed. `WorkspaceSettingsReader` returns typed
settings with an optional concrete file snapshot; consumers never reparse a
second permission document. Missing or invalid input yields settings defaults
and an explicit observation state. Ordinary parent and no-follow leaf checks
protect the control-file boundary.

`WorkspaceAllowList` evaluates destination strings with the accepted portable
identity and descendant rule. It does not know package IDs or Library sources.
Neutral permission decision/action/outcome and receipt projection remain under
Settings; JSON projections and prompt choices live under
`Commands/Shared/Permissions`. Library's file/directory scope proposal stays in
the Library command family because that proposal describes its link plan.

Extension and Library operations retain their command-local stage composition,
lease, revalidation and recovery machinery. They evaluate the same settings
list. Explicit `--allow-path` values and interactive Allow-always choices are
staged by `WorkspaceSettingsChangePlanner` in the permission plan; prospective
grants are used in memory while the plan is reviewed. The settings create or
replace is published only during confirmed application under the existing lease,
revalidation and recovery rules. Allow-once has no settings effect, and cancel
or refusal before application leaves settings unchanged. Compare the actual
settings observation again immediately before application. A stale observation
cannot keep a revoked grant alive.

The `--allow-path` writer remains an explicit authored edit. It shares the
settings reader and codec, refuses unsafe/invalid content, preserves unknown
keys and order, and uses a newly created same-directory temporary file followed
by atomic replacement when the confirmed application publishes it. Dry-run
skips this authoring path. Interactive publication rides the content command's
existing recovery and file receipt pipeline.

Only whole-file snapshots needed by current mutation/recovery are kept in memory.
No stored integrity baseline, source-root binding, grant-subject codec, migration,
legacy reader or new presentation framework is introduced. Ownership, reserved
paths, Library source containment and physical safety remain separate checks.
