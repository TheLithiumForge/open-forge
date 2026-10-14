---
title: "Greenfield: build it from an idea"
sidebar_label: Greenfield
description: Build a small expense splitter from an empty folder, with requests from one sentence to a full set of Open Forge records.
---

# Greenfield: build it from an idea

Start from an empty folder and a request for a command-line tool that splits shared expenses between friends. The whole demo is in [`demos/expense-splitter/greenfield`](../../../../demos/expense-splitter/greenfield/).

For a first try, start with level 1:

> Build a small command-line tool that helps a group of friends split shared expenses and see who owes whom.

The other levels let you compare how more detail changes the result.

## Run it

1. Create an empty project outside this repository, and commit a starting point.
2. Install Open Forge and the [Development Toolkit](../extensions/development-toolkit.md):

   ```sh
   open-forge install
   open-forge extension install development-toolkit
   ```

   Decisions, and Documents such as a Vision or Architecture, come from these Extensions, not from the base install.

3. Give your agent the level 1 request above, or choose another level for a comparison:

   | Level | Request                                                                                                                              |
   | ----- | ------------------------------------------------------------------------------------------------------------------------------------ |
   | 1     | [One sentence](../../../../demos/expense-splitter/greenfield/seeds/1-idea.md)                                                        |
   | 2     | [A short brief](../../../../demos/expense-splitter/greenfield/seeds/2-brief.md), with money handling deliberately left unsaid        |
   | 3     | [The brief plus expected results](../../../../demos/expense-splitter/greenfield/seeds/3-scenarios.md), including rounding            |
   | 4     | [A Vision, an Architecture, two Decisions, and scenarios](../../../../demos/expense-splitter/greenfield/seeds/4-records/) as records |

   At level 4, copy the records into your project first, as the [level 4 README](../../../../demos/expense-splitter/greenfield/seeds/4-records/README.md) describes.

4. Answer the agent's questions as a product owner would, and accept the proposals you agree with.
5. Check the result against [the checklist](../../../../demos/expense-splitter/greenfield/checks.md).

## What to watch

- **Money.** Splitting 10.00 three ways in floating point gives 3.3333333333333335. Does the agent notice that money needs whole cents, or ask about it, before you tell it?
- **Rounding.** Someone has to pay the extra cent. Does the agent choose a rule and tell you, or does it happen by accident?
- **Reasons.** Do the choices end up as Decisions with their reasons, or only in the conversation?
- **Level 4.** With the records in place, does the agent follow them, or rediscover them?

**Next:** [Grow your own framework](../getting-started/grow-your-framework.md).
