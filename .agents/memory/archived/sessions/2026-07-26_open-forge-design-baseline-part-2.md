---
open-forge:
  description: Historical continuation of the approved Open Forge design baseline covering knowledge roles, principles, templates, decisions, terminology, and README direction
  tags: [Memory, Archived, Session, WorkHistory, Contextual, Historical, Framework, Documentation, Principle, Template, Decision, Terminology, Migration]
---

# Open Forge Design Baseline, Part 2

This session continues the [first Open Forge Design Baseline](2026-07-26_open-forge-design-baseline.md). It preserves accepted reasoning developed while the Vision, top Architecture, Framework Architecture, scoped MVP architectures, document templates, and knowledge roles were reviewed.

It is historical design context, not a competing current authority. The linked current documents, Core routes, and decisions own the accepted state.

## Document Responsibilities

Open Forge should provide useful generic document shapes without implying that every subject needs every document.

The accepted distinctions are:

| Document | Primary question |
|---|---|
| Vision | What should become true, why does it matter, and what would success mean? |
| Principles | Which stable identity-level filters guide unfamiliar choices and resolve tensions? |
| Operating Context | Which external realities, stakeholders, assumptions, constraints, and dependencies shape the subject? |
| Strategy | How will accepted focus move the subject toward its Vision? |
| Architecture | How is the accepted system structured, how do its parts relate, and which boundaries govern it? |
| Roadmap | Which accepted outcomes come next, in what order, and what may change that order? |
| Maintenance Contract | What must a governed source preserve when it changes, and how is that verified? |
| Project Status | Where does active work stand, what matters now, and how can it resume? |
| Decision | What discrete choice was accepted, why, and with which consequences? |

These are optional owners earned by distinct knowledge. They are not a mandatory document suite or a disguised lifecycle.

The repository dogfoods Vision, Principles, Architecture, scoped architecture, Maintenance Contracts, Decisions, and Working Memory because current Open Forge knowledge has earned those owners. It does not create Operating Context, Strategy, Roadmap, or Project Status documents merely to fill the template catalogue.

## Principles, Foundation, Identity, And Essence

`Principles` is the formal current-document role.

`Foundation` describes how load-bearing a principle is. Changing a foundational principle redefines the subject rather than merely revising one implementation.

`Identity` describes what the complete foundational set preserves. It is the outcome of the principles, not another document role.

`Essence` is useful summary language. It normally belongs in a description or opening paragraph rather than a separate `essence.md` owner.

A scope earns its own Principles document only when several recurring unfamiliar decisions depend on stable filters specific to that scope and not already answered by broader principles. Scoped principles supplement broader principles; they do not silently override them.

The Open Forge Framework has not yet earned `framework/principles.md`. Its present structural invariants are consequences of the global [Open Forge Principles](../../crystallized/documents/principles.md), while the [Framework Architecture](../../crystallized/documents/framework/architecture.md) owns their exact structural expression.

## Decisions And Current State

A current document states what is accepted now and explains the coherent concept well enough to use.

A decision states what was chosen and why. It preserves useful alternatives, tradeoffs, consequences, and rationale without becoming a fragmented substitute for current documentation.

The relationship is:

1. Existing current state frames a consequential question
2. A decision preserves the accepted change and its rationale
3. Every affected current document, Core route, implementation, or external system integrates the accepted result
4. Current state links to the decision when its rationale remains useful
5. The decision links forward to the places that now express its result

These links are bidirectional without creating circular authority. Chronology explains how the files evolved. Each knowledge role determines what its file governs now.

The route identifies a Memory record's role while the filename identifies its subject. For example, `decisions/core-primitives.md` is rationale about Core primitive roles. It should not be renamed to `core-principles.md`, because that would hide its Decision role.

Decision approval and supersession timestamps remain a useful candidate rather than an accepted generic field. Partial supersession, archival behavior, timezone precision, and replacement relationships must be resolved before adding lifecycle metadata to every decision.

## Core Primitive Admission

Core uses several semantically distinct reusable content roles rather than one generic knowledge bucket.

A new primitive must:

- Answer a different primary question
- Carry authority or lifecycle meaning that existing primitives cannot express clearly
- Provide reusable value beyond one subject or artifact
- Justify another routing and maintenance surface

Framework contracts refer to #Core collectively when any suitable Core route can satisfy the requirement. They name a primitive when its distinct semantics matter and enumerate concrete routes only when the shipped defaults themselves are the subject.

The Core primitive decision owns this admission model. The Framework Architecture and installed entrypoints own the current catalogue and exact definitions.

## Templates

Templates earned a distinct Core primitive because their lifecycle is the reverse of a continuing Pattern.

- A Template provides copy-ready starting content
- Instantiation transfers ownership to the destination
- Later Template changes do not update the result
- A Pattern continues to guide related results
- A Directive or Axiom binds continuing behavior

Generic Templates are fallbacks, not universal schemas. A specialization earns a separate file only when it provides materially different copy-ready contents.

Users may edit, scope, replace, or remove Templates. Normal updates must not silently restore removed defaults. A future update workflow may show distribution changes, exact diffs, and choices to apply all, select changes, preserve local content, or cancel. Updating a Template never implies updating artifacts previously created from it.

The repository dogfoods document and Memory Templates locally. The installable Framework currently ships the Templates category contract without promoting every candidate starter artifact into the shared payload.

## Template Catalogue

The current generic document candidates are:

- Vision
- Principles
- Operating Context
- Strategy
- Architecture
- Roadmap
- Maintenance Contract

The current Memory candidates are:

- Decision
- Idea
- Analysis
- Observation
- Handoff
- Project Status

Observation, Analysis, and Handoff earned distinct templates through repeated local use and semantics that could not be represented cleanly by Idea or Project Status.

Session remains flexible chronological capture. Specification, Risk, Research, README, and similar domain-sensitive artifacts remain deferred until repeated use reveals stable generic copy-ready contents. Extensions may provide specialized Templates without expanding the common catalogue.

## Primary Questions And Descriptions

Primary questions are authoring and migration diagnostics. They help decide whether knowledge has a distinct owner and which role fits it.

They are not currently a required frontmatter property. A separate field would duplicate the route description without an independent consumer.

Runtime descriptions remain natural, descriptive, and suggestive. They should not all begin with a formula such as "This document is used when." Template descriptions may explicitly use "template is used when" because their job is selection before instantiation.

If a future CLI or retrieval tool needs primary questions as independently machine-readable data, that consumer must justify the additional contract.

## User, Operator, Maintainer, And Agent

The accepted role distinctions are:

- `user` owns, adopts, installs, customizes, or consumes an Open Forge environment
- `operator` establishes goals, priorities, consequential tradeoffs, and accepted direction during active work
- `maintainer` changes the Open Forge distribution, repository contracts, source payload, tooling, tests, or release surfaces
- `agent` investigates, suggests, challenges, executes, verifies, and preserves context within applicable direction
- `contributor` changes a shared project when product ownership or active operating authority is not the point
- `person` or `people` is used when no Open Forge-specific responsibility needs distinction

The same person may occupy several roles.

`Current owner` remains understandable inside the design discussion but may be too opaque or socially loaded for public language. The intended meaning is the file, route, system, person, or group that authoritatively expresses a subject now.

Candidate replacements to evaluate before a repository-wide scrub:

| Candidate | Strength | Limitation |
|---|---|---|
| Authoritative source | Explicit, neutral, works for files and external systems | May sound like supporting evidence rather than the place expressing accepted state |
| Canonical source | Familiar in technical documentation | Can imply centralization or immutability |
| Source of record | Familiar in records management | Less natural for directives, code, or active systems |
| Authoritative home | Makes placement intuitive | Informal and awkward for people or external systems |
| Current authority | Emphasizes governing meaning rather than possession | Can sound like a person or institution |
| Semantic authority | Precise about meaning | More technical and less friendly |

Subsequent maintainer review accepted typed vocabulary rather than one mechanical replacement. Current documentation uses `authoritative source` when the type is unknown or irrelevant; `authoritative document`, `authoritative route`, or `authoritative system` when the type is known; and `responsible person` or `responsible role` for human accountability. Ownership language remains valid for possession and managed lifecycle. The [terminology decision](../../crystallized/decisions/authoritative-source-terminology.md) preserves the accepted rationale.

## Tooling Direction

The CLI remains optional and deterministic. It accelerates the human-readable contract without becoming its private authority.

Accepted candidates for the CLI overhaul include:

- `open-forge help syntax` for canonical Markdown authored or interpreted by Open Forge
- `open-forge help roles` for document, Memory, and Core role selection
- Placement and scaffolding assistance derived from human-readable Framework definitions
- Previewable install and update plans with explicit preservation choices

Role help should make selection feel automatic. Users should not need to memorize a governance table before ordinary use.

The temporary repository owner and terminology helpers exist to support migration. Their successful outcome is removal after current files, public documentation, and tooling make the distinctions obvious.

## README Direction

The public README should:

1. Define Adaptive Context Engineering
2. State the exact accepted Open Forge vision without weakening it
3. Explain the problem and differentiated approach
4. Provide the fastest safe useful start
5. Explain advantages, operating flow, and concrete examples
6. Introduce recursive scopes, customization, Core, Memory, Extensions, and deterministic tools progressively
7. Link detailed CLI, Extensions, and maintainer documentation instead of duplicating it
8. State current maturity honestly
9. List only confirmed users or dogfood usage

The former README remains valuable implementation history but leads with the superseded plain-file substrate framing and spends too much public surface on detailed MVP mechanics. It should be archived when the ACE README replaces it.

## Current References

- [Open Forge Vision](../../crystallized/documents/vision.md)
- [Open Forge Principles](../../crystallized/documents/principles.md)
- [Top Open Forge Architecture](../../crystallized/documents/architecture.md)
- [Framework Architecture](../../crystallized/documents/framework/architecture.md)
- [Distinct Core Primitive Roles decision](../../crystallized/decisions/core-primitives.md)
- [Templates As A Core Primitive decision](../../crystallized/decisions/template-primitive.md)
- [Knowledge Role Helper](../../crystallized/documents/maintenance/helpers/knowledge-roles.md)
- [Terminology Helper](../../crystallized/documents/maintenance/helpers/terminology.md)
