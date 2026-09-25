---
open-forge:
  description: Fixture setup that supports existing tests and branches that remain unexercised
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Exercised And Unused Fixtures

Return to the [review](./_cli-experience-coverage.md). A helper or seeded file contributes coverage only when the actual test exercises and asserts the relevant behavior. These findings qualify the method and scenario matrices.

## Core And Navigation

| Fixture | Evidence limit |
| --- | --- |
| [PublishedCleanupWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedCleanupWorkspace.cs) | ForeignBlockedPath points to foreign recovery workspace bucket. Cleanup E2E cannot prove same selected workspace mixed damaged+valid continuation. |
| [PublishedFindWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedFindWorkspace.cs) | CreateAttention, CreateIncomplete and AddUnavailableFrontmatterSources have no test consumers; source presence is not E2E coverage. |
| [PublishedRouteInitWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInitWorkspace.cs) | CreateMetadataIncomplete unused. |
| [PublishedContextWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedContextWorkspace.cs) | AddRoutedSkills unused; no native Skill resources journey. |
| [PublishedRouteWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteWorkspace.cs) | Native Skill seeded by CreateComplete, but tests assert neither native row nor bytes. CreateIncomplete and CreateAttention unused. |
| [PublishedRouteInspectWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteInspectWorkspace.cs) | CreateIncomplete unused; Complete has LoadNow and overwrite but tests assert identity/layers only. |
| [PublishedUpdateWorkspace.cs](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedUpdateWorkspace.cs) | Retired target inserted by editing ownership JSON and version; not a real pinned source VERSION-PAIR. |

## Authoring And Extensions

| Command fixture | Exercised state | Unexercised branches |
| --- | --- | --- |
| [route create](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteCreateWorkspace.cs) | Creates loader plus valid memory/project-alpha parent and absent overview target; cleanup positively owns the application-created target. | No exercised Template, optional metadata, absent intermediate parent, occupied target, live lock, cancellation or deterministic partial-write fixture. |
| [route update](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteUpdateWorkspace.cs) | Creates target/parent/template with authored body; only SeedAmbiguousTarget is exercised by owned process tests. | RemoveTemplate, RemoveAgentsRoot, SeedEmptyBodyTarget and template fixtures are helpers without a corresponding EndToEnd test; no cancellation/lock/partial fixture. |
| [route move](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteMoveWorkspace.cs) | Seeds leaf, overwrite, category, relative links, README/definitions and opaque image resource; occupied destination is exercised. | Category destination, ownership removal, invalid UTF-8 and other safety helpers are not exercised by owned EndToEnd methods; no self/lock/cancel/partial public process fixture. |
| [route remove](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedRouteRemoveWorkspace.cs) | Seeds leaf/category plus incoming README and definitions references, child/notes/opaque resource and lifecycle lock. | No owned EndToEnd ambiguous selection, managed source, unsafe link, live lock, cancellation, partial failure or unmanaged ownership fixture. |
| [extension list](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionListWorkspace.cs) | Creates empty or trusted ownership and one local extension.json source; explicit available source is read-only. | CreateMalformedSource and leftoverState options are not exercised; no missing/unreadable/blocked source or changed/missing installed file process test. |
| [extension inspect](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInspectProcessTests.cs) | Creates toolkit ownership, current target, selected package and malformed leftover lifecycle; Theory has unchanged and changed rows. | No dependency/version/retired/missing-source/unknown-ownership/ambiguous-source/cycle fixture in owned EndToEnd methods. |
| [extension create](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionCreateWorkspace.cs) | Creates empty catalogue with unrelated sibling and workspace note/lifecycle/recovery sentinels; apply and JSON dry-run use stable development-toolkit. | No occupied/missing catalogue, invalid ID, prompt, cancellation, partial-write or unreadable-source process fixture. |
| [extension install/update/remove](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedExtensionInstallProcessTests.cs) | Shared workspace/source has workspace-note and two independent no-dependency packages alpha/toolkit plus persistent lock-store environment; update edits source payload; remove tests preserve source and unowned note. | No dependency closure, shared owner, external permission, force conflict, retired/prune content, source removal, live lock, cancellation, ownership publication failure or partial post-effect public process fixture. |

## Libraries

[PublishedLibraryWorkspace](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/PublishedLibraryWorkspace.cs) creates minimal .agents/source roots, optionally a consumer route, direct ownership JSON and real symlinks. Most tests therefore start from hand-seeded registrations. Attach apply checks an actual relative file link; the grant theory publicly attaches before its sync/detach rows; mixed sync genuinely carries preview into apply.

Library snapshots include ordinary files, directory presence and raw link targets without following links. No real terminal driver, live competing writer, denied source read, absent source-root lifecycle or synchronized after-effect failure is exercised in these methods. GrantDocs writes settings directly; its presence is not evidence of a successful user grant. Most mutation cases lack a separate unrelated-file sentinel and complete independent membership-set check.
