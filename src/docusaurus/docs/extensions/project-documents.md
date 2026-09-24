---
title: Project Documents
description: Vision and Architecture workflows, a Documents Memory category, and five document Templates.
---

# Project Documents

Keep understandable, current project knowledge without duplicating it. Keep documentation where your project already has it, or use the supplied Documents convention.

- **Package ID:** `project-documents`
- **Depends on:** [Workflow Support](workflows.md)
- **Loads at startup:** Only the Documents entry line, through Crystallized Memory. Documents themselves load when relevant.

## What it installs

```text
.agents/
  memory/crystallized/documents/
    _documents.md                  <- Memory category: current explanations
  skills/use-workflow/references/project-documents/
    _project-documents.md          <- recipe scope entrypoint
    vision.md                      <- workflow recipe
    architecture.md                <- workflow recipe
  templates/documents/
    _documents.md                  <- Template category entrypoint
    document.md  vision.md  architecture.md
    principles.md  maintenance-contract.md
```

## What each file is for

### `memory/crystallized/documents/_documents.md`

**Kind:** Memory category (Crystallized). **Used when:** work needs accepted current knowledge.

A Document is one coherent, current explanation of a subject, written so readers don't have to piece its meaning together from Decisions. When a Document names another source as authoritative, it follows that source instead of duplicating it.

### The Vision recipe: `vision.md`

**Kind:** workflow recipe. **Used when:** clarifying what something is for.

**Goal:** a proposed or accepted Vision covering purpose, audience, core value, first useful version, boundaries, and observable success. The agent starts from what's already known, resolves only the choices that would change the direction, challenges it against audience fit and risk, and states clearly whether the result is proposed, accepted, revised, deferred, or rejected.

### The Architecture recipe: `architecture.md`

**Kind:** workflow recipe. **Used when:** defining or reviewing structure.

**Goal:** an accepted or decision-ready structure future work can adopt and verify. The agent maps the relevant system, finds the qualities and constraints that actually drive the decision, compares viable directions, defines the smallest coherent structure, and keeps a recommendation _proposed_ until you accept it. When authorized, it splits adoption into verifiable pieces.

### The Templates

| Template                  | Answers                                          | Main sections                                                                                               |
| ------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------------------- |
| `document.md`             | What is the current explanation of this subject? | Summary (scope, authority and status), Current Explanation, Boundaries And Limits, Sources                  |
| `vision.md`               | Why does this exist, and for whom?               | Vision, Why And For Whom, Scope (first useful version, growth direction), Success, Non-Goals                |
| `architecture.md`         | How is it structured?                            | Overview And Scope, Drivers, System Model, Important Flows, Boundaries And Invariants, Tradeoffs And Limits |
| `principles.md`           | What guides unfamiliar choices?                  | Principles (each with why, in practice, tradeoff), Tensions And Ordering, Change Boundary                   |
| `maintenance-contract.md` | What must stay true about a maintained source?   | Source (what it owns, related surfaces), Contract, Verification                                             |

## How to use it

> Use the architecture workflow to review this design. Keep proposed changes distinct from the current architecture and update the existing source when accepted.

> Draft a vision for the reporting feature. Keep it proposed until I accept it.

## Good to know

- Start with the question. Use a general Document for a current explanation, and add specialized documents only when their questions need separate answers. This isn't a required document set.
- Drafts stay candidates until accepted.
- Existing documentation doesn't need to move into Memory.
- The package works without Planning. Link to rationale wherever your workspace already keeps it.
