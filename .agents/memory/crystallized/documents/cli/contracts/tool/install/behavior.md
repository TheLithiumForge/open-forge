---
open-forge:
  description: Define Tool install checks and effects
  tags: [Memory, Document, CLI, Tool, CurrentTruth]
---

# Tool install behavior

Tool install behavior is the flow behind `tool install`.
For example, when CodeGraph is already installed, Install skips the npm command and still runs CodeGraph's project setup.

Status: Lean design decided on 17 November 2026.

## Named install

1. Read the settings, detect the tool and read its integration files. Stop on a conflict.
2. Check the tool's package manager or runner unless the tool is already installed. Stop if it's missing.
3. Show the plan and obtain consent.
4. Run the install command unless the tool is already installed.
5. Run the tool's project setup, if it has one. Running it again repairs the setup.
6. Apply Open Forge's integration changes that are missing, then add the ID to the settings `tools` list if it's missing.

## Bare install

1. Read the settings `tools` list and detect each configured tool.
2. If none are configured, or all are installed, report that and run nothing.
3. Check every missing tool's package manager or runner. If any is missing, run nothing and list what to install.
4. Show the plan and obtain consent.
5. Run each missing tool's install command in catalogue order.

Bare Install never runs project setup or changes files. The project configuration already exists.

## Results

Stop at the first failed command or change.
Report done, failed and not-started steps separately.
`--dry-run` runs no commands, changes no files and asks nothing.
Use the common status precedence and shared schema-3 exits.

## Conformance

Required cases cover:

- A plan and run for each tool, with the install step skipped when the tool is detected.
- A missing package manager or runner, which runs nothing.
- A failed command with its exit code, output tail and next step.
- No consent without a terminal, a declined plan and `--dry-run`.
- Bare install with nothing configured, nothing missing and one missing tool.
- Unchanged unrelated settings and file content.
