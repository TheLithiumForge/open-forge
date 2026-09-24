---
title: What Open Forge doesn't do
sidebar_label: Limits
description: Honest limits to know before you adopt Open Forge.
---

# What Open Forge doesn't do

Some limits to know before you adopt it:

- **It doesn't make a model deterministic.** Open Forge makes the right context cheap to find and the rules explicit. An agent can still misread or skip them, so review stays with you.
- **It depends on your harness reading `AGENTS.md`** (or `CLAUDE.md` through the bridge). Everything starts there.
- **It isn't a methodology.** There's no mandatory workflow, role, or ceremony. Extensions offer methods, but installing one doesn't make its methods required.
- **It doesn't record everything.** Memory holds what's worth keeping, and agents save outcomes deliberately rather than logging every conversation.
- **It isn't a runtime.** There's no hidden database, background process, or agent host. Tags and routes are conventions that agents follow by reading them, not code that enforces them.
- **It doesn't grant permissions.** Installing a workflow or finishing a recipe never authorizes commits, merges, publishing, or contacting external systems. Your harness's permission model still applies.

## The CLI is still in beta

The CLI is in public beta. Command output and some behaviors may still change before 1.0. The Framework files themselves work with or without the CLI.

Found something rough? [Open an issue](https://github.com/TheLithiumForge/open-forge/issues).
