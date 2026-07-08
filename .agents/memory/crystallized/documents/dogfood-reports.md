---
open-forge:
  description: Route to the dogfood compliance and experiment reports and the recommendation syntheses at the repository root
  tags: [Memory, Document, Record, CurrentTruth, Dogfood, Evidence]
---

# Dogfood Reports

Accepted evidence records from dogfooding Open Forge against real agent sessions. The report files own detailed truth; they live at the repository root and are historical records once written.

- `dogfood-v2-compliance-report.md` - first structured compliance round against an installed workspace.
- `dogfood-v2-compliance-report-opinion-on-staged-changes.md` - v2 follow-up opinion on the then-staged framework changes.
- `dogfood-v3-compliance-report.md` - compliance round after the v2-driven framework updates.
- `dogfood-v4-experiments-report.md` - two redesign experiments, protocol restructuring versus a bulk-dump CLI tool; both closed the required-skills gap by reducing friction, neither through mechanical enforcement.
- `recommendations-general.md` - general recommendations from the initial static critique.
- `recommendations-open-forge-native.md` - framework-native recommendations from the initial static critique.
- `recommendations-synthesis.md` - the capstone synthesis across all rounds; grounds the loading-reliability decision.

Headline conclusions are crystallized in `.agents/memory/crystallized/decisions/loading-reliability.md`.
