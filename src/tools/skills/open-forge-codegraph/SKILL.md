---
name: open-forge-codegraph
description: Find code and trace call paths with the active CodeGraph index. Use for structural code questions before grep, then confirm call sites in source.
---

# CodeGraph

CodeGraph is a local code index that finds symbols and traces call paths.

When this tool is active and `.codegraph/` exists, use `codegraph_explore` for structural questions before grep. If the MCP tool is deferred, load it by name. Without MCP, run `open-forge tool run codegraph explore "<symbol names or question>"` for the same output.

If CodeGraph is declared but missing locally, `tool run` tells you to run `open-forge tool install`. That bare command installs every declared tool with consent.

If you need to create or refresh the index, run from the workspace root after the user agrees to indexing:

- `open-forge tool run codegraph init --yes` - creates the first index.
- `open-forge tool run codegraph index --quiet` - rebuilds an initialized index.
- `open-forge tool run codegraph sync` - refreshes changed files.

An absent index means skip CodeGraph until indexing is agreed. An index can be stale after edits. Confirm call sites in the source. Our C# trial measured 41% recall, so a missing graph result doesn't rule out a caller. TypeScript and Python also have incomplete call coverage. JavaScript call accuracy wasn't measured. PowerShell has no CodeGraph graph support in this trial.

CodeGraph is third-party software maintained by colbymchenry/codegraph under MIT. Its optional Claude prompt hook adds local index context on structural prompts. The default is on. `CODEGRAPH_NO_PROMPT_HOOK=1` disables it.
