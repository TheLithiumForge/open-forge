---
open-forge:
  description: Implement bounded route content and metadata update without identity drift
  tags: [Memory, Working, CLI, Task, Route, Update, Mutation, Contextual]
---

# Implement Route Update

## Task State

- State: Planned after Route Create.
- Parent: [Route Mutation Commands](_route-mutation.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/route/update/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/route/update/behavior.md).

## Expected Outcome

`route update` changes only accepted content or metadata of one exact route while
preserving route identity, path, unrelated authored sections, overwrite ownership,
and generated navigation.

## Architecture

- Separate `RouteUpdateObservation`, command-local `RouteUpdatePlan`, content
  transformation, generated projection, apply receipts, and result.
- Reuse strict source reads, Markdown/frontmatter facts, exact route identity,
  lock/revalidation, atomic replacement, and external recovery-bundle support.
  Prepare one verified bundle covering every existing Replace/Delete before the
  first target effect; no-op plans create none.
- Keep update-field policy and preservation rules command-local.

## Evidence

Cover exact ID/path, ambiguity, detached/compatibility forms, no-op, dry run,
accepted fields, unknown or repeated fields, invalid metadata, overwrite pair,
line endings and preservation, identity race, generated navigation, read/write
failures, bundle retention, no unrelated changes, presentation, process, and AOT.

## Preparation Closeout

Read-only preparation on clean no-op branch `codex/route-update` at exact base
`33913dfe7f8f80598ca4765c516d308ed179c3ab` produced no commit, Gray, Red, or
Green change. Route Update is not ready before integrated Route Create and the
accepted pre-Gray callable/public-result freeze below.

- Expected implementation paths are
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Update/**`,
  `src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Commands/Route/Update/**`, and
  `src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Route/Update/**`,
  including a command-local real-filesystem fixture.
- `src/cli/root/OpenForge.Cli/Composition/CliCompositionRoot.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliJsonContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Shell/Serialization/CliYamlContext.cs`,
  `src/cli/core/OpenForge.Cli.Core/Commands/Route/Shared/Rendering/RouteHelpSections.cs`,
  `src/cli/core/OpenForge.Cli.Core/Framework/**`, EndToEnd/Native AOT evidence,
  preceding commands, and shared program ledgers remain protected integration or
  predecessor surfaces.
- Decisive evidence must cover binding, exact target resolution, preservation of
  unsupported content, metadata/body transformation, intended navigation,
  dry-run/no-op, lock/revalidation/recovery, typed result and process behavior,
  and Native AOT.

### Accepted Pre-Gray Freeze And YAML Meaning

- After Route Create and before Gray, architecture authority will freeze the exact
  wire graph, finding/status mapping, `next` content, callable stages, allowed
  paths, and final test disposition. Those exact details are not frozen yet.
- Parse the complete frontmatter, edit only recognized fields including
  `responsibility`, and preserve unrecognized YAML source exactly as encountered.
  Prefer parsed-span edits; do not deserialize and reserialize the whole document.
  The later callable freeze owns exact placement. Promote Route Create Template
  selection or body extraction only if its implemented meaning is proven
  identical.

## Stop Conditions

Stop if update changes route path or ID, rewrites whole content without contract
permission, infers intent, merges overwrite ownership, or uses a generic patch
language not accepted by the interface.
