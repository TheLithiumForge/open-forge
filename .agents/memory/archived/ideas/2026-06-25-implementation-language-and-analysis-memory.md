---
open-forge:
  description: Implementation wording pass, reserved terms, and possible Layer 2 analysis memory
  tags: [Implementation, Language, Terminology, Memory, Analysis]
---

# Implementation Language And Analysis Memory

## Context

The current governance descriptors are approved.

The installed implementation files still need a readability and specificity pass before Core is considered stable.

The goal is not to make the files verbose. The goal is to find the sweet spot where a human or weaker agent can read the shipped files once and understand exactly what to do without guessing.

## Implementation Wording Issues

Current implementation files use some terms too broadly.

The main offender is `category`. It sometimes means:

- a named root area such as `directives`
- a folder that has an entrypoint
- the entrypoint file itself
- the conceptual class of routed material
- a generated route in `Entries`

This can make correct axioms feel unclear.

Example issue:

```md
- Load the directives category for every request when it appears in Entries.
```

The meaning is correct, but it asks the reader to resolve `category`, `directives`, and `Entries` at once.

More natural alternatives:

```md
- If `Entries` lists `directives`, load `.agents/directives/_directives.md` before other routes.
- Load `directives` first when it appears in `Entries`.
```

Another issue:

```md
- Load a category entrypoint before its routed files.
```

This is true but abstract. It may be clearer as:

```md
- When a route points to `folder/_folder.md`, load that entrypoint before files listed inside that folder.
```

## Reserved Terms

Open Forge should define a small vocabulary and use it consistently in shipped files.

Candidate reserved terms:

- `AGENTS.md`
- `loader`
- `Entries`
- `entrypoint`
- `route`
- `routed file`
- `child route`
- `overwrite`
- `scope`
- `directives`
- `guidelines`
- `patterns`
- `skills`
- `workflows`
- `workspace`
- `memory`
- `extensions`

Reserved terms should use backticks in shipped implementation files when the term is used with framework meaning.

Governance docs should probably use the same convention, even though they are already approved, because backticks make framework terms easier to scan and harder to reinterpret.

## Possible Term Standards

Working definitions:

- `Entries`: the generated route list at the end of an entrypoint.
- `entrypoint`: the `_{folder}.md` file that defines a routed folder and lists its direct routes.
- `route`: a generated line that points to a file or child entrypoint.
- `routed file`: a markdown file listed in `Entries`.
- `child route`: a route to a nested folder entrypoint.
- `overwrite`: a `{name}.overwrite.md` companion loaded after `{name}.md`.
- `scope`: the positive context where a file applies.

Use concrete names when possible.

Prefer:

```md
Load `directives` first.
```

over:

```md
Load the directives category first.
```

Use `category` only when referring to the general mechanism.

## Implementation Pass Goals

The implementation pass should:

- remove ambiguous uses of `category`
- use concrete category names when possible
- backtick reserved framework terms
- make loading steps more procedural and natural
- keep the files short
- preserve the approved governance meaning
- preserve the generated region shape exactly
- incorporate useful user review notes without keeping inline review comments in shipped files

The pass should apply to:

- `src/open-forge/AGENTS.md`
- `src/open-forge/.agents/loader.md`
- `src/open-forge/.agents/directives/_directives.md`
- `src/open-forge/.agents/guidelines/_guidelines.md`
- `src/open-forge/.agents/patterns/_patterns.md`
- `src/open-forge/.agents/skills/_skills.md`
- `src/open-forge/.agents/workflows/_workflows.md`
- `src/open-forge/.agents/workspace/_workspace.md`

After Layer 2 Memory is defined, the implementation files need another pass because the loader and route descriptions may change when the full installable layer set is known.

## Analysis Memory

Layer 2 may need an `analysis/` folder.

Potential meaning:

`analysis/` stores structured reasoning outputs that are more substantial than an observation but not necessarily a final decision or durable document.

Examples:

- tradeoff analysis
- architecture option comparison
- product opportunity analysis
- design critique
- bug investigation summary
- root-cause analysis
- research synthesis before it becomes a document

This may be useful across personas:

- developers analyze architecture, bugs, refactors, performance, and tests
- product people analyze opportunities, requirements, metrics, and tradeoffs
- designers analyze UX options, research, accessibility, and design systems
- normal users analyze choices, plans, purchases, writing, or personal projects

## Analysis Placement Options

### Option A: First-Class Memory Category

```text
.agents/memory/
  analysis/
    _analysis.md
```

Pros:

- clear destination for deeper reasoning
- useful for many personas
- avoids bloating sessions with long reasoning
- creates a place to extract reusable conclusions from chat

Cons:

- could overlap with `documents/`
- could become a dumping ground
- may need explicit promotion rules

### Option B: Document Type

`analysis` is a document kind inside `documents/`.

Pros:

- fewer folders
- analysis often becomes a document

Cons:

- weaker routing signal
- mixes final docs with intermediate reasoning

### Option C: Session Section

Analysis stays inside sessions until promoted.

Pros:

- minimal structure
- no extra folder

Cons:

- hard to find later
- repeated analysis is trapped in long raw logs
- weaker Layer 3 output target

## Current Bias

Include `analysis/` as a Layer 2 candidate.

Updated possible Memory shape:

```text
.agents/
  memory/
    _memory.md
    active/
      _active.md
    sessions/
      _sessions.md
    ideas/
      _ideas.md
    observations/
      _observations.md
    analysis/
      _analysis.md
    decisions/
      _decisions.md
    documents/
      _documents.md
    handoffs/
      _handoffs.md
```

`analysis/` should not be active truth by default. It is structured reasoning that may later be promoted into `decisions/`, `documents/`, Core categories, or external systems.

## Open Questions

- Should `analysis/` be installed in the first Memory package or created by Layer 3 workflows when needed?
- Should `analysis/` include templates by analysis type, or should templates live only in Layer 3 Extensions?
- Should governance docs use backticks for all reserved terms now, or wait until after implementation wording stabilizes?
- Should shipped implementation files include a tiny `Terms` section, or should the loader define enough terminology for all files?
- Should `Entries` be capitalized everywhere because it is a generated section name?
