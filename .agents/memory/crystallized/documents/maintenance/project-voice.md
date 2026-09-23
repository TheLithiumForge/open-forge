---
open-forge:
  description: How Open Forge READMEs and introductions explain the project in the maintainer's own engineering voice
  responsibility: Define the voice of public project introductions, with accuracy and terminology supplied by the Writing Standard
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Writing, Documentation, Voice, Internal]
---

# Open Forge Project Voice

## Purpose

READMEs and project introductions should sound like the engineer who built the project explaining it to a peer: direct, concrete, and plainly excited about the ideas behind it. Readers should come away understanding what it is, why it works the way it does, and whether it suits them.

Use this guide for public introductions to Open Forge and its packages. The [Writing Standard](writing.md) supplies shared clarity, accuracy, terminology, and review requirements. It also defines the more direct voice used in shipped rules and technical reference text. Choose the voice for the passage's purpose: a README can argue for an idea in its introduction and give precise instructions in its setup section.

These are repository authoring guides. The installed Framework remains understandable through its own files.

## Voice And Rhythm

Write like an engineer explaining something they believe in. Lead with the idea and the mechanism, then show what it looks like in practice: a file tree, a command, a table, a real example. Let the enthusiasm come from the ideas themselves, such as Adaptive Context Engineering, growing your own framework, or a CLI that accelerates the work without being required. Do not add it through adjectives.

Use `I` for the maintainer's own opinions and design choices, and `you` when inviting readers to try or adapt something. Neither needs to start every paragraph. Contractions and varied sentence openings keep the prose natural. Prefer active voice when the actor matters. Passive phrasing can read better when the result deserves the focus. Do not mechanically rewrite every sentence into an instruction.

Short, plain declaratives are welcome when they carry a real point, such as `That's the whole setup.` A dense explanation still deserves connected sentences or a list. Avoid packing several features or abstract qualities into one comma-separated sentence. Read it aloud: it should sound like a conversation with a technically curious peer, not a lecture, a sales pitch, or a greeting card.

State limits plainly. A short section on what the project doesn't do earns more trust than another list of benefits.

Avoid:

- Cute or clever headings, such as `A CLI Worth Keeping Nearby` or `Small Metadata, Useful Context`
- Taglines that close a section, such as `What grows from it is yours.`
- Inspirational or ceremonial framing, forced jokes, slogans, and exaggerated promises
- Lines that congratulate the reader for understanding something simple
- Semicolons. Use a period, or a comma and a conjunction.

Use plain, descriptive headings in sentence case that name what the section covers, such as `Get started`, `What you get`, or `The CLI`. Humor is welcome when it comes naturally. Refer to people by the relationship that matters, such as readers, contributors, or users.

## Shape Of An Introduction

Help readers move quickly from curiosity to trying the project. Actionability comes first: a reader should reach working setup steps within the first screen, and the rationale can follow once they have something running.

- Say what it is and the central idea in a few sentences.
- Give the setup commands right after that. Order setup routes from least to most friction, such as a package manager, then a downloadable release, then a manual copy, and label each one clearly.
- Show a small worked example the reader can copy, such as adding a first rule, rather than only describing what is possible.
- Give enough of an overview to make the files and tools approachable.
- Explain why it works the way it does.
- Be honest about limits.
- Link to deeper explanations where readers are likely to want them.

Scale this shape to the subject. A package README may need only a short introduction, its contents, and setup. It should not repeat the whole Framework explanation.

Keep introductory claims grounded in the sources that define them. Prefer measured facts, such as a file or token count with its method linked, to impressions. Explain how scopes support growth instead of promising unlimited active context. Describe the ideas behind ACE without claiming measured superiority over other approaches. Distinguish the self-contained Framework from the CLI that accelerates maintenance. Keep category questions aligned with their defining files.

## Examples

An opening can say what the project is and why it exists in a few plain sentences:

> Open Forge is a small Markdown framework for working with AI agents, built around your projects, your tools, and the way you like to work. It's agent- and harness-agnostic, and the whole design comes from an idea I call **Adaptive Context Engineering (ACE)**.
>
> An agent doesn't need to know everything, but it does need to know where everything is.

An opinion can be stated as the maintainer's own:

> Big predefined methodologies assume everyone works the same way, and nobody does. My use cases differ from yours, and yours differ from the next person's.

A worked example makes growth concrete. Open with the problem, then give the exact file to create and the command to run:

> Say your agent keeps calling work done without running the tests. Create `.agents/directives/testing.md`:

A plain heading and a direct first line do more than a clever title:

> Less natural: `## A CLI Worth Keeping Nearby`
>
> Plainer: `## The CLI`, followed by `The CLI is an accelerant, not a requirement. Everything it does, you can do by editing files.`

These passages illustrate the voice. They are not fixed copy. Adapt their rhythm and detail to the surrounding text. The [README](../../../../../README.md) shows the approach across a complete introduction.

## Review

Check that the passage sounds like a person, makes its value concrete, and gives readers an easy next step. Preserve every technical boundary, condition, and requirement. Personality should make the meaning easier to approach, and precise instructions should remain easy to act on.

The [User-Facing Writing Decision](../../decisions/framework/user-facing-writing.md) records why introductions and rules use different voices.
