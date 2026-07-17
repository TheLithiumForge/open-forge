---
open-forge:
  description: Keep all implementation, tests, and generated artifacts inside this workspace
  tags: [Extension, Directive, Workspace, Safety]
---

# Scope Control

All task work belongs inside the current workspace unless the user explicitly says otherwise.

## Axioms

- Before the first file operation in a session, verify the working directory is the workspace root; prefer workspace-absolute paths for patch and edit tools whose defaults may resolve elsewhere.
- Create implementation files under the project folder routed by workspace docs.
- Keep test fixtures, integration-test data, smoke-test data, and temporary files inside the workspace. This includes test code: do not use the operating system temp directory (`os.tmpdir()` or equivalents) in tests or scripts; use a workspace-local temp folder that is cleaned up and git-ignored.
- Do not read prior or sibling implementations of similar projects for design or code unless the user explicitly asks for comparison.
- Do not change sibling repositories or any path outside the workspace.
- If a write accidentally lands outside the workspace, remove it when safe and report it; a corrected violation still counts as a violation and must appear in the final report.
