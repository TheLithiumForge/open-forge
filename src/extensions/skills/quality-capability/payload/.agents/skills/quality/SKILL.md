---
name: quality
description: Evidence-driven review, testing, behavior-preserving refactoring, and systematic debugging. Use to assess implementation or technical-design correctness, design tests or checks, improve internals safely, isolate failures, or verify fixes.
---

# Quality

Produce trustworthy evidence before changing confidence or behavior.

## Process

- Establish intended behavior or outcome, scope, mutation authority, and the evidence needed for a conclusion.
- Prefer reproducible observations and stable contracts over intuition or implementation-shaped tests.
- Preserve valid assertions and baseline behavior. Investigate failures instead of forcing green results.
- Separate diagnosis and findings from fixes unless mutation is explicitly in scope.

## References

- Read [Review evidence](references/review-evidence.md) when reviewing a change or making a correctness claim from inspectable evidence.
- Read [Design tests](references/design-tests.md) when selecting, authoring, running, or interpreting tests.
- Read [Preserve behavior](references/preserve-behavior.md) before refactoring or changing internal structure without intended behavior change.
- Read [Debug systematically](references/debug-systematically.md) when a failure, regression, flaky result, or unexplained state needs root-cause analysis.
