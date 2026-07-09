---
open-forge:
  description: What the standup CLI's tests must cover to count as verified
  tags: [Extension, Pattern, Testing, Contract]
---

# Test Contracts

## Required Coverage

- Window resolution as a pure function with injected now/zone: Monday `laststandup` includes Friday and the weekend; `yesterday` at 00:10 local; a late-night entry near midnight lands in the correct window.
- Correction resolution: original, amended once, amended twice (chain resolves to latest).
- Storage: append-only property (existing lines untouched after log/amend), unparseable line reports its line number, missing file and missing parent directory on first log.
- CLI end-to-end: at least one real spawn of the executable against a workspace-local temp data file covering log → report.
- Argument errors: empty message, unknown command, bad `--since` token — usage plus exit 1.

## Style

Match the repo's runtime test conventions; keep fixtures workspace-local.
