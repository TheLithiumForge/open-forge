---
name: quality
description: Evidence-driven review, testing, behavior-preserving refactoring, and systematic debugging. Use to assess implementation or technical-design correctness, design tests or checks, improve internals safely, isolate failures, or verify fixes.
---

# Quality

Produce trustworthy evidence before changing confidence or behavior.

## Process

- Establish intended behavior or outcome, scope, mutation authority, and the evidence needed for a conclusion.
- Prefer reproducible observations and stable contracts over intuition or implementation-shaped tests.
- Preserve valid assertions and baseline behavior; investigate failures instead of forcing green results.
- Separate diagnosis and findings from fixes unless mutation is explicitly in scope.

## References

- `references/review-evidence.md` - Read when reviewing a change or making a correctness claim from inspectable evidence.
- `references/design-tests.md` - Read when selecting, authoring, running, or interpreting tests.
- `references/preserve-behavior.md` - Read before refactoring or changing internal structure without intended behavior change.
- `references/debug-systematically.md` - Read when a failure, regression, flaky result, or unexplained state needs root-cause analysis.
