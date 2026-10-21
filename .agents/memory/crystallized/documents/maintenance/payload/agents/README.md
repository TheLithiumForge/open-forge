---
open-forge:
  description: Current maintenance contract for the optional installed project guide
  responsibility: Keep the README removable and useful without becoming a rule or format dependency
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Guide]
---

# Workspace Guide Maintenance Contract

## Source

[`src/open-forge/.agents/README.md`](../../../../../../../src/open-forge/.agents/README.md) is the optional installed guide for people running the project. This repository does not carry a dogfood guide.

The [loader](loader.md) defines reading behavior and the [authoring reference](authoring.md) defines workspace file formats.

## Contract

- The opening says this README is an optional guide that may be deleted. No rule, agent reading procedure or command depends on it.
- Keeping it deleted through Update requires recording `.agents/README.md` in `removedFiles` in `open-forge.json`. Deletion does not remove the loader or authoring reference.
- It stays unrouted and has no frontmatter or Instructions section. File-format requirements live in authoring.md.
- The guide starts with reusing an existing document. Its Include example and equivalent handwritten loader row agree on destination, description and loading. Exclude leaves the target file alone.
- It encourages one useful project Directive first, with rules under Instructions and a link to the authoring reference. Content kinds are explained briefly and adoption stays optional.
- Memory states require the Memory catalogue but not each other. The saving policy requires Working. Other categories need only the loader unless their own content adds a dependency.
- Turning a branch off, moving or deleting nested files, recording exclusions and stopping CLI management remain distinct actions. Disabling progress saving stays separate from Emerging candidate capture.
- Include reuses a file in this project, a Library projects a registered workspace folder through links and an Extension installs packaged files across projects. The guide does not promise a different Library model.
- Detail links lead to the loader, authoring reference, Help Concepts, command help and website. The guide stays short and does not become a second runtime rule source.

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes and [FrameworkPayloadSelectionTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Distribution/Shared/Sources/FrameworkPayloadSelectionTests.cs) for exact-file exclusions.
- Review the guide directly and check relative links. Compare examples with the current command definitions.
- In an isolated installed fixture, remove README, record its removedFiles exclusion and verify Update keeps it absent. Trace startup, selected authoring and writing a valid Directive and catalogue without the guide. Confirm no required link depends on README.
