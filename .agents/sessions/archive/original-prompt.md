i'LL give you a lot of data containing my oroginal prompt as well as the ai draft and handover and so on
you need only to reference those ideas but you will have to work with me for the next hour to define each and every file and structure
we will basically target the .temp-github-private folder to extract/move some things from it 
this repo will be a source/template/something for anyone to use as agentic agnostic workflows basis, to help them in their development, brainstorming, organization, and so on withough being a bother 




here is a lot of data do not treate it as absolute truth, it's just a bunch of ideas and needs 


--------- AI HANDOVER ----------
Confidence: 94%.

# Comprehensive summary: reusable agent workflow system

## 1. Core ambition

You want to create a **minimal, reusable, organic agent workflow system** that can start as a private template repo, but could later become a standalone SDD-style product or methodology.

The system should help with the full flow from:

```text
idea
→ clarification
→ vision / PRD / intent
→ architecture
→ work splitting
→ implementation
→ review
→ testing
→ refactor
→ handoff
→ learning / organic improvement
```

But it should not feel like a heavy methodology. It should feel like a small, flexible set of files that any agent can understand.

The key idea is:

```text
Every agent starts from one loader.
The loader teaches the agent how to load the workspace.
The workspace decides its own structure.
The system grows through local rules, examples, overrides, and observations.
```

---

# 2. Possible names

You are not fully decided on naming.

Current possible directions:

```text
Inflow
Forge Method
The Forge Method
Agent Workflow Template
Agentic Workspace
Agent Loader
Element-inspired naming
```

The name should feel:

```text
fluid
organic
lightweight
elemental
not too corporate
not too BMAD-like
not too framework-heavy
```

`Inflow` is attractive because it suggests:

```text
ideas flowing into structure
work flowing through review
agents flowing through context
organic movement
not rigid process
```

Possible naming model:

```text
Product / methodology name: Inflow
Repo template name: inflow-template
Core folder: .agents
Core entrypoint: loader.md
```

Alternative branded model:

```text
Methodology name: Forge Method
Template repo: forge-method-template
Core idea: forge work from idea to reviewed implementation
```

Current best neutral direction:

```text
Name the standalone thing later.
Keep the files generic now.
```

---

# 3. Core file-system decision

The **only guaranteed agent-owned root** should be:

```text
.agents/
```

Everything else should be repo-defined.

This makes the system usable in:

```text
normal single repos
monorepos
multi-repo workspaces
private vault repos
public open-source repos
```

The repo owner defines where docs, work, archive, guides, directives, context, and scripts live.

The agent discovers that through:

```text
.agents/workspace-map.md
```

No heavy YAML config for now.

---

# 4. The minimal required structure

The reusable template should start with:

```text
AGENTS.md

.agents/
  loader.md
  workspace-map.md

  loader/
    00-start-here.md
    01-authority.md
    02-loading.md
    03-request-types.md
    04-locality-rule.md
    05-work-cycle.md
    06-delivery-cycle.md
    07-handoff.md
    08-learning.md

  observations/
    00-index.md

  sessions/
    00-index.md

  handoffs/
    00-index.md

  state/
    .gitkeep
```

Optional later:

```text
.agents/
  roles/
  skills/
  patterns/
  examples/
  templates/
```

But the first version should stay minimal.

---

# 5. The loader model

The loader is the “framework of the framework.”

Every agent, role, skill, or wrapper should eventually start with:

```text
Read AGENTS.md.
Read .agents/loader.md.
Follow the loader.
```

The loader then reads:

```text
.agents/workspace-map.md
.agents/loader/00-start-here.md
.agents/loader/01-authority.md
.agents/loader/02-loading.md
.agents/loader/04-locality-rule.md
```

Then it classifies the request using:

```text
.agents/loader/03-request-types.md
```

After that, it loads only what is relevant.

The system is reference-based:

```text
files link to the next useful files
the agent follows the graph
no giant upfront context load
no large static config
```

---

# 6. Workspace map

The workspace map is the one file to edit when applying the template to a specific repo.

File:

```text
.agents/workspace-map.md
```

It should define:

```text
workspace mode
issue source
commit format
branch format
agent-owned material
human-facing active material
work placement
locality rules
archive rules
active truth rules
```

Example generic fields:

````md
# Workspace Map

## Workspace mode

Mode:

```text
<single-repo | vault-repo | monorepo | custom>
````

## Issue source

Issue repository:

```text
<org>/<issue-repo>
```

Commit message format:

```text
<org>/<issue-repo>#<issue-number>: <summary>
```

Branch format:

```text
feature/<work-package-slug>
```

## Agent-owned material

* Loader: `.agents/loader.md`
* Loader instructions: `.agents/loader/`
* Observations: `.agents/observations/`
* Sessions: `.agents/sessions/`
* Handoffs: `.agents/handoffs/`
* State: `.agents/state/`

## Human-facing active material

* `<label>`: `<path>/`
* `<label>`: `<path>/`

## Work placement

Default:

```text
<local-owner>/work/<work-package>/
```

## Locality rule

For any `<thing>`, related material should live beside that thing:

```text
<thing>/archive/
<thing>/changelog.md
<thing>/decisions/
<thing>/work/
```

````

For your private repo, this can be customized to:

```text
Issue repository: TheLithiumForge/Vault
Commit format: TheLithiumForge/Vault#<issue-number>: <summary>
````

---

# 7. Important new rule: optional `.overwrite.md`

Each loader file should allow a local override file.

Pattern:

```text
.agents/loader/00-start-here.md
.agents/loader/00-start-here.overwrite.md
```

or:

```text
<file>.overwrite.md
```

Meaning:

```text
The base file provides default behavior.
The overwrite file provides local changes.
When present and referenced by the loader, the overwrite file modifies or replaces the base behavior.
```

This supports fluid, organic growth.

There are three possible override modes:

```text
append
replace
disable
```

A simple markdown convention could be:

```md
# Overwrite Mode

Mode: append
```

or:

```md
# Overwrite Mode

Mode: replace
```

Recommended initial rule:

```text
If <file>.overwrite.md exists, load it immediately after <file>.
The overwrite file can refine, narrow, extend, or replace the base file.
The overwrite file must state its mode near the top.
```

Example:

```text
.agents/loader/06-delivery-cycle.md
.agents/loader/06-delivery-cycle.overwrite.md
```

This lets a specific repo adapt the delivery cycle without editing the template file.

This is probably central to the standalone product idea.

---

# 8. The pattern rule

You clarified that “pattern” is just a rule. It must be explicit and explained.

The system should not vaguely say “use patterns.” It should say:

```text
A pattern is a reusable structural rule.
A pattern describes where something goes, how it is named, and how it relates to nearby files.
Patterns must be generic first, with examples second.
```

Example:

```text
Rule:
For any <thing>, place historical material under <thing>/archive/.

Explanation:
This keeps history searchable beside the active thing.

Examples:
docs/architecture/archive/
repo/<repo-name>/architecture/archive/
directives/testing/archive/
```

So the template should use explicit rules, not abstract pattern language.

---

# 9. Most important structural rule: locality

This is one of the highest-priority decisions.

Rule:

```text
Preserve locality of behavior, search, ownership, history, and entries.
```

Meaning:

```text
related material should live beside the thing it belongs to
```

Generic structure:

```text
<thing>/archive/
<thing>/changelog.md
<thing>/decisions/
<thing>/work/
<thing>/handoff.md
```

Why:

```text
when searching architecture, you want architecture history nearby
when changing a directive, you want its archive nearby
when reviewing work, you want evidence and handoff nearby
```

Preferred:

```text
repo/<repo-name>/architecture/archive/
repo/<repo-name>/architecture/changelog.md
repo/<repo-name>/work/<work-package>/handoff.md
directives/<directive-name>/archive/
```

Less preferred:

```text
archive/architecture/
archive/directives/
handoffs/all-work/
```

Exception:

```text
A top-level archive is acceptable for orphaned, imported, abandoned, or no-longer-local material.
```

---

# 10. Human-owned versus agent-owned

## Agent-owned

Agent-owned material belongs under:

```text
.agents/
```

Examples:

```text
loader
loader instructions
observations
sessions
handoffs without local owner
state
future skills
future roles
future wrappers
```

## Human-facing active material

The workspace decides.

Examples:

```text
docs/
directives/
guides/
repo/
architecture/
scripts/
product/
tasks/
```

These are not fixed by the template.

## Gray area

Directives and guides can be either:

```text
root-level human-facing folders
```

or:

```text
.agents-scoped folders
```

In a standalone app repo, they may stay under `.agents`.

In your private Vault, they probably deserve root-level folders because they are part of the reusable source of truth.

---

# 11. Request types

The loader should classify requests into generic types:

```text
casual thought
question
rewrite
document creation
greenfield idea
brownfield change
implementation
review
testing
handoff
migration
cleanup
workflow improvement
cross-area work
```

Each request type should define:

```text
what it means
likely files to load
likely output
artifact level
```

The agent should choose the smallest useful type.

---

# 12. Work cycle

The general work cycle is:

```text
idea
→ clarify intent
→ decide artifact level
→ create or update active truth
→ split into work packages if implementation is needed
→ deliver one work package
→ review
→ handoff
→ record observations
```

For greenfield:

```text
new idea
→ clarify purpose
→ define goals
→ define non-goals
→ define constraints
→ decide active truth location
→ create first active document if useful
→ split into work packages
```

For brownfield:

```text
change request
→ load active truth
→ load local history if needed
→ detect drift
→ update active truth
→ move superseded material to local archive
→ update changelog
→ create work package if needed
→ deliver
```

Brownfield work must avoid creating parallel truth. It should update active truth and preserve history locally.

---

# 13. Work package

Use “work package” for now.

Definition:

```text
A work package is a coherent, reviewable, testable, documented, committable slice of work.
```

Generic structure:

```text
<local-owner>/work/<work-package>/
  00-index.md
  01-scope.md
  02-evidence.md
  03-implementation.md
  04-review.md
  05-handoff.md
  changelog.md
  archive/
```

`<local-owner>` can be:

```text
docs/architecture
repo/<repo-name>
repo/_cross-repo
directives/<directive-name>
apps/<app-name>
packages/<package-name>
```

The location is decided by workspace map plus locality.

---

# 14. Delivery cycle

The implementation cycle should be:

```text
1. Read task or work package.
2. Load relevant active truth.
3. Restate scope and non-scope.
4. Define evidence.
5. Implement.
6. Self-review implementation.
7. Red testing.
8. Green testing.
9. Blue testing.
10. Refactor.
11. Re-run affected validation.
12. Independent review.
13. Fix findings.
14. Re-review if needed.
15. Update docs, evidence, and handoff.
16. Commit if allowed.
17. Record observations.
```

Important addition:

```text
Refactor happens after testing because the tests provide a safety net.
After refactor, validation must be re-run.
```

Testing meanings:

```text
Red = prove the issue, missing behavior, failing case, or negative case.
Green = prove the intended behavior works.
Blue = hardening, public boundary, docs, platform, security, typecheck, build, or similar confidence checks.
```

---

# 15. Handoff

Handoffs should make future continuation easy.

Preferred location:

```text
<local-owner>/work/<work-package>/handoff.md
```

Fallback location:

```text
.agents/handoffs/
```

Template:

```md
# Handoff

## Current objective

## Completed

## Evidence

## Files changed

## Decisions made

## Drift found

## Local archives or changelogs updated

## Remaining work

## Next safe action

## Useful files to load next
```

---

# 16. Learning and observations

Agent observations should live under:

```text
.agents/observations/
```

They are not active truth.

They are candidate learning material.

Observation template:

```md
# Observation

## Context

## Repeated pattern or friction

## Why it matters

## Candidate action

## Suggested destination if promoted
```

Promotion rule:

```text
Observations can suggest updates to rules, guides, workspace map, loader files, roles, or skills.
They become active only after review.
```

This supports organic growth without letting agents silently change the methodology.

---

# 17. Skill wrapper

You likely need only one skill initially.

Path:

```text
.agents/skills/use-loader/SKILL.md
```

Purpose:

```text
Wrapper for tools that support skills.
Its only job is to make the agent load .agents/loader.md.
```

Content idea:

```md
# Use Agent Loader

Before planning, writing, reviewing, editing, or implementing:

1. Read `AGENTS.md`.
2. Read `.agents/loader.md`.
3. Follow the loader instructions.
4. Read `.agents/workspace-map.md`.
5. Continue using the files referenced by the loader.

The repository loader is the source of workflow behavior.
This skill only ensures the loader is used.
```

Future agents and skills can be custom, but they should all include this at the top:

```text
Start by loading .agents/loader.md.
```

---

# 18. Roles and subagents

You are not sure yet if many agents are needed.

Current conclusion:

```text
Start with a generic loader.
Add roles only if repeated use proves they help.
```

Potential future roles:

```text
planner
architect
implementer
reviewer
tester
curator
```

Rule:

```text
Every role must load .agents/loader.md first.
Roles are lenses over the same loader, not separate systems.
```

This means a generic agent plus good loader may already solve much of the subagent problem.

---

# 19. Commit and branch rules for your repos

For your private implementation:

```text
Issue repository: TheLithiumForge/Vault
```

Commit format:

```text
TheLithiumForge/Vault#<issue-number>: <summary>
```

Important correction:

```text
The issue repo is not necessarily the repo being modified.
Commits reference the repo that holds the issues.
```

Branch format:

```text
feature/<work-package-slug>
```

For multi-repo work:

```text
Use the same branch name across all affected repos.
Each repo can have commits referencing the same issue source.
```

No AI co-author footer.

---

# 20. Standalone product view

As a standalone SDD-style product, the system can be described as:

```text
A repo-native agent workflow template that turns vague intent into local active truth, reviewable work packages, tested delivery, handoff, and reusable learning.
```

Core product promises:

```text
minimal setup
plain markdown
repo-native
tool-agnostic
vendor-agnostic
single loader entrypoint
workspace-defined structure
locality-first organization
organic growth
works for single repo, monorepo, vault repo, and multi-repo source-of-truth repo
```

Potential public positioning:

```text
A tiny SDD substrate for agentic development.
A loader-based workflow system for AI-assisted repositories.
A local-first agent workflow template.
```

Possible repo names:

```text
inflow
inflow-template
inflow-sdd
agent-workflow-template
agentic-workspace-template
forge-method-template
```

---

# 21. Your private repo view

For your own repos, you probably want a Vault-style setup.

Possible shape:

```text
Vault/
  AGENTS.md

  .agents/
    loader.md
    workspace-map.md
    loader/
    observations/
    sessions/
    handoffs/
    state/
    skills/
      use-loader/
        SKILL.md

  directives/
  guides/

  repo/
    00-index.md

    Monolith/
      00-index.md
      context/
      architecture/
        00-index.md
        decisions/
        archive/
        changelog.md
      work/
        00-index.md
      archive/

    TheLithium.Config/
      00-index.md
      context/
      architecture/
      work/
      archive/

    TheLithium.PublicApiGenerator/
      00-index.md
      context/
      architecture/
      work/
      archive/

    _cross-repo/
      00-index.md
      decisions/
      work/
      archive/

  scripts/

  archive/
    imported/
    abandoned/
    orphaned/
```

Top-level archive is only for material without a meaningful local owner.

Repo-local history stays local.

Architecture-local history stays under architecture.

Work-local handoffs stay under work package.

---

# 22. Template repo view

As a public or reusable template, it should look like a normal repo after applying it:

```text
template-repo/
  README.md
  AGENTS.md

  .agents/
    loader.md
    workspace-map.md

    loader/
      00-start-here.md
      01-authority.md
      02-loading.md
      03-request-types.md
      04-locality-rule.md
      05-work-cycle.md
      06-delivery-cycle.md
      07-handoff.md
      08-learning.md

    skills/
      use-loader/
        SKILL.md

    observations/
      00-index.md

    sessions/
      00-index.md

    handoffs/
      00-index.md

    state/
      .gitkeep
```

README should contain configuration examples.

The only necessary custom file should be:

```text
.agents/workspace-map.md
```

---

# 23. Organic growth model

Growth should happen in this order:

```text
observation
→ repeated observation
→ candidate rule
→ owner-reviewed rule
→ loader update or guide
→ optional role or skill
```

This prevents bloat.

Agent-generated insights start as:

```text
.agents/observations/
```

Then they can be promoted to:

```text
loader file
directive
guide
workspace map entry
skill
role
template
```

---

# 24. Current strongest design decisions

The strongest decisions so far are:

```text
1. .agents is the only guaranteed agent-owned structure.
2. AGENTS.md points to .agents/loader.md.
3. workspace-map.md is the only normal customization file.
4. Loading is reference-based, not config-heavy.
5. Locality is a hard structural rule.
6. Rules must be generic and template-style, with examples after.
7. Each loader file may have an optional .overwrite.md.
8. Work packages are the reviewable delivery unit.
9. Delivery includes red, green, blue testing, then refactor, then validation rerun.
10. Handoffs should be local whenever possible.
11. Observations are candidate learning, not truth.
12. Roles and skills are optional later expansions.
13. One initial skill wrapper is enough.
14. The same core should work for your private Vault and as a standalone SDD product.
```

---

# 25. Open decisions

Still unresolved:

```text
final product name
whether to call the standalone method Inflow, Forge Method, or something else
whether .overwrite.md should support append / replace / disable explicitly
whether role files should be included in v1 or v2
whether templates/ should be included in the initial template repo
whether examples/ should be included in the initial template repo
whether work package files should always be numbered or only by recommendation
whether public docs should call this SDD, agent workflow, or agentic workspace
```

My recommendation:

```text
Use “Inflow” as the candidate product name.
Use “agent workflow template” as the descriptive public phrase.
Keep v1 minimal.
Add .overwrite.md support from the beginning.
Add examples in README, not as separate folders yet.
Add templates later after real use.
```


--------- AI HANDOVER FINISH --------


and now 

------- HUMAN ORIGINAL MESSAGE BEFORE HANDOVER ---------
This is a handofer from another ai agent. Your role is to h lp me define an actual ai workflow system that works for my brain because right now i started encountering clutter and blockers after blockers in the current implementations of what i use. Ideally everything would be streamlined like this • i have an idea (green or brown field) • i discuss with ai If green then ai suggests let's do a prd/vision, we go back and Forward on the voncept until im happy. Then ai suggests, hey I'm running out of context, let's start a new chat, here's the handoff of the most imp things discussed. Then we do an actual architectural discusson. After that is done then again we handoff and we start creating tasks/epics/features/stories/etc whatever is needed (synced through gh-sync. We need to decide what we actually need).  After the tasks are completed which i imagine should be somehow feature by feature or sort of a good rewiewable split, like , my time is valuable, i want to review only fully capable, reviewable, eocumented and tested chuknks of work, you need o figure out that sort of split.  After this split Is done then we need to have an implementation cycle something like  Read the task, do the task, do a review of impl, then a cycle of red green blue testing, then another review pass (all of them with different subagents) . The tests implemented need to be based on the type of work. After such a split is thorowly done without bits and pieces let "for the future" or as "todo" or "in a real impl" or other bs like that) then the ai will make me actually review it. The ai will have the right to commit on a feature branch across repos, each of them will have the same festure branch name and all repos will use commits messages like <reponame>#<issueNum>: <what was done> , no coauthored by ai message.  At this point we kind of finished a full development flow, technically the user reserves the right to allow the ai to just spawn another review agent at the end instead of the user with totally no before context except for the task, project context, all dorectives and guides from the repos, but this is more like the exception.  For brownfield, we will need to adjust the current vision/prd, architecture docs and add change logs (in an archive folder) for each, and add tasks docs (+sync) then do the same impl cycle.  Very important is a split of the docs, we would have patterns in our vault (currently named github-private will be renamed to vault or something similar) repo. One such pattern is always have <docName>/{index, <filename-split>}.md + chantelog or archive, more to be seen in the project files.  We also need something that grows organically and that the core of can be extracted and reused in different projects.  Here is what the conclusions of another ai agent are. Btw you may disregard how the files are now structured because right now it's a lot of clutter, idk if i want to keep using the bmad terminology, and idk if i want to remove most of the things i have, i know i want to remove bmad, but i will still need for example wrappers for multi agent-harnesses, also the current architecture and prd and stuff might get moved, or rereviewd or simply deleted or just used as "mvp", so your decisions should be made with the assumption of starting a new repo called idk lith-vault or just vault or something like that (thematic with the whole the lithium forge, lith, elements etc) that is meant to keep safe all the imp secred docs, tasks, ideas, scripts, etc the current github-private will be archived and only the imp things from it will be moved like the directives scripts guides and some of the prd amd architecture that we have in some way or another ========




 Confidence: 91%. Some uncertainty remains because this is a conceptual spec, not yet tested against multiple real repos. But the direction is now clear enough to hand to another agent. The Forge Methodology, v0 Spec 0. One-sentence definition The Forge Methodology is a lightweight, repo-native, agent-readable system for helping a person define what they want, turn it into useful work, self-manage implementation, preserve learning, and evolve the system organically through normal files. It is not primarily a CLI, not a giant agent pack, and not BMAD with patches. BMAD, Rune, Monolith, and your current directives are inputs and inspiration. The actual thing should be smaller, more personal, more organic, and more strict where it matters. ─── 1. What the user wants 1.1 A methodology, not a tool-first framework The user wants a way of working that can be expressed as files in a repo. It should work with: text Claude Codex Copilot OpenCode ChatGPT future agents manual human reading It should not require: text a CLI a database a plugin a server a full framework install a specific vendor Those things may come later, but the methodology must be useful as plain markdown first. 1.2 A living knowledge system The user wants the methodology to grow with them. It should support: text temporary agent observations human-reviewed long-term truth repo-specific overlays global/org-level guides tasks project context archive history session state implementation handoff The Rune/Monolith ideas are important here. The current Rune model already separates workspace root from tool-state root, and keeps user-facing paths resolved from the workspace rather than hidden state. That principle should be reused. 1.3 A system that helps define wants and needs The user does not just want agents to execute. They want agents to help them think. The agent should notice when the user asks for “X” but “X” is underdefined and respond with something like: text I understand the direction, but before building this we should define: why it should exist, who it is for, where it should live, how strict it should be, what existing rules apply, what would prove it works, and what the smallest useful version is. The agent should not blindly turn vague wishes into tasks. 1.4 A self-managing implementation loop During implementation, the system should manage: text loaded context active assumptions task scope evidence needed tests needed implementation steps review pass drift found docs to update handoff for next agent candidate learnings Your existing .impl.md companion files already show this pattern: tasks, dev notes, file structure, testing requirements, references, dev agent record, completion notes, debug log, file list, and changelog live beside the planning story. This should become a general Forge concept, not just a BMAD artifact. 1.5 Lightweight by default, strict when needed The user wants a system that can say: text This is just a thought. No artifact needed. This is a repeated pattern. Create a guide. This affects public CLI behavior. Create tests. This changes architecture. Create a decision record. This is destructive. Require explicit approval. No universal ceremony. No forced PRD for everything. No 40-page story for a small fix. 1.6 Shareable, but personalizable The methodology should be generic enough that others can use it, but each person or org should have their own canon. Generic: text method knowledge classes loading model strictness levels intent forge delivery forge review forge handoff forge Personal or org-specific: text directives guides context preferred testing style repo structure communication style tool preferences project history ─── 2. What the user cannot stand 2.1 Absolute-language frameworks overriding local intent The user dislikes systems whose skills say things in a way that overpowers the repo’s own directives. BMAD has useful pieces, but it often acts like its own instructions are the top-level truth. Your current directive loader already tries to solve this by making directives the entry point and task-specific files load on demand. The Forge Methodology should make authority order explicit. 2.2 Giant workflow beasts The user does not want a huge system that tries to cover everything. Bad: text Install 50 agents. Run this exact workflow. Follow all phases. Always produce PRD, architecture, epics, stories, tests, docs. Good: text Read the entry file. Classify the request. Load relevant context. Use the smallest reliable loop. Escalate only when risk demands it. 2.3 Manual command memorization The user should not need to remember: text run workflow X then run validation Y then run agent Z The current Avery idea is already close to this: it routes natural language to specialist loops with minimal ceremony and enforces sync guardrails and approval gates. Forge should preserve this, but without depending on BMAD as runtime. 2.4 Acceptance criteria that are not tested The user dislikes acceptance criteria that sit in a document but do not become evidence. Every meaningful task should answer: text What proves this works? What test level is appropriate? What negative case matters? What command should be run? What is explicitly not required? 2.5 Polluted current docs The user does not want current architecture or product docs filled with old debates, abandoned directions, and implementation noise. Current truth should be clean. History should still exist, but in archive, decisions, change records, or session logs. 2.6 Agents that are either too passive or too autonomous The user wants agents to be proactive, but not reckless. Bad passive mode: text What do you want me to do? Should I continue? Please confirm every obvious thing. Bad reckless mode: text I changed your architecture and promoted a new directive without asking. Good Forge mode: text I made the smallest safe assumption. I documented it. This part needs approval because it changes canon. This part can be done directly because it is only task-local. ─── 3. Core design principles 3.1 Repo-native first Everything important must be expressible as files: text markdown yaml frontmatter json config only if needed No hidden system should be required for the methodology to function. 3.2 Canon beats framework Authority order: text explicit user instruction > safety and destructive-action rules > repo directives > project directives > org directives > personal method > guides > rituals > examples > external frameworks BMAD, TEA, Rune, and other systems are references unless explicitly adopted. 3.3 Load only what matters Agents should not load the whole world. The existing directive loader pattern is right: load relevant directives based on the current task, such as tests, TypeScript code, C# code, docs, sprint planning, custom agents, or cross-platform design.  Forge should generalize that into a generic loading model. 3.4 Smallest reliable structure The agent should ask: text What is the smallest amount of structure that makes this work safe, clear, and resumable? 3.5 Evidence over ceremony A task is not done because the task list says so. A task is done when: text the implementation exists the appropriate evidence exists the review loop found no blocking issue the handoff is clear drift is recorded 3.6 Organic growth The system should grow through: text signal candidate note guide directive workflow agent or tool adapter, only if needed Agents may suggest growth. They should not silently promote agent-generated signals into canon. ─── 4. Knowledge model 4.1 Knowledge classes The methodology should define these classes: text Canon Signals Archive Context Tasks State 4.2 Canon Canon is human-reviewed active truth. Includes: text directives guides rituals workflows current project context current repo context architecture decisions testing rules documentation rules Agents read canon as authority. Agents may propose canon changes. Agents should not silently change canon unless the task explicitly allows it. 4.3 Signals Signals are agent-generated observations. Examples: text agents repeatedly over-plan small fixes this guide is ambiguous this repo context is stale this test pattern keeps recurring this task exposed missing directive Signals are not authority. Signals may decay, be archived, or be promoted after review. 4.4 Archive Archive is historical, superseded, low-priority, or inactive material. It is searchable, but not loaded by default. Archive never overrides canon unless explicitly restored. Rune already has a useful concept here: archived material can be weighted lower in query and not treated like active memory by default. The Monolith implementation notes also distinguish active query paths and archived weighting.  4.5 Context Context is current factual material scoped to: text person org domain project repo task session Repo-specific context should be only one overlay, not a different system. 4.6 Tasks Tasks are active work objects. They can be: text global project-specific repo-specific session-specific Tasks reference canon and context. Tasks produce implementation state, evidence, handoff notes, signals, and sometimes canon-change proposals. 4.7 State State is tool-owned data. Examples: text indexes embeddings query logs cache SQLite audit logs generated refs State is not canon. Rune already shows this separation through workspace root vs tool-state root.  ─── 5. Scope and overlay model 5.1 Global-first, scoped overlays Default shape: text global canon org canon project context repo context task context session state signals, only if relevant archive, only if needed 5.2 Repo-specific is not special Repo-specific knowledge is just: text context/repos/<repo-name> tasks/repos/<repo-name> signals/repos/<repo-name> archive/repos/<repo-name> It should not become the primary folder layout for everything. 5.3 Configurable roots The method should support both styles: Category-first: text context/repos/Monolith/ tasks/repos/Monolith/ signals/repos/Monolith/ archive/repos/Monolith/ Repo-first: text repos/Monolith/context/ repos/Monolith/tasks/ repos/Monolith/signals/ repos/Monolith/archive/ Default recommendation: category-first. Reason: directives, guides, rituals, and workflows are usually global. Repo-specific things are overlays. ─── 6. Strictness model The system needs explicit strictness levels. text S0: Thought S1: Note S2: Guide S3: Workflow S4: Directive S5: Guardrail S0: Thought Use for casual exploration. No artifact required. S1: Note Use for something useful but not stable. May become a signal or candidate note. S2: Guide Use for repeated explanation or pattern. Helpful but not mandatory. S3: Workflow Use for a repeatable sequence. Flexible, but expected to be followed when invoked. S4: Directive Use for hard rules. Agents must follow unless user explicitly overrides and it is safe. S5: Guardrail Use for safety, data loss, destructive actions, secrets, sync, public contracts, legal or irreversible actions. Must not be bypassed casually. ─── 7. Core loops 7.1 Intent Forge Purpose: help the user define what they want. Use when: text the idea is vague the user is unsure the user asks for strategy the user wants to build something underdefined the agent detects missing purpose or scope Questions: text Why should this exist? Who is it for? What pain disappears if this works? Where should it live? What existing rules apply? What tools are acceptable? What should be out of scope? How strict should this be? What would prove it works? What would make this a bad idea? What is the smallest useful version? Perspectives: text conservative version pragmatic version ambitious version weird version anti-version adversarial critique builder view historian view Output: text best current understanding remaining uncertainty recommended strictness level smallest useful next step whether to create note, guide, directive, workflow, task, or nothing 7.2 Planning Forge Purpose: turn intent into a plan. Use when: text the user wants product definition architecture task breakdown roadmap implementation strategy Outputs can include: text brief context update decision record task outline evidence target implementation plan Planning should not always produce PRD, architecture, and epics. It should choose depth based on strictness and risk. 7.3 Delivery Forge Purpose: self-manage implementation. Loop: text load relevant canon and context restate intent define scope and non-scope classify strictness define evidence implement smallest coherent change run validation review against directives fix gaps update task/session state record drift record reusable learning handoff The existing Avery delivery loop already describes implementation-oriented orchestration and routing through story prep, implementation, QA, review, and documentation while keeping the user at the summary layer. Forge should turn that into a generic runtime behavior. 7.4 Evidence Forge Purpose: turn requirements into proof. For every meaningful task: text what behavior changed? what test level fits? what negative cases matter? what public boundary changed? what command proves it? what evidence is intentionally not required? Output: md ## Evidence ### Required Proof ### Negative Proof ### Public Boundary Proof ### Validation Commands ### Not Required ### Result 7.5 Review Forge Purpose: review the result from multiple angles. Review passes: text intent review scope review directive review architecture review test review public-boundary review security/safety review maintainability review documentation review handoff review Output: text blocking findings important findings optional improvements evidence gaps next safe action 7.6 Handoff Forge Purpose: make the work resumable. Output: md ## Handoff ### Completed ### Evidence Run ### Files Changed ### Drift Found ### Decisions Made ### Follow-Up ### Candidate Learnings ### Next Safe Action Your repo already has this idea in readiness handoffs with next commit, remaining work, helpful files, and drift notes.  7.7 Learning Forge Purpose: evolve the methodology. At the end of meaningful work, agent asks: text Did I discover a reusable rule? Did a directive fail? Was a guide missing? Did context drift? Should this become signal, note, guide, directive, workflow, task, or archive? Rules: text create signals freely propose canon changes do not silently promote to canon archive stale material when approved ─── 8. Agent behavior requirements 8.1 The agent must classify before acting Every request should be classified by: text intent type scope strictness risk needed context needed evidence artifact impact 8.2 The agent must ask fewer but better questions The agent should ask only if the answer materially changes the result. Bad: text What do you want me to do? Good: text This can be either a personal guide, an org directive, or a reusable workflow. Which strictness do you want? 8.3 The agent must not overfit to tools The method should not say: text Run BMAD workflow X. It should say: text Use the appropriate planning ritual. BMAD may be used as reference if available. 8.4 The agent must keep working state During implementation, maintain a compact state: md ## Forge State ### Intent ### Loaded Canon ### Loaded Context ### Assumptions ### Scope ### Strictness ### Evidence Needed ### Current Step ### Drift ### Candidate Learnings This can live in: text task companion session file implementation note agent scratch artifact depending on repo setup. 8.5 The agent must distinguish active truth from history If something is superseded: text do not leave it as active truth move or link it to archive/history update canon to current state record the change 8.6 The agent must be pragmatic If a task is small, do the task. If a task is risky, slow down. If a task is unclear, forge the intent first. If a task changes canon, propose the update. ─── 9. Files to create, one by one This is the order I would give to another agent. Phase 1: Entry and authority File 1: AGENTS.md Purpose: universal entry point for any agent. Must include: text read method read loading model classify request load only relevant files respect authority order external frameworks are references maintain Forge State during implementation record learnings File 2: forge/method/forge-methodology.md Purpose: define the methodology. Must include: text definition principles core loops knowledge classes strictness organic growth File 3: forge/method/authority.md Purpose: define what overrides what. Must include: text explicit user instruction safety rules repo directives project directives org directives method guides rituals examples external frameworks File 4: forge/method/loading-model.md Purpose: teach agents how to load files. Must include: text load minimal relevant canon load scoped context load task/session state do not load archive unless needed do not load signals unless learning or drift matters Phase 2: Knowledge classes File 5: forge/method/knowledge-classes.md Purpose: define canon, signals, archive, context, tasks, state. File 6: forge/method/scoped-overlays.md Purpose: define global-first and repo/project overlays. File 7: forge/method/knowledge-maturation.md Purpose:




------- END ---------



we will create this repo such that it can be easily "converted" in something usefull, so the repo itself will be very flat, all the files will be in src, all agentic files will be in src/.agents and then the other files. we will do it like this because i might want to also have a very minimal cli that will basically just get/copy the files to a new place (from the public git main), and at most will inform of some conflicts or something like numbered or unnumbered folders, so absolutely nothing fancy 

now, let's go from start to finish
let's start with the loader and if i actually want a config/project-map file and if yes what do i want in them actually (like what's the minimal)
and then we can proceed with more detailed things