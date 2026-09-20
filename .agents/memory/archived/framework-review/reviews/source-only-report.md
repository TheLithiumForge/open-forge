---
open-forge:
  description: Original source review with frozen findings and the disclosed context independence limitation
  tags: [Memory, Archived, Contextual, Historical, Framework, Review]
---

# Task 28 — Frozen source framework review

This is an earlier review snapshot retained for the active Task 28 Git review. Its status and paths describe the recorded stage. Use [Task 28](../../cli-development/tasks/source-framework-review.md) for current decisions, completion, and remaining work.

Status: proposals only. Stage 1 completed before the local comparison. No product meaning or shared files were changed.

The most valuable improvements are to make continuity loading predictable, distinguish checking a claim from accepting it as current state, and give authors a small, complete route example. The shipped structure is coherent enough to retain. It does not need a new execution engine, more mandatory stages, or a larger taxonomy.

## Evidence and independence

- Source baseline: Git commit `8a52ede13ab6d622578a6cfae04031a9a5b2eb2d`.
- Reviewed every file under `src/open-forge/`, including hidden paths: **23 files, 751 lines, 38,637 bytes**. Every working source blob matched its blob at that commit.
- [source-inventory.json](../evidence/source-inventory.json) records each path, byte and line counts, SHA-256, working Git blob, and commit Git blob. The inventory was captured before source review. [source-static-checks.json](../evidence/source-static-checks.json) records the mechanical checks.
- Permitted writing guidance: `.agents/directives/public-facing-writing.md`. Its linked local references were not opened in Stage 1.
- **Independence limitation:** the root local `.agents/loader.md` was read before the attachment containing the context override was opened. That exposure cannot be undone. Its text was not used as evidence for these findings. No local roles, memory bodies, additional directives, or writing references were read before this report. Git status exposed filenames of concurrent work, not their contents. This is a source-evidenced assessment with disclosed prior exposure, not a claim of perfect context isolation.
- The review was performed sequentially by the primary agent, without delegated reviews. Source instructions were reviewed as product text, not executed as workspace instructions.
- This report's conclusions and source identities are frozen before reading local comparison evidence. Later comparison results belong in a separate report.

Locators below are relative to `src/open-forge/` at the recorded baseline; line ranges identify the exact reviewed passages. Drafts are proposals and have not been applied.

## What source alone establishes

| Question              | Explicit source statement                                                                                                                                                                                                        | Inference or remaining gap                                                                                                                                                                                              |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Purpose               | `AGENTS.md:5–9` provides workspace rules and context, entered through the loader.                                                                                                                                                | Open Forge is a Markdown framework for helping agents select relevant instructions and preserve useful knowledge. This is a synthesis, not a separately stated product promise.                                         |
| Users                 | The loader addresses agents and user direction; Skills delegate execution to the active agent runtime.                                                                                                                           | Agents are the immediate readers; people maintaining workspaces are also authors. No supported-runtime or user-profile list is supplied.                                                                                |
| Installation          | `AGENTS.md:7` expects `.agents/loader.md`. `CLAUDE.md:3–4` references both files.                                                                                                                                                | The source describes an installed layout, not how to obtain or install it. No install, upgrade, uninstall, or existing-file merge procedure can be established from this subtree.                                       |
| Daily entry           | Read the loader, follow relevant branches recursively, load tagged entries, and inherit ancestor Axioms (`loader.md:42–70`).                                                                                                     | Native discovery of `AGENTS.md` or interpretation of Claude imports belongs to the runtime and was not verified.                                                                                                        |
| Authority             | User direction and runtime/platform boundaries constrain work; only the loader and loaded recognized entrypoints define Axioms. Directives are required, and narrower binding instructions add to parents (`loader.md:17–29`).   | The interaction between accepted workspace defaults, Memory, and binding overrides needs clarification; see S03 and S06.                                                                                                |
| Navigation            | Root routes come from the loader. Entrypoints expose direct routes. Slugs narrow scope. A familiar folder name or tag does not create root or managed behavior (`loader.md:35–56`).                                              | Selection is semantic and agent-driven. The source does not promise enforcement, a search index, or deterministic relevance selection.                                                                                  |
| Loading               | `LoadNow` follows listed order; `KeepInMind` refreshes continuity at boundaries and globally reaches tagged non-entrypoint files (`loader.md:64–71`).                                                                            | The global exception has an ancestor-loading ambiguity; see S01.                                                                                                                                                        |
| Memory                | Working supports active continuation; Emerging holds candidates; Crystallized holds accepted knowledge; Archived holds history. Reuse and capture are deliberate (`memory/_memory.md:9–45`).                                     | These are states and ownership rules, not a requirement that every record move through all four states.                                                                                                                 |
| Optional capabilities | Guidance offers adaptable advice; Patterns define default shapes; Templates are copied and then independent; Workflows are optional recipes; Skills follow native runtime rules. `Extension` denotes optional packaged material. | There is one substantive Guidance file and no shipped leaf Directive, Pattern, Template, Workflow, Skill, or extension package in this source snapshot. Packaging and manager implementations are outside the evidence. |
| Maintenance/recovery  | Users can change or remove routes; removed defaults stay removed; overwrite companions specialize their base. Memory must stay current and avoid competing copies. CLI descriptions include indexing, status, and diagnosis.     | No actual recovery example or complete manual authoring procedure is included. CLI behavior cannot be verified from prose alone.                                                                                        |

The main ownership distinctions are useful and should survive revisions: Maps point to detail owned elsewhere; Decisions preserve choices and rationale; Documents explain current knowledge; Directives require behavior; Patterns define reusable shapes; Workflows describe optional execution; Templates supply independent starting content. Empty catalogs are valid extension points, not missing content by themselves.

## Findings, ordered by value

### T28-S01 — Specify what loading continuity ancestors activates

**Priority/type:** high; behavioral clarification requiring a policy choice.

**Locators:** `.agents/loader.md:26,42,49,64–70`; `.agents/directives/_directives.md:13–19`; `.agents/memory/working/checkpoints/_checkpoints.md:15–16`.

**Scenario:** a workspace has separate project A and project B scopes. An active B checkpoint carries `KeepInMind`. A task selects only A. The global continuity rule requires reading B's checkpoint and its missing ancestor entrypoints; a B ancestor also exposes `LoadNow` material unrelated to A.

**Problem and consequence:** `LoadNow` applies whenever an already-loaded parent exposes an entry. The continuity rule requires loading ancestors but also says to exclude unrelated descendants. It does not say whether merely loading those ancestors activates their `LoadNow` siblings. One reading loads unrelated B material; another skips a normally mandatory load. The current shipped tree has no cross-project checkpoint, so this is a rule-level customization scenario, not a demonstrated failure in the pristine tree.

**Proposed small change:** distinguish ancestors read to locate and interpret global continuity from routes selected for current work. A proposed replacement for the order/boundary clauses is:

> Read missing ancestor entrypoints before the continuity file so its route, scope, and inherited Axioms are known. Reading an ancestor only to reach a global continuity file does not select its other descendants or trigger their `LoadNow` entries. If the route is also selected for the task, apply its ordinary loading rules. Read the continuity file and its overwrite, then follow any `LoadNow` entries it exposes. Its scope remains unchanged.

**Meaning to preserve:** global continuity survives unrelated route changes; ancestry remains known; loaded applicable Axioms retain their scope; ordinary selected routes still honor `LoadNow`.

**Decision needed:** explicitly exempt continuity-only ancestor traversal from sibling `LoadNow`, as proposed, or explicitly accept that such siblings load and qualify “exclude unrelated descendants.” The proposal changes the broad literal reading of `LoadNow`; it must not be passed off as an editorial fix.

### T28-S02 — Separate validation and restoration from acceptance

**Priority/type:** high; semantic clarification with possible behavior change.

**Locators:** `.agents/loader.md:75–76`; `.agents/memory/emerging/_emerging.md:17`; `.agents/memory/crystallized/_crystallized.md:14–19`; `.agents/memory/archived/_archived.md:17`.

**Scenario:** an agent checks the calculations in an Emerging analysis proposing a new project direction. The calculations are correct, but nobody has accepted the recommendation. Alternatively, an old record is restored for current investigation.

**Problem and consequence:** the definition of `Contextual` says material is not accepted current state “unless restored, validated, accepted, or promoted.” Emerging likewise ends its contextual qualification upon validation or promotion. These alternatives can be read as independently sufficient to create accepted state, while Crystallized lists specific sources of acceptance and says tags and confidence do not create it. Verification of evidence and acceptance of the resulting choice are different acts.

**Before:** “Useful context that is not accepted current state unless restored, validated, accepted, or promoted.”

**Proposed after:**

> `Contextual` — Useful context whose storage or tag does not establish acceptance. Checking, restoring, or moving material does not by itself make it accepted current state. Preserve any explicitly accepted temporary choice with its source, scope, and expected expiration. Promote durable accepted knowledge only when an acceptance source defined under Crystallized Memory supports it.

For Emerging, use: “Keep material contextual until an acceptance source supports its current meaning. Validation may establish evidence without accepting a recommendation.” Link to the existing acceptance rule rather than copying its list.

**Meaning to preserve:** validation matters; delegated authority, choices necessarily entailed by requested actions, and declared external authority can establish acceptance without another user confirmation; temporary accepted choices may remain in Working Memory.

**Decision needed:** whether validation or promotion was intended as independent authority. If yes, define who may perform it, for what kind of claim, and within which scope. Otherwise, adopt the clarification. Do not require users to approve every verified fact.

### T28-S03 — Clarify which workspace state can replace a default

**Priority/type:** medium; authority clarification, not a proven universal contradiction.

**Locators:** `.agents/loader.md:26–28,55–56,60`; `.agents/memory/_memory.md:15–18,43`; `.agents/patterns/_patterns.md:24–25`.

**Scenario:** a Crystallized document records an accepted project practice that differs from a shipped Pattern, or from an active Directive. A returning agent must determine whether to use the recorded choice immediately or report a binding conflict.

**Problem and consequence:** “Accepted workspace-specific state replaces Open Forge defaults” does not name the relevant kind of default or replacement. Adjacent rules make binding instructions additive; Memory places relevant Core routes above Memory and says recording behavior does not activate it. The user-direction and overwrite rules resolve some cases, but the broad default-replacement sentence invites readers to skip that distinction.

**Proposed replacement:**

> Apply accepted workspace-specific choices within their scope. They may specialize non-binding Open Forge defaults. Record required future behavior in the applicable Directive or entrypoint. A Memory record alone does not override a loaded binding rule. Use explicit user direction or the defined base/overwrite mechanism for a binding replacement; otherwise report the conflict.

**Meaning to preserve:** clear user direction remains controlling; accepted customization is usable; Memory records evidence and context without becoming a second instruction system; ordinary child rules do not silently override their parents.

**Decision needed:** confirm whether “defaults” includes binding shipped rules based solely on an accepted Memory record. The draft assumes it does not. If the broader meaning is intended, specify that exception and reconcile Memory's Core precedence statement.

### T28-S04 — Supply one complete authoring and repair example

**Priority/type:** medium; documentation addition. Any new schema or recovery policy needs separate acceptance.

**Locators:** `.agents/loader.md:7–10,35–56,83–95`; `.agents/directives/_directives.md:13–18`; all generated `Entries` blocks.

**Scenario:** a maintainer adds a project-specific Directive, renames a routed folder, or repairs two competing entrypoint filenames without the CLI.

**Problem and consequence:** the rules describe the concepts, and source files demonstrate the syntax, but no complete example connects a narrow scope, its entrypoint, a required sibling, metadata, and its parent entry. The text requires exactly one recognized entrypoint but does not tell the reader how to recover from two candidates. It calls Entries generated while also saying the plain files remain complete without the CLI. Authors must reverse-engineer conventions or guess how manual maintenance and generation interact.

**Proposed small structure:** add one on-demand authoring guide linked from the loader. Keep it outside normal startup loading. Include:

1. A two-level route with a child entrypoint and one Directive.
2. The child's frontmatter and the parent's matching entry, distinguishing descriptive metadata from loading tags and authority.
3. The `Instructions` section and the fact that selecting the child activates its required siblings only in that scope.
4. Rename repair: update the conventional entrypoint name, inbound links and generated navigation; inspect the final result.
5. Duplicate-entrypoint recovery: identify the intended entrypoint, preserve useful text, repair links, and leave one recognized file. Avoid silently inventing filename precedence.
6. With CLI: refer to the documented index/diagnosis commands. Without CLI: explain the supported manual index-editing procedure, if that is intended.

**Representative draft, schematic rather than an installed example:**

```text
.agents/directives/
  _directives.md
  payments/
    _payments.md
    review.md
```

> The root lists `payments/_payments.md` on demand. Its description says when the payments scope applies. The child entrypoint lists `review.md` with `LoadNow`; that Directive has one non-empty `## Instructions` section. Reading a similarly named folder elsewhere does not create a new Directives root.

**Meaning to preserve:** one entrypoint per routed folder; on-demand scope selection; additive binding rules; users may customize routes and removed defaults stay removed. The guide must demonstrate existing rules rather than add a new route manager or mandatory authoring workflow.

**Decision needed:** confirm the supported manual navigation-maintenance convention and malformed-entrypoint recovery behavior before documenting them as guarantees. The rest can be an explanatory example.

### T28-S05 — Define archive tag cleanup, including sealed handoffs

**Priority/type:** medium; lifecycle clarification plus an editorial correction.

**Locators:** `.agents/memory/archived/_archived.md:9,14–17`; `.agents/memory/emerging/_emerging.md:23–24`; `.agents/memory/working/checkpoints/_checkpoints.md:26–27`; `.agents/memory/working/handoffs/_handoffs.md:25–26`; `.agents/loader.md:68,75–79`.

**Scenario:** a routed candidate or handoff has a continuity tag and is later archived. A Crystallized record retains `CurrentTruth` in its own metadata after moving. Checkpoints have an explicit tag-removal rule, but other record kinds do not. A handoff is also sealed against editing.

**Problem and consequence:** archive placement does not expressly neutralize retained leaf tags. The loader globally reaches any routed non-entrypoint `KeepInMind` file, so an archived record retaining that tag still loads. `CurrentTruth` and `Evergreen` can also continue to describe an obsolete file misleadingly. A sealed handoff cannot simply have its metadata rewritten without clarifying the immutability boundary. Separately, the archive opening says history follows loss of `CurrentTruth`, although never-accepted Emerging material and contextual handoffs are also explicitly archived.

**Editorial before:** “Archived Memory keeps useful history after it stops being #CurrentTruth.”

**Editorial after:** “Archived Memory keeps useful records that no longer support current work, including replaced knowledge, rejected candidates, and completed transfers.”

**Proposed lifecycle addition:**

> Before archiving a record, remove current-state, active-continuity, and ongoing-maintenance tags that no longer describe it, and refresh its navigation entry. Retain its former state as historical content when useful. Archived records load on demand unless a continuing continuity need is explicitly recorded.

For Handoffs, choose and document whether sealing protects the snapshot content while allowing metadata/path maintenance. A possible narrow addition is: “Do not change the recorded boundary state. Archival metadata and link maintenance may change without rewriting that snapshot.”

**Meaning to preserve:** the transferred boundary state stays immutable; useful history and provenance survive; archive restoration requires validation and an explicit destination; active Checkpoints already have valid cleanup rules and need not be duplicated.

**Decision needed:** whether any archived leaf should retain global continuity loading, and whether sealing permits metadata/link maintenance. If byte immutability is intended, use an external archive entry for lifecycle state and specify how old activation tags are treated. Do not add that machinery without a demonstrated need.

### T28-S06 — Say how an entrypoint overwrite contributes rules

**Priority/type:** medium; interpretation clarification.

**Locators:** `.agents/loader.md:7,11,26,43,55–56`.

**Scenario:** a user puts replacement Axioms in `_payments.overwrite.md` next to `_payments.md`.

**Problem and consequence:** only the loader and recognized loaded entrypoints can define active Axioms. An overwrite is not one of the recognized entrypoint filenames, is not selected independently, and is expressly loaded after its base. “Shares ... route, scope, and loading behavior” does not expressly say whether its Axioms are part of the base's effective rules. The intended substitution is plausible, but a literal reader can reject the companion's Axioms or incorrectly treat it as a second entrypoint.

**Proposed addition:**

> Treat the companion as part of the base source for interpreting its content. A companion to the loader or an entrypoint may amend that base's Axioms; it is not a second entrypoint. A companion to another file does not gain permission to define Axioms. Its replacement applies only to the question and scope defined by the base.

**Meaning to preserve:** immediate companion loading, one recognized entrypoint, user-owned overrides, local replacement rather than unrelated overrides, and no authority created by a tag.

**Decision needed:** confirm that entrypoint/loader companions are intended to amend Axioms. If only leaf content may be overwritten, document that limit instead. No runtime support was inspected.

### T28-S07 — Make the installed-payload boundary visible

**Priority/type:** medium for first-time use; source coverage/documentation gap, not a demonstrated missing repository-wide installation guide.

**Locators:** `AGENTS.md:5–9`; `CLAUDE.md:3–4`; `.agents/loader.md:3,74,81–95`.

**Scenario:** a person receives this subtree and wants to install it into an existing workspace or add an optional extension.

**Problem and consequence:** the entry files begin after installation, and the CLI paragraph refers to a “replacement” CLI without explaining what is replaced. `Extension` and managed routes are defined, but no supplied source identifies acquisition, compatibility, overwrite-safe installation, or packaging instructions. The reviewer cannot establish those operations from source alone. They may be documented outside the permitted subtree.

**Proposed small change:** add a short human-facing orientation with a link to the actual authoritative installation/customization documentation once identified. Explain that these files are installed workspace context, what a first task reads, and where extension lifecycle instructions are owned. Do not invent installation commands or duplicate lifecycle specifications here.

**Representative draft based only on established source meaning:**

> Open Forge organizes the instructions and context an agent uses in a workspace. Agents enter through `AGENTS.md` and `.agents/loader.md`, then select relevant routes. Memory preserves useful work and accepted knowledge. Optional guidance and capabilities are selected when they help. Installation and extension-management instructions belong in the linked setup guide.

The final sentence requires a verified real link before shipping.

**Meaning to preserve:** plain Markdown remains usable without the CLI; runtime ownership of Skills; no promise that arbitrary packages or agents are supported.

**Decision needed:** identify the authoritative public setup source and supported relationship to this installed payload. Do not turn this scoped evidence gap into a claim that the whole repository lacks setup documentation.

### T28-S08 — Reduce repeated prose without weakening rules

**Priority/type:** low; editorial consolidation.

**Locators:** `.agents/guidance/adaptive-collaboration.md:15–20,24–44,48–54`; `.agents/loader.md:21–24,50,83`; `.agents/memory/_memory.md:9`.

**Scenario:** an agent loads Adaptive Collaboration and repeatedly encounters the same opening sequence, one-choice limit, permission boundary, and progressive-depth advice already present in the loader and adjacent paragraphs.

**Problem and consequence:** repeated formulations increase reading cost and the number of passages that must stay aligned. The Guidance's added value is situational depth, convergence, and the independent-review tradeoff; those should remain visible. This is maintainability and clarity, not evidence that current wording fails in practice.

**Before, representative:** “Growth is deliberate rather than automatic.” and “Memory is self-growing Markdown state ... without a fixed structural ceiling ...” in the same opening.

**Proposed after:** “Memory preserves useful state for ongoing work, accepted knowledge, candidates, and history. Add records and scopes when their future value justifies maintaining them. Load unrelated branches only under the loader's continuity rules.”

For Guidance, keep one brief opening rather than Preferred Approach plus a second numbered disclosure sequence:

> Start with the user's request and accepted context. Give the understood outcome, strongest recommendation, and at most one important open choice unless deeper analysis is requested or needed. Make assumptions and unresolved choices visible. When the outcome is clear, proceed. When it is still forming, compare a few distinct directions and their tradeoffs. Preserve the user's detailed constraints.

Retain the existing convergence instructions, when-to-ask boundary, important-risk exception, complete independent-review conditions, and tradeoffs. Link to the loader for required behavior instead of rewriting those requirements as weaker advice. This excerpt is a representative draft, not a complete approved replacement for the 70-line file.

Also replace “when the replacement Open Forge CLI is available” with “when the Open Forge CLI is available,” unless the transition distinction is an intentional public requirement. Consolidate the two statements that management does not create runtime authority (`loader.md:38,50`) without deleting the ownership restriction at line 51.

**Meaning to preserve:** mandatory loader rules stay mandatory; Guidance remains adaptable; deep work remains available when requested or needed; independent review still depends on value, expense authorization, and context isolation.

**Decision needed:** none for ordinary sentence cleanup. Confirm whether “replacement” carries a real supported-version distinction before removing it.

## Journey and consistency checks

| Journey                                       | Assessment                                                                                                                                                                                                      |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| First task in an installed workspace          | The AGENTS-to-loader path resolves and root descriptions expose the main choices. An onboarding explanation is missing from this bounded source pack (S07).                                                     |
| Ordinary task with no useful stored memory    | Empty catalogs are valid. Memory explicitly allows no useful candidate material and discourages saving every conversation. No mandatory record factory is justified.                                            |
| Resume one active workstream                  | Checkpoint state, refresh timing, and links to durable sources are clear. The global `KeepInMind` ancestor case needs S01.                                                                                      |
| Accept a temporary choice                     | Working Memory explicitly permits it with source, scope, and expiration. This is a deliberate exception to durable promotion, not a contradiction to remove.                                                    |
| Turn an observation into reusable knowledge   | Evidence, recurrence, uncertainty, and consolidation are covered. Repetition alone does not accept it. Validation/acceptance terminology needs S02.                                                             |
| Record a durable behavior change              | Core owns behavior; Memory preserves context and rationale. S03 clarifies how prior accepted customization relates to binding rules.                                                                            |
| Customize a narrow scope                      | Root identity, additive Directives, and overwrite companions provide a useful model. S04 and S06 make it easier to apply consistently.                                                                          |
| Select a Workflow, Skill, or Template         | Their boundaries are differentiated. Workflows explicitly honor opt-out, Skills defer to the runtime, and copied Templates are independent. No actual leaf recipe or package exists here to execute or inspect. |
| Recover from a rename or duplicate entrypoint | The valid end state is defined, but the procedure is not (S04). No filename-precedence algorithm should be inferred.                                                                                            |
| Archive and later revisit a record            | Provenance and validation are covered; stale activation tags and sealed-record maintenance need S05.                                                                                                            |

No broken source link, malformed shipped Workflow recipe, or conflicting sibling Directive was found. There are no shipped leaf recipes or Directives to test for the latter two categories. `Core`, `CurrentTruth`, and `Evergreen` serve different purposes; they should not be collapsed into one tag. Authority and loading are intentionally distinct, except for the specific interpretation gaps identified above.

## Verification and coverage limits

- All **20 relative Markdown links** in the source resolve inside the source tree. Both Claude import targets exist. This verifies targets, not runtime import behavior.
- All 20 linked entry descriptions and tag lists match the destination frontmatter, including order. Every one of the 19 routed directories below `.agents` has exactly one recognized entrypoint. The loader serves the root and is not miscounted as a conventional folder entrypoint.
- The source was checked against its initial SHA-256 inventory after inspection and matched. Concurrent CLI and local Memory work was not used to infer source behavior.
- The source contains syntax conventions and command signatures, but no worked CLI invocation with a concrete source reference, install example, complete leaf Workflow, or extension package. No behavior, command compatibility, or installation guarantees were tested. Running maintenance commands would be inappropriate for this read-only review.
- No CLI implementation, tests, repository documentation outside the permitted subtree, external web sources, or global installations were inspected in Stage 1. This report makes no claims about their correctness or completeness.
- Findings based on plausible customizations are labeled as scenarios. They identify under-specified contracts rather than pretending to reproduce execution defects.
- Later implementation should first resolve the decisions in S01–S03, S05, and S06, then author one coherent wording pack with S04/S07 examples, and finally make S08 editorial reductions. Existing behavior and requirement strength should be checked against accepted decisions before applying any draft.

This frozen report is the complete Stage 1 deliverable. The local comparison may identify optional capabilities, but must not retroactively supply intent or evidence for these conclusions.
