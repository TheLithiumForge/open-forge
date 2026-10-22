---
open-forge:
  description: Current maintenance contract for the installed project guide
  responsibility: Keep the README removable and useful, holding the file formats that Help also prints
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Guide]
---

# Workspace Guide Maintenance Contract

## Source

[`src/open-forge/.agents/README.md`](../../../../../../../src/open-forge/.agents/README.md) is the installed guide for people running the project. This repository does not carry a dogfood guide.

The [loader](loader.md) defines reading behavior. The README's last section, `Writing workspace files`, describes workspace file formats, and `open-forge help concepts` prints that same section from the shipped copy.

## Contract

- The guide holds workspace file formats that remain available through `open-forge help concepts`. Keeping it deleted through Update requires recording `.agents/README.md` in `removedFiles` in `open-forge.json`.
- It stays unrouted and has no frontmatter. Its file-format section ends the file and keeps the heading `## Writing workspace files`, which Help locates. Its example catalogue and rule file show `## Axioms`, `## Entries` and `## Instructions` inside code blocks only.
- The catalogue format and example put the folder's question in a `##` heading immediately under the title, followed by a short paragraph on what the folder holds. The format requires a catalogue to describe its own folder without naming other routes.
- The guide starts with reusing an existing document. Its Include example and equivalent handwritten loader row agree on destination, description and loading. Exclude leaves the target file alone.
- It encourages one useful project Directive first, with rules under Instructions and a link to its own rule-file format. Content kinds are explained briefly.
- Turning a branch off, moving or deleting nested files, recording exclusions and stopping CLI management remain distinct actions.
- Include reuses a file in this project, a Library projects a registered workspace folder through links and an Extension installs packaged files across projects. The guide does not promise a different Library model.
- Detail links lead to the loader, Help Concepts, command help and website. The guide part stays short, and the format section describes shapes, not runtime rules.

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes and [FrameworkPayloadSelectionTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Distribution/Shared/Sources/FrameworkPayloadSelectionTests.cs) for exact-file exclusions.
- Review the guide directly and check relative links. Compare examples with the current command definitions.
- In an isolated installed fixture, remove README, record its removedFiles exclusion and verify Update keeps it absent. Trace startup and writing a valid Directive and catalogue from `open-forge help concepts` alone. Confirm the loader names README by path, not by link, so deleting it leaves no broken link.
