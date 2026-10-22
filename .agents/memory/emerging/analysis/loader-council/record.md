---
open-forge:
  description: Council analysis and staged plan for simplifying the loader and shipped Markdown, making required reading mechanical and letting docs outside .agents take part
  responsibility: Preserve the October 2026 loader council's evidence, ranking, chair decisions, staged plan, dissent and open decisions without accepting a contract change
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Loader, Loading, Routing, Simplification, Blending]
---

# Loader Simplification Council

Status: unaccepted proposal from 2026-10-19 on branch `council/loader-simplification`, based on `main` at `e9d4fc9a7`. Step 1 is drafted in the branch's payload and dogfood files. Steps 2 and 3 are specifications. Nothing here is accepted until the maintainer says so.

## Question

The maintainer asked to simplify the Markdown, especially the loader. The feedback behind the request: Open Forge leans too much on the model making decisions, and the loader has too many rules and concepts for a person to follow. The maintainer also asked for smaller footprint, better rule following, more customization, pieces that work on their own, and a blended layout where ordinary folders such as `docs/` take part without moving into `.agents/`.

## Method

Ten independent read-only agents each received the same brief and a different persona, then followed three stages modeled on a multi-model council:

1. **Proposals.** Each member wrote a diagnosis with quoted evidence, a concept budget, a complete replacement loader, a model-versus-mechanism table, a blended layout and an experiment.
2. **Anonymous peer review.** Proposals were shuffled under letters with persona names removed. Each member ranked the other nine against seven shared goals, listed ideas to steal, named the most dangerous idea, and revised its own position.
3. **Chair synthesis and ratification.** The overseer chose the design below, wrote Step 1 into the branch, and returned it to the council for a vote.

The personas were Prosecutor (adversarial), Demolitionist (contrarian), Newcomer (friendly first-time reader), Unix Greybeard, Steward (advisory, migration cost), Model Whisperer (instruction-following), Scout (ecosystem conventions), Wildcard Inventor, Tinkerer (customization) and Determinist (mechanism over judgment). [Proposals](proposals.md) and [reviews](reviews.md) keep the sanitized evidence.

## Ranking

Average position across the nine members who ranked each proposal. Lower is better.

| Rank | Proposal | Average | First places | Central idea |
| ---- | -------- | ------- | ------------ | ------------ |
| 1 | Prosecutor | 2.56 | 2 | A flat, path-matched required checklist whose completeness can be inspected |
| 2 | Newcomer | 2.89 | 3 | A reading recipe with required branches made reachable through tagged chains |
| 2 | Demolitionist | 2.89 | 2 | A 264-word loader where Directives directly lists every required source |
| 4 | Unix Greybeard | 3.22 | 2 | A `load.d/` drop-in folder of read lists that link anywhere in the workspace |
| 5 | Wildcard Inventor | 4.33 | 0 | A `When / Read / Use` reading table, with a compiled task packet as runner-up |
| 6 | Determinist | 6.00 | 0 | Required reads computed from frontmatter across registered folders |
| 7 | Scout | 6.44 | 0 | Converge on AGENTS.md, Skills and Cursor or Copilot style conventions |
| 8 | Tinkerer | 7.00 | 0 | Checked replacements that quote the exact text they change |
| 9 | Steward | 7.11 | 0 | Ship a shorter guide on today's syntax, add docs roots next |
| 10 | Model Whisperer | 7.56 | 1 | A start procedure with a concrete trigger for every rule |

The leading group included both explicit lists and tagged chains. Model Whisperer and Steward ranked last, and Tinkerer, eighth, also kept parent-gated discovery, so the ranking alone does not isolate that design choice.

## What the council converged on

These points combine broad agreement after review with chair decisions. They were not unanimous across both stages. Required-rule discovery, unknown-path handling and which tag obligations to keep stayed disputed.

- The loader should teach a reading procedure before any vocabulary.
- Required reading must not depend on the model choosing the right parent. A recurring diagnosis quoted: "These tags act only through loaded parents. Neither activates an unselected ancestor or scope." A required rule stays invisible when the agent skips the folder above it.
- The CLI guide should not be a startup read. Today it is required by prose in the Skills entrypoint, outside the closure `open-forge context` computes, so the CLI and a plain-file reader disagree about startup.
- Package management, slugs, root routes, managers, the inherited-axioms marker and frontmatter authoring detail belong in the CLI Skill and authoring references, not in every task's context.
- Only #LoadNow and #KeepInMind change behavior in the CLI. The other six defined tags are labels, so their loader definitions can go without code changes. Retiring those definitions is a chair decision. Several proposals kept status or maintenance meanings in prose.
- Ordinary documentation should take part at its existing path. Linking a file grants reading, never write access or installer ownership. Maps stay useful as curated pointers, but stop being the only door.
- Saving, tagging, moving or repeating a note never makes it accepted.
- Unknown working files leave conditioned reads pending. This is a chair decision. Unix Greybeard and Determinist first proposed reading conditional rules broadly when paths are unknown. Unix Greybeard later adopted pending conditions, and Determinist asked to compare both in the experiment.
- Judge success by observed reads and resulting behavior, not by word counts or self-reported compliance.

## Rejected ideas

- **Reading every required file regardless of `applyTo` when the CLI is absent.** Five members named it the most dangerous idea. It makes plain-file use expensive and asks the model to ignore rules it has just read.
- **"A missing or empty directory requires nothing."** A missing configuration must be reported, so an accidental deletion cannot look like a deliberate empty policy.
- **Whole-file replacement as the default meaning of `{name}.overwrite.md`.** A one-sentence change would silently fork the whole source and hide upstream fixes.
- **Following the host tool's instruction precedence for project sources.** Project rules need one meaning whichever agent reads them.

## Chair decisions

The council's revised positions converged on a staged plan. Six members prefer explicit required lists as the destination. Four prefer to make tagged chains reliable first and compare. Both groups asked to ship the Markdown reduction first and to keep each later step independently testable. The chair adopts that order.

### Step 1: Simplify the Markdown on today's semantics

Drafted on this branch. It needs no CLI code change.

- The loader becomes a five-step reading procedure, a short list of rules for using what was read, and a short authoring section. It drops from 2,052 to 798 words.
- Automatic required reading follows the CLI's loading-tag closure: #LoadNow and #KeepInMind links followed from the loader, filtered by `applyTo`, where one working file must satisfy every condition in the chain. Loaded files can still require further reads in prose, which the CLI does not infer, and the loader says to read them. A new authoring rule closes the hidden-parent gap: a file that must always be read needs #LoadNow on every catalogue between it and the loader. `applyTo` on a catalogue limits its whole branch.
- #KeepInMind is #LoadNow plus a reread before hand off or finish. Emerging Memory no longer carries it by default. Working records use it when they need it.
- Guidance, Maps, Patterns, Skills and Templates load on demand. Their loader descriptions now name the moment to open them, such as "Default shapes to check before creating or reviewing structure". Directives, Memory, Working and Crystallized stay required.
- Catalogue rules still apply to what they list. The inherited marker is no longer taught.
- The universal response recipe becomes one line: lead with the answer and keep writing short.
- Each entrypoint keeps a purpose sentence, three or four rules with triggers, and Entries. Question headings are gone.
- The CLI Skill is read before running commands other than `context`.
- AGENTS.md says to read the loader and follow its reading steps.

A fresh install reads 1,425 words at startup instead of 4,563. All shipped Markdown drops from 11,581 to 9,417 words.

In this repository, Maps stays #LoadNow because it lists a required build map, and Emerging stays #KeepInMind so the Authors' Findings notebook is still read at startup. Both follow the new chain rule.

### Step 2: Blended documentation and explainable reading

This requires a read-only CLI change before any loader text claims it.

The loader gains one authoring line:

> Entries may also link to a catalogue or file anywhere in the workspace, such as `../docs/_docs.md` or a design note beside code. A link grants reading, not authority or write permission. Catalogues outside `.agents/` are written by hand.

A small blended workspace then looks like this:

```text
AGENTS.md
README.md
.agents/
  loader.md            links ../docs/_docs.md
  directives/
    _directives.md
    workspace.md
  memory/
    _memory.md
    working/
      _working.md
      checkout.md
docs/
  _docs.md             written by people, read by agents too
  architecture.md
  contributing/
    csharp.md
  adr/
    0001-storage.md
src/
  checkout/
    design.md
```

```markdown
- [Architecture, contribution rules and accepted decisions](../docs/_docs.md) - #LoadNow
```

```markdown
# Project documentation

## Entries

- [System architecture](architecture.md)
- [C# conventions](contributing/csharp.md) - #LoadNow - applies to `**/*.cs`
- [Accepted decisions](adr/0001-storage.md)
```

The CLI work:

- Source identity accepts workspace-relative paths outside `.agents/`. Links may contain `..` while they stay inside the workspace. Escapes and overlapping roots are rejected.
- Entries may skip intermediate folders, so folders under `docs/` need no catalogue. Each source must still have one unambiguous catalogue ancestry for inherited rules and conditions. Competing catalogue parents are rejected. Ordinary cross-links stay references and add no ancestry.
- Index may refresh entries inside `.agents/` while preserving authored membership and targets. Catalogues and documents outside `.agents/` are read-only in this step, including their Entries and frontmatter. Writing outside `.agents/`, and its permission flags, comes later.
- Context follows these links with the same #LoadNow and `applyTo` rules, and prints each source with the reason it was selected, plus pending and missing sources.
- Status reports a #LoadNow file whose catalogue chain is not required, and a required link whose target is missing.

### Step 3: Explicit required lists, a benchmarked alternative

Most members preferred this destination, but it changes loading semantics and three members dissented (see [Ratification](#ratification)). It stays an alternative to Step 2's checked chains until the experiment below shows that it misses no required read the chains catch, keeps rule scope and continuity, and gives a practical advantage such as independent Extension packaging, less maintenance or fewer misses, without unacceptable growth in required context.

- `.agents/load.d/` holds small read lists. Each `*.md` file, read in filename order, lists required files under `## Entries`. Every entry in a read list is required. Links may point anywhere in the workspace.
- A missing `load.d/` is an incomplete installation and must be reported. An empty folder means nothing is required.
- `applies to` on a read-list entry is the only condition. Conditions do not inherit.
- Catalogues become navigation only, but only after a versioned, reviewed migration. Retained catalogue Axioms move into explicitly listed rule files that state their own scope, so a rule copied to another project still makes sense. Each source keeps its complete inherited condition. Where equivalence cannot be established, the workspace stays on chains. Loading modes are never mixed.
- An Extension installs its own read list, such as `load.d/40-planning.md`, and removing the Extension removes that file. Pieces stay independent.
- Index never edits `load.d/`.

```markdown
# Workspace rules

## Entries

- [Rules for every task](../directives/workspace.md)
- [C# conventions](../../docs/contributing/csharp.md) - applies to `**/*.cs`
- [Checkout work in progress](../memory/working/checkout.md) - #KeepInMind
```

Step 3's loader replaces steps 2 and 3 of the reading procedure with: "Read every immediate `*.md` file in `.agents/load.d/` in filename order. Read each applicable destination listed under its `## Entries` once per pass. Ordinary links elsewhere remain optional. Match entry conditions against working files, leaving unknown conditions pending before affected work." It also replaces every ancestor-dependent instruction, including search recovery, inherited catalogue rules and the chain rule.

### Later: checked replacements

The Tinkerer's checked replacement was on most steal lists: a companion that names a heading, quotes the exact old text and gives the new text. An upstream change then shows up as a visible conflict instead of a guessed merge. Ship it as an optional capability after Step 2, keep today's `{name}.overwrite.md` meaning unchanged, and offer whole-file substitution only as an explicit choice.

## Experiment

Members proposed related fixture-based evaluations. The chair combines them into this experiment, which can begin without CLI work:

1. Build small fixtures with a nested file-specific rule, a task whose working files are unknown at first, a task that adds a second language midway, a required document under `docs/`, a draft conflicting with accepted knowledge, an overwrite, and a resume from a work record. Add one large, grown fixture so a flat list's scaling cost is visible.
2. Write the expected required-read set for each task by hand.
3. Run the current payload, Step 1, and hand-built Step 2 and Step 3 layouts in fresh sessions, twice each, with the same model and without the CLI first.
4. For the tasks whose working files start unknown, also compare pending conditions with conservative broad reading, as Determinist asked.
5. Score missed required reads, rules applied outside their scope, proposals treated as accepted, unnecessary reads, startup words and closeout omissions from actual read traces.

Ship Step 1 only if it misses no rule the current payload catches. Choose between Step 2 chains and Step 3 lists from the same data.

## Ratification

The council reviewed the Step 1 diff and this record and voted on each part. No member voted to reject anything.

| Part | Accept | Accept with changes | Reject |
| ---- | ------ | ------------------- | ------ |
| Step 1 as drafted | 0 | 10 | 0 |
| Step 2 specification | 8 | 2 | 0 |
| Step 3 specification | 8 | 2 | 0 |
| Record faithfulness | 0 | 10 | 0 |

The must-fix items, all applied before this record was finalized:

- **Same-file condition matching (all ten members).** The first draft let one working file satisfy a parent's `applies to` while another satisfied the child's. The CLI requires one file to satisfy the whole chain, so the loader now says so and applies conditioned rules only to matching files.
- **Overstated unanimity (all ten members).** This record first called every point unanimous. It now separates convergence from chair decisions and names the disputed points.
- **Routed Markdown only (three members).** Catalogue and frontmatter rules now cover routed Markdown, not code folders or native Skill packages.
- **Prose-required reads (three members).** "Everything else is optional" contradicted instructions such as Working Memory's "read the record of your work". On-demand now yields to explicit requirements, and search recovery covers routed Markdown only.
- **#KeepInMind at startup (one member, also found by the chair).** The first reading step named only #LoadNow, so a literal reader could skip #KeepInMind entries until closeout.
- **Step 2 and Step 3 boundaries.** Outside `.agents/` stays read-only in Step 2, each source keeps one catalogue ancestry, Step 3 requires only entries listed under `## Entries`, and Step 3 needs a reviewed migration that never mixes loading modes.

Dissent, recorded verbatim:

- Steward: "Explicit lists should remain a benchmarked alternative to checked chains until they preserve rule scope and continuity without unacceptable growth in required context."
- Model Whisperer: "I favor retaining validated chains unless explicit lists demonstrate a practical advantage beyond matching their required-read accuracy."
- Scout: "Explicit required lists should remain a benchmarked option until the comparison establishes their advantage over validated chains."

The Step 3 gate above was written to satisfy all three.

## Sources to update on acceptance

Step 1 changes accepted contracts. These sources must change with it:

- The [loader maintenance contract](../../../crystallized/documents/maintenance/payload/agents/loader.md): section order, the compact CLI command list, the 102-line baseline and the defined-tag list.
- Maintenance contracts for each rewritten entrypoint under `crystallized/documents/maintenance/payload/`.
- The [tags decision](../../../crystallized/decisions/framework/tags.md) and [loading reliability decision](../../../crystallized/decisions/framework/loading-reliability.md), which record why the six labels and early category loading exist.
- The documentation site's [loading and tags](../../../../../src/docusaurus/docs/concepts/loading-and-tags.md) table, routing and customizing concepts, and the README and development guide startup figures.
- Payload snapshot tests and Install, Update and Context fixtures that capture shipped text or the startup set.
- Route scaffolding and workspace adoption, which still write the inherited marker, and the first-party Extension catalogues that carry it. The marker stays valid, so these can change on their next revision.
- [Task 53](../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md), which this work largely answers.

## Open decisions for the maintainer

1. Accept Step 1 as the next payload change, with the listed contract updates.
2. Make guidance, maps, patterns and skills on-demand by default, as drafted, or keep them at startup.
3. Fund Step 2 as the next CLI task, read-only first.
4. Run the experiment before choosing between Step 2 chains and Step 3 lists.
5. Decide whether this repository's own C# and CLI directive scopes should become #LoadNow branches with `applyTo`, so their loading stops depending on description matching.
