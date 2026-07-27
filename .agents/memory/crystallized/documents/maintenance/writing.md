---
open-forge:
  description: Current standard for clear, direct, consistent, and reviewable Open Forge-authored prose
  responsibility: Define how explanatory Open Forge prose should sound and remain understandable across public, installed, and maintainer documents
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Writing, Documentation, Voice, Clarity]
---

# Open Forge Writing Standard

## Scope

This document is authoritative for explanatory prose authored by Open Forge across public documentation, installed Framework files, current documents, maintenance contracts, CLI help, and first-party package documentation.

It does not redefine canonical Markdown notation, component semantics, code identifiers, external formats, historical quotations, or archived wording. The [Canonical Markdown Syntax](../framework/markdown/syntax.md) defines notation. The authoritative source for each component defines what its prose must mean.

## Voice

Open Forge sounds:

- Direct and engineering-minded
- Confident in accepted direction without pretending uncertainty or tradeoffs do not exist
- Professional and conversational rather than corporate or academic
- Warm enough to feel written for a person
- Lightly funny only when humor makes the idea clearer

State the actual vision, contract, or conclusion first. Do not replace a precise idea with a weaker simplified tagline because it seems easier to market.

Prefer the fewest words that make the meaning aggressively clear, not the fewest words possible.

## Clarity And Structure

- Lead with the rule, result, or relationship the reader needs
- Use complete natural sentences rather than compressed newspaper-style fragments
- Explain an unfamiliar Open Forge term at first meaningful use or link directly to its definition
- Add a concrete example when an abstract mechanism remains difficult to understand without prior context
- Keep one responsibility per authoritative source and link to related responsibilities instead of blending them together
- Repeat meaning only at a genuine independent entry boundary, such as the exact product summary in Vision and README
- Describe the positive current contract first. Use negative constraints when they close a concrete ambiguity, safety risk, or rejected alternative
- State material tradeoffs directly instead of hiding them behind generic words such as flexible, robust, or scalable
- Use short headings and compact paragraphs, but do not turn brevity into fragments or remove the explanation required to act correctly

## Actors And Relationships

State relationships directly.

- Use `user` when the person directing or accepting work must be named
- Omit the actor when the instruction or relationship is already clear, such as `Update the affected current document`
- Use `agent` only when the agent is the meaningful actor
- Avoid `operator` unless a technical interface defines that exact role
- Name the authoritative document, route, system, person, or role instead of using an unexplained `owner`
- Reserve ownership language for possession or managed lifecycle

The [typed authority terminology decision](../../decisions/authoritative-source-terminology.md) preserves the rationale for distinguishing semantic authority from ownership.

## Open Forge Terms

- Use `Core`, `Memory`, `Framework`, and `Extensions` for their defined Open Forge meanings
- Use `standard` for the default routes or configuration Open Forge provides
- Use ordinary lowercase words when no defined Open Forge concept is intended
- Keep route descriptions natural, descriptive, and suggestive rather than starting every entry with the same formula
- Use optional frontmatter `responsibility` only when a stable edit boundary adds information beyond the route and description
- Prefer established terms and links over introducing a synonym for variety

## Sentences And Punctuation

- Prefer active voice and concrete verbs
- Use commas, parentheses, colons, semicolons, or separate sentences instead of em dashes
- Omit terminal periods from isolated one-sentence definitions and one-sentence list items
- Keep normal punctuation when a paragraph or list item contains several sentences
- Avoid stacked parenthetical qualifications when separate sentences would expose the relationship more clearly
- Do not hedge accepted current state with `maybe`, `probably`, or `generally`; preserve those words only when uncertainty is real and useful
- Use backticks and links according to the canonical Markdown contract rather than as visual decoration

## Examples

### Complete Sentence

Less clear:

```text
Small, file-native, recursively adaptable.
```

Clearer:

```text
Open Forge starts with a small file-native Framework that a workspace can adapt recursively.
```

The complete sentence identifies the subject, action, and practical meaning.

### Direct Relationship

Less clear:

```text
The operator should update the owner when direction changes.
```

Clearer:

```text
Update the affected current document when accepted direction changes.
```

The clearer version removes two roles the reader would otherwise have to interpret.

### Typed Authority

Less clear:

```text
This file owns architecture.
```

Clearer:

```text
The Framework Architecture is authoritative for Core and Memory internals.
```

The clearer version names both the source type and the question it answers.

### Concrete Mechanism

Less clear:

```text
Memory supports recursive scope.
```

Clearer:

```text
Place a scope after a Memory state when it applies only to that state. Place it before the state routes when the subject needs its own Memory states.
```

The example makes an abstract capability usable without requiring prior Framework knowledge.

### Positive Boundary

Less clear:

```text
Extensions must not inject blocks, create hidden state, redefine routes, or add another runtime.
```

Clearer:

```text
Extensions add complete files through ordinary Framework routes. Installed files retain the meaning of their destination routes.
```

The positive form defines the valid model. A separate negative sentence is still appropriate when it closes a specific risk not excluded by that model.

## Review Checks

- Can a reader unfamiliar with the current conversation understand the changed passage?
- Does the passage state its main rule or conclusion before qualifications?
- Are specialized terms defined or linked where first needed?
- Does every abstraction that needs prior context have a useful example?
- Are authority, ownership, scope, and actor relationships explicit?
- Could a link replace repeated detail without making an independent entry point incomplete?
- Does the prose sound direct, natural, precise, and honest?
- Are fragments, generic claims, unnecessary role nouns, unexplained synonyms, and em dashes absent?

## Related Current Sources

- [Canonical Markdown Syntax](../framework/markdown/syntax.md)
- [Routed Markdown Representation](../framework/markdown/routes.md)
- [Open Forge Principles](../principles.md)
- [README-specific documentation voice](../../../../../docs/dev.md#documentation-voice)

## Decisions And Rationale

- [User-facing writing](../../decisions/user-facing-writing.md)
- [Typed authority terminology](../../decisions/authoritative-source-terminology.md)
- [Canonical Markdown authoring](../../decisions/canonical-markdown.md)
