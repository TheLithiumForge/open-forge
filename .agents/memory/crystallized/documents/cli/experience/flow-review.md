---
open-forge:
  description: Compact flow list for maintainer validation before new tests
  tags: [Memory, Document, CLI, Review]
---

# Flow Validation

Validate these user outcomes before new end-to-end tests are implemented. F01–F24 adapt the supplied collection; F25–F26 are recommended additions. Full [scenario dispositions](assessment.md) distinguish retained, improved, deferred and omitted cases. Execution observations are recorded separately and do not redefine the targets.

| Flow | User outcome | Selection |
| --- | --- | --- |
| [F01](flows/f01-install-and-confirm-the-workspace-is-ready.md) | Install Core and confirm the workspace is ready. | Retained |
| [F02](flows/f02-paste-a-native-skill-and-use-it.md) | Paste a native Skill and retrieve it with support files intact. | Retained |
| [F03](flows/f03-write-a-plain-note-and-let-indexing-help.md) | Index a plain or partial-metadata note without changing its bytes. | Retained |
| [F04](flows/f04-copy-a-template-and-keep-the-new-document-independent.md) | Copy a Template into Memory and keep the copy independent. | Retained |
| [F05](flows/f05-create-and-refine-a-nested-scope.md) | Create and refine nested guidance without unnecessary parent setup. | Retained |
| [F06](flows/f06-work-in-the-intended-workspace.md) | Run commands against the explicitly intended workspace. | Retained |
| [F07](flows/f07-find-the-relevant-guidance-without-loading-everything.md) | Find relevant guidance without loading unrelated material. | Retained |
| [F08](flows/f08-repair-a-link-after-a-manual-rename.md) | Repair references after a manual rename. | Retained |
| [F09](flows/f09-move-a-route-and-preserve-links-in-one-operation.md) | Move guidance while preserving authored links. | Retained |
| [F10](flows/f10-remove-a-route-without-deleting-authored-meaning.md) | Remove a route while retaining surrounding authored text; repeat safely. | Retained |
| [F11](flows/f11-install-update-and-remove-a-package-with-dependencies.md) | Install, update and remove a package with dependencies. | Retained |
| [F12](flows/f12-create-a-custom-package-then-install-from-that-source.md) | Create a custom package and install from its real source. | Retained |
| [F13](flows/f13-handle-an-occupied-target-without-accidental-overwrite.md) | Handle an occupied destination while preserving user edits. | Retained |
| [F14](flows/f14-approve-an-external-destination-once-or-persistently.md) | Approve exact external destinations once or persistently. | Retained |
| [F15](flows/f15-continue-past-unrelated-corruption-without-ignoring-safety.md) | Continue useful work despite unrelated malformed content. | Retained |
| [F16](flows/f16-attach-a-library-and-synchronize-changing-source-membership.md) | Attach a Library and sync source additions, changes and removals. | Retained |
| [F17](flows/f17-recover-a-missing-link-and-protect-a-changed-destination.md) | Restore missing links and preserve replacement files while continuing independent work. | Retained |
| [F18](flows/f18-detach-after-the-source-folder-disappears.md) | Detach after source disappearance, including already absent links. | Retained |
| [F19](flows/f19-inspect-a-partial-operation-and-choose-a-safe-continuation.md) | Inspect a partial operation and choose an evidence-based continuation. | Retained |
| [F20](flows/f20-run-concurrent-work-without-misleading-lock-advice.md) | Handle actual concurrent writers and retry after lock release. | Retained |
| [F21](flows/f21-read-more-detail-without-changing-what-happened.md) | Keep facts consistent across text/detail/JSON views and expose useful default warnings. | Retained |
| [F22](flows/f22-correct-an-incomplete-command-in-one-attempt.md) | Create without optional metadata, then enrich the same file. | Retained |
| [F23](flows/f23-review-and-remove-recovery-leftovers.md) | Clean eligible recovery leftovers while retaining ambiguous candidates. | Retained |
| [F24](flows/f24-keep-authored-content-intact-across-reading-and-maintenance.md) | Preserve authored bytes across reading and maintenance. | Retained |
| [F25](flows/f25-customize-through-an-overwrite.md) | Load an overwrite after its base without indexing it separately. | Recommended addition |
| [F26](flows/f26-select-only-needed-extensions.md) | Install only selected optional extensions and their declared dependencies. | Recommended addition |

The main proposed changes are optional metadata, unambiguous nested creation, harmless repeated removal/detach, useful partial results, and actionable warnings without extra verbosity flags. Preserve user changes and stop only the effects that lack a trustworthy target or necessary input.
