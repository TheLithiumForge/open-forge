# ADR 055: Review behavior and evidence together

This record explains the rationale for reviewing behavior and evidence before integration.

## Decision context

The project uses small modules, strict typing and several layers of tests, but none of those tools independently decides whether a change expresses the intended game behavior. A parser can be well typed and accept the wrong fields. A timer can be thoroughly tested and still use a mistaken definition of active time. Review connects the stated behavior to the implementation and its evidence.

The workflow discussion favored review before integrating a change so that behavior, implementation and evidence could be assessed together.

## What a reviewer can assess

A useful change description starts with the trigger and resulting behavior. For a mismatch fix, the reviewer needs to know whether the problem occurred at an exact closing edge, after visibility changed or after replacing a deal. Those are different risks even if all involve a timer.

The source then makes the mechanism inspectable. Pure transitions live in `game.ts`; browser observation and lifecycle live in the session hooks; focus and scrolling live in the board. A change that crosses those boundaries deserves an explanation of why each piece moves. [Architecture](architecture.md) supplies that shared vocabulary.

Evidence follows the claim. A literal deck vector supports generation stability. A parser test supports rejection of an inconsistent snapshot. A browser journey supports the focus result after Resume. A screenshot supports an artwork observation. Review is stronger when the evidence matches the behavior than when a long list of unrelated commands is attached.

## An example review path

Consider changing the clock so a late callback closes a mismatch correctly. The reviewer can inspect whether measured deltas remain fractional, whether the current generation is checked and whether synchronization still happens before activation. Focused clock tests can exercise arithmetic; browser edge tests can show the adapter's effect.

A broad refactor that merely moves code has a different burden: the reviewer looks for preserved boundaries, unchanged format constants and stable imported behavior. A formatting-only change has less semantic surface. Scaling explanation to the actual change keeps the process useful rather than ceremonial.

## Alternatives and costs

Relying solely on a green suite would make untested assumptions easy to integrate. Requiring human review to manually repeat every automated check would waste time and still be less precise at numeric edges. The chosen rationale gives tools the repeatable assertions and people the interpretation, scope and maintainability questions.

Review adds latency, especially when the change is small. Small behavior files and focused descriptions reduce that cost by making the result understandable. The point is not to expand every fix into a design process; it is to keep the relationship between problem, code and proof visible.

## Historical and current evidence

The repository contains source audits, exact-version checks and a README verification record. Those are concrete aids to review.

[Code review guide](code-review-guide.md), [ADR 052](adr-052-release-tags.md), [ADR 053](adr-053-docs-in-repo.md) and [ADR 054](adr-054-acceptance-journeys.md) are the neighboring pages. The practical outcome is a reviewable implementation with proportionate evidence.
