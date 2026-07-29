---
open-forge:
  description: Historical novice and expert UI-shaping probes used to evaluate whether Open Forge adapts decision elicitation to the completeness of a request
  tags: [Memory, Archived, Session, WorkHistory, Contextual, Historical, Product, Experience, Evaluation, Decision, Elicitation]
---

# Adaptive Decision Elicitation Probe

## Purpose

This session tested whether the accepted [Adaptive Decision Elicitation](../../crystallized/decisions/adaptive-decision-elicitation.md) contract makes an agent behave like an adaptive expert collaborator.

The expected behavior was not one fixed discovery conversation. A vague nontechnical request should receive plain-language framing, a useful recommendation, and only the smallest consequential question. A detailed technical request should retain its settled constraints, expose only material gaps, and converge without forcing the user through introductory discovery.

The Experience Design lens shaped the evaluation around user effort, trust, progressive disclosure, risk visibility, and continuity rather than the presence of particular documents or internal Framework terms.

## Method

Two independent read-only agents entered the real workspace through its normal `AGENTS.md` and loader contract. Neither received the design analysis, the expected answers, a selected Workflow, or permission to edit or implement.

Both agents handled the same broad subject, a possible UI for Open Forge, at different levels of request completeness. Each interaction stopped after two responses.

## Nontechnical Probe

The first request said:

> I want a nice UI for Open Forge so I do not have to use the terminal as much. I am not technical and I do not know what it should look like yet. Help me figure out what we should build. Do not implement anything.

The agent:

- Reframed the task around what the UI should help accomplish before visual appearance
- Recommended a calm guided experience organized around outcomes rather than files or commands
- Kept technical detail behind progressive disclosure
- Asked one concrete question about the last outcome for which the terminal became a barrier
- Explicitly said that technical vocabulary and visual preferences were not needed yet

The follow-up explained that the user wanted to understand what the agent knows and is about to change, feared breaking files, worked alone on one laptop, preferred simplicity, and wanted review before application.

The agent converged on an `Understand → Review → Apply safely` journey with three areas:

- A plain-language view of goals, relied-on facts, assumptions, uncertainty, and optional source details
- A review surface for intended results, affected files, risky changes, and before-and-after differences
- History and recovery concepts for understanding and reversing applied changes

It excluded team features, dashboards, marketplaces, visual customization, and command-by-command UI parity from the first version. It asked no further question.

## Technical Probe

The first request already established a local-first web UI over the CLI, Markdown and CLI authority, no daemon or private database, deterministic shared operations, and an initial focus on health, routed-context inspection, plan preview, and diff review.

The agent:

- Preserved every stated constraint
- Framed the UI as a projection of the CLI rather than another source of truth
- Recommended an ephemeral loopback browser session and a shared deterministic operations layer
- Identified the missing common write-preview contract as the main architectural prerequisite
- Asked only about runtime packaging, first-release mutation boundary, and initial audience

The follow-up resolved one local workspace, synchronous reads, persisted inspectable plans before mutation, restart survival, non-interactive execution, local-only browser access, accessibility, and keyboard operation. It asked the agent to recommend the remaining choice.

The agent converged without further questions. It recommended using generated-index rebuilding as the first bounded end-to-end mutation and proposed a `plan → persist → review → apply → verify` contract with immutable plan input, stale-plan refusal, CLI parity, loopback protection, and accessible interaction requirements.

## Findings

The central theory held:

- Question depth adapted to request completeness
- The nontechnical path used outcome language and hid Framework taxonomy
- The technical path did not repeat settled constraints or force an introductory workflow
- Both paths recommended a coherent direction before requesting more information
- Both stopped asking when remaining choices could be handled through professional judgment
- Neither treated installed Workflows as mandatory stages or asked the user to select one

The nontechnical second response exposed one useful weakness. It presented a few implementation-dependent recommendations, including a desktop shell and recovery behavior, with nearly the same confidence as accepted needs. Initiative was correct, but accepted direction, recommendations, and assumptions should remain visibly distinct.

## Resulting Refinement

The loader, Vision Workflow, loader Maintenance contract, and accepted Decision now require coherent recommendations to be distinguished from accepted direction. This keeps the agent decisive without turning a plausible default into accidental truth.

No UI was implemented and no probe agent changed workspace files.

## Limits And Future Evidence

This was a small qualitative probe with two agents, one subject, and two turns each. It supports the behavioral contract but does not prove reliability across models, domains, long conversations, or implementation work.

Future evaluations should vary user expertise, domain, ambiguity, risk, and request completeness. Useful measures include repeated questions, low-impact questions, reopened settled choices, unlabeled assumptions, premature implementation, user vocabulary burden, and whether accepted results reach the appropriate durable sources.

## Related Records

- [Adaptive Decision Elicitation analysis](../analysis/2026-07-29_adaptive-decision-elicitation.md)
- [Open Forge Vision](../../crystallized/documents/vision.md)
- [Open Forge Principles](../../crystallized/documents/principles.md)
- [Open Forge Architecture](../../crystallized/documents/architecture.md)
- [Framework Architecture](../../crystallized/documents/framework/architecture.md)
