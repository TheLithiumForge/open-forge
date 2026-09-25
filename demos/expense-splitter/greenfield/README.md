# Greenfield demo: build it from an idea

Start from an empty folder and a request. The same product exists half-built in the [brownfield demo](../brownfield/), so you can compare what an agent builds from scratch with what it does inside an existing codebase.

The four [seed levels](seeds/) change only how much you tell the agent up front. Level 1 is one sentence. Level 4 hands it a Vision, an Architecture, two Decisions, and scenarios as Open Forge records.

## Run it

1. Create an empty project outside this repository and commit a starting point:

   ```sh
   mkdir ~/expense-splitter-fresh && cd ~/expense-splitter-fresh
   git init && git commit --allow-empty -m "Starting point"
   ```

2. Install Open Forge and the Development Toolkit:

   ```sh
   open-forge install
   open-forge extension install development-toolkit
   git add -A && git commit -m "Added Open Forge"
   ```

3. **Pick a seed level** from [`seeds/`](seeds/) and give your agent that request. At level 4, copy the records first, as its README describes.

4. **Answer its questions** as a product owner would. With lower levels, a good agent asks about money handling and rounding, or proposes a Decision and waits for you. Accept what you agree with.

5. **Check the result** against [`checks.md`](checks.md).

## What to watch

- At levels 1 and 2, does the agent notice that money needs exact arithmetic, or does it reach for floating point?
- Does it propose Decisions as choices come up, or leave the reasons only in the conversation?
- By level 4, is it following the records, or rediscovering them?
