---
open-forge:
  description: How Open Forge READMEs and introductions welcome readers and share the project naturally
  responsibility: Define the voice of public project introductions, with accuracy and terminology supplied by the Writing Standard
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Writing, Documentation, Voice, Internal]
---

# Open Forge Project Voice

## Purpose

READMEs and project introductions should sound like someone sharing a project they are pleased to have made: friendly, thoughtful, and comfortable letting readers decide whether it suits them.

Use this guide for public introductions to Open Forge and its packages. The [Writing Standard](writing.md) supplies shared clarity, accuracy, terminology, and review requirements. It also defines the more direct voice used in shipped rules and technical reference text. Choose the voice for the passage's purpose: a README can welcome readers in its introduction and give precise instructions in its setup section.

These are repository authoring guides. The installed Framework remains understandable through its own files.

## Voice And Rhythm

Write naturally, with quiet confidence in what the project offers. Show its value through concrete possibilities: preserving a useful decision, keeping relevant context close, or adapting the framework to a project's own tools and habits. Let readers see why those choices matter without telling them how impressed to be.

Use `we` when sharing a design choice and `you` when inviting readers to try or adapt something. Neither needs to start every paragraph. Contractions, varied sentence openings, and a slightly more eloquent turn of phrase can make the prose flow. Prefer active voice when the actor matters; passive or participial phrasing can read better when the idea or result deserves the focus. Do not mechanically rewrite every sentence into an instruction.

Give connected ideas room to develop. Avoid packing several features or abstract qualities into one comma-separated sentence. Develop the relationship in prose, or use a list when the items deserve separate attention. A few extra words can make a passage feel natural, while a string of short declarations can sound rehearsed. Read it aloud: it should feel like a conversation with a technically curious peer, without becoming a lecture or a sales pitch.

Humor is welcome when it comes naturally. It is not a requirement. Avoid forced jokes, slogans, exaggerated promises, and lines that congratulate the reader for understanding something simple. Refer to people by the relationship that matters, such as readers, contributors, or users.

## Shape Of An Introduction

Help readers move quickly from curiosity to trying the project:

- Explain what it is and how it could fit their work.
- Connect the central ideas to a practical benefit.
- Provide a short path to setup and first use.
- Give enough of an overview to make the files and tools approachable.
- Link to deeper explanations where readers are likely to want them.

Scale this shape to the subject. A package README may need only a short introduction, its contents, and setup. It should not repeat the whole Framework explanation.

Keep introductory claims grounded in the sources that define them. Explain how scopes support growth instead of promising unlimited active context. Describe the ideas behind ACE without claiming measured superiority over other approaches. Distinguish the self-contained Framework from the CLI that accelerates maintenance. Keep category questions aligned with their defining files.

## Examples

A project opening can establish fit without giving the reader a lesson:

> Open Forge is a customizable, AI-agnostic framework built around your projects, your tools, and the way you like to work.

A personal account of the project's origins can lead into its central idea:

> Inspired by spec-driven development and progressive disclosure, we built Open Forge around an idea we call Adaptive Context Engineering (ACE).
>
> Each task starts with a clear sense of what you want to achieve. Relevant details come into view as the work unfolds. What you learn along the way can then become part of the framework, ready to help with the next task.

An invitation can make customization concrete:

> From there, make it yours. Keep your favorite tools, add your own ideas, and change the parts that don't fit. We've included a CLI to make maintenance easier, but the files work on their own.

These passages illustrate the voice; they are not fixed copy. Adapt their rhythm and detail to the surrounding text. The [README](../../../../../README.md) shows the approach across a complete introduction.

## Review

Check that the passage sounds natural, makes its value concrete, and gives readers an easy next step. Preserve every technical boundary, condition, and requirement. Warmth should make the meaning easier to approach, and precise instructions should remain easy to act on.

The [User-Facing Writing Decision](../../decisions/framework/user-facing-writing.md) records why introductions and rules use different voices.
