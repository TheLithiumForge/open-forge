---
open-forge:
  description: Assessment of the C# design and style directives against the shipped CLI, the rules that already exist and are violated, the asymmetry that permits unmigrated duplication, and ranked refactoring targets
  tags: [Memory, Analysis, Contextual, Candidate, CLI, CSharp, Directives, Refactoring, Duplication]
---

# C# Directives And Structure

## Assessment of the directives

They are good — specific, evidence-backed, with rationale and authoritative
references. Several rules are better than what most codebases write down:

- _"Treat repeated member forwarding as a design signal"_, with a concrete
  example of the failure shape.
- The enum-exhaustiveness rule with an explicit discard arm throwing
  `ArgumentOutOfRangeException`, and the instruction **not** to add analyzer,
  source-generator or discriminated-union machinery to claim exhaustiveness.
- _"Do not promote a local branch merely to claim reuse. A static strategy earns
  a wider scope when an accepted shared contract already owns the policy or
  multiple real consumers demonstrate identical meaning."_
- _"A leaf-local `Shared` folder states ownership rather than multi-consumer
  reuse."_ — a precise statement of a distinction most projects blur.
- _"Do not replace a crowded flat folder with one-file microfolders."_

I would keep all of them. Three observations follow.

### The parser rule already exists

The rule proposed in [hand-rolled-parsing.md](hand-rolled-parsing.md) is already
written, in `directives/csharp/design.md`:

> _"Let the accepted YAML, Markdown and JSON libraries interpret their formats.
> Use their models, tokens, syntax trees and source spans for the facts the
> application needs. … **Do not rescan raw text with splits or regular
> expressions to duplicate syntax the library already handles.**"_

That is precisely the rule, and there are four violations of it. **Nothing needs
to be added.** What is missing is enforcement, which is the same pattern found in
the framework itself — the placement guidance exists and the placement feedback
does not.

The one thing the rule does _not_ say, and which the incident argues for, is the
positive architectural half: that a shared parser layer produces one document
model everything else consumes. The rule forbids re-scanning; it does not
require a single owner. Proposed addition, as one clause:

> Markdown and YAML enter the system through `Framework/Documents` and leave it
> as the typed document model. Every other layer consumes that model. A command,
> renderer, planner or script never re-derives document structure.

### Gap 1 — the sharing rule is asymmetric

_"Do not promote a local branch merely to claim reuse"_ guards against premature
abstraction, and correctly. But **nothing guards the other direction.** There is
no rule saying: when a shared owner already exists, use it; when a promotion
happens, migrate the copies.

The measured consequence is in §2 below, and it is the dominant structural
defect: shared types exist, are correct, are used by some callers, and are
reimplemented verbatim by others. The directive set makes sharing hard to
_start_ and says nothing about finishing it.

Proposed clause:

> When an accepted shared owner exists for a policy, use it. Promotion is not
> complete until the local copies are removed; a shared type with surviving
> duplicates is worse than either alternative, because a change now has to be
> found in several places.

### Gap 2 — no naming rule

> **Corrected 2026-09-11.** The measurement below is wrong on both counts, and
> the rename it proposed was withdrawn before phase 3 started. `CliOutputFormat`
> has exactly two members, `Human` and `Json`, so "Human" names a format rather
> than a negation; and stripping `Human` from all 65 names produces zero
> collisions, so none are indistinguishable. The proposed clause was still
> accepted into the style directive and remains correct in general — it simply
> does not apply here. What is genuinely wrong is smaller: four suffixes for one
> role across nine types. See
> [CLI Experience Remediation](../../../working/cli-development/tasks/task30-cli-experience-remediation.md).

Nothing addresses naming by negation. The result is 65 distinct `*Human*` types
— `DoctorEvidenceHumanRenderer`, `ExtensionUpdatePathsHumanRenderer`,
`ContextFramingHumanRenderer` — where "Human" means _not JSON_ and says nothing
about what the type does. Two of those are indistinguishable by name.

Proposed clause:

> Name a type by what it produces or owns, never by what it is not. A renderer
> is named for its output shape or its subject, not for the format it is not
> emitting.

### Gap 3 — one question, one implementation

The parser rule covers _formats_. It does not cover the more general case found
in §2: two implementations of "find the `## Axioms` section", neither of which
re-parses a format — one asks the document model, the other splits lines. The
directive would benefit from stating the general form:

> A structural question about a document has one implementation. A second answer
> to the same question is a defect even when both are correct today.

## Measured conformance

### 1. One-file microfolders — an explicit rule, 27% violated

The directive: _"Do not replace a crowded flat folder with one-file
microfolders."_

**168 of 624 folders under `OpenForge.Cli.Core` contain exactly one `.cs` file
and no subfolders.** Examples: `Commands/Cleanup/Models/Binding`,
`Commands/Doctor/Models/Request`, `Commands/Context/Shared/Graph`,
`Commands/Extension/Create/Models/Binding`.

This is the structural signature of task-boundable decomposition described in
[contract-versus-code.md](../cli-design-retrospective/contract-versus-code.md):
1,866 files at a median of 72 lines. Each Task produced its folder and moved on.

**Highest-value, lowest-risk refactor in the codebase.** Collapsing a one-file
folder into its parent changes no behavior, touches one namespace line per file,
and makes the tree navigable. Roughly 168 folders removed.

### 2. Shared owners that exist and are bypassed

This is the dominant duplication pattern, and it is not "no sharing" — it is
**promotion without migration**.

**`WorkspaceSelectionWireVocabulary`** lives at
`Commands/Shared/Rendering/WorkspaceSelectionWireVocabulary.cs`, maps two enum
values to two strings, and is used at **16 call sites**.

**Twelve other files reimplement the identical mapping locally:**

```
Commands/Cleanup/Shared/Rendering/CleanupWireVocabulary.cs
Commands/Doctor/Shared/Rendering/DoctorWireVocabulary.cs
Commands/Status/Shared/Rendering/StatusWireVocabulary.cs
Commands/Extension/{Install,Remove,Update}/Shared/Rendering/*JsonProjection.cs
Commands/Library/{Attach,Detach,Inspect,List,Sync}/Shared/Rendering/*Presentation.cs
Shell/Presentation/Shared/Rendering/CliCompactJsonProjection.cs
```

The bodies are byte-identical apart from the exception helper:

```csharp
CliWorkspaceSelectionMethod.CurrentDirectory  => "current-directory",
CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
```

**`CliResultHelp`** lives at `Shell/Presentation/Shared/Help/CliResultHelp.cs`.
**10 of 22 `*HelpSections` files do not use it** — `CleanupHelpSections`,
`DoctorHelpSections`, `ExtensionHelpSections`, `FindHelpSections`,
`LibraryHelpSections`, `RouteHelpSections`, and four `Route*` leaves. That is
part of why the exit-code and stream block repeats across every command's help.

Both are mechanical, behavior-preserving deletions.

### 3. The `*Vocabulary` cluster

Seven `*Vocabulary` types, 1,091 lines, each mapping a mixture of
command-specific enums and **shared** ones. The command-specific halves are
correct and should stay local — that is exactly what the directive intends. The
shared halves (`CliWorkspaceSelectionMethod`, `OperationalValueState`,
`CliSemanticStatus`) should not be there.

Worth noting the counter-example: `CliSemanticStatus` **is** properly centralized
in `Shell/Definitions/CliStatusDefinitions.cs` and is not duplicated anywhere.
So the pattern is understood; it was applied to one enum and not the others.

### 4. Files that outgrew their type

Against a codebase whose median file is 72 lines:

| Lines | File                                                                        |
| ----- | --------------------------------------------------------------------------- |
| 1,092 | `Commands/Extension/Update/Shared/Planning/ExtensionUpdatePlanner.cs`       |
| 1,019 | `Commands/Cleanup/Models/Result/CleanupResultFacts.cs`                      |
| 809   | `Commands/Repair/Shared/Application/RepairLibraryRecoveryApplication.cs`    |
| 748   | `Commands/Extension/Update/Shared/Planning/ExtensionUpdateReconciler.cs`    |
| 613   | `Commands/Library/Shared/Completion/LibraryMutationCompletionProjection.cs` |

A 1,019-line file under `Models/Result/` is a data shape that grew past what one
type should carry, and the directive already covers it: _"When one `Models/`
folder grows to roughly five to ten types, group the models further by cohesive
topic."_ These are the inverse — one type carrying what should be several.

Lower priority than §1 and §2 because splitting them requires judgment about
where the seams are, not just deletion.

## Ranked targets

Ordered by value per unit of risk. All are behavior-preserving.

1. **Delete the 12 duplicate `WorkspaceSelection` mappings**, use the shared
   type. Smallest change, clearest win, and it establishes the pattern.
2. **Route the 10 non-conforming `*HelpSections` through `CliResultHelp`.**
   Prerequisite for the help restructuring in G5, and removes the repeated
   exit-code block at its source.
3. **Collapse the 168 one-file folders.** Mechanical, one namespace line per
   file, and it is the single biggest improvement to navigability.
4. **Move the shared enum mappings out of the `*Vocabulary` types**, keeping the
   command-specific halves local.
5. **Delete the second `## Axioms` parser and its fence tracker** (§Gap 3 and
   [hand-rolled-parsing.md](hand-rolled-parsing.md)). Behavior-changing in the
   sense that it _fixes_ a divergence, so it needs a test first.
6. **Split the five oversized files.** Judgment required; do last, and only where
   a seam is obvious.

Items 1–4 are pure subtraction and could be done in one pass. Item 5 belongs with
the parsing work. Item 6 is optional and should not block a release.

Note that this list deliberately excludes the largest structural opportunity —
the View-selection layer in G4, which removes renderer volume rather than
rearranging it. Refactoring the renderers before that contract exists would be
rearranging code that is about to shrink.

## Backlog ordering

The remediation backlog's ordering has grown organically across this audit and
now carries 129 tasks in seven groups. Sequencing it properly — resolving which
tasks are made unnecessary by earlier decisions, and which are genuinely
parallel — is its own analysis and should be done in a dedicated session rather
than incrementally. Recorded here so it is not lost.
