# Open Forge, rewritten (second pass)

This pass revisits [the first pass](fable-open-forge.md) after reading the Writing Directive, the Writing Standard, Project Voice, and the Dictionary. Every category is here. Tags, frontmatter, and headings are unchanged. Descriptions are unchanged, so no `Entries` need regenerating.

What the standard changed, compared with the first pass:

- **The definition sentence under each category question is back.** The standard requires it ("question, then definition"). Each one now does a job the question cannot: it names what the category is, or draws the line to its nearest sibling.
- **"You" is mostly gone.** The standard prefers the actor omitted when the action is clear, and the shipped files already use that shape. "You" survives only where dropping it would leave the sentence ambiguous.
- **Periods instead of semicolons.** The first pass used a few.
- **Defined terms wear backticks at first use.** After that the plain word is used, as the standard describes.
- **`self-growing` is back in Memory's definition.** The standard asks for that descriptor to stay.
- **`slug` is back in the loader's Terms.** The CLI uses the word in one message, so the installed Framework should define it. The rules still say "scope folder", which reads better.
- **Two meaning slips from the first pass are fixed.** "Every Directive listed under an entrypoint's Entries" would have included child entrypoints, which are on demand. The original "sibling" meant same-folder files only. The same fix applies to the Workflow recipe rule.
- **"Check `Entries` when ..." replaces "Look here when ..."** for predictability. The phrase is established, and it names the thing to read.

Structural changes from the first pass are kept: grouped loader axioms, one general "check the Entries" rule in the loader, the collapsed manager rules, Handoffs owning the when-to-create rule, and the Analysis rule in base vocabulary. Nothing from the source is dropped.

---

## `AGENTS.md`

```markdown
<!-- open-forge:start -->

# Open Forge

Open Forge provides the working rules and context for this workspace.

Before starting a task, read `.agents/loader.md`. Use it to select every relevant scope, including nested scopes, and follow the loaded rules until the task ends.
<!-- open-forge:end -->
```

---

## `CLAUDE.md`

Unchanged.

```markdown
<!-- open-forge:start -->

@AGENTS.md
@.agents/loader.md
<!-- open-forge:end -->
```

---

## `.agents/loader.md`

```markdown
# Open Forge Loader

Read this after `AGENTS.md`. It explains how the workspace is organized, what wins when sources disagree, how to work with the user, and where new knowledge belongs.

## Terms

A few words carry exact meanings in this workspace:

- `entrypoint` - The Markdown file that makes a folder navigable. Open Forge names it `_{folder-name}.md`. `index.md`, `_index.md`, `references.md`, and `_references.md` are accepted as compatibility names.
- `entry` - One generated line under an entrypoint's `Entries`. It links to a child and describes it.
- `route` - A path followed through entrypoints and their entries.
- `root route` - A route this loader exposes directly.
- `scope` - A folder inserted below a root that narrows everything beneath it.
- `slug` - The concrete folder name in a route path.
- `description` - The one-line summary that helps a reader decide whether to open a file.
- `responsibility` - An optional sentence that tells an editor what belongs in a file. It carries no authority and changes no loading.
- `axiom` - A rule under an `Axioms` heading in this loader or in a loaded entrypoint.
- `accepted` - Approved by clear user direction, delegated authority, or a source the user declared authoritative. Until then, material is a candidate.

## Axioms

### What Wins

When sources disagree, resolve in this order:

1. Platform constraints and runtime safety bound every action.
2. Clear user direction sets goals, priorities, important choices, and accepted changes. Do not ask the user to confirm direction they have already given.
3. A source the user declared authoritative for a fact wins for that fact.
4. Loaded Axioms and Directives govern within their scope.
5. The accepted source for a subject defines its current state.

Loading a file makes it visible. Tags classify it. Neither gives it authority.

### Working With The User

- A request to act allows the routine, reversible, in-scope choices the work needs.
- Use the request and accepted context to settle unclear points. When an unsettled choice could materially change the result, scope, risk, cost, external effects, or reversibility, keep it #Contextual and ask before relying on it.
- Match explanation and planning to the request. Unless the user asks for deeper analysis, open with the current understanding, one recommendation, and at most one important open choice.
- Continue through safe, in-scope work when accepted direction or a stated reversible assumption covers it. Do not stop only to ask whether to continue.
- State assumptions so they can be corrected. Stop when uncertainty, a conflict, or an authority boundary could materially change the work.

### Where Meaning Lives

- Each source answers one clear question. Link to related sources instead of repeating them. A link changes nothing about either source's authority, scope, loading, or lifecycle.
- Put detail in the narrowest source that owns the question. Higher entrypoints and records keep only the summary needed to select that source.
- Validation asks whether evidence supports a claim. Acceptance decides whether knowledge or a decision counts as current within its scope. Restoring or moving a record does not accept it.
- When two #CurrentTruth sources seem to conflict, investigate before changing either. When accepted direction changes current state, update the source that defines it and keep useful prior context.
- Once an Analysis reaches a conclusion and work proceeds from it, treat the Analysis as sealed provenance. Record what execution discovers in the working record for that work, not in the Analysis.

### How Rules Combine

- Only this loader and loaded entrypoints define active Axioms. A loaded child route inherits its ancestors' Axioms and adds only rules for its narrower scope.
- Accepted workspace-specific content replaces the Open Forge default for the same role, within its accepted scope.
- Narrower non-binding material may specialize broader material of the same kind. Binding instructions add to one another. They never silently override one another. Report conflicts that cannot be resolved.

### Routing

- An `entrypoint` lists its direct routes under `Entries`. Read the entrypoint before its children. Decide what matters from descriptions, tags, paths, ancestor routes, and explicit links.
- Follow relevant branches recursively. Every folder on a `route` has exactly one recognized entrypoint. Do not open file bodies only to discover routes.
- Before deciding a category has nothing for the task, check its `Entries`.
- A `root route` exists only where this loader exposes it. It cannot be recreated or scoped inside another route.
- Below a root, add `scope` folders anywhere in the path. Each one narrows everything after it. A scope holds only the routes useful there. It need not mirror another scope or the defaults. Managed routes keep their segment order through scopes: a scope may sit before, between, or after managed segments, but never reorders them.
- Put specialized material in the narrowest useful scope. Reserve workspace-wide placement for material that applies across the workspace.
- Keep separately selected scopes as separate chains and follow their explicit links. Recheck the selection after an important change in the task.
- Each entrypoint chooses the loading its contents justify. Entries stay on demand unless tagged. Use #LoadNow only when missing the content would cost more than reading it every time the parent loads. Use #KeepInMind only when that content also needs refreshing to preserve continuity while its scope stays active.
- Users may add, move, replace, or remove routes. A familiar folder name or tag does not turn a route into a root or give it managed status. Removed defaults stay removed unless the user asks to restore them.
- A tool such as the CLI or an Extension installer may manage the files it recognizes. Management affects file lifecycle only. It changes nothing about meaning or authority.
- A user-owned `{name}.overwrite.md` loads right after `{name}.md` and shares its route, scope, and loading. It is not indexed or selected on its own. Read it as part of the base file. Where the two answer the same question differently, the overwrite wins for that content only.

### Tags And Loading

Tags change what loads, when it loads, or how content is classified. They never create authority. Tags not defined here are search and routing signals.

#LoadNow and #KeepInMind act only through loaded parents. Neither activates an ancestor or scope that was not selected.

- #LoadNow - Read this entry, in listed order, when an already-loaded parent exposes it. If it is an entrypoint, apply the same rule to its Entries.
- #KeepInMind - Read this continuity context when its parent loads, then refresh it while its scope stays active: at task start or resume, after context restoration, and before handoff or closeout. Read the tagged file and its overwrite. For an entrypoint, apply its child loading rules in listed order. Refresh only active scopes. Recheck mid-task only when the continuity set may have changed.
- #Core - Routing, loading, workspace orientation, and reusable agent-facing roles.
- #Memory - Self-growing Markdown state: active work, coordination, accepted knowledge, candidates, and history.
- #Extension - Optional packaged routes, capabilities, integrations, and support files.
- #Contextual - Useful context that is not accepted. Treat it as a candidate unless applicable authority accepts it within its scope.
- #CurrentTruth - Accepted current state within its stated scope. It sits below user direction, runtime safety, platform constraints, and declared external sources.
- #Evergreen - Must stay aligned with accepted current state. Update it before dependent work relies on it and no later than closeout. Batch related updates when safe. Preserve useful prior context in the right Memory route, and report updates that cannot be made.

### CLI

When the Open Forge CLI is available, use these commands. The files stay complete without it.

- `open-forge --help` - Show the complete current command interface
- `open-forge context [<source-reference>...]` - Read startup or selected context
- `open-forge route list [<source-reference>]` - List routed sources and descendants
- `open-forge route inspect <source-reference>` - Inspect one source's route behavior
- `open-forge find [options]` - Find Markdown sources by tags and headings
- `open-forge references <source-reference>` - Inspect direct authored references
- `open-forge index [<source-reference>...]` - Rebuild generated `Entries`
- `open-forge status` - Summarize workspace, context, lifecycle, generated navigation, and recovery
- `open-forge doctor` - Diagnose workspace, routes, references, lifecycle, and recovery without changes

## Entries

- [Required instructions loaded through selected routes](directives/_directives.md) - #LoadNow #Core #Directive
- [Advice for recurring choices, tradeoffs, and work situations](guidance/_guidance.md) - #LoadNow #Core #Guidance
- [Concise maps to important local and external sources and when to use them](maps/_maps.md) - #LoadNow #Core #Map
- [Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history](memory/_memory.md) - #LoadNow #Memory #OrganicGrowth
- [Reusable default shapes for code, files, APIs, documents, and other work](patterns/_patterns.md) - #LoadNow #Core #Pattern
- [Specialized capabilities provided through native SKILL.md packages](skills/_skills.md) - #LoadNow #Core #Skill
- [Copy-ready files for starting independently maintained workspace content](templates/_templates.md) - #Core #Template
- [Repeatable Markdown recipes for reaching a defined goal](workflows/_workflows.md) - #LoadNow #Core #Workflow
```

---

## `.agents/directives/_directives.md`

```markdown
---
open-forge:
  description: Required instructions loaded through selected routes
  tags: [LoadNow, Core, Directive]
---

# Directives

## What behavior is required in this scope?

Directives are the instructions that must be followed within their scope.

## Axioms

- Every Directive file listed beside its `entrypoint` carries #LoadNow.
- Each Directive has one non-empty `## Instructions` section.
- Directives listed beside this root entrypoint apply throughout the workspace.
- A child entrypoint narrows the scope first. The Directives listed beside it apply only within that route.
- Child Directives add to the parent Directives already in force. A narrower scope grants no higher authority.
- Report any instruction that conflicts with another or cannot be followed, and explain why.

## Entries

- none - No entries - #Empty
```

---

## `.agents/guidance/_guidance.md`

```markdown
---
open-forge:
  description: Advice for recurring choices, tradeoffs, and work situations
  tags: [LoadNow, Core, Guidance]
---

# Guidance

## What approach is recommended, and when does it fit?

Guidance recommends an approach for a recurring situation and explains when it fits. A Directive binds. Guidance advises, and can be adapted to the work at hand.

## Axioms

- Check `Entries` when the work involves a recurring situation or choice.
- Each Guidance file explains the situation, the recommended approach, why it helps, and its tradeoffs.
- Apply Guidance when it fits. When another approach fits better, explain why.

## Entries

- [Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review](adaptive-collaboration.md) - #Core #Guidance #Collaboration #Ideation #Decision #Convergence #Review #Experience
```

---

## `.agents/guidance/adaptive-collaboration.md`

```markdown
---
open-forge:
  description: Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review
  tags: [Core, Guidance, Collaboration, Ideation, Decision, Convergence, Review, Experience]
---

# Adaptive Collaboration

## Scenario

Use this Guidance when exploring an idea, resolving an important uncertainty, clarifying the desired outcome, or finishing a broad piece of work.

## Preferred Approach

- Start from what the user has already said and what accepted context already settles.
- Build the best current understanding before asking for more input.
- Unless the user asks for deep analysis, open with the outcome as currently understood, the strongest recommendation, and at most one important open choice.
- Work through one important choice at a time. It may bundle a few tightly related questions.
- Give the user something concrete to react to. Do not ask them to invent the solution.
- Make it clear which points are accepted direction, which are recommendations, which are assumptions, and which are open questions.

## Interaction Depth

Match the depth to the request:

- When the outcome and constraints are already sufficient, proceed, or give the requested plan without a discovery phase.
- When one important choice remains, recommend a default and ask only for the judgment needed to resolve it.
- When the desired outcome is still forming, compare a few meaningfully different directions by the outcomes and tradeoffs they offer.
- When deep design is requested or clearly needed, give the architecture, alternatives, risks, and verification without holding back useful detail.

A short request does not mean low expertise. Technical language does not mean the user wants a long answer. Follow the available context, the detail requested, and how involved the user wants to be.

The first response sets direction. Do not front-load a finished document, an exhaustive feature list, implementation stages, an acceptance matrix, or research detail before the user asks or the current choice needs it.

## Progressive Disclosure

Present information in this order:

1. Current understanding and desired outcome
2. Recommended direction and why it fits
3. The important assumption, tradeoff, or choice
4. Deeper alternatives, architecture, implementation, and verification when requested or needed for the current judgment

Offer the next layer rather than supplying it automatically. Never hide an important risk to keep a response short.

## Questions And Convergence

- Ask only when the answer could materially change the outcome, boundary, risk, reversibility, or the authority to proceed.
- Use outcome language with users who should not have to design the implementation.
- Preserve detailed constraints from experienced users. Challenge only important contradictions, hidden costs, or risks.
- Use visible, reversible assumptions when safe progress is possible.
- Stop exploring once the direction is ready for a decision at the level of detail the work needs.
- At convergence, summarize what is accepted, what stays deliberately open, and the smallest safe next step.
- For each accepted outcome, identify the question it answers, where it applies, and how long it should last. Put it in the source that answers that question. Related outcomes may need several linked sources. Acceptance alone does not make an outcome durable or reusable. Ask the user only when its meaning or placement stays unclear enough to change the result.

## Independent Review

Consider a fresh review after broad, important, hard-to-reverse work, or after changes that span several durable knowledge roles. Offer it only when a new perspective could catch omissions, over-promotion, duplication, contradiction, or risk.

When an independent agent is available and the review is likely worth its cost, tell the user what it would check and that it spends additional model tokens. Ask before running it unless standing direction already covers the expense.

When context isolation is available, start the reviewer without the implementation discussion. Give it the accepted goal, the rules, the workspace, and the resulting changes, and require it to find the relevant sources itself. Without isolation, call the check an adversarial second pass rather than an independent review. Keep either one read-only unless changes are separately authorized.

## Tradeoffs

Progressive disclosure can leave out detail the user would have valued. Keep the deeper reasoning available and provide it when uncertainty, consequence, or explicit interest justifies the cost.

A strong recommendation can anchor the conversation too early. Offer meaningfully different directions when the desired outcome is still unclear, but avoid an unbounded catalogue of options.

Independent review costs tokens and can produce false positives when context is missing. Offer it only when the likely value justifies the cost, and check its findings against accepted sources.
```

---

## `.agents/maps/_maps.md`

```markdown
---
open-forge:
  description: Concise maps to important local and external sources and when to use them
  tags: [LoadNow, Core, Map]
---

# Maps

## Where is a useful local or external source, and when should it be used?

A Map points to an important local or external source and says when it matters. The destination keeps its own detail and authority.

## Axioms

- Check `Entries` when the work needs an important local or external source.
- Each Map links to one or more destinations and says what they contain and when they matter.
- Local links are relative to the Map file. External sources use their normal URL.
- Keep Maps broad: modules, projects, repositories, systems, or scopes rather than members and functions, unless finer routing clearly helps.
- Each workspace chooses its own Map filenames, grouping, and depth.
- Maps do not replace Memory or the destination's current information.

## Entries

- none - No entries - #Empty
```

---

## `.agents/patterns/_patterns.md`

```markdown
---
open-forge:
  description: Reusable default shapes for code, files, APIs, documents, and other work
  tags: [LoadNow, Core, Pattern]
---

# Patterns

## What reusable shape makes related work easy to create and inspect?

A Pattern is the default shape for related work: code, files, naming, placement, boundaries, an API, a document, or another inspectable result. It keeps such work consistent and easy to inspect.

## Axioms

- Check `Entries` when the work creates, changes, or reviews something with a visible structure.
- Create or update a Pattern only when an accepted shape should guide future related work. One-off work, a temporary transition, or an unsettled candidate is not a Pattern only because it has structure.
- Keep each Pattern to one shape. Examples must be valid for the APIs, formats, and tools they use. Label an intentionally incomplete example as schematic.
- Treat an applicable Pattern as the default shape in its scope. Depart from it only for a deliberate reason, and explain the departure before work depends on it.
- An exception to an agreed shape needs existing authority. When that authority is missing, ask the user first.

## Entries

- none - No entries - #Empty
```

---

## `.agents/skills/_skills.md`

```markdown
---
open-forge:
  description: Specialized capabilities provided through native SKILL.md packages
  tags: [LoadNow, Core, Skill]
---

# Skills

## Which specialized capability would help with this work?

Skills are specialized capabilities packaged as native `SKILL.md` files. Each Skill follows the rules in its own `SKILL.md`.

## Axioms

- Check `Entries` when a Skill might help.
- Follow the selected `SKILL.md` for its metadata, use, instructions, and resource loading.
- The active agent runtime controls how Skills are activated, invoked, installed, and run.

## Entries

- none - No entries - #Empty
```

---

## `.agents/templates/_templates.md`

```markdown
---
open-forge:
  description: Copy-ready files for starting independently maintained workspace content
  tags: [Core, Template]
---

# Templates

## What starting content can be copied, adapted, and maintained independently?

Templates are copy-ready starting files. Copy one, adapt it, and maintain the result independently. The Template does not follow it.

## Axioms

- Check `Entries` when copy-ready starting content would help with a new artifact.
- Choose the most relevant Template. Copy and adapt only what the destination needs.
- Replace the metadata and placeholders so they describe the destination's ownership, scope, state, authority, and relationships.
- The result is independent. Later changes to the Template do not reach it.
- If the result needs continuing guidance or requirements, link the matching #Core route. A Template gives starting content only.
- State in each Template's description and instructions the need and primary question it answers.
- Generic Templates are fallbacks. Add a specialized one only when its starting content differs meaningfully.
- Users may edit, replace, scope, or remove Templates. Removed defaults stay removed unless the user asks to restore them.

## Entries

- none - No entries - #Empty
```

---

## `.agents/workflows/_workflows.md`

```markdown
---
open-forge:
  description: Repeatable Markdown recipes for reaching a defined goal
  tags: [LoadNow, Core, Workflow]
---

# Workflows

## What repeatable method can help reach this goal?

Workflows are optional Markdown recipes for reaching defined goals. Work directly when none adds value.

## Axioms

- Check `Entries` when the user selects a Workflow, or when a description shows the recipe would clearly help the goal.
- Choose by description, tags, route meaning, and user direction. After loading, check `Goal` to confirm the fit. Honor an explicit choice or opt-out.
- Choose the smallest Workflow that resolves an important missing decision or execution risk. Installed Workflows are never mandatory stages, and users never have to name them.
- When a Workflow noticeably changes the interaction, say in one natural sentence which approach is in use and why. Route details are optional.
- Create a Workflow only when repeating its recipe clearly changes execution, preserves a deliberate user method, or improves reliability beyond normal agent behavior.
- Every Workflow file listed beside its `entrypoint` is a complete recipe: one non-empty section for each of `## Goal`, `## Steps`, and `## Completion`, in that order. An entrypoint may omit them only when it organizes descendants. Any file that uses one of these sections must have all three.
- Recipe-specific headings may add context without joining the Framework schema.
- Steps may link to sources, invoke capabilities, delegate bounded work, repeat on evidence, or hand off to another Workflow. State those relationships directly.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/_memory.md`

```markdown
---
open-forge:
  description: Self-growing Markdown memory for active work, coordination, accepted knowledge, candidates, and history
  tags: [LoadNow, Memory, OrganicGrowth]
---

# Memory

## What is worth remembering for current or future work?

Memory is self-growing Markdown state that evolves with the work. It holds records for active work, coordination, accepted knowledge, candidates, and history. People and agents add records and routed scopes deliberately, without a fixed structural limit. Unselected branches stay outside the active context.

## Axioms

### Authority And Classification

- Each record answers its own question within its scope. An accepted record can define current knowledge or a decision. Recording another category's content does not give the record that category's role.
- Treat clear user direction as accepted within its scope. Keep anything tentative, exploratory, inferred, or materially unclear in a #Contextual route until it is accepted.

### Durable Outcomes

- Save agent communication and coordination only when they must outlive the current context. Do not save every conversation.
- Before creating a durable record from accepted direction, ask:
  - What must survive the task?
  - Does it change accepted current meaning?
  - Will its reasoning matter later?
  - Does it define mandatory behavior?
  - Should an inspectable shape guide future related results?
- If none applies, create nothing.
- Show the source and uncertainty when they affect trust or later acceptance. Say whether something was stated, observed, or inferred.
- Integrate each useful durable outcome before other work depends on it and before closeout. Do not leave it only in chat.
- When accepted direction changes durable meaning, update each affected source for the part it defines. Keep useful reasoning in Memory and link related sources.
- Before closeout, account for accepted durable outcomes, unsettled reusable findings, and temporary state needed to continue or transfer.
- Accepted durable meaning must not live only in a #Contextual route. An accepted temporary choice may stay contextual when its source, scope, and expected expiration are clear.

### Placement And Lifecycle

- Add records and routed scopes when their likely future value justifies the cost of finding and reviewing them.
- Move material when its state, scope, or intended use changes. The destination's rules take over. Leave no stale or competing copy.
- Accepted behavior that should guide future work goes in the matching #Core route. Memory keeps the context and reasoning.
- If material fits no current Memory route, propose a clearer route before writing it.
- Get explicit user direction before creating a new top-level Memory state.

## Entries

- [Useful history that no longer controls current work](archived/_archived.md) - #Memory #Archived #Contextual #Historical
- [Accepted knowledge that should remain current](crystallized/_crystallized.md) - #LoadNow #Memory #Crystallized #CurrentTruth
- [Useful material that is not accepted yet](emerging/_emerging.md) - #KeepInMind #Memory #Emerging #OrganicGrowth #Contextual #Candidate
- [Temporary memory that helps agents continue or resume active work](working/_working.md) - #LoadNow #Memory #Working #Contextual
```

---

## `.agents/memory/working/_working.md`

```markdown
---
open-forge:
  description: Temporary memory that helps agents continue or resume active work
  tags: [LoadNow, Memory, Working, Contextual]
---

# Working Memory

## What temporary context is needed to continue or resume this work?

Working Memory is temporary context for continuing or resuming active work.

## Axioms

- Check `Entries` before deciding that no working memory applies.
- Saving something here helps work resume. It does not make the material accepted. Working Memory may hold an explicitly accepted temporary choice when its source, scope, and expected expiration are clear.
- Keep Working Memory small, current, and easy to replace.
- When the active need ends, keep any useful result, then move, archive, consolidate, or prune the record.

## Entries

- [Current state, current step, and next steps for one active workstream](checkpoints/_checkpoints.md) - #LoadNow #Memory #Working #Checkpoint #Contextual
- [Sealed transfer snapshots that preserve one boundary for resumption](handoffs/_handoffs.md) - #LoadNow #Memory #Handoff #AgentCommunication #Contextual
```

---

## `.agents/memory/working/checkpoints/_checkpoints.md`

```markdown
---
open-forge:
  description: Current state, current step, and next steps for one active workstream
  tags: [LoadNow, Memory, Working, Checkpoint, Contextual]
---

# Checkpoints

## Where does this workstream stand, and what comes next?

A Checkpoint records the current state, current step, and next steps for one active workstream. Update it as the work changes. It is neither history nor accepted truth.

## Axioms

- Keep one Checkpoint per active workstream when durable sources alone are not enough to resume after a pause, context restoration, or transfer.
- Tag it #Active and #KeepInMind only while it is active.
- Record only what helps the work resume:
  - The current goal, state, and step.
  - Accepted decisions and evidence.
  - Unresolved questions and next steps.
  - Links to durable sources.
- Refresh it after an important state change and after context restoration.
- Update it before an actual transfer or explicitly planned resumption. If that boundary needs a snapshot that will not change, create a Handoff. Routine pauses and ordinary closeout do not need one.
- At closeout, save durable outcomes outside the Checkpoint. When its active need ends, remove #Active and #KeepInMind, then archive or prune it.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/working/handoffs/_handoffs.md`

```markdown
---
open-forge:
  description: Sealed transfer snapshots that preserve one boundary for resumption
  tags: [LoadNow, Memory, Handoff, AgentCommunication, Contextual]
---

# Handoffs

## What state must survive this transfer or planned resumption?

A Handoff is a sealed snapshot for another reader. It preserves the state needed to resume after an actual transfer or an explicitly planned resumption across a context boundary.

## Axioms

- Check `Entries` when work is resumed, transferred, delegated, interrupted, or reviewed after a context break.
- Handoffs are transfer notes, not complete history or accepted truth.
- Create and seal a Handoff when an actual transfer or explicitly planned resumption needs a stable snapshot while the Checkpoint keeps changing. Do not create one for a routine pause, ordinary closeout, or a possible future interruption.
- Make the intended reader or resumed work clear from the route, description, or content.
- Keep it short. Link to current state, durable sources, and code instead of copying them.
- Record boundary status, next action, blockers, and verification state in the Handoff itself. A live Checkpoint may supplement it but not replace it.
- Do not change a sealed Handoff. Record later state in the Checkpoint or a new Handoff.
- When it no longer supports an active transfer, keep any useful result and archive it.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/emerging/_emerging.md`

```markdown
---
open-forge:
  description: Useful material that is not accepted yet
  tags: [KeepInMind, Memory, Emerging, OrganicGrowth, Contextual, Candidate]
---

# Emerging Memory

## What is useful but still unsettled?

Emerging Memory keeps useful findings, possibilities, and reasoning that are not accepted yet.

## Axioms

- At each applicable #KeepInMind refresh, read the visible `Entries` and save useful candidate material before it is lost. "Nothing worth saving" is a valid result. Do not turn raw activity into Memory.
- Treat everything here as contextual until it is accepted within its scope. Keep uncertainty, source, and scope visible.
- Refine material here while its validity, use, or destination is still open.
- When material is accepted, move the durable result to the source that should define it. Preserve useful outcomes and reasoning before archiving or pruning what was rejected or replaced.
- Revisit repeated or stale material and consolidate, promote, archive, or prune it.

## Entries

- [Structured reasoning, investigation, or comparison that is useful but not accepted truth](analysis/_analysis.md) - #LoadNow #Memory #Analysis #Reasoning #Contextual #Candidate
- [Future possibilities, experiments, open questions, and options to explore later](ideas/_ideas.md) - #LoadNow #Memory #Idea #Exploration #OrganicGrowth #Contextual #Candidate
- [Concrete occurrences or patterns noticed in evidence that may become reusable learning](observations/_observations.md) - #KeepInMind #Memory #Observation #AgentLearning #OrganicGrowth #Contextual #Candidate
```

---

## `.agents/memory/emerging/analysis/_analysis.md`

```markdown
---
open-forge:
  description: Structured reasoning, investigation, or comparison that is useful but not accepted truth
  tags: [LoadNow, Memory, Analysis, Reasoning, Contextual, Candidate]
---

# Analysis

## What does the available evidence support, and what remains uncertain?

Analysis is structured reasoning, investigation, or comparison that is useful but not accepted. It keeps the evidence, assumptions, limits, and current conclusion visible.

## Axioms

- Check `Entries` when current work needs prior reasoning.
- Keep the question, evidence, assumptions, limits, and current conclusion clear.
- Check that the assumptions still hold before relying on an Analysis.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/emerging/ideas/_ideas.md`

```markdown
---
open-forge:
  description: Future possibilities, experiments, open questions, and options to explore later
  tags: [LoadNow, Memory, Idea, Exploration, OrganicGrowth, Contextual, Candidate]
---

# Ideas

## What possibility is worth exploring?

Ideas keep possibilities, experiments, open questions, and options worth exploring later.

## Axioms

- Check `Entries` when the work explores possibilities, plans future work, revisits postponed options, or needs earlier exploration.
- Record a requested idea or exploration here without treating it as accepted.
- Keep the problem, opportunity, or motivation visible enough to pick up later.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/emerging/observations/_observations.md`

```markdown
---
open-forge:
  description: Concrete occurrences or patterns noticed in evidence that may become reusable learning
  tags: [KeepInMind, Memory, Observation, AgentLearning, OrganicGrowth, Contextual, Candidate]
---

# Observations

## What was observed, and why might it matter later?

An Observation is a concrete occurrence or pattern noticed in evidence that may matter after the current work: a surprising result, a recurring failure, or a detail the next session would otherwise rediscover.

## Axioms

- Before handoff or closeout, record an Observation when a concrete occurrence or pattern may matter later. If the write is blocked, report it.
- One occurrence is enough when it may be reusable, surprising, or costly to rediscover.
- Keep the evidence and details clear enough to verify and reuse.
- Add later matching occurrences to the same Observation when scope and meaning align. Recurrence strengthens the case for consolidation or promotion. It does not by itself validate or accept the Observation.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/crystallized/_crystallized.md`

```markdown
---
open-forge:
  description: Accepted knowledge that should remain current
  tags: [LoadNow, Memory, Crystallized, CurrentTruth]
---

# Crystallized Memory

## What accepted knowledge should remain current within this scope?

Crystallized Memory holds accepted knowledge that should remain current within its scope.

## Axioms

- Check `Entries` before deciding that no crystallized memory applies.
- Treat material as Crystallized only when it is accepted by clear user direction, delegated authority, a requested action that clearly requires the choice, or a declared external authority. Tags, repetition, and agent confidence accept nothing.
- Update, split, merge, or reshape existing Crystallized Memory. Never create a competing current version.
- Archive or link Crystallized material that is no longer current. Keep enough context to understand the change.

## Entries

- [What was chosen, why, and what follows from the choice](decisions/_decisions.md) - #LoadNow #Memory #Decision #Rationale #CurrentTruth
- [Complete current explanations of accepted project knowledge](documents/_documents.md) - #LoadNow #Memory #Document #Record #CurrentTruth
```

---

## `.agents/memory/crystallized/decisions/_decisions.md`

```markdown
---
open-forge:
  description: What was chosen, why, and what follows from the choice
  tags: [LoadNow, Memory, Decision, Rationale, CurrentTruth]
---

# Decisions

## What was chosen, why, and what follows from the choice?

A Decision records an important accepted choice and why it was made. It may also keep consequences and other context that help future work.

## Axioms

- Check `Entries` when the work needs the reason behind an important choice.
- Record what was chosen and why. Link to the source that defines the current result when one exists.
- Keep alternatives, tradeoffs, constraints, and consequences only when they help future work.
- One Decision covers one choice or a tightly related group of choices. Split unrelated choices and consolidate compatible overlap. Exact current specifications stay in the sources that define them.
- Consolidate, reshape, or link overlapping Decisions when their reasoning agrees. Archive or link the reasoning behind a replaced choice. Surface important disagreement instead of merging it silently.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/crystallized/documents/_documents.md`

```markdown
---
open-forge:
  description: Complete current explanations of accepted project knowledge
  tags: [LoadNow, Memory, Document, Record, CurrentTruth]
---

# Documents

## What is the complete current explanation of this subject?

A Document explains accepted current knowledge as one coherent view, so a reader can use it without reconstructing it from Decisions.

## Axioms

- Check `Entries` when the work needs accepted current knowledge.
- Explain current meaning completely. Link to a Decision when its reasoning helps.
- When a Document names another source as authoritative, follow that source for the detail it defines. Whatever a Document points to still defines its own detail.

## Entries

- none - No entries - #Empty
```

---

## `.agents/memory/archived/_archived.md`

```markdown
---
open-forge:
  description: Useful history that no longer controls current work
  tags: [Memory, Archived, Contextual, Historical]
---

# Archived Memory

## What useful history should remain available without governing current work?

Archived Memory keeps useful history that does not govern current work.

## Axioms

- Check `Entries` when current work may need archived context.
- Before archiving, move anything still current into the source that defines it. Keep the rest when its history is useful, or link to it when it belongs elsewhere.
- Preserve where the material came from, why it was archived, and what replaced it.
- Archived material follows its destination's rules. Remove metadata that would keep its former behavioral role or claim current authority.
- Keep, consolidate, or transform history by its future value. Archiving need not keep every detail.
- Delete retained material only under user direction or accepted retention preferences.
- Before restoring, validate against current conditions and move to an explicit destination. Establish acceptance before treating it as current.

## Entries

- none - No entries - #Empty
```
