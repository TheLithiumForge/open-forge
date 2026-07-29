---
open-forge:
  description: Historical brownfield and greenfield probes testing whether Open Forge adapts questions, recommendations, technical depth, and plans across three expertise levels
  tags: [Memory, Archived, Session, WorkHistory, Contextual, Historical, Product, Experience, Evaluation, Decision, Elicitation, Expertise]
---

# Adaptive Expertise Probes

## Purpose

This session extended the initial [Adaptive Decision Elicitation probe](2026-07-29_adaptive-decision-elicitation-probe.md) across:

- Three expressed expertise levels: nontechnical, practitioner, and expert
- One brownfield feature in the Open Forge repository
- One greenfield product in a clean Core-only workspace
- Two turns per interaction, with accepted answers supplied after the first response

The test asked whether Open Forge adapts not only question count, but also recommendation depth, technical vocabulary, planning detail, use of accepted context, and convergence.

## Setup

The current Open Forge CLI was built and linked globally. It installed Core at the root of a temporary `.temp-agent/storyframe` project. The clean project contained no development toolkit, project Memory, implementation, or requirements beyond the prompts.

All probe agents were read-only. Brownfield agents entered the current repository normally. Greenfield agents were told to treat the temporary Storyframe directory as the complete workspace and not inspect its parent or siblings.

The Experience Design lens evaluated user effort, comprehension, control, progressive disclosure, trust, recovery, and whether implementation detail was justified by the current decision.

## Brownfield Subject

All three users requested a package-management experience over the existing Open Forge Extension lifecycle.

### Nontechnical Path

The request described a safe app-store-like place for finding, installing, updating, and removing add-ons.

The agent:

- Reused the existing Extension safety substrate instead of proposing a replacement
- Explained the first release through browsing, preview, confirmation, validation, and recovery
- Recommended a curated first-party catalogue rather than an open marketplace
- Asked one material question about whether the first catalogue should contain only maintainer-reviewed packages

After the user accepted that recommendation, the agent produced a bounded staged plan and asked no further question.

### Practitioner Path

The request named Extensions, dependency explanations, affected-file previews, lifecycle operations, later remote registries, and Markdown completeness.

The agent:

- Proposed an explicit `extensions` command surface over the current lifecycle
- Distinguished domain, operation, plan, and adapter boundaries
- Preserved existing containment, ownership, Git, rollback, and plain-file behavior
- Recommended postponing remote registries
- Asked two material questions about first-party package granularity and locally modified managed files

After the user kept `development-toolkit` intact and accepted preserve-and-detach behavior, the agent produced a repository-specific implementation and verification plan without reopening either choice.

### Expert Path

The request specified identity, SemVer, deterministic resolution, trust, offline caching, immutable plan and apply, stale-plan refusal, transactions, state boundaries, and remote registries.

The agent:

- Proposed configuration, lock, receipt, registry, cache, plan, transaction, and CLI contracts
- Distinguished practical crash-recoverable atomicity from impossible general filesystem snapshot atomicity
- Defined deterministic resolution, provenance, namespace binding, and offline behavior
- Mapped concrete modules, migration stages, and adversarial verification
- Asked three product-level questions about identity, trust depth, and atomicity

After the user accepted all three recommendations, the agent produced an ordered gated implementation plan with no remaining product questions.

## Greenfield Subject

All three users requested a Storybook-like component workbench with stronger framework compatibility and customizable application layout.

### Nontechnical Path

The request described the desired product without choosing frameworks or architecture.

The agent:

- Recommended an independent local-first component studio
- Explained component examples, isolated previews, rearrangeable panels, useful presets, and initial framework coverage
- Distinguished workbench rearrangement from a visual page-composition canvas
- Asked one material product question between those two meanings of layout freedom

The first response still exposed unnecessary implementation choices, external libraries, and architecture for the stated expertise level. After the user selected workbench rearrangement and explicitly requested a simple approvable plan, the final response became appropriately outcome-led and avoided architecture.

### Practitioner Path

The request established adapters, portable stories, TypeScript, a local server, and replaceable application layout while leaving several structures open.

The agent:

- Recommended a headless manager, isolated previews, explicit adapters, CSF compatibility, a plugin boundary, and a Vite host seam
- Proposed a package topology and first vertical release
- Asked three material questions about unmodified CSF support, mixed adapters, and static output

The response was technically sound but closer to expert depth than the request required. After the user accepted the recommendations, the agent produced a practical repository and delivery plan without further questions.

### Expert Path

The request already established the manager-preview protocol, preview isolation, first-party adapters, statically analyzable stories, layout regions, accessibility, and later integration boundaries.

The agent:

- Defined serializable story, adapter, protocol, layout, navigation, accessibility, package, delivery, and verification contracts
- Kept visual testing and hosted collaboration outside Core
- Asked three remaining questions about story compatibility, adapter granularity, and manager-plugin trust

After those recommendations were accepted, the agent converged on ten implementation gates and asked no further questions.

## Findings

The accepted behavior held across both contexts:

- Question count and question type adapted to the information already supplied
- Every question could materially change product or architecture
- Settled answers were not reopened
- Each second turn converged without manufacturing new uncertainty
- Brownfield responses reused existing mechanisms and current contracts
- Greenfield responses built from the clean project rather than assuming Open Forge repository state
- Nontechnical paths used product outcomes for their decisive questions
- Expert paths exposed implementation contracts and verification appropriate to their requests
- No agent required the user to select a Workflow, understand Open Forge taxonomy, or complete a document form

One gap repeated clearly enough to justify a Framework refinement:

> Adaptive questioning does not automatically produce adaptive delivery.

The first-turn nontechnical and practitioner responses sometimes included architecture, package topology, external library choices, or verification detail that was not needed for the user's current judgment. The novice Storyframe response became appropriately simple only after the user explicitly requested a simple plan.

## Resulting Refinement

The loader now requires explanation and planning depth to match the request's demonstrated expertise and desired fidelity. The accepted Decision, Vision, Principles, Vision Workflow, README, and loader Maintenance contract carry the same current meaning.

This does not mean withholding useful risks from less technical users. It means expressing the consequential outcome first and revealing implementation detail only when the user requests it, demonstrates that it is useful, or needs it for a current decision.

## Limits

This was qualitative evidence from one model family, two subjects, three prompt styles, and two turns per path. The greenfield agents were reused after the brownfield probes, although they were given a separate Core-only workspace and explicit isolation instructions. External web research also influenced some greenfield recommendations.

The experiment supports the contract but does not establish reliable expertise inference across domains, cultures, accessibility needs, or long-running work. Future evaluation should measure answer length, unrequested jargon, premature implementation commitments, decision coverage, repeated questions, and whether optional detail remains discoverable when omitted from the first layer.

## Related Records

- [Adaptive Decision Elicitation decision](../../crystallized/decisions/adaptive-decision-elicitation.md)
- [Adaptive Decision Elicitation analysis](../analysis/2026-07-29_adaptive-decision-elicitation.md)
- [Open Forge Vision](../../crystallized/documents/vision.md)
- [Open Forge Principles](../../crystallized/documents/principles.md)
- [Open Forge loader](../../../loader.md)
