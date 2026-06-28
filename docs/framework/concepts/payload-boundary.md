# Payload Boundary

## Description

This descriptor governs the boundary between maintainer governance and installed Open Forge files.

Users receive the installable payload from `src/open-forge/`. They do not receive `docs/framework/` governance descriptors.

## Represents

The payload boundary represents what users and their agents can actually know after installation.

Governance descriptors are maintainer source documents. Installed files are the user-facing and agent-facing framework.

## Installed Contract

Every installed Open Forge file must stand on its own.

An installed file must contain the definitions, loading rules, authority boundaries, and routing instructions needed by an agent that has only the installed workspace.

Installed files may be short, but they must not rely on governance descriptors, maintainer notes, source repository history, or unpublished design context to be interpreted correctly.

## Descriptor Contract

Governance descriptors define what installed files must mean and contain.

A descriptor may be more explicit than its installed file, but any rule required for correct agent behavior must appear in the installed file or in another installed file that the agent is instructed to load.

If a descriptor changes required behavior, the matching installed file must change in the same work.

## User Documentation Contract

README files and user guides may explain concepts with examples.

User documentation does not replace installed agent instructions. If an agent must obey or route by a rule during normal work, that rule belongs in the installed payload.

## Implementation Contract

Maintainers must review installed files as the final user-facing product.

Implementation wording must use concrete installed routes and terms that are defined in installed files. Governance-only terms must not leak into installed files unless the installed payload defines them first.

Generated entries remain navigation metadata plus reserved load policy. They expose routes, but authored installed content must explain how those routes are used.

## Why

This boundary keeps Open Forge honest.

The framework is plain files on the user's machine. A user can inspect only what was installed, and an agent can follow only the instructions it can load.

## Alignment Checks

The payload boundary is aligned when:

- users can understand the installed payload without `docs/framework/`
- agents can route correctly from installed files alone
- governance descriptors never become hidden runtime context
- required behavior appears in installed files
- installed files avoid governance-only terms
- descriptor changes that affect behavior update the matching payload file
- user documentation explains but does not replace installed agent instructions
