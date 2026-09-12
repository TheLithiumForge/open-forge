---
open-forge:
  description: How Open Forge maintainers write clear, consistent, and easy-to-read prose
  responsibility: Define shared clarity, structure, terminology, and review requirements, and the precise voice of rules and reference text
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Writing, Documentation, Voice, Clarity, Internal]
---

# Open Forge Writing Standard

## Scope

Use this repository-only standard when writing or reviewing Open Forge text. This includes public documentation, installable Framework source, current documents, maintenance documents, CLI text, and first-party packages. Workspaces that install Open Forge do not receive this standard.

This standard does not define Markdown syntax, component behavior, code identifiers, external formats, quotations, or archived text. [Canonical Markdown Syntax](../framework/markdown/syntax.md) defines Markdown notation. The source for each component defines what its text must mean. The root [Writing Directive](../../../../directives/public-facing-writing.md) makes this standard mandatory for repository work. Use the [Open Forge Dictionary](helpers/dictionary.md) when choosing Open Forge terms.

## Choose The Voice

Use this standard's direct, precise voice for Framework rules, contracts, technical reference text, and maintenance explanations. Use [Project Voice](project-voice.md) for READMEs and public introductions: welcoming, natural, and quietly proud of the project. Choose by the purpose of the passage, including within a single document.

Both voices follow the shared requirements below for clarity, correctness, terminology, source boundaries, and lossless rewriting. Introductions may use a more varied rhythm and sentence structure; they do not need to turn every explanation into a direct instruction.

## Values

Write for technically literate readers who may be new to Open Forge. Assume they understand basic software concepts such as files, folders, commands, and source code. Do not assume they already understand this Framework.

Make each idea easy to understand on the first read. Use the same term for the same concept. Do not change words only for variety.

Prefer familiar, precise words and direct sentences. State the rule, result, or relationship first. Add conditions and exceptions only when they matter.

Be concise, but do not remove context the reader needs. Explain defined Open Forge terms at first use, then use them consistently.

For rules and reference text, the desired voice is calm, direct, and conversational. It should not sound legal, academic, promotional, or written like a news headline.

Make the text enjoyable to read through clear explanations, natural flow, and easy navigation. Use familiar structure for similar material so readers can find what they need. Let each sentence build on what came before.

## Priorities

Apply these priorities in order. Correctness is a hard boundary: clearer wording must not change meaning.

1. **Easy to understand:** A technically literate reader can follow the text on the first read.
2. **Correct:** Meaning, scope, relationships, and whether something is required or optional remain exact.
3. **Consistent:** The same term means the same thing in the same context.
4. **Predictable:** Similar files and ideas use familiar structure and wording.
5. **Concise:** Every sentence earns its place without hiding needed context.

Shorter is not better when the reader must guess what was removed.

## Voice For Rules And Reference Text

Write like one technically literate person explaining the system to another.

Open Forge writing is:

- Direct and precise
- Confident about accepted direction
- Honest about uncertainty and tradeoffs
- Calm and conversational
- Predictable in structure and easy to navigate

State the real rule, conclusion, or relationship first. Do not replace a precise idea with a slogan.

Avoid:

- Pompous, ceremonial, or needlessly formal language
- Legal, academic, marketing, or news-style writing
- Long noun stacks that hide actions or relationships
- Sentences that combine several separate rules
- Compressed wording that removes grammar needed for understanding
- Empty claims such as `flexible`, `robust`, or `scalable` when the real tradeoff can be stated

Use the fewest words that make the meaning clear, not the fewest words possible.

## Structure And Clarity

### Order

- Lead with the purpose, rule, result, or relationship the reader needs.
- Put one main idea in each sentence or list item.
- Keep related rules together.
- Make conditions explicit and keep them close to the actions they control.
- Separate different concerns with the smallest useful heading or grouping.
- State valid behavior before listing exceptions or prohibitions.
- Add a negative rule when it closes a real ambiguity, safety risk, or rejected alternative.
- State tradeoffs directly.
- Add an example when an abstract rule is still hard to use.

### Prose, Lists, And Tables

- Use complete natural sentences in explanatory prose.
- Keep headings short.
- Use short paragraphs for distinct concerns. Let each paragraph develop one connected thought.
- Introduce lists with a sentence or heading.
- Nest child items under the item that introduces them.
- Keep list items parallel.
- Use a table when readers need to compare the same fields across several items.
- Otherwise, prefer prose or a list.

### Descriptions

A `description` helps a reader decide whether to open the file.

- Start with the source's purpose, trigger, or useful result.
- Use ordinary words instead of a stack of classification terms.
- Keep enough detail to distinguish sibling routes.
- Do not force every description into the same sentence pattern.

### Responsibilities

An optional `responsibility` helps an editor decide what belongs in the file by stating what it defines. Include it only when it adds a useful boundary beyond the route and description. It creates no authority or loading behavior.

### Category Openings

Keep the category name as the level-1 title. Put its primary question in a level-2 heading immediately below it, followed by the definition and any necessary supporting explanation. Explain how the category helps answer that question.

The question describes the category's role. It does not expand the entrypoint's responsibility or make the entrypoint define every answer beneath it. The heading has no special parsing or loading meaning and adds no metadata field.

### Essence And Summaries

An essence summary keeps only what a reader needs to understand the subject's identity, mechanism, important distinctions, and boundary.

- Use complete sentences or short parallel bullets.
- Put one idea in each sentence or bullet.
- Keep concrete mechanisms and meaningful contrasts.
- Remove promotion, repetition, history, and edge cases that do not change the summary.
- Let the detailed source carry qualifications that are not needed at the entry boundary.

## Sources And Links

Let each source answer one clear question. Keep enough meaning there for the source to stand on its own.

Link to related sources instead of copying their full detail. Repeat only the meaning needed at an independent entry boundary, such as the shared product summary in Vision and README.

A link shows a relationship. It does not merge or broaden authority, scope, loading, responsibility, or lifecycle.

Name or link a standard `route` only when its role or direct relationship changes how the source is selected or used. A parent may name the standard child `routes` it exposes. Another source may name a directly related `route` only when omitting it would hide real behavior or a meaningful boundary. Name an exact path only for navigation, loading, validation, management, mutation, migration, or standard topology. Physical depth alone never justifies a reference. Otherwise, name the role or authoritative source that applies.

## Actors, Authority, And Terms

State actors and relationships directly.

- Use `user` when the person directing or accepting work matters.
- Use `agent` only when the agent is the meaningful actor.
- Use `people` or `readers` when that is the relevant relationship. Use `plain` or `readable` for text when that is the intended property.
- Omit the actor when the action is already clear, such as `Update the affected current document`.
- Use `operator` only when a technical interface defines that role.
- Name the authoritative document, `route`, system, person, or role instead of using an unexplained `owner`.
- Use ownership language only for possession or managed lifecycle.

The [typed authority terminology decision](../../decisions/framework/authoritative-source-terminology.md) explains the difference between authority and ownership. The [Open Forge Dictionary](helpers/dictionary.md) shows which term to use.

Prefer a direct verb when it explains the relationship:

- `The Architecture document defines the current structure.`
- `The Decision records why the choice was made.`
- `GitHub contains the current issue state.`

Use `authoritative source` when authority itself matters, such as choosing which source to follow when two sources disagree. Use `authoritative document`, `authoritative route`, or `authoritative system` when the source type helps the reader.

Explain an unfamiliar Open Forge term at its first useful appearance in each standalone file, or link directly to its definition. After that, use the established term consistently.

- Use `Core`, `Memory`, `Framework`, and `Extensions` only for their defined meanings.
- Use `standard` for default Open Forge routes or configuration.
- Use ordinary lowercase words when no defined concept is intended.
- Put backticks around an exact defined term, such as `root route`, `responsibility`, or `entrypoint`.
- Prefer established terms and links over synonyms introduced only for variety.
- Keep `self-growing` in Memory's definition. Explain the deliberate growth described in the [dictionary](helpers/dictionary.md#memory).

## Sentences And Punctuation

- Prefer active voice and concrete verbs when they make the relationship clearer. Use passive voice when the result deserves the focus or the actor is immaterial.
- Prefer separate sentences to stacked qualifications.
- Prefer periods to semicolons.
- Use commas, parentheses, colons, or separate sentences instead of em dashes.
- End complete sentences with terminal punctuation.
- Do not punctuate true fragments such as headings, labels, table cells, frontmatter descriptions, generated entry labels, or fragment-only list items.
- Do not mix fragments and complete sentences in one list.
- Use question marks only for actual questions.
- Do not hedge accepted current state with `maybe`, `probably`, or `generally`. Keep those words when uncertainty is real.
- Use backticks and links according to the Markdown contract, not as decoration.

## Examples

Keep examples proportionate to the point they explain. Include one when it makes a rule or relationship easier to use.

Use examples that are valid for the APIs, formats, and tools they use. If an example is intentionally incomplete, label it as schematic.

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

### List Relationships

Less clear:

```text
- Supported formats:
- Markdown
- YAML
```

Clearer:

```text
- Supported formats:
  - Markdown
  - YAML
```

The nested list makes the relationship clear.

### Direct Relationship

Less clear:

```text
The operator should update the owner when direction changes.
```

Clearer:

```text
Update the affected current document when accepted direction changes.
```

The clearer version removes roles the reader would otherwise need to interpret.

### Typed Authority

Less clear:

```text
This file owns architecture.
```

Clearer:

```text
The Framework Architecture defines how Core and Memory work.
```

The clearer version uses a direct verb to name the source and the question it answers.

### Concrete Mechanism

Less clear:

```text
Memory supports recursive scope.
```

Clearer:

```text
Place a scope after a Memory state when it applies only to that state. Place it before the state routes when the subject needs its own Memory states.
```

The example makes the mechanism usable without prior Framework knowledge.

### Positive Boundary

Less clear:

```text
Extensions must not inject blocks, create hidden state, redefine routes, or add another runtime.
```

Clearer:

```text
Extensions add complete files. Routed content follows its Framework route. Native and support files follow the rules of the capability or tool that uses them.
```

The positive form defines the valid model. Add a negative sentence when a specific risk remains unclear.

## Lossless Rewriting

A language rewrite may improve wording, sentence structure, order, or layout. It must preserve the source's rules, logic, and meaning.

Before accepting a rewrite, compare it with the source and confirm that it preserves:

- Rules, conclusions, permissions, and prohibitions
- Scope, authority, and ownership boundaries
- Conditions, exceptions, qualifications, and tradeoffs
- Actor, source, sequence, dependency, and state relationships
- Defined terms, links, metadata, and selection meaning
- Whether each statement is required, allowed, recommended, optional, or uncertain

Do not rewrite historical quotations or archived wording unless the task targets them.

## Review Checks

- Can a reader understand the passage without the current conversation?
- Can a technically literate reader explain it correctly after one read?
- Does it state the main point before its qualifications?
- Does each sentence or list item carry one main idea?
- Are related points together and different concerns separated?
- Are conditions clearly connected to the actions they control?
- Does the flow help readers move from one thought to the next and find the detail they need?
- Are unfamiliar terms defined or linked where first used?
- Are authority, ownership, scope, and actors clear?
- Does every abstraction that needs context have an example?
- Does each named `route` express a direct behavioral relationship or required path identity rather than convenient topology?
- Could a link remove repeated detail without making the source incomplete?
- Is the language direct, natural, precise, and free of needless formality?
- Did the rewrite preserve the complete meaning?

## Related Current Sources

- [Canonical Markdown Syntax](../framework/markdown/syntax.md)
- [Routed Markdown Representation](../framework/markdown/routes.md)
- [Open Forge Dictionary](helpers/dictionary.md)
- [Open Forge Principles](../principles.md)
- [Project Voice](project-voice.md)

## Decisions And Rationale

- [User-facing writing](../../decisions/framework/user-facing-writing.md)
- [Typed authority terminology](../../decisions/framework/authoritative-source-terminology.md)
- [Canonical Markdown authoring](../../decisions/framework/canonical-markdown.md)
