---
open-forge:
  description: Proposed lossless wording improvements across Core and Extensions, with inheritance evidence and protected repetition
  tags: [Memory, Working, Proposal, Wording, Framework, Contextual]
---

# Source wording proposal

## Outcome and status

The maintainer requested this proposal on 2026-09-22: review wording in `src`
from the top down for logical consistency and duplicated inherited ideas,
preserving meaning, tone and style. The maintainer authorized applying W01–W08 on 2026-09-22. All eight are now
applied to source and the matching workspace copies for the maintainer’s review.

Baseline: `develop` at `f5f91a92`. Work is isolated on
`codex/src-wording-proposal`. The earlier beta correction is squash-integrated;
its tested product tree is unchanged. The Planning recipe structures this work.

The eight changes below record the authorized before/after wording. Most of the current wording should stay. No new universal
rule, primitive, loading tag, mandatory document, permission or implementation
behavior is proposed. Compression is useful only where the surviving sentence
still carries each condition, exception and distinction.

The separate [default Skill indexing task](cli-development/tasks/task47-default-skill-indexing.md)
records the requested future capability. Do not describe that capability as
current behavior or fold it into this lossless wording change.

## Coverage and evidence limits

[The inventory](src-wording-inventory.md) accounts for all 71 Markdown files
under `src`, all seven Extension manifests and the 101 OutputText C# files.
The semantic top-down read covers the 14 Core files and 55 Extension Markdown
files, package descriptions and dependency relationships. It includes templates,
standalone READMEs, recipe entrypoints and their bodies.

CLI treatment is deliberately different: the test README was read, preserved
transcripts were identified as historical evidence, and all OutputText files
were inventoried with a textual scan for loading, inheritance, authority,
default selection and Skill wording. Shared help, Index help and the relevant
command-definition surfaces were inspected. This is not an exhaustive audit of
all runtime messages, exception paths or C# comments. Task 39 still owns that
behavior-backed audit; this proposal makes no runtime-message changes or claim
that every command sentence is correct. Snapshots, JSON schema, text IDs and
code identifiers are not independent prose candidates.

## Top-down meaning and loading

| Level                                 | Meaning defined here                                                     | What lower levels must retain                                                                             |
| ------------------------------------- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- |
| Startup shims and Loader              | Selection, authority, inherited Axioms, tags, overwrites and restoration | The Loader remains the common rule source; an ordinary link does not activate a scope or change authority |
| Core primitive and Memory entrypoints | Local roles, required shapes, status and lifecycle                       | Child rules specialize the role without silently weakening or duplicating the inherited general rule      |
| Optional Extension routes             | Additional conventions in the installed Core routes                      | Package dependency is not loading inheritance; Memory roles do not become a mandatory process             |
| Native Skill and recipe catalogue     | Native entry boundary, recipe selection and recipe conventions           | Native discovery may enter SKILL.md directly; keep sufficient local context and authority limits          |
| Selected recipe                       | Goal, ordered method and completion evidence                             | Steps and Completion have different jobs; similar text is often a useful acceptance check                 |
| Template and copied result            | Starting prompts and independent destination                             | A copy does not inherit the Template catalogue; preserve its preparation and status safeguards            |
| Package README and CLI output         | Independent entry for a human reader                                     | Do not assume the reader has read a parent README, Loader, prior command or sibling output section        |

Safe deletion requires a guaranteed loaded source, the same scope and requirement
strength, and no lost local condition. Physical nesting, dependency installation,
similar words and a link are insufficient evidence on their own.

## Authorized replacements

Each location is relative to the repository root. The quoted before-text was
checked against the baseline. Keep the surrounding headings, metadata, links and
order unless an individual candidate explicitly describes a move.

### W01 — Keep the restoration rule in the Loader

Source: [src/open-forge/.agents/templates/_templates.md](../../../src/open-forge/.agents/templates/_templates.md) at baseline line 27.

The second sentence repeats the Loader rule word for word. Templates are reached through that Loader.

Before:

```markdown
- Users may edit, replace, scope, or remove Templates. Removed defaults stay removed unless the user asks to restore them.
```

Proposed:

```markdown
- Users may edit, replace, scope, or remove Templates.
```

Preservation check: The Loader, Routing / Management And Customization, retains the identical prohibition and its user-request exception. Keep all four Template customization permissions. Do not remove the independently copied Template instructions.

### W02 — Use the inherited on-demand rule for child Directives

Source: [src/open-forge/.agents/directives/_directives.md](../../../src/open-forge/.agents/directives/_directives.md) at baseline line 19.

The Loader already defines untagged entries as on demand and makes loading tags conditional on selected parents.

Before:

```markdown
- Child entrypoints remain on demand unless explicitly given a loading tag.
```

Proposed: delete this bullet.

Preservation check: Retain the preceding requirement to select the child entrypoint before its Directive files and their narrower applicability. Retain the same-folder #LoadNow requirement and conflict reporting. This deletion is safe only under the existing Loader-first contract.

### W03 — Combine the Pattern promotion condition and its examples

Source: [src/open-forge/.agents/patterns/_patterns.md](../../../src/open-forge/.agents/patterns/_patterns.md) at baseline line 19.

Both bullets answer the same selection question. One item retains the rule and all three limiting examples.

Before:

```markdown
- Create or update a Pattern only when an accepted shape should guide future related work.
- Structure alone does not justify a Pattern. Do not promote one-off work, temporary transitions, or unsettled candidates on that basis.
```

Proposed:

```markdown
- Create or update a Pattern only when an accepted shape should guide future related work. Structure alone does not justify promoting one-off work, temporary transitions, or unsettled candidates into a Pattern.
```

Preservation check: Acceptance, applicability to future related work and the insufficiency of structure remain explicit. No optional shape becomes mandatory. Keep every separate application and exception rule.

### W04 — State the Vision source choice once

Source: [src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/vision.md](../../../src/extensions/project-documents/content/.agents/skills/use-workflow/references/project-documents/vision.md) at baseline line 11.

The two adjacent paragraphs currently repeat the instruction to use an existing source. Keep that instruction together with its destination and Template qualifications.

Before:

```markdown
Produce a candidate or accepted Vision that makes the subject's purpose, core value, first useful version, boundaries, and success clear. Use existing sources when they already own the answer; a new document is optional.

[Document Templates](../../../../templates/documents/_documents.md) provide optional starting files. Use existing defining sources when they already answer the question; the Documents category is a convention, not a required destination.
```

Proposed:

```markdown
Produce a candidate or accepted Vision that makes the subject's purpose, core value, first useful version, boundaries, and success clear.

Use existing defining sources when they already answer the question. A new document is optional, and the Documents category is a convention, not a required destination. [Document Templates](../../../../templates/documents/_documents.md) provide optional starting files.
```

Preservation check: Preserve candidate versus accepted status, every part of the Vision outcome, the optional new document, optional Templates, the existing link and the non-mandatory Documents destination. No dependence on another recipe is introduced.

### W05 — Keep the Documents delegation rule and acceptance limit together

Source: [src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md](../../../src/extensions/project-documents/content/.agents/memory/crystallized/documents/_documents.md) at baseline line 17.

The Loader already states that a link does not change either source’s authority or other boundaries. Keep the Document-specific delegation and the explicit acceptance limit.

Before:

```markdown
- When a Document names another source as authoritative, follow that source for the detail it defines rather than duplicating it.
- A linked destination retains its own detail and authority. Linking to it does not make its content accepted.
```

Proposed:

```markdown
- When a Document names another source as authoritative, follow that source for the detail it defines rather than duplicating it. Linking to it does not make its content accepted.
```

Preservation check: The loaded Loader retains link/authority separation. Keep “when ... names ... as authoritative”, the detail boundary, the no-duplication instruction and the acceptance warning. Do not turn every link into authoritative delegation.

### W06 — Use defines for a work record’s answer

Source: [src/extensions/planning/content/.agents/patterns/work-records.md](../../../src/extensions/planning/content/.agents/patterns/work-records.md) at baseline line 34.

Here “owns” means which source answers a question, not possession or file lifecycle. The dictionary already distinguishes those meanings.

Before:

```markdown
- The Task owns the outcome and completion criteria. A separate Plan owns the sequence only when separated; the Task then links to it.
- A Checkpoint owns additional live continuation state. Link to the Task and Plan rather than repeating their specifications or finished history.
```

Proposed:

```markdown
- The Task defines the outcome and completion criteria. A separate Plan defines the sequence only when separated; the Task then links to it.
- A Checkpoint records additional live continuation state. Link to the Task and Plan rather than repeating their specifications or finished history.
```

Preservation check: Preserve all three source roles, conditional splitting, Task-to-Plan linkage and the prohibition on copied specifications/history. The table header “Owns” in this file should become “Defines”. Do not mechanically replace ownership language describing package files or operating-system ownership.

### W07 — Keep only the collaboration-specific adaptation instruction

Source: [src/extensions/collaboration/content/.agents/templates/collaboration/_collaboration.md](../../../src/extensions/collaboration/content/.agents/templates/collaboration/_collaboration.md) at baseline line 13.

The loaded Core Templates parent already requires replacing metadata/placeholders and rebasing links. The decision-specific trimming criterion belongs here.

Before:

```markdown
- Replace metadata and prompts, rebase links, and remove sections that do not help the decision.
```

Proposed:

```markdown
- Remove sections that do not help the decision.
```

Preservation check: Core Templates / Selection And Use retains those exact preparation duties. Keep the full preparation prompt inside brainstorming.md, which may be copied and read independently. Keep all candidate-status, record-need and optional-Task rules.

### W08 — Remove one repeated selection sentence inside the Skill

Source: [src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md](../../../src/extensions/workflows/content/.agents/skills/use-workflow/SKILL.md) at baseline line 16.

The same file’s selection step 2 already requires only relevant scope entrypoints and the selected recipe; Follow The Recipe separately requires its context before acting.

Before:

```markdown
Do not read every recipe to choose one. The catalogue is navigation, not a list of stages to execute. Following a linked recipe requires that recipe's applicable scope context, not its unrelated siblings.
```

Proposed:

```markdown
Do not read every recipe to choose one. The catalogue is navigation, not a list of stages to execute.
```

Preservation check: This relies only on the same SKILL.md, not on native discovery loading the Open Forge Loader. Keep both supporting instructions, Goal inspection, explicit opt-out, permission limits and the no-bulk-reading instruction. Do not remove similar safeguards from independently invoked Skills on the assumption of physical ancestry.

## Repetition to preserve

- Keep both Memory closeout bullets. Integrating useful durable outcomes before
  dependent work is a different duty from accounting for accepted outcomes,
  unsettled findings and temporary continuation state before closeout. Combining
  them produced a denser sentence without removing a redundant obligation.

- The shared customization paragraph appears in five package READMEs. Those
  READMEs are independent human entrypoints, not children that inherit prose.
  Keep the safety information local; identical wording also prevents drift.
- Fifteen copied starters repeat metadata/prompt/link preparation. Keep it:
  their future readers may never open the catalogue. Do not introduce includes
  or a template engine just to avoid source duplication.
- Four recipe scope entrypoints use the explicit inherited-Axioms marker. It
  communicates the absence of local additions and participates in the current
  Markdown convention. Leave it alone.
- Recipe Steps specify actions; Completion specifies acceptance evidence. Keep
  both rather than deleting completion checks as restated implementation steps.
- Handoffs freeze a transfer boundary; Checkpoints hold changing continuation
  state. Keep their distinct timing and preservation rules, including the
  exception for a planned resumption and the prohibition on routine copies.
- Memory acceptance, evidence and authority are different facts. Never compress
  “evidence supports” into “evidence accepts”, or turn candidate storage into
  permission to act.
- No-files-changed, no-recovery-needed, no-recovery-created and unknown-state
  messages are not interchangeable. Global options also need to remain usable
  from standalone command help. No CLI deduplication is proposed.

## Consistency questions outside a lossless rewrite

The current default Index scope excludes detached Skill reference catalogues;
its help accurately says reachable entrypoints. That is a behavior gap tracked
separately, not wording to silently correct ahead of implementation.

The CLI test README says native evidence compiles “this project” although it
introduces several projects. A later clarification should identify the exact
native suites from the delivery configuration. It is not included in the eight
replacements without that more focused contract check.

Remaining ownership vocabulary in templates and related recipe descriptions
should be reviewed by relationship, not globally replaced. A package owning a
file and a source defining a fact must remain distinguishable. W06 is a bounded
example, not permission for a bulk synonym substitution.

## Implementation and acceptance

1. The maintainer authorized all eight replacements, including the W06 table-header change. Their baseline was checked before editing.
2. Apply the prose change to source, then reconcile affected maintenance contracts
   and the workspace's corresponding installed prose under the repository's
   existing dogfood rules. Preserve user overwrites and independently copied work.
3. Compare before/after meaning clause by clause: actors, scope, authority,
   timing, requirement strength, conditions, exceptions, links and selection.
   Recheck each inheritance premise against the actual loaded chain.
4. Review the coherent changed prose pack once. An independent writing review
   belongs at implementation acceptance, not as a claim that this planning record
   changes current authority.
5. Check formatting, relative links, package assembly and generated navigation
   with the applicable existing checks. Do not regenerate captures to absorb
   unexpected output changes; qualify affected payload behavior if an actual
   implementation changes it.

Acceptance requires no meaning loss and no changed loading, authority, runtime
or filesystem behavior. Keep the current calm, direct voice, category questions,
terminology and complete natural sentences. Do not impose a word-count target.

## Original proposal verification

All eight before-text excerpts match the baseline files. The source inventory
and package dependencies were inspected; repeated prose was compared by loading
boundary. Proposal links and formatting are checked before closeout. No runtime
build or scenario run is needed for this task-only change, and none is claimed.
That planning-only boundary ended with the maintainer’s implementation request.

## Implementation verification

All eight source edits and eight matching workspace edits are applied. Local
generated Entries, frontmatter and overwrite files are preserved. The Templates
maintenance contract now locates the unchanged restoration requirement in the
inherited Loader. The formatter also aligned the Work Records tables.

Verification passed: focused Markdown formatting, all eight authored counterpart
pairs, preserved metadata and Entries, and 245 relative links. The managed Release
build passed with zero warnings or errors; all four embedded Framework/Extension
payload checks passed with no skips. The independent coherent writing review
reported no findings across the 17 source, counterpart and maintenance files.
No CLI logic, output factories, snapshots or default Skill indexing behavior is
changed. Full managed/native scenario suites were not rerun for this prose-only
change; embedded source bytes and hashes were checked by the focused payload tests.
