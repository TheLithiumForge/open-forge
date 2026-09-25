---
open-forge:
  description: Approved journey qualification and remaining platform coverage
  tags: [Memory, CLI, Testing, Complete, Archived, Historical, Contextual]
---

# Approved journey qualification

Stages 5–6 are complete for the approved 26 flows. All 82 journey cases pass in each of three public execution modes: managed, native, and the managed runner using the native CLI. Each mode passes all 245 public cases.

The final Windows qualification passed 3,271 unit tests. Each integration mode passed 2,343 cases, failed none, and skipped 17 existing platform-specific cases. Windows Native AOT build and .NET analyzer/format checks passed. This is not evidence of Linux or macOS execution.

Four prior failures were stale expectations. Two Install serialization assertions now expect 27 effects. Extension List includes Collaboration and the four Planning templates; Status reflects the same Planning inventory and measured totals. Production behavior was unchanged by these expectation corrections.

The delivery report qualifier rejects skipped tests, so these local results do not establish that the complete CI delivery command passes. CI must qualify its actual platform runs independently.

C17-07 remains deferred until there is a concrete Markdown example where detaching a link cannot preserve authored meaning. No additional scenario expectations were introduced.

Detailed execution reports and temporary scripts remain in the local evidence archive. The executable suites and reviewed snapshots remain in source control. Standard verification entry points are `npm run verify`, `npm run build:native`, and `npm run test:built`.

## Entries

- [Correction of the four baseline test failures before the beta](beta-baseline-acceptance.md) - #Memory #CLI #Testing #Archived #Historical #Contextual
- [Flow-by-flow qualification of the 26 approved journeys](g6-flow-report.md) - #Memory #CLI #Testing #Archived #Historical #Contextual
- [Plain-language journey descriptions](journeys-in-plain-language.md) - #Memory #CLI #Archived #Historical #Contextual
