---
open-forge:
  description: Assess whether the current Experience Design Skill earns first-party distribution beyond ordinary agent reasoning
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Extension, Skill]
---

# Experience Design Value

## Question And Status

Does the current Experience Design Skill provide enough reusable value to ship, or can an ordinary capable agent handle this work with the project's existing context?

The user accepted removal after this assessment. The [focused package decision](../../../crystallized/decisions/extensions/focused-extension-packages.md) records that outcome, including removal of the shipped files and matching repository Skill. This analysis preserves the evidence and its limits.

## Inspected Content

At `d2e2e33f`, the Skill at `src/extensions/development-toolkit/content/.agents/skills/experience-design/SKILL.md` (historical Git path) consists of a short method and three references for journey mapping, design review, and implementation handoff. All four files contain prose. They provide no executable tools, integrations, examples, reusable output shapes, project research, or demonstrated before-and-after results.

Its most useful content is attention to interaction states: interruption, permissions, errors, cancellation, and recovery as well as the main journey. It also keeps observed evidence, proposed design, accepted requirements, and implementation discretion distinct. Those are useful review prompts.

The references repeat much of the main method and name broad criteria rather than teaching a difficult operation. Accessibility is named, but no concrete inspection procedure, supported standard, test method, or worked finding is supplied. A mention of accessibility does not establish an accessibility assessment.

## Capability And Added Value

A harness supplies access to tools and context. A prose Skill supplies additional instructions through that harness. This Skill adds attention and a preferred review method; it adds no tool access or executable inspection capability.

The [Agent Skills overview](https://agentskills.io/home) explains that Skills package specialized knowledge, procedures, and optional resources. Scripts are optional: a prose-only Skill can be valuable when its method supplies knowledge or consistency that the task otherwise lacks. [Anthropic's authoring guidance](https://www.anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills) recommends beginning with observed gaps on representative tasks and evaluating whether the Skill addresses them.

My assessment is that most of this Skill's advice overlaps with what a capable agent can reason about when asked to review an experience. Its possible benefit is more consistent coverage of overlooked states. That is a reasoned content assessment, not a measured claim about every model or harness. No comparison with and without this Skill was run, and none was found in the inspected task evidence. The earlier [Toolkit decision](../../../crystallized/decisions/extensions/development-toolkit.md#rationale) records the intended specialized value; it does not demonstrate that benefit.

## Recommendation

Leave Experience Design out of the proposed first-party catalogue for now. Do not create a separate package for these four files or replace them with another generic Skill merely to keep a Skill in the catalogue. Retain useful reasoning for a future targeted proposal after removal.

Reconsider it when actual use exposes a repeatable gap. A stronger candidate would address that gap with a focused method, examples or a useful output shape, and evidence from representative work. A large checklist or a larger resource directory would not by itself establish value.

The smallest useful future comparison would use the same project brief and target revision with and without the candidate method. Compare consequential omissions, false findings, and actionable output. Keep project facts and task authority identical. This evaluation is proposed, not performed or required for the current content corrections.

## Disposition

Removal is accepted. The [package proposal](extension-package-proposal.md) preserves the reduced catalogue rationale. Any future Skill proposal should be evaluated on its own useful contribution.
