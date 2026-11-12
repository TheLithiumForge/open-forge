---
open-forge:
  description: Issue 21 comment-link implementation and focused verification state
  tags: [Memory, Working, Checkpoint, CLI, Contextual]
---

# Comment Links
Comment links is the issue 21 workstream that connects code and docs through path comments.

The maintainer's lane assignment selects shared recognition, resolution, References, Context, Doctor, Repair and verified historical link fixes. Work stays on `feature/comment-links`. CI runs full suites. Local work runs a build and focused Unit and Integration checks.

## Execution

The CLI edits local project files. Git and existing recovery bundles provide recovery. Existing containment, expected-byte and mutation guards handle malformed input, stale plans and cooperating processes. Same-user hostile namespace changes stay outside the existing safety model.

Use the current Markdig facts, BCL file reads and shared reference resolver. The product-defined comment grammar belongs in Sources. No dependency or runtime change is selected. The only grammar addition is the requested marker syntax. Existing bounded host Git reads provide history evidence. No exceptional machinery is selected.

Unit checks cover marker and path rules. Integration checks cover command consumers, real file moves and exact repair bytes. Full managed, EndToEnd and Native AOT gates run in CI under the lane instruction.

## State

Done: Loaded lane instructions, issue spec and scoped rules. Updated shared and consuming contracts. The managed Unit-project build passes with zero warnings. The focused `Feature=references` selection passes 121 tests with no skips.
Now: Implement shared comment extraction and command integration.
Next: Run focused checks, update public docs and Skills, then verify and fix the 66 audited links and three historical ranges.
Blocker: The standard repository build reports NU1903 for the existing ILCompiler pin. The workspace-local APPDATA retry did not produce a verified repository build. A local-only warning override is awaiting maintainer direction. No dependency pin was changed.

## Verification

The first managed reference selection ran 121 tests. It found one scanner defect and three new finding-inventory expectations. The corrected selection passes 121 of 121 with no skips. Commands:

```powershell
dotnet build src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/OpenForge.Cli.Core.UnitTests.csproj --configuration Release --no-restore -p:OpenForgeSkipDevelopmentPublish=true
dotnet artifacts/bin/OpenForge.Cli.Core.UnitTests/release/OpenForge.Cli.Core.UnitTests.dll --filter-trait Feature=references --minimum-expected-tests 1 --fail-warns on --fail-skips on --no-ansi --progress off
```

`npm ci` passes in the repository and documentation site. `git diff --check` passes. Index preview and application update only this checkpoint's catalogue. Full managed, EndToEnd and Native AOT suites have not run locally.
