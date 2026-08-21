---
open-forge:
  description: Design cohesive C# call surfaces and finite strategies without parameter plumbing or speculative frameworks
  tags: [LoadNow, Directive, CSharp, Design, Parameters, Records, Strategy, Efficiency, Readability]
---

# C# Callable Design

## Instructions

- Prefer one or two explicit parameters for a behavioral method, constructor, or delegate. Treat four or five as the normal maximum. Use more only at an exceptional framework, interop, serialization, recursive traversal, or data-contract boundary where the separate values are genuinely clearer than a cohesive input.
- Pass an existing cohesive immutable record or stage context directly when the callee operates within that record's semantic boundary. Do not unpack a record merely to forward or repack many of its members. Let the receiving capability select the facts it owns.
- Introduce a context only when its values share one stage, lifecycle, invariant, or ownership boundary. Do not hide unrelated dependencies in an `Args`, `Options`, `Context`, dictionary, or service bag merely to satisfy a parameter count. Prefer a narrower typed view when passing a broad record would grant unrelated authority or couple distant layers.
- Distinguish behavior call surfaces from genuine data shapes. A concrete serialized result, immutable fact record, or source-generated model may contain more than five fields when that is its accepted schema. Keep construction readable with named members, focused factories, or cohesive nested values rather than long positional plumbing through behavioral methods.
- When one finite typed key repeatedly selects stable data or behavior, prefer one centralized immutable static map or table of small strategy values or non-capturing delegates over duplicated switches, condition chains, strategy class hierarchies, dependency injection, or reflective dispatch. Use enum or other typed keys, validate unknown values exhaustively, and keep the table at the nearest scope that owns the shared meaning.
- Do not promote a local branch merely to claim reuse. A static strategy earns a wider scope when an accepted shared contract already owns the policy or multiple real consumers demonstrate identical meaning. Keep command-specific payload formation and rendering local even when shared process presentation, status, or stream policy is centralized.
- Optimize for readable data flow and avoid redundant extraction, allocation, rendering, and dispatch. Do not evaluate unselected strategies or create per-call capturing delegates. Treat runtime-efficiency claims as boundary-specific: passing a reference-type record is cheap, while large value types and measured hot paths still require evidence.
- Do not nest or chain conditional (`?:`) expressions. Use ordered `if` returns for precedence and guard flow, or one clear exhaustive switch when it reads better. A single non-nested conditional expression is acceptable only for one obvious two-way value; never use indentation to make a conditional chain appear simpler than it is.
- Do not declare custom operator overloads or user-defined implicit or explicit conversion operators. Prefer visible named construction and conversion methods such as `Parse`, `TryParse`, `From`, `Create`, or `Read`. If an exceptional external boundary truly requires implicit framework invocation, obtain maintainer acceptance and annotate/register that behavior explicitly at the boundary; do not rely on hidden conversion or convention as ordinary application flow.
