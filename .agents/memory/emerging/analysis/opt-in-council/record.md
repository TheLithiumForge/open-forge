---
open-forge:
  description: Second council analysis on audience, opt-in adoption and a two-file kernel, building on the first loader council
  responsibility: Preserve the October 2026 opt-in council's audience verdict, ranking, chair decisions, module map, CLI changes, ratification and open decisions without accepting a contract change
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loader, Audience, Positioning, Customization, Simplification]
---

# Opt-In Council

Status: unaccepted proposal from 2026-10-19 on branch `council/opt-in-framework`, based on `main` at `e9d4fc9a7`. It builds on the first [loader council](../loader-council/_loader-council.md), whose branch `council/loader-simplification` is kept as a separate idea. The payload and dogfood files on this branch are drafted. Several behaviors need the CLI changes listed below before they ship. Nothing here is accepted.

## Question

After the first council, the maintainer gathered more feedback and set six directions:

1. The framework must work completely without the CLI. The CLI should make it far better, but the model chooses whether to use it and sometimes will not.
2. Users are not convinced the defaults are what they need. Install already offers Essentials, Full Core and Custom.
3. The loader is hard to understand at first glance. Models have improved, so many rules and explanations can be compressed, moved, removed or improved.
4. Too much relies on the model deciding. No harness will be shipped: only Markdown and an assistant CLI.
5. It may feel like a hobby project without a clear audience, shaped around the maintainer's own pain points.
6. Most important: it is too complicated and takes over context construction, placement and method. Keep strong opinions, but let users take as much or as little as they want and still be fine.

## Method

Eight independent read-only agents received the same brief, the first council's record and Step 1 files, and a different perspective: Audience Strategist, Minimalist Adopter, Essence Guardian, No-CLI Realist, Behavior Engineer, Modular Architect, Editor and Council 1 Critic. They proposed, ranked each other anonymously and voted on the chair's synthesis. [Proposals](proposals.md) and [reviews](reviews.md) keep the sanitized evidence.

## Ranking

Average position across the seven members who ranked each proposal. Lower is better.

| Rank | Proposal | Average | First places | Central idea |
| ---- | -------- | ------- | ------------ | ------------ |
| 1 | Minimalist Adopter | 1.00 | 7 | The unit of adoption is one useful link to an existing source |
| 2 | Council 1 Critic | 2.29 | 1 | The loader states what this workspace chooses to use, and updates preserve that choice |
| 3 | Audience Strategist | 3.71 | 0 | Position for maintainers of existing repositories, first success is an existing document used in a fresh session |
| 3 | Modular Architect | 3.71 | 0 | A kernel plus modules that each declare what they need and what happens without them |
| 5 | Behavior Engineer | 3.86 | 0 | Separate having files from activating their behavior, with optional behavior packs |
| 6 | No-CLI Realist | 4.86 | 0 | One flat required list, and CLI output that names its reading contract and reasons |
| 7 | Editor | 5.57 | 0 | A 326-word loader and every entrypoint compressed to its behavior |
| 8 | Essence Guardian | 7.00 | 0 | Keep a seven-file recommended default and a consistent trust contract |

The Minimalist Adopter was first on all seven ballots that could rank it. Its own ballot ranked the Council 1 Critic first. The Essence Guardian was last on all seven ballots that could rank it, mainly because it kept automatic progress capture in the kernel. Its trust-contract argument still shaped the kernel's acceptance rule.

## Audience verdict

Every reviewer gave nearly the same verdict. The primary audience is **maintainers and small teams using coding agents in existing repositories**, whose agents keep missing rules, documentation and decisions the project already has. Close neighbors are solo developers who resume work across sessions and teams working across several packages or languages. Coordinated agent fleets are a specialized tier.

The "hobby without an audience" critique holds for positioning and breadth, not for purpose. The vision reaches for "any person, project, team, discipline", and the onboarding teaches a taxonomy before solving a problem. The repository does show recurring jobs that many developers share. It does not yet show validated demand.

Features that mainly serve the maintainer's own way of working: hierarchical orchestration with permanent task IDs and exact status formats, councils and review-evidence identities, the sealed Analysis-to-Task lifecycle, protected findings notebooks, and the depth of maintenance contracts. They are good evidence for optional packs, and weak candidates for defaults.

A first screen for that audience could read:

> Open Forge helps coding agents use the rules, docs and decisions your project already has. Point it at an existing file and agents read it before the work it applies to, in every session, with any agent that reads `AGENTS.md`. Add folders for rules, resumable notes or accepted decisions only when you want them. Plain Markdown, and an optional CLI that makes it faster.

## What the council converged on

These combine broad agreement with chair decisions. They were not unanimous.

- **A two-file kernel.** `AGENTS.md` and `.agents/loader.md` are the whole requirement. Every category, Memory state and policy is optional.
- **The loader's Entries are the switch.** Listed means on, unlisted means off. Removing a line turns that part's Open Forge behavior off and keeps the files as notes, without cancelling accepted project requirements or instructions the host loads on its own. Reading those files through search does not bring their rules back. Index, Install and Update must never bring a removed line back. A missing listed file is an error. An empty list is valid.
- **Existing documents join by one line.** Entries may link straight to a file anywhere in the workspace. The linked file needs no move, no catalogue and no frontmatter, because its entry line carries the description, tags and conditions. Linking gives reading, not acceptance or write access.
- **Automatic progress capture is a separate policy.** All seven reviewers who ranked the Essence Guardian's proposal named capture inside the kernel as the most dangerous idea, because rejecting Memory would still leave its working habit in force. It now lives in `memory/working/saving.md`, which can be removed together with its entry in `_working.md` while records stay.
- **The response recipe leaves the kernel.** Communication style belongs to the user, the host or an optional pack.
- **Each optional piece says what it needs and what happens without it,** in one line where it is used, such as "Optional. Nothing else depends on this folder."
- **The CLI shortcut sits where the model reads first.** It appears in step 2 of the loader and in the `AGENTS.md` block, and needs no Skill read before the first call. It covers the required reads only. The agent still makes the relevance choices and follows reads that files require in prose, and falls back to its own file tools when the CLI is missing, fails or cannot follow a link.
- **The acceptance boundary stays in the kernel.** Reading, linking, saving or tagging never makes content accepted.
- **Disable, stop managing and delete are three different operations.** Today's Custom "leave out" releases management but keeps content routable, which users read as disabling.
- **Kept from the first council:** procedure before vocabulary, same-file condition matching, pending conditions while paths are unknown, on-demand Guidance, Maps, Patterns, Skills and Templates, no startup read of the CLI Skill, unbroken required chains, and current overwrite semantics.

## Chair decisions

1. **Recommended first run: connect one existing source.** Install offers to link one to three existing documents it finds, such as `CONTRIBUTING.md`, `docs/architecture.md` or an ADR folder. The two-file kernel with an empty list stays a valid choice. Essentials, Full Core and Custom remain for people who want the folders.
2. **Needs-based options instead of category names first.** "Rules for this project" installs Directives. "Resume work across sessions" installs Memory, Working and the saving policy. "Keep accepted decisions" installs Crystallized. "Ideas and history", "Reusable shapes", "Advice", "Starting files", "Curated pointers" and "CLI guide" install the rest. Category names appear underneath.
3. **Keep tagged chains, lead with direct links.** The flat required list stays a benchmarked alternative, as the first council also concluded. Only one member preferred it.
4. **Update preserves edited files, as a separate lifecycle change.** Protecting the loader's membership is part of the routing work. Preserving every edited Framework and Extension file with an upstream diff is reviewed and shipped on its own.
5. **Human explanation moves to `.agents/README.md`.** It shows how to link a document, add a rule, turn something off, resume work and change a shipped file. Agents do not read it at startup.
6. **The Templates trigger regression is fixed.** The first council's draft turned "when copy-ready starting content would help" into a check before every artifact. The original condition is restored.

## The drafted files

| File | Role | Words |
| ---- | ---- | ----- |
| `AGENTS.md` block | Handoff plus the CLI shortcut | 56 |
| `.agents/loader.md` | Reading procedure, trust boundary, activation list | 823 |
| `.agents/README.md` | Guide for people, never a startup read | 322 |
| `directives/_directives.md` | Optional rules | 101 |
| `memory/_memory.md` | Optional container | 163 |
| `memory/working/_working.md` | Optional resume records | 107 |
| `memory/working/saving.md` | Optional capture policy | 104 |
| `memory/crystallized/_crystallized.md` | Optional accepted knowledge | 79 |
| Guidance, Maps, Patterns, Skills, Templates, Emerging, Archived | Optional, on demand | 72 to 107 each |

Startup reading:

| Setup | Words |
| ----- | ----- |
| Current payload | 4,563 |
| First council Step 1 | 1,425 |
| Kernel with an empty list | 792 |
| Kernel with rules and resumable work | 1,433 |

In the demo workspace, `open-forge status` reports a startup of 7 routed files and about 2.2k tokens, against 11 files and about 6.6k tokens in the current payload's snapshots. Linked documents outside `.agents/` are not part of that figure.

This repository keeps Maps as #LoadNow because it lists a required build map, and Emerging as #KeepInMind so the Authors' Findings notebook is still read at startup. The payload-only `README.md` and `saving.md` are not added to this repository's own `.agents/`, which already has its own closeout rules.

## Module map

| Module | Provides | Needs | Without it |
| ------ | -------- | ----- | ---------- |
| Kernel | Reading procedure, conditions, acceptance boundary, activation list | Nothing | Not Open Forge |
| Direct links | Existing documents read before matching work | Kernel | Nothing is linked |
| Directives | A home for project rules with `## Instructions` | Kernel | Project rules live wherever the project keeps them |
| Memory | A container for the four record folders | Kernel | No record folders |
| Working | Resume records read at start | Memory | No prescribed resume location |
| Saving policy | Update the record before pausing | Working | Agents save only when asked |
| Crystallized | Accepted knowledge kept current | Memory | Existing current documents stay authoritative |
| Emerging, Archived | Candidates and history | Memory | Ideas and history stay in existing notes |
| Guidance, Patterns, Templates, Maps, Skills | Advice, shapes, starters, pointers, capability discovery | Kernel | Ordinary reasoning and native discovery |
| CLI | One-call reading, reasons, maintenance | The same files | Agents follow the files themselves |
| Extensions | Methods and record shapes | Their declared modules | No extra methods |

## CLI changes, in order

1. **Read-only blending and activation. Prototyped on this branch.** `context` follows Entries links that leave `.agents/` but stay in the workspace, including linked catalogues such as `docs/_docs.md`. It takes tags and `applies to` from the entry line, keeps one parent per linked file, reports a missing required link, and asks linked documents for no frontmatter. `index`, `status`, `doctor` and the route readers keep the loader's listed roots, so a removed line stays removed, and `index` keeps authored `..` links in place. Install, Configure and Extension install still add the roots they install. Still open: printing a reading-contract version in `context` output, and per-source selection reasons beyond the existing "included because" line.
2. **Disable, stop managing and delete.** Separate operations in Custom, `remove` and Configure, each previewed.
3. **Install presets.** "Connect existing docs" as the recommended first run, the two-file kernel, needs-based options, and the existing presets for compatibility.
4. **README exemption. Prototyped on this branch.** `.agents/README.md` stays in the catalogue so Install, Update and Configure manage it, but `doctor` no longer reports it as unreachable or missing metadata, and nothing lists it.
5. **Update keeps edits.** Preserve edited managed files and offer the upstream change as a diff. Keep overwrite companions.
6. **Benchmark the flat list** as an alternative, under the first council's gate.

## Experiment

Recruit three to six developers outside this repository: one established codebase with documentation or ADRs, one monorepo or mixed-language team, one solo project. Compare plain `AGENTS.md`, the current payload, the first council's Step 1 and this design in fresh sessions, without the CLI first. Keep the enabled obligations matched across versions so that removing duties cannot look like better compliance.

Each participant links one existing document, adds one scoped rule, pauses and resumes a task, and turns off one opinion. Measure time to first useful work, missed required reads, rules applied outside their scope, unaccepted notes treated as accepted, whether a disabled opinion stays off after maintenance, and whether the agent reaches for the CLI on its own once it is installed.

Proceed only if most participants finish without help, no required read is missed, and disabled behavior stays off. If plain `AGENTS.md` does as well with less upkeep, narrow the audience further.

## Ratification

The council reviewed the drafted files and this record and voted on each part. No member voted to reject anything.

| Part | Accept | Accept with changes | Reject |
| ---- | ------ | ------------------- | ------ |
| Drafted files | 0 | 8 | 0 |
| Audience verdict and positioning | 8 | 0 | 0 |
| CLI change plan | 8 | 0 | 0 |
| Record faithfulness | 0 | 8 | 0 |

The must-fix items, all applied before this record was finalized:

- **The CLI path skipped step 5 (all eight members).** The first draft told agents with the CLI to skip steps 3 to 5. `open-forge context` computes the tagged required set, but does not make relevance choices or follow reads that files require in prose, and an installed CLI can still be blocked. Step 2 now covers steps 3 and 4 only, keeps step 5 with the agent, and falls back to file tools. The `AGENTS.md` block and README say the CLI gathers the required reads, not all reading.
- **The saving opt-out broke the setup (all eight members).** Deleting `saving.md` alone left a #LoadNow entry pointing at a missing file, which the loader rightly treats as an error. The instructions now remove the entry with the file and record the removal for CLI-managed updates.
- **Turning a part off had no limits (two members, plus two dissents).** The loader and README now say that turning a part off does not cancel accepted project requirements or host instructions, that reading its files through search does not bring its rules back, and that dependents are reported first.
- **Counting errors in this record (all eight members).** The Council 1 Critic had one first-place vote, the "every ballot" statements now count the seven eligible ballots, and seven reviewers, not five, named capture in the kernel the most dangerous idea.

Dissent, recorded verbatim:

- Minimalist Adopter: "Disabling an Open Forge route should deactivate its policies without revoking independent host instructions or accepted project requirements."
- Council 1 Critic: "Disabling an Open Forge route must not by itself revoke accepted project requirements or independently supplied host instructions."

Both are now satisfied by the loader's turn-off rule.

## Fresh-agent probes

Three agents with no Open Forge context worked read-only in a copy of the demo workspace outside any Git repository, with the CLI unavailable to them. Each ran in its own context.

- **Task start**, adding a field to `src/Orders/Order.cs`. The agent read the expected set in order: `AGENTS.md`, the loader, Directives, Memory, Crystallized, Working, `saving.md`, `CONTRIBUTING.md`, `docs/contributing/csharp.md` (matched by `**/*.cs`), `docs/_docs.md` and the storage ADR (matched by `src/**`). It skipped Guidance, Maps, Patterns, Templates and `README.md` because they were not listed. It found all four project rules. It tried the CLI, was blocked by the shell's execution policy, and fell back to the files as step 2 says.
- **Customization**, stopping automatic saving and always reading `docs/architecture.md`. The agent deleted `saving.md`, removed its entry from `_working.md`, and tagged the architecture entry #LoadNow inside the already-required `docs/_docs.md`, citing the loader lines that justify each step.
- **Explanation** to a new teammate. Correct in five sentences. It reported three confusions: no CLI operation yet turns a part off while keeping its files, the saving policy said "delete" while the loader said "remove a line", and the boundary between an overwrite and a conflict could be clearer.

## Corrections after ratification

- The decision-authority sentence returned to the kernel: take routine reversible steps without asking again, and ask before choices that significantly change result, scope, risk or cost. Council 2 had removed it with the response recipe, but it is an authority rule, not a style rule.
- The kernel names a folder's catalogue `_{folder-name}.md`, so a plain-file agent can create one without the CLI.
- The loader says that lines inside other catalogues follow the files in their folder, so turning one off means deleting or moving its file. This answers the second probe confusion.
- With these, the loader is 823 words including its Entries.

[Loader change map](loader-change-map.md) traces every rule of the old loader to its new place.

## Sources to update on acceptance

Everything the [first council's list](../loader-council/record.md#sources-to-update-on-acceptance) names, plus:

- The [product direction decision](../../../crystallized/decisions/product/product-direction.md), which rejects "an empty routing library". Five members argued the two-file kernel should be a supported endpoint while the recommended first run still connects something useful.
- The [principles](../../../crystallized/documents/principles.md) line that defaults should give more value than an empty substrate, and the [vision](../../../crystallized/documents/vision.md) audience and breadth.
- The README, installation guide and ten-minute guide, rewritten around connecting one existing document.
- The Collaboration Extension, which can take the communication recipe the loader no longer carries.

## Open decisions for the maintainer

1. Adopt the primary audience and first-screen promise above, or name another.
2. Accept the two-file kernel as a supported endpoint, which reverses part of the product direction decision.
3. Make "connect existing docs" the recommended first run.
4. Fund CLI change 1 next, since several drafted lines depend on it.
5. Choose between this branch and the first council's branch, or merge their Step 1 parts.
