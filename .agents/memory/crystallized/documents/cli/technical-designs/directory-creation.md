---
open-forge:
  description: Exact lease-bound ordinary-BCL directory-create effect design
  responsibility: Define the shared Create-only directory mechanics without changing command planning policy
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Mutation, Directory, Filesystem]
---

# Directory Creation Technical Design

## Boundary

Directory creation is one separate shared native effect. It is not a
`PlannedFileChangeKind` and does not broaden file replacement. Commands name
every missing directory, order parents before children, and retain selection,
ordering among other effects, findings, and result policy.

Root Install and Route Init consume the same capability only where their
directory mechanics have identical meaning. Their plans and command semantics
remain local.

## Effect

The directory applier requires a live same-workspace lease and one explicitly
planned missing descendant below `.agents`. Immediately before the effect it
revalidates that:

- the target remains missing;
- the target's physical parent is the exact expected contained ordinary
  directory; and
- the lease still covers the selected workspace.

It then calls ordinary `Directory.CreateDirectory` and verifies the target as
the expected contained ordinary directory. Missing `.agents` is an ordinary
lease-bound effect and appears first when a command needs it, followed by its
ordered descendants.

## Retained State And Limits

A verified created directory remains when a later effect fails or the operation
is interrupted. The capability reports it as residual state and never removes,
rolls back, or compensates for it. Directory creation has no recovery-bundle
entry.

The design uses no P/Invoke or native package and promises no creator identity
against a hostile same-user process. A difference in descendant mechanics between
consumers remains command-local rather than creating a broader directory engine.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Mutation And Recovery Technical Design](mutation-and-recovery.md)
- [Install Contract](../contracts/install/_install.md)
- [Route Init Contract](../contracts/route/init/_init.md)
